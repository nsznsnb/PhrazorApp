using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data.Entities;
using PhrazorApp.Data.UnitOfWork;
using PhrazorApp.Models;
using PhrazorApp.Models.Mappings;

namespace PhrazorApp.Services
{
    /// <summary>
    /// フレーズ帳ユースケース（CTなし／UiOperationRunnerと連携）
    /// </summary>
    public sealed class PhraseBookService
    {
        private readonly UnitOfWork _uow;
        private readonly UserService _userService;
        private readonly ILogger<PhraseBookService> _logger;
        private const string MSG_PREFIX = "フレーズ帳";

        public PhraseBookService(UnitOfWork uow, UserService userService, ILogger<PhraseBookService> logger)
        {
            _uow = uow;
            _userService = userService;
            _logger = logger;
        }

        // ===== 読み取り =====

        /// <summary>フレーズ帳一覧（件数込み）</summary>
        public Task<ServiceResult<List<PhraseBookListItemModel>>> GetPhraseBooksAsync()
        {
            return _uow.ReadAsync(async u =>
            {
                var uid = _userService.GetUserId();

                var list = await u.PhraseBooks.Queryable()
                    .Where(b => b.UserId == uid)             // ★ 自分のブックのみ
                    .SelectList()
                    .OrderBy(x => x.Name)
                    .ToListAsync();

                return ServiceResult.Success(list, message: "");
            });
        }

        /// <summary>指定フレーズ帳の中身（フレーズ一覧）</summary>
        public Task<ServiceResult<List<PhraseBookItemModel>>> GetItemsAsync(Guid phraseBookId)
        {
            return _uow.ReadAsync(async u =>
            {
                // そのブックに紐づくフレーズを作成日時降順で
                var q = from bi in u.PhraseBookItems.Queryable()
                        where bi.PhraseBookId == phraseBookId
                        join p in u.Phrases.Queryable() on bi.PhraseId equals p.PhraseId
                        orderby (p.CreatedAt ?? DateTime.MinValue) descending
                        select p;

                var rows = await q.SelectItems().ToListAsync();
                return ServiceResult.Success(rows, message: "");
            });
        }

        /// <summary>追加候補検索（未追加のみ）</summary>
        public Task<ServiceResult<List<PhraseSuggestionModel>>> SearchPhrasesAsync(Guid phraseBookId, string? term, int take = 20)
        {
            return _uow.ReadAsync(async u =>
            {
                var uid = _userService.GetUserId();
                term ??= string.Empty;

                var exists = u.PhraseBookItems.Queryable()
                    .Where(x => x.PhraseBookId == phraseBookId)
                    .Select(x => x.PhraseId);

                var list = await u.Phrases.Queryable()
                    .Where(p => p.UserId == uid
                                && !exists.Contains(p.PhraseId)
                                && (((p.Phrase ?? "").Contains(term)) || ((p.Meaning ?? "").Contains(term))))
                    .OrderByDescending(p => (p.CreatedAt ?? DateTime.MinValue))
                    .Select(p => new PhraseSuggestionModel
                    {
                        Id = p.PhraseId,
                        Label = (p.Phrase ?? "") + "  —  " + (p.Meaning ?? "")
                    })
                    .Take(take)
                    .ToListAsync();

                return ServiceResult.Success(list, message: "");
            });
        }

        // ===== 変更系（Create/Update/Delete）=====

        /// <summary>フレーズ帳の新規作成（作成IDを返す）</summary>
        public async Task<ServiceResult<Guid>> CreateAsync(string name)
        {
            try
            {
                var trimmed = (name ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(trimmed))
                    return ServiceResult.Error<Guid>(string.Format(AppMessages.MSG_E_REQUIRED_DETAIL, "名称"));

                var uid = _userService.GetUserId();

                var newId = await _uow.ExecuteInTransactionAsync<Guid>(async u =>
                {
                    var id = Guid.NewGuid();
                    await u.PhraseBooks.AddAsync(new MPhraseBook
                    {
                        PhraseBookId = id,
                        UserId = uid,
                        PhraseBookName = trimmed
                    });
                    return id;
                });

                return ServiceResult.Success(newId, string.Format(AppMessages.MSG_I_SUCCESS_CREATE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "フレーズ帳作成で例外が発生しました。");
                return ServiceResult.Error<Guid>(string.Format(AppMessages.MSG_E_FAILURE_CREATE_DETAIL, MSG_PREFIX));
            }
        }

        /// <summary>フレーズ帳アイテムの追加（複数可／重複は自動除外）</summary>
        public async Task<NoContentResult> CreateAsync(Guid phraseBookId, IEnumerable<Guid> phraseIds)
        {
            try
            {
                var ids = (phraseIds ?? Array.Empty<Guid>()).Distinct().ToList();
                if (ids.Count == 0)
                    return ServiceResultNoContent.Error(string.Format(AppMessages.MSG_E_REQUIRED_DETAIL, "追加対象"));

                await _uow.ExecuteInTransactionAsync(async u =>
                {
                    // 既に入っているIDを除外
                    var exists = await u.PhraseBookItems.Queryable()
                        .Where(x => x.PhraseBookId == phraseBookId)
                        .Select(x => x.PhraseId)
                        .ToListAsync();

                    var toAdd = ids.Where(id => !exists.Contains(id))
                                   .Select(pid => new MPhraseBookItem
                                   {
                                       PhraseBookId = phraseBookId,
                                       PhraseId = pid
                                   })
                                   .ToList();

                    if (toAdd.Count > 0)
                        await u.PhraseBookItems.AddRangeAsync(toAdd);
                });

                return ServiceResultNoContent.Success(string.Format(AppMessages.MSG_I_SUCCESS_UPDATE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "フレーズ帳アイテム追加で例外が発生しました。");
                return ServiceResultNoContent.Error(string.Format(AppMessages.MSG_E_FAILURE_UPDATE_DETAIL, MSG_PREFIX));
            }
        }

        /// <summary>フレーズ帳の名称更新</summary>
        public async Task<NoContentResult> UpdateAsync(Guid phraseBookId, string newName)
        {
            try
            {
                var trimmed = (newName ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(trimmed))
                    return ServiceResultNoContent.Error(string.Format(AppMessages.MSG_E_REQUIRED_DETAIL, "名称"));

                await _uow.ExecuteInTransactionAsync(async u =>
                {
                    var book = await u.PhraseBooks.Queryable()
                        .FirstOrDefaultAsync(b => b.PhraseBookId == phraseBookId);
                    if (book is null)
                        throw new InvalidOperationException(string.Format(AppMessages.MSG_E_NOT_FOUND, MSG_PREFIX));

                    book.PhraseBookName = trimmed;
                    await u.PhraseBooks.UpdateAsync(book);
                });

                return ServiceResultNoContent.Success(string.Format(AppMessages.MSG_I_SUCCESS_UPDATE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "フレーズ帳更新で例外が発生しました。");
                return ServiceResultNoContent.Error(string.Format(AppMessages.MSG_E_FAILURE_UPDATE_DETAIL, MSG_PREFIX));
            }
        }

        /// <summary>フレーズ帳の削除（中身ごと）</summary>
        public async Task<NoContentResult> DeleteAsync(Guid phraseBookId)
        {
            try
            {
                await _uow.ExecuteInTransactionAsync(async u =>
                {
                    var items = await u.PhraseBookItems.Queryable()
                        .Where(x => x.PhraseBookId == phraseBookId)
                        .ToListAsync();
                    if (items.Count > 0)
                        await u.PhraseBookItems.DeleteRangeAsync(items);

                    var book = await u.PhraseBooks.Queryable()
                        .FirstOrDefaultAsync(b => b.PhraseBookId == phraseBookId);
                    if (book is null)
                        throw new InvalidOperationException(string.Format(AppMessages.MSG_E_NOT_FOUND, MSG_PREFIX));

                    await u.PhraseBooks.DeleteAsync(book);
                });

                return ServiceResultNoContent.Success(string.Format(AppMessages.MSG_I_SUCCESS_DELETE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "フレーズ帳削除で例外が発生しました。");
                return ServiceResultNoContent.Error(string.Format(AppMessages.MSG_E_FAILURE_DELETE_DETAIL, MSG_PREFIX));
            }
        }

        /// <summary>フレーズ帳アイテムの削除（複数可）</summary>
        public async Task<NoContentResult> DeleteAsync(Guid phraseBookId, IEnumerable<Guid> phraseIds)
        {
            try
            {
                var ids = (phraseIds ?? Array.Empty<Guid>()).Distinct().ToArray();
                if (ids.Length == 0)
                    return ServiceResultNoContent.Error(string.Format(AppMessages.MSG_E_REQUIRED_DETAIL, "削除対象"));

                await _uow.ExecuteInTransactionAsync(async u =>
                {
                    var targets = await u.PhraseBookItems.Queryable()
                        .Where(x => x.PhraseBookId == phraseBookId && ids.Contains(x.PhraseId))
                        .ToListAsync();

                    if (targets.Count == 0)
                        throw new InvalidOperationException(string.Format(AppMessages.MSG_E_NOT_FOUND, "フレーズ帳アイテム"));

                    await u.PhraseBookItems.DeleteRangeAsync(targets);
                });

                return ServiceResultNoContent.Success(string.Format(AppMessages.MSG_I_SUCCESS_DELETE_DETAIL, "フレーズ"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "フレーズ帳アイテム削除で例外が発生しました。");
                return ServiceResultNoContent.Error(string.Format(AppMessages.MSG_E_FAILURE_DELETE_DETAIL, "フレーズ"));
            }
        }
    }
}
