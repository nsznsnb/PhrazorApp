using Microsoft.EntityFrameworkCore;
using MudBlazor;
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
            return _uow.ReadAsync(async repos =>
            {
                var uid = _userService.GetUserId();

                var list = await repos.PhraseBooks.Queryable()
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
            return _uow.ReadAsync(async repos =>
            {
                // そのブックに紐づくフレーズを作成日時降順で
                var q = from bi in repos.PhraseBookItems.Queryable()
                        where bi.PhraseBookId == phraseBookId
                        join p in repos.Phrases.Queryable() on bi.PhraseId equals p.PhraseId
                        orderby p.CreatedAt descending
                        select p;

                var rows = await q.SelectItems().ToListAsync();
                return ServiceResult.Success(rows, message: "");
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

                var newId = await _uow.ExecuteInTransactionAsync<Guid>(async repos =>
                {
                    var id = Guid.NewGuid();
                    await repos.PhraseBooks.AddAsync(new MPhraseBook
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
        public async Task<ServiceResult<Unit>> CreateAsync(Guid phraseBookId, IEnumerable<Guid> phraseIds)
        {
            try
            {
                var ids = (phraseIds ?? Array.Empty<Guid>()).Distinct().ToList();
                if (ids.Count == 0)
                    return ServiceResult.None.Error(string.Format(AppMessages.MSG_E_REQUIRED_DETAIL, "追加対象"));

                await _uow.ExecuteInTransactionAsync(async repos =>
                {
                    // 既に入っているIDを除外
                    var exists = await repos.PhraseBookItems.Queryable()
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
                        await repos.PhraseBookItems.AddRangeAsync(toAdd);
                });

                return ServiceResult.None.Success("フレーズ帳にフレーズを追加しました。");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "フレーズ帳アイテム追加で例外が発生しました。");
                return ServiceResult.None.Error(string.Format(AppMessages.MSG_E_FAILURE_UPDATE_DETAIL, MSG_PREFIX));
            }
        }

        /// <summary>フレーズ帳の名称更新</summary>
        public async Task<ServiceResult<Unit>> UpdateAsync(Guid phraseBookId, string newName)
        {
            try
            {
                var trimmed = (newName ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(trimmed))
                    return ServiceResult.None.Error(string.Format(AppMessages.MSG_E_REQUIRED_DETAIL, "名称"));

                await _uow.ExecuteInTransactionAsync(async repos =>
                {
                    var book = await repos.PhraseBooks.Queryable()
                        .FirstOrDefaultAsync(b => b.PhraseBookId == phraseBookId);
                    if (book is null)
                        throw new InvalidOperationException(string.Format(AppMessages.MSG_E_NOT_FOUND, MSG_PREFIX));

                    book.PhraseBookName = trimmed;
                    await repos.PhraseBooks.UpdateAsync(book);
                });

                return ServiceResult.None.Success(string.Format(AppMessages.MSG_I_SUCCESS_UPDATE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "フレーズ帳更新で例外が発生しました。");
                return ServiceResult.None.Error(string.Format(AppMessages.MSG_E_FAILURE_UPDATE_DETAIL, MSG_PREFIX));
            }
        }

        /// <summary>フレーズ帳の削除（中身ごと）</summary>
        public async Task<ServiceResult<Unit>> DeleteAsync(Guid phraseBookId)
        {
            try
            {
                await _uow.ExecuteInTransactionAsync(async repos =>
                {
                    var items = await repos.PhraseBookItems.Queryable()
                        .Where(x => x.PhraseBookId == phraseBookId)
                        .ToListAsync();
                    if (items.Count > 0)
                        await repos.PhraseBookItems.DeleteRangeAsync(items);

                    var book = await repos.PhraseBooks.Queryable()
                        .FirstOrDefaultAsync(b => b.PhraseBookId == phraseBookId);
                    if (book is null)
                        throw new InvalidOperationException(string.Format(AppMessages.MSG_E_NOT_FOUND, MSG_PREFIX));

                    await repos.PhraseBooks.DeleteAsync(book);
                });

                return ServiceResult.None.Success(string.Format(AppMessages.MSG_I_SUCCESS_DELETE_DETAIL, "フレーズ帳からフレーズ"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "フレーズ帳削除で例外が発生しました。");
                return ServiceResult.None.Error(string.Format(AppMessages.MSG_E_FAILURE_DELETE_DETAIL, MSG_PREFIX));
            }
        }

        /// <summary>フレーズ帳アイテムの削除（複数可）</summary>
        public async Task<ServiceResult<Unit>> DeleteAsync(Guid phraseBookId, IEnumerable<Guid> phraseIds)
        {
            try
            {
                var ids = (phraseIds ?? Array.Empty<Guid>()).Distinct().ToArray();
                if (ids.Length == 0)
                    return ServiceResult.None.Error(string.Format(AppMessages.MSG_E_REQUIRED_DETAIL, "削除対象"));

                await _uow.ExecuteInTransactionAsync(async repos =>
                {
                    var targets = await repos.PhraseBookItems.Queryable()
                        .Where(x => x.PhraseBookId == phraseBookId && ids.Contains(x.PhraseId))
                        .ToListAsync();

                    if (targets.Count == 0)
                        throw new InvalidOperationException(string.Format(AppMessages.MSG_E_NOT_FOUND, "フレーズ帳アイテム"));

                    await repos.PhraseBookItems.DeleteRangeAsync(targets);
                });

                return ServiceResult.None.Success(string.Format(AppMessages.MSG_I_SUCCESS_DELETE_DETAIL, "フレーズ帳からフレーズ"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "フレーズ帳アイテム削除で例外が発生しました。");
                return ServiceResult.None.Error(string.Format(AppMessages.MSG_E_FAILURE_DELETE_DETAIL, "フレーズ"));
            }
        }
    }
}
