using PhrazorApp.Data.Entities;
using PhrazorApp.Data.UnitOfWork;
using PhrazorApp.Models;
using PhrazorApp.Models.Mappings;

namespace PhrazorApp.Services
{
    /// <summary>
    /// フレーズのユースケース。
    /// </summary>
    public sealed class PhraseService
    {
        private readonly UnitOfWork _uow;
        private readonly UserService _userService;
        private readonly ILogger<PhraseService> _logger;
        private const string MSG_PREFIX = "フレーズ";

        public PhraseService(UnitOfWork uow, UserService userService, ILogger<PhraseService> logger)
        {
            _uow = uow;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>一覧</summary>
        public Task<ServiceResult<List<PhraseModel>>> GetPhraseViewModelListAsync(CancellationToken ct = default)
        {

            return _uow.ReadAsync(async (u, token) =>
            {
                var phrases = await u.Phrases.GetAllPhrasesAsync(token);
                var list = phrases.Select(p => p.ToModel()).ToList();
                return ServiceResult<List<PhraseModel>>.Success(list, message: "");
            }, ct);
        }

        /// <summary>詳細</summary>
        public Task<ServiceResult<PhraseModel>> GetPhraseViewModelAsync(Guid? phraseId, CancellationToken ct = default)
        {

            return _uow.ReadAsync(async (u, token) =>
            {
                var phrase = await u.Phrases.GetPhraseByIdAsync(phraseId, token);
                var model = (phraseId == null || phrase == null)
                    ? new PhraseModel { Id = Guid.NewGuid(), Phrase = "", Meaning = "", Note = "", ImageUrl = "" }
                    : new PhraseModel
                    {
                        Id = phrase.PhraseId,
                        Phrase = phrase.Phrase ?? "",
                        Meaning = phrase.Meaning ?? "",
                        Note = phrase.Note ?? "",
                        ImageUrl = phrase.DPhraseImage?.Url ?? "",
                        SelectedDropItems = phrase.MPhraseGenres.ToDropItemModels()
                    };

                return ServiceResult<PhraseModel>.Success(model, message: "");
            }, ct);
        }

        /// <summary>作成</summary>
        public async Task<ServiceResult> CreatePhraseAsync(PhraseModel model, CancellationToken ct = default)
        {
            try
            {
                await _uow.ExecuteInTransactionAsync(async (u, token) =>
                {
                    var userId = _userService.GetUserId();

                    await u.Phrases.AddAsync(model.ToEntity(userId));

                    if (!string.IsNullOrWhiteSpace(model.ImageUrl))
                        await u.PhraseImages.AddAsync(model.ToImageEntity(DateTime.UtcNow));

                    if (model.SelectedDropItems?.Count > 0)
                        await u.PhraseGenres.AddRangeAsync(model.ToPhraseGenreEntities());
                }, ct);

                return ServiceResult.Success(string.Format(AppMessages.MSG_I_SUCCESS_CREATE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "フレーズ登録で例外が発生しました。");
                return ServiceResult.Failure(string.Format(AppMessages.MSG_E_FAILURE_CREATE_DETAIL, MSG_PREFIX));
            }
        }

        /// <summary>一括作成</summary>
        public async Task<ServiceResult> CreatePhrasesAsync(IEnumerable<PhraseModel> models, CancellationToken ct = default)
        {
            if (models == null) return ServiceResult.Failure("入力が null です。");

            var list = models.Where(m => m != null).ToList();
            if (list.Count == 0) return ServiceResult.Failure("登録対象がありません。");

            var userId = _userService.GetUserId();

            try
            {
                await _uow.ExecuteInTransactionAsync(async (u, token) =>
                {
                    var nowUtc = DateTime.UtcNow;

                    var phraseEntities = new List<DPhrase>(list.Count);
                    var imageEntities = new List<DPhraseImage>();
                    var phraseGenreEntities = new List<MPhraseGenre>();

                    foreach (var model in list)
                    {
                        // ここで必ず新規発番（画面の Id は無視）
                        model.Id = Guid.NewGuid();

                        var phrase = model.ToEntity(userId);
                        phraseEntities.Add(phrase);

                        if (!string.IsNullOrWhiteSpace(model.ImageUrl))
                            imageEntities.Add(model.ToImageEntity(nowUtc));

                        if (model.SelectedDropItems?.Count > 0)
                            phraseGenreEntities.AddRange(model.ToPhraseGenreEntities());
                    }

                    await u.Phrases.AddRangeAsync(phraseEntities);
                    if (imageEntities.Count > 0) await u.PhraseImages.AddRangeAsync(imageEntities);
                    if (phraseGenreEntities.Count > 0) await u.PhraseGenres.AddRangeAsync(phraseGenreEntities);
                }, ct);

                return ServiceResult.Success(string.Format(AppMessages.MSG_I_SUCCESS_CREATE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "フレーズ一括登録で例外が発生しました。");
                return ServiceResult.Failure(string.Format(AppMessages.MSG_E_FAILURE_CREATE_DETAIL, MSG_PREFIX));
            }
        }

        /// <summary>更新（画像はUpsert、ジャンルは全差し替え）</summary>
        public async Task<ServiceResult> UpdatePhraseAsync(PhraseModel model, CancellationToken ct = default)
        {
            try
            {
                var userId = _userService.GetUserId();

                await _uow.ExecuteInTransactionAsync(async (u, token) =>
                {
                    var phraseEntity = await u.Phrases.GetPhraseByIdAsync(model.Id, token);
                    if (phraseEntity == null)
                        throw new InvalidOperationException(string.Format(AppMessages.MSG_E_NOT_FOUND, MSG_PREFIX));

                    // 本体更新（AutoDetectChanges OFF 前提なので Update 明示）
                    phraseEntity.Phrase = model.Phrase;
                    phraseEntity.Meaning = model.Meaning;
                    phraseEntity.Note = model.Note;
                    phraseEntity.UpdatedAt = DateTime.UtcNow;
                    await u.Phrases.UpdateAsync(phraseEntity);

                    // 画像 Upsert
                    if (!string.IsNullOrWhiteSpace(model.ImageUrl))
                    {
                        if (phraseEntity.DPhraseImage is null)
                        {
                            await u.PhraseImages.AddAsync(model.ToImageEntity(DateTime.UtcNow));
                        }
                        else
                        {
                            phraseEntity.DPhraseImage.Url = model.ImageUrl;
                            phraseEntity.DPhraseImage.UpdatedAt = DateTime.UtcNow;
                            await u.PhraseImages.UpdateAsync(phraseEntity.DPhraseImage);
                        }
                    }
                    else
                    {
                        if (phraseEntity.DPhraseImage is not null)
                            await u.PhraseImages.DeleteAsync(phraseEntity.DPhraseImage);
                    }

                    // ジャンル全差し替え（子→親の順で安全）
                    if (phraseEntity.MPhraseGenres is { Count: > 0 })
                        await u.PhraseGenres.DeleteRangeAsync(phraseEntity.MPhraseGenres);

                    if (model.SelectedDropItems is { Count: > 0 })
                        await u.PhraseGenres.AddRangeAsync(model.ToPhraseGenreEntities());
                }, ct);

                return ServiceResult.Success(string.Format(AppMessages.MSG_I_SUCCESS_UPDATE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "フレーズ更新で例外が発生しました。");
                return ServiceResult.Failure(string.Format(AppMessages.MSG_E_FAILURE_UPDATE_DETAIL, MSG_PREFIX));
            }
        }

        /// <summary>削除</summary>
        public async Task<ServiceResult> DeletePhraseAsync(Guid phraseId, CancellationToken ct = default)
        {
            try
            {
                var userId = _userService.GetUserId();

                await _uow.ExecuteInTransactionAsync(async (u, token) =>
                {
                    var phrase = await u.Phrases.GetPhraseByIdAsync(phraseId, token);
                    if (phrase == null)
                        throw new InvalidOperationException(string.Format(AppMessages.MSG_E_NOT_FOUND, MSG_PREFIX));

                    if (phrase.DPhraseImage is not null)
                        await u.PhraseImages.DeleteAsync(phrase.DPhraseImage);

                    if (phrase.MPhraseGenres is { Count: > 0 })
                        await u.PhraseGenres.DeleteRangeAsync(phrase.MPhraseGenres);

                    await u.Phrases.DeleteAsync(phrase);
                }, ct);

                return ServiceResult.Success(string.Format(AppMessages.MSG_I_SUCCESS_DELETE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "フレーズ削除で例外が発生しました。");
                return ServiceResult.Failure(string.Format(AppMessages.MSG_E_FAILURE_DELETE_DETAIL, MSG_PREFIX));
            }
        }

        /// <summary>一括削除</summary>
        public async Task<ServiceResult> DeletePhrasesAsync(IEnumerable<Guid> phraseIds, CancellationToken ct = default)
        {
            try
            {
                if (phraseIds is null || !phraseIds.Any())
                    return ServiceResult.Failure(string.Format(AppMessages.MSG_E_REQUIRED_DETAIL, "削除対象"));

                var userId = _userService.GetUserId();
                var idSet = phraseIds.Distinct().ToArray(); // Guid 重複除去

                await _uow.ExecuteInTransactionAsync(async (u, token) =>
                {
                    const int chunkSize = 500; // SQL パラメータ上限対策
                    var allPhrases = new List<DPhrase>(capacity: idSet.Length);

                    foreach (var chunk in idSet.Chunk(chunkSize))
                    {
                        var phrases = await u.Phrases.GetByPhrasesIdsAsync(chunk, token); // 画像・ジャンル込み取得
                        allPhrases.AddRange(phrases);
                    }

                    // 存在チェック（1件でも不足したらエラー）
                    if (allPhrases.Count != idSet.Length)
                        throw new InvalidOperationException(string.Format(AppMessages.MSG_E_NOT_FOUND, MSG_PREFIX));

                    // 子→親の順に削除
                    var genres = allPhrases.Where(p => p.MPhraseGenres is { Count: > 0 })
                                           .SelectMany(p => p.MPhraseGenres)
                                           .ToList();

                    var images = allPhrases.Where(p => p.DPhraseImage != null)
                                           .Select(p => p.DPhraseImage!)
                                           .ToList();

                    if (genres.Count > 0)
                        await u.PhraseGenres.DeleteRangeAsync(genres);

                    if (images.Count > 0)
                        await u.PhraseImages.DeleteRangeAsync(images);

                    await u.Phrases.DeleteRangeAsync(allPhrases);
                }, ct);

                return ServiceResult.Success(string.Format(AppMessages.MSG_I_SUCCESS_DELETE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "フレーズ一括削除で例外が発生しました。");
                return ServiceResult.Failure(string.Format(AppMessages.MSG_E_FAILURE_DELETE_DETAIL, MSG_PREFIX));
            }
        }
    }
}
