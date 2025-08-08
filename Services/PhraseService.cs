using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using PhrazorApp.Commons;
using PhrazorApp.Data;
using PhrazorApp.Data.Repositories;
using PhrazorApp.Models;

namespace PhrazorApp.Services
{

	/// <summary>
	/// フレーズサービス
	/// </summary>
	public class PhraseService
	{
		private readonly IDbContextFactory<EngDbContext> _dbContextFactory;
		private readonly PhraseRepository _phraseRepository;
		private readonly PhraseImageRepository _phraseImageRepository;
		private readonly PhraseGenreRepository _phraseGenreRepository;
		private readonly UserService _userService;

        private readonly ILogger<PhraseService> _logger;
        private readonly string MSG_PREFIX = "フレーズ";

		public PhraseService(
            IDbContextFactory<EngDbContext> dbContextFactory,
            PhraseRepository phraseRepository,
            GenreRepository genreRepository,
            PhraseImageRepository phraseImageRepository, 
			PhraseGenreRepository phraseGenreRepository,
			UserService userService,
            ILogger<PhraseService> logger)
        {
            _dbContextFactory = dbContextFactory;
            _phraseRepository = phraseRepository;
			_phraseImageRepository = phraseImageRepository;
			_phraseGenreRepository = phraseGenreRepository;
			_userService = userService;
            _logger = logger;
        }


        /// <summary>
        /// すべてのフレーズ情報を取得します。
        /// </summary>
        public async Task<List<PhraseModel>> GetPhraseViewModelListAsync()
		{
			var userId = _userService.GetUserId();
            await using var context = await _dbContextFactory.CreateDbContextAsync();

            // フレーズ情報をリポジトリから取得
            var phrases = await _phraseRepository.GetAllPhrasesAsync(context, userId);
			// モデルに変換して返却
			return phrases.Select(p => p.ToPhraseModel()).ToList();
		}

		/// <summary>
		/// 指定されたフレーズIDに基づいてフレーズ情報を取得します。
		/// </summary>
		public async Task<PhraseModel> GetPhraseViewModelAsync(Guid? phraseId)
		{
			var userId = _userService.GetUserId();
            await using var context = await _dbContextFactory.CreateDbContextAsync();

            var phrase = await _phraseRepository.GetPhraseByIdAsync(context, phraseId, userId);


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
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {



                // モデルをエンティティに変換
                var entity = model.ToPhraseEntity();
				await _phraseRepository.AddAsync(context, entity);

				// 画像があれば画像も保存
				if (!string.IsNullOrEmpty(model.ImageUrl))
				{
					var imageEntity = model.ToImageEntity(DateTime.UtcNow);
					await _phraseImageRepository.AddAsync(context, imageEntity);
				}

				// ジャンルを保存
				if (model.SelectedDropItems != null && model.SelectedDropItems.Count > 0)
				{
					var phraseGenres = model.ToPhraseGenreEntities();
					await _phraseGenreRepository.AddRangeAsync(context, phraseGenres);

                }

                await transaction.CommitAsync();


                return ServiceResult.Success(string.Format(ComMessage.MSG_I_SUCCESS_CREATE_DETAIL, MSG_PREFIX));
			}
			catch (Exception ex)
			{
                await transaction.RollbackAsync();
                _logger.LogError(ex, "フレーズ登録で例外が発生しました。");
				return ServiceResult.Failure(string.Format(ComMessage.MSG_E_FAILURE_CREATE_DETAIL, MSG_PREFIX));
			}
		}

		/// <summary>
		/// フレーズ情報を更新します。
		/// </summary>
		public async Task<IServiceResult> UpdatePhraseAsync(PhraseModel model)
		{
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
			{
				var userId = _userService.GetUserId();

                var phraseEntity = await _phraseRepository.GetPhraseByIdAsync(context, model.Id, userId);
				if (phraseEntity == null)
					return ServiceResult.Failure(string.Format(ComMessage.MSG_E_NOT_FOUND, MSG_PREFIX));

				// フレーズ情報を更新
				phraseEntity.Phrase = model.Phrase;
				phraseEntity.Meaning = model.Meaning;
				phraseEntity.Note = model.Note;
				phraseEntity.UpdatedAt = DateTime.UtcNow;


                // 画像があれば画像も更新
                if (!string.IsNullOrEmpty(model.ImageUrl))
				{
					var imageEntity = model.ToImageEntity(DateTime.UtcNow);
					await _phraseImageRepository.UpdateAsync(context, imageEntity);
				}


				// ジャンルを削除して新しいジャンルを追加
				await _phraseGenreRepository.DeleteRangeAsync(context, phraseEntity.MPhraseGenres);
				if (model.SelectedDropItems != null && model.SelectedDropItems.Count > 0)
				{
					var newGenres = model.ToPhraseGenreEntities();
					await _phraseGenreRepository.AddRangeAsync(context, newGenres);
                }

                await transaction.CommitAsync();

                return ServiceResult.Success(string.Format(ComMessage.MSG_I_SUCCESS_UPDATE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
			{
                await transaction.RollbackAsync();
                _logger.LogError(ex, "フレーズ更新で例外が発生しました。");
                return ServiceResult.Failure(string.Format(ComMessage.MSG_E_FAILURE_UPDATE_DETAIL, MSG_PREFIX));
			}
		}

		/// <summary>
		/// フレーズを削除します。
		/// </summary>
		public async Task<IServiceResult> DeletePhraseAsync(Guid phraseId)
		{
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
			{
                var userId = _userService.GetUserId();

                var phrase = await _phraseRepository.GetPhraseByIdAsync(context, phraseId, userId);
				if (phrase == null)
                    return ServiceResult.Failure(string.Format(ComMessage.MSG_E_NOT_FOUND, MSG_PREFIX));


				// フレーズと関連するジャンルを削除
				if (phrase.DPhraseImage != null)
				{
                    await _phraseImageRepository.DeleteAsync(context, phrase.DPhraseImage);
                }
				if (phrase.MPhraseGenres != null && phrase.MPhraseGenres.Count > 0)
				{
                    await _phraseGenreRepository.DeleteRangeAsync(context, phrase.MPhraseGenres);
                }
                
				await _phraseRepository.DeleteAsync(context, phrase);

                await transaction.CommitAsync();

                return ServiceResult.Success(string.Format(ComMessage.MSG_I_SUCCESS_DELETE_DETAIL, MSG_PREFIX));
			}
			catch (Exception ex)
			{
                await transaction.RollbackAsync();
                _logger.LogError(ex, "フレーズ削除で例外が発生しました。");
                return ServiceResult.Failure(string.Format(ComMessage.MSG_E_FAILURE_DELETE_DETAIL, MSG_PREFIX));
			}
		}
	}
}
