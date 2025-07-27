using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PhrazorApp.Data;
using PhrazorApp.Data.Repositories;
using PhrazorApp.Models.Mappings;
using PhrazorApp.Models.ViewModels;

namespace PhrazorApp.Services
{
    public interface IPhraseService
    {
        Task<List<PhraseModel>> GetPhraseViewModelListAsync();
        Task<PhraseModel> GetPhraseViewModelAsync(Guid phraseId);
        Task<IServiceResult> CreatePhraseAsync(PhraseModel model);
        Task<IServiceResult> UpdatePhraseAsync(PhraseModel model);
        Task<IServiceResult> DeletePhraseAsync(Guid phraseId);
    }
    public class PhraseService : IPhraseService
    {
        private readonly IPhraseRepository _phraseRepository;
        private readonly ICategoryRepository _categoryRepository;  // CategoryRepositoryを追加
        private readonly IUserService _userService;
        private readonly ILogger<PhraseService> _logger;

        public PhraseService(IPhraseRepository phraseRepository, ICategoryRepository categoryRepository, IUserService userService, ILogger<PhraseService> logger)
        {
            _phraseRepository = phraseRepository;
            _categoryRepository = categoryRepository;  // 依存性注入
            _userService = userService;
            _logger = logger;
        }


        /// <summary>
        /// すべてのフレーズ情報を取得します。
        /// </summary>
        public async Task<List<PhraseModel>> GetPhraseViewModelListAsync()
        {
            // フレーズ情報をリポジトリから取得
            var phrases = await _phraseRepository.GetAllPhrasesAsync();
            // モデルに変換して返却
            return phrases.Select(p => p.ToPhraseModel()).ToList();
        }

        /// <summary>
        /// 指定されたフレーズIDに基づいてフレーズ情報を取得します。
        /// </summary>
        public async Task<PhraseModel> GetPhraseViewModelAsync(Guid phraseId)
        {
            var phrase = await _phraseRepository.GetPhraseByIdAsync(phraseId);
            var userId = _userService.GetUserId();
            var largeCategories = await _categoryRepository.GetAllCategoriesAsync(userId);

            var dropItems = largeCategories.ToDropItemModelList(phrase?.MPhraseCategories.ToList());

            // フレーズ情報とカテゴリを結びつけてモデルに変換して返却
            return new PhraseModel
            {
                Id = phrase?.PhraseId ?? phraseId,
                Phrase = phrase?.Phrase ?? string.Empty,
                Meaning = phrase?.Meaning ?? string.Empty,
                Note = phrase?.Note ?? string.Empty,
                ImageUrl = phrase?.DPhraseImage?.Url ?? string.Empty,
                AllCategoryItems = dropItems,
                SelectedItems = dropItems.Where(d => d.DropTarget == DropItemType.Target).ToList()
            };
        }

        /// <summary>
        /// 新しいフレーズを作成します。
        /// </summary>
        public async Task<IServiceResult> CreatePhraseAsync(PhraseModel model)
        {
            try
            {
                // モデルをエンティティに変換
                var entity = model.ToPhraseEntity(_userService.GetUserId(), DateTime.UtcNow);
                await _phraseRepository.AddPhraseAsync(entity);

                // 画像があれば画像も保存
                if (!string.IsNullOrEmpty(model.ImageUrl))
                {
                    var imageEntity = model.ToImageEntity(DateTime.UtcNow);
                    await _phraseRepository.AddPhraseImageAsync(imageEntity);
                }

                // カテゴリを保存
                var phraseCategories = model.AllCategoryItems.ToPhraseCategoryEntities(model.Id, DateTime.UtcNow);
                await _phraseRepository.AddPhraseCategoryAsync(phraseCategories);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "フレーズ登録で例外が発生しました。");
                return ServiceResult.Failure("フレーズの登録に失敗しました。");
            }
        }

        /// <summary>
        /// フレーズ情報を更新します。
        /// </summary>
        public async Task<IServiceResult> UpdatePhraseAsync(PhraseModel model)
        {
            try
            {
                // フレーズを取得
                var phraseEntity = await _phraseRepository.GetPhraseByIdAsync(model.Id);
                if (phraseEntity == null)
                    return ServiceResult.Failure("指定されたフレーズは存在しません。");

                // フレーズ情報を更新
                phraseEntity.Phrase = model.Phrase;
                phraseEntity.Meaning = model.Meaning;
                phraseEntity.Note = model.Note;
                phraseEntity.UpdatedAt = DateTime.UtcNow;

                // 画像があれば画像も更新
                if (!string.IsNullOrEmpty(model.ImageUrl))
                {
                    var imageEntity = model.ToImageEntity(DateTime.UtcNow);
                    await _phraseRepository.UpdatePhraseImageAsync(imageEntity);
                }

                // カテゴリを削除して新しいカテゴリを追加
                await _phraseRepository.DeletePhraseCategoriesAsync(phraseEntity.MPhraseCategories);
                var newCategories = model.AllCategoryItems.ToPhraseCategoryEntities(model.Id, DateTime.UtcNow);
                await _phraseRepository.AddPhraseCategoryAsync(newCategories);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "フレーズ更新で例外が発生しました。");
                return ServiceResult.Failure("フレーズの更新に失敗しました。");
            }
        }

        /// <summary>
        /// フレーズを削除します。
        /// </summary>
        public async Task<IServiceResult> DeletePhraseAsync(Guid phraseId)
        {
            try
            {
                var phrase = await _phraseRepository.GetPhraseByIdAsync(phraseId);
                if (phrase == null)
                    return ServiceResult.Failure("指定されたフレーズは存在しません。");

                // フレーズと関連するカテゴリを削除
                await _phraseRepository.DeletePhraseCategoriesAsync(phrase.MPhraseCategories);
                await _phraseRepository.DeletePhraseAsync(phrase);

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "フレーズ削除で例外が発生しました。");
                return ServiceResult.Failure("フレーズの削除に失敗しました。");
            }
        }
    }
}
