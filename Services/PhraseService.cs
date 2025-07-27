using PhrazorApp.Common;
using PhrazorApp.Data.Repositories;
using PhrazorApp.Models.Mappings;
using PhrazorApp.Models.ViewModels;

namespace PhrazorApp.Services
{
	/// <summary>
	/// フレーズサービスインターフェース
	/// </summary>
	public interface IPhraseService
	{
		Task<List<PhraseModel>> GetPhraseViewModelListAsync();
		Task<PhraseModel> GetPhraseViewModelAsync(Guid phraseId);
		Task<IServiceResult> CreatePhraseAsync(PhraseModel model);
		Task<IServiceResult> UpdatePhraseAsync(PhraseModel model);
		Task<IServiceResult> DeletePhraseAsync(Guid phraseId);
	}

	/// <summary>
	/// フレーズサービス
	/// </summary>
	public class PhraseService : IPhraseService
	{
		private readonly IPhraseRepository _phraseRepository;
		private readonly IGenreRepository _genreRepository;
		private readonly IUserService _userService;
		private readonly ILogger<PhraseService> _logger;

		public PhraseService(IPhraseRepository phraseRepository, IGenreRepository genreRepository, IUserService userService, ILogger<PhraseService> logger)
		{
			_phraseRepository = phraseRepository;
			_genreRepository = genreRepository;
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
			var genres = await _genreRepository.GetAllGenresAsync(userId);

			var dropItems = genres.ToDropItemModelList(phrase?.MPhraseGenres.ToList());

			// フレーズ情報とジャンルを結びつけてモデルに変換して返却
			return new PhraseModel
			{
				Id = phrase?.PhraseId ?? phraseId,
				Phrase = phrase?.Phrase ?? string.Empty,
				Meaning = phrase?.Meaning ?? string.Empty,
				Note = phrase?.Note ?? string.Empty,
				ImageUrl = phrase?.DPhraseImage?.Url ?? string.Empty,
				AllGenreItems = dropItems,
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

				// ジャンルを保存
				var phraseGenres = model.AllGenreItems.ToPhraseGenreEntities(model.Id, DateTime.UtcNow);
				await _phraseRepository.AddPhraseGenreAsync(phraseGenres);

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

				// ジャンルを削除して新しいジャンルを追加
				await _phraseRepository.DeletePhraseGenresAsync(phraseEntity.MPhraseGenres);
				var newGenres = model.AllGenreItems.ToPhraseGenreEntities(model.Id, DateTime.UtcNow);
				await _phraseRepository.AddPhraseGenreAsync(newGenres);

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

				// フレーズと関連するジャンルを削除
				await _phraseRepository.DeletePhraseGenresAsync(phrase.MPhraseGenres);
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
