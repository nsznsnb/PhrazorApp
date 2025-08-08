using PhrazorApp.Commons;
using PhrazorApp.Data.UnitOfWork;
using PhrazorApp.Models;
using PhrazorApp.Models.Mappings;

namespace PhrazorApp.Services
{
    public class PhraseService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserService _userService;
        private readonly ILogger<PhraseService> _logger;
        private const string MSG_PREFIX = "フレーズ";

        public PhraseService(IUnitOfWork uow, UserService userService, ILogger<PhraseService> logger)
        {
            _uow = uow;
            _userService = userService;
            _logger = logger;
        }

        public async Task<List<PhraseModel>> GetPhraseViewModelListAsync(CancellationToken ct = default)
        {
            var userId = _userService.GetUserId();
            await _uow.BeginAsync(ct);
            var phrases = await _uow.Phrases.GetAllPhrasesAsync(userId, ct);
            return phrases.Select(p => p.ToModel()).ToList();
        }

        public async Task<PhraseModel> GetPhraseViewModelAsync(Guid? phraseId, CancellationToken ct = default)
        {
            var userId = _userService.GetUserId();
            await _uow.BeginAsync(ct);
            var phrase = await _uow.Phrases.GetPhraseByIdAsync(phraseId, userId, ct);
            if (phraseId == null || phrase == null)
            {
                return new PhraseModel
                {
                    Id = Guid.NewGuid(),
                    Phrase = string.Empty,
                    Meaning = string.Empty,
                    Note = string.Empty,
                    ImageUrl = string.Empty,
                };
            }
            return new PhraseModel
            {
                Id = phrase.PhraseId,
                Phrase = phrase.Phrase ?? string.Empty,
                Meaning = phrase.Meaning ?? string.Empty,
                Note = phrase.Note ?? string.Empty,
                ImageUrl = phrase.DPhraseImage?.Url ?? string.Empty,
                SelectedDropItems = phrase.MPhraseGenres.ToDropItemModels()
            };
        }

        public async Task<IServiceResult> CreatePhraseAsync(PhraseModel model, CancellationToken ct = default)
        {
            await _uow.BeginAsync(ct);
            try
            {
                await _uow.Phrases.AddAsync(model.ToEntity());
                if (!string.IsNullOrEmpty(model.ImageUrl))
                    await _uow.PhraseImages.AddAsync(model.ToImageEntity(DateTime.UtcNow));
                if (model.SelectedDropItems?.Count > 0)
                    await _uow.PhraseGenres.AddRangeAsync(model.ToPhraseGenreEntities());

                await _uow.CommitAsync(ct);
                return ServiceResult.Success(string.Format(ComMessage.MSG_I_SUCCESS_CREATE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                _logger.LogError(ex, "フレーズ登録で例外が発生しました。");
                return ServiceResult.Failure(string.Format(ComMessage.MSG_E_FAILURE_CREATE_DETAIL, MSG_PREFIX));
            }
        }

        /// <summary>
        /// フレーズ情報を更新します。
        /// </summary>
        public async Task<IServiceResult> UpdatePhraseAsync(PhraseModel model, CancellationToken ct = default)
        {
            await _uow.BeginAsync(ct);
            try
            {
                var userId = _userService.GetUserId();

                var phraseEntity = await _uow.Phrases.GetPhraseByIdAsync(model.Id, userId, ct);
                if (phraseEntity == null)
                    return ServiceResult.Failure(string.Format(ComMessage.MSG_E_NOT_FOUND, MSG_PREFIX));

                // 本体更新
                phraseEntity.Phrase = model.Phrase;
                phraseEntity.Meaning = model.Meaning;
                phraseEntity.Note = model.Note;
                phraseEntity.UpdatedAt = DateTime.UtcNow;
                await _uow.Phrases.UpdateAsync(phraseEntity);

                // 画像（必要なら）
                if (!string.IsNullOrEmpty(model.ImageUrl))
                {
                    var imageEntity = model.ToImageEntity(DateTime.UtcNow);
                    await _uow.PhraseImages.UpdateAsync(imageEntity);
                }

                // ジャンル差し替え（全削除→再追加）
                if (phraseEntity.MPhraseGenres is { Count: > 0 })
                    await _uow.PhraseGenres.DeleteRangeAsync(phraseEntity.MPhraseGenres);

                if (model.SelectedDropItems is { Count: > 0 })
                    await _uow.PhraseGenres.AddRangeAsync(model.ToPhraseGenreEntities());

                await _uow.CommitAsync(ct);
                return ServiceResult.Success(string.Format(ComMessage.MSG_I_SUCCESS_UPDATE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                _logger.LogError(ex, "フレーズ更新で例外が発生しました。");
                return ServiceResult.Failure(string.Format(ComMessage.MSG_E_FAILURE_UPDATE_DETAIL, MSG_PREFIX));
            }
        }

        /// <summary>
        /// フレーズを削除します。
        /// </summary>
        public async Task<IServiceResult> DeletePhraseAsync(Guid phraseId, CancellationToken ct = default)
        {
            await _uow.BeginAsync(ct);
            try
            {
                var userId = _userService.GetUserId();

                var phrase = await _uow.Phrases.GetPhraseByIdAsync(phraseId, userId, ct);
                if (phrase == null)
                    return ServiceResult.Failure(string.Format(ComMessage.MSG_E_NOT_FOUND, MSG_PREFIX));

                // 画像・ジャンル連鎖削除
                if (phrase.DPhraseImage is not null)
                    await _uow.PhraseImages.DeleteAsync(phrase.DPhraseImage);

                if (phrase.MPhraseGenres is { Count: > 0 })
                    await _uow.PhraseGenres.DeleteRangeAsync(phrase.MPhraseGenres);

                await _uow.Phrases.DeleteAsync(phrase);

                await _uow.CommitAsync(ct);
                return ServiceResult.Success(string.Format(ComMessage.MSG_I_SUCCESS_DELETE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                _logger.LogError(ex, "フレーズ削除で例外が発生しました。");
                return ServiceResult.Failure(string.Format(ComMessage.MSG_E_FAILURE_DELETE_DETAIL, MSG_PREFIX));
            }
        }
    }
}
