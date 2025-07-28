using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using PhrazorApp.Commons;
using PhrazorApp.Data;
using PhrazorApp.Data.Repositories;
using PhrazorApp.Models;

namespace PhrazorApp.Services
{
	/// <summary>
	/// フレーズサービスインターフェース
	/// </summary>
	public interface IPhraseService
	{
		Task<List<PhraseModel>> GetPhraseViewModelListAsync();
		Task<PhraseModel> GetPhraseViewModelAsync(Guid? phraseId);
		Task<IServiceResult> CreatePhraseAsync(PhraseModel model);
		Task<IServiceResult> UpdatePhraseAsync(PhraseModel model);
		Task<IServiceResult> DeletePhraseAsync(Guid phraseId);
	}

	/// <summary>
	/// フレーズサービス
	/// </summary>
	public class PhraseService : IPhraseService
	{
		private readonly IDbContextFactory<EngDbContext> _dbContextFactory;
		private readonly IPhraseRepository _phraseRepository;
		private readonly IGenreRepository _genreRepository;
		private readonly ILogger<PhraseService> _logger;
        private readonly string MSG_PREFIX = "フレーズ";

		public PhraseService(IDbContextFactory<EngDbContext> dbContextFactory, IPhraseRepository phraseRepository, IGenreRepository genreRepository, ILogger<PhraseService> logger)
        {
            _dbContextFactory = dbContextFactory;
            _phraseRepository = phraseRepository;
            _genreRepository = genreRepository;
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
		public async Task<PhraseModel> GetPhraseViewModelAsync(Guid? phraseId)
		{
            var phrase = await _phraseRepository.GetPhraseByIdAsync(phraseId);


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


			// フレーズ情報とジャンルを結びつけてモデルに変換して返却
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

		/// <summary>
		/// 新しいフレーズを作成します。
		/// </summary>
		public async Task<IServiceResult> CreatePhraseAsync(PhraseModel model)
		{

            try
            {
                await using var context = await _dbContextFactory.CreateDbContextAsync();

                // モデルをエンティティに変換
                var entity = model.ToPhraseEntity();
				_phraseRepository.CreatePhrase(context, entity);

				// 画像があれば画像も保存
				if (!string.IsNullOrEmpty(model.ImageUrl))
				{
					var imageEntity = model.ToImageEntity(DateTime.UtcNow);
					_phraseRepository.CreatePhraseImage(context, imageEntity);
				}

				// ジャンルを保存
				if (model.SelectedDropItems != null && model.SelectedDropItems.Count > 0)
				{
					var phraseGenres = model.ToPhraseGenreEntities();
                    _phraseRepository.CreatePhraseGenreRange(context, phraseGenres);

                }

				context.SaveChanges();

                return ServiceResult.Success(string.Format(ComMessage.MSG_I_SUCCESS_CREATE_DETAIL, MSG_PREFIX));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "フレーズ登録で例外が発生しました。");
				return ServiceResult.Failure(string.Format(ComMessage.MSG_E_FAILURE_CREATE_DETAIL, MSG_PREFIX));
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
					return ServiceResult.Failure(string.Format(ComMessage.MSG_E_NOT_FOUND, MSG_PREFIX));

				// フレーズ情報を更新
				phraseEntity.Phrase = model.Phrase;
				phraseEntity.Meaning = model.Meaning;
				phraseEntity.Note = model.Note;
				phraseEntity.UpdatedAt = DateTime.UtcNow;

                await using var context = await _dbContextFactory.CreateDbContextAsync();

                // 画像があれば画像も更新
                if (!string.IsNullOrEmpty(model.ImageUrl))
				{
					var imageEntity = model.ToImageEntity(DateTime.UtcNow);
					_phraseRepository.UpdatePhraseImage(context, imageEntity);
				}


				// ジャンルを削除して新しいジャンルを追加
				_phraseRepository.DeletePhraseGenreRange(context, phraseEntity.MPhraseGenres);
				if (model.SelectedDropItems != null && model.SelectedDropItems.Count > 0)
				{
					var newGenres = model.ToPhraseGenreEntities();
                    _phraseRepository.CreatePhraseGenreRange(context, newGenres);

                }

                return ServiceResult.Success(string.Format(ComMessage.MSG_I_SUCCESS_UPDATE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
			{
				_logger.LogError(ex, "フレーズ更新で例外が発生しました。");
                return ServiceResult.Failure(string.Format(ComMessage.MSG_E_FAILURE_UPDATE_DETAIL, MSG_PREFIX));
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
                    return ServiceResult.Failure(string.Format(ComMessage.MSG_E_NOT_FOUND, MSG_PREFIX));

                await using var context = await _dbContextFactory.CreateDbContextAsync();

				// フレーズと関連するジャンルを削除
				if (phrase.DPhraseImage != null)
				{
                    _phraseRepository.DeletePhraseImage(context, phrase.DPhraseImage);
                }
				if (phrase.MPhraseGenres != null && phrase.MPhraseGenres.Count > 0)
				{
                    _phraseRepository.DeletePhraseGenreRange(context, phrase.MPhraseGenres);
                }
                _phraseRepository.CreatePhrase(context, phrase);

				await context.SaveChangesAsync();
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
