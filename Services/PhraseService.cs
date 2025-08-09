using PhrazorApp.Commons;
using PhrazorApp.Data.Entities;
using PhrazorApp.Data.UnitOfWork;
using PhrazorApp.Models;
using PhrazorApp.Models.Mappings;

namespace PhrazorApp.Services
{
    public class PhraseService
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

        public Task<List<PhraseModel>> GetPhraseViewModelListAsync()
        {
            var userId = _userService.GetUserId();
            return _uow.ReadAsync(async u =>
            {
                var phrases = await u.Phrases.GetAllPhrasesAsync(userId);
                return phrases.Select(p => p.ToModel()).ToList();
            });
        }

        public Task<PhraseModel> GetPhraseViewModelAsync(Guid? phraseId)
        {
            var userId = _userService.GetUserId();
            return _uow.ReadAsync(async u =>
            {
                var phrase = await u.Phrases.GetPhraseByIdAsync(phraseId, userId);
                if (phraseId == null || phrase == null)
                    return new PhraseModel { Id = Guid.NewGuid(), Phrase = "", Meaning = "", Note = "", ImageUrl = "" };

                return new PhraseModel
                {
                    Id = phrase.PhraseId,
                    Phrase = phrase.Phrase ?? "",
                    Meaning = phrase.Meaning ?? "",
                    Note = phrase.Note ?? "",
                    ImageUrl = phrase.DPhraseImage?.Url ?? "",
                    SelectedDropItems = phrase.MPhraseGenres.ToDropItemModels()
                };
            });
        }

        public async Task<IServiceResult> CreatePhraseAsync(PhraseModel model)
        {
            try
            {
                await _uow.ExecuteInTransactionAsync(async u =>
                {
                    await u.Phrases.AddAsync(model.ToEntity());

                    if (!string.IsNullOrEmpty(model.ImageUrl))
                        await u.PhraseImages.AddAsync(model.ToImageEntity(DateTime.UtcNow));

                    if (model.SelectedDropItems?.Count > 0)
                        await u.PhraseGenres.AddRangeAsync(model.ToPhraseGenreEntities());
                });

                return ServiceResult.Success(string.Format(ComMessage.MSG_I_SUCCESS_CREATE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "フレーズ登録で例外が発生しました。");
                return ServiceResult.Failure(string.Format(ComMessage.MSG_E_FAILURE_CREATE_DETAIL, MSG_PREFIX));
            }
        }

        public async Task<IServiceResult> CreatePhrasesAsync(IEnumerable<PhraseModel> models)
        {
            if (models == null) return ServiceResult.Failure("入力が null です。");

            var list = models.Where(m => m != null).ToList();
            if (list.Count == 0) return ServiceResult.Failure("登録対象がありません。");

            try
            {
                await _uow.ExecuteInTransactionAsync(async u =>
                {
                    var nowUtc = DateTime.UtcNow;

                    var phraseEntities = new List<DPhrase>(list.Count);
                    var imageEntities = new List<DPhraseImage>();
                    var phraseGenreEntities = new List<MPhraseGenre>();

                    foreach (var model in list)
                    {
                        var phrase = model.ToEntity();
                        phraseEntities.Add(phrase);

                        if (!string.IsNullOrWhiteSpace(model.ImageUrl))
                            imageEntities.Add(model.ToImageEntity(nowUtc));

                        if (model.SelectedDropItems?.Count > 0)
                            phraseGenreEntities.AddRange(model.ToPhraseGenreEntities());
                    }

                    await u.Phrases.AddRangeAsync(phraseEntities);
                    if (imageEntities.Count > 0) await u.PhraseImages.AddRangeAsync(imageEntities);
                    if (phraseGenreEntities.Count > 0) await u.PhraseGenres.AddRangeAsync(phraseGenreEntities);
                });

                return ServiceResult.Success(string.Format(ComMessage.MSG_I_SUCCESS_CREATE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "フレーズ一括登録で例外が発生しました。");
                return ServiceResult.Failure(string.Format(ComMessage.MSG_E_FAILURE_CREATE_DETAIL, MSG_PREFIX));
            }
        }

        /// <summary>フレーズ情報を更新します。</summary>
        public async Task<IServiceResult> UpdatePhraseAsync(PhraseModel model)
        {
            try
            {
                var userId = _userService.GetUserId();

                await _uow.ExecuteInTransactionAsync(async u =>
                {
                    var phraseEntity = await u.Phrases.GetPhraseByIdAsync(model.Id, userId);
                    if (phraseEntity == null)
                        throw new InvalidOperationException(string.Format(ComMessage.MSG_E_NOT_FOUND, MSG_PREFIX));

                    // 本体更新
                    phraseEntity.Phrase = model.Phrase;
                    phraseEntity.Meaning = model.Meaning;
                    phraseEntity.Note = model.Note;
                    phraseEntity.UpdatedAt = DateTime.UtcNow;
                    await u.Phrases.UpdateAsync(phraseEntity);

                    // 画像 Upsert
                    if (!string.IsNullOrEmpty(model.ImageUrl))
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

                    // ジャンル差し替え（全削除→再追加）
                    if (phraseEntity.MPhraseGenres is { Count: > 0 })
                        await u.PhraseGenres.DeleteRangeAsync(phraseEntity.MPhraseGenres);

                    if (model.SelectedDropItems is { Count: > 0 })
                        await u.PhraseGenres.AddRangeAsync(model.ToPhraseGenreEntities());
                });

                return ServiceResult.Success(string.Format(ComMessage.MSG_I_SUCCESS_UPDATE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "フレーズ更新で例外が発生しました。");
                return ServiceResult.Failure(string.Format(ComMessage.MSG_E_FAILURE_UPDATE_DETAIL, MSG_PREFIX));
            }
        }

        /// <summary>フレーズを削除します。</summary>
        public async Task<IServiceResult> DeletePhraseAsync(Guid phraseId)
        {
            try
            {
                var userId = _userService.GetUserId();

                await _uow.ExecuteInTransactionAsync(async u =>
                {
                    var phrase = await u.Phrases.GetPhraseByIdAsync(phraseId, userId);
                    if (phrase == null)
                        throw new InvalidOperationException(string.Format(ComMessage.MSG_E_NOT_FOUND, MSG_PREFIX));

                    if (phrase.DPhraseImage is not null)
                        await u.PhraseImages.DeleteAsync(phrase.DPhraseImage);

                    if (phrase.MPhraseGenres is { Count: > 0 })
                        await u.PhraseGenres.DeleteRangeAsync(phrase.MPhraseGenres);

                    await u.Phrases.DeleteAsync(phrase);
                });

                return ServiceResult.Success(string.Format(ComMessage.MSG_I_SUCCESS_DELETE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "フレーズ削除で例外が発生しました。");
                return ServiceResult.Failure(string.Format(ComMessage.MSG_E_FAILURE_DELETE_DETAIL, MSG_PREFIX));
            }
        }
    }
}
