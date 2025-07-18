using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhrazorApp.Common;
using PhrazorApp.Data;
using PhrazorApp.Extensions;
using PhrazorApp.Models;
using PhrazorApp.ViewModels;
using System.Security.Cryptography.Xml;

namespace PhrazorApp.Services
{
    public interface ICategoryService
    {
        public Task<List<LargeCategoryModel>> GetCategoryViewModelListAsync();

        public Task<LargeCategoryModel> GetCategoryViewModelAsync(Guid largeCategoryId);

        public Task<IServiceResult> CreateCategoryAsync(LargeCategoryModel model);

        public Task<IServiceResult> UpdateCategoryAsync(LargeCategoryModel model);

        public Task<IServiceResult> DeleteCategoryAsync(Guid largeCategoryId);
    }


    /// <summary>
    /// カテゴリ情報サービス
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly IDbContextFactory<EngDbContext> _dbContextFactory;
        private readonly IUserService _userService;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(IDbContextFactory<EngDbContext> dbContextFactory, IUserService userService, ILogger<CategoryService> logger)
        {
            _dbContextFactory = dbContextFactory;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// カテゴリ情報を取得します
        /// </summary>
        /// <returns></returns>
        public async Task<List<LargeCategoryModel>> GetCategoryViewModelListAsync()
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var userId = _userService.GetUserId();

            // カテゴリ一覧取得
            var categories = await context.MLargeCategories
                .Where(x => x.UserId == userId)
                .Include(x => x.MSmallCategories)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

            // エンティティ => モデル
            return categories.Select(x => new LargeCategoryModel
            {
                Id = x.LargeId,
                Name = x.LargeCategoryName,
                SubCategories = x.MSmallCategories.Select(xc => new SmallCategoryModel
                {
                    Id = xc.SmallId,
                    Name = xc.SmallCategoryName,
                    SortOrder = xc.SortOrder
                })
                .OrderBy(x => x.SortOrder)
                .ToList()
            })
            .ToList();
        }

        /// <summary>
        /// カテゴリ情報を取得します
        /// </summary>
        /// <returns></returns>
        public async Task<LargeCategoryModel> GetCategoryViewModelAsync(Guid largeCategoryId)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var userId = _userService.GetUserId();

            // 対象カテゴリ取得
            var largeCategory = await context.MLargeCategories
                .Include(x => x.MSmallCategories)
                .FirstOrDefaultAsync(x => x.LargeId == largeCategoryId && (x.UserId == userId));

            if (largeCategory == null) return new();

            // エンティティ => モデル
            var largeModel = new LargeCategoryModel
            {
                Id = largeCategoryId,
                Name = largeCategory.LargeCategoryName,
                SubCategories = largeCategory.MSmallCategories.Select(x => new SmallCategoryModel
                {
                    Id = x.SmallId,
                    Name = x.SmallCategoryName,
                    SortOrder = x.SortOrder
                })
                .OrderBy(x => x.SortOrder)
                .ToList()
            };

            return largeModel;
        }


        /// <summary>
        /// カテゴリを新規作成します
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<IServiceResult> CreateCategoryAsync(LargeCategoryModel model)
        {
            var userId = _userService.GetUserId();
            var sysDateTime = DateTime.Now;
            
            // モデル => エンティティ
            var largeCategoryEntity = new MLargeCategory
            {
                LargeId = model.Id,
                LargeCategoryName = model.Name,
                CreatedAt = sysDateTime,
                UpdatedAt = sysDateTime,
                UserId = userId
            };

            var smallCategoryEntities = model.SubCategories.Select(x => new MSmallCategory
            {
                LargeId = model.Id,
                SmallId = x.Id,
                SmallCategoryName = x.Name,
                SortOrder = x.SortOrder,
                CreatedAt = sysDateTime,
                UpdatedAt = sysDateTime,
                UserId = userId
            }).ToList();

            await using var context = await _dbContextFactory.CreateDbContextAsync();
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // エンティティ追加
                context.MLargeCategories.Add(largeCategoryEntity);
                context.MSmallCategories.AddRange(smallCategoryEntities);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                var message = string.Format(ComMessage.MSG_E_ERROR_CREATE_DETAIL, model.Name);
                _logger.LogErrorWithContext(ComLogEvents.CreateItem, ex, message);
                return ServiceResult.Failure(message);
            }

            var successMessage = string.Format(ComMessage.MSG_I_SUCCESS_CREATE_DETAIL, model.Name);
            return ServiceResult.Success(successMessage);
        }

        /// <summary>
        /// カテゴリを更新します
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<IServiceResult> UpdateCategoryAsync(LargeCategoryModel model)
        {
            var userId = _userService.GetUserId();
            var sysDateTime = DateTime.Now;

            await using var context = await _dbContextFactory.CreateDbContextAsync();
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // 対象カテゴリの取得
                var existingCategory = await context.MLargeCategories
                    .Include(x => x.MSmallCategories)
                    .FirstOrDefaultAsync(c => c.LargeId == model.Id && (c.UserId == userId));

                if (existingCategory == null)
                {
                    var message = string.Format(ComMessage.MSG_E_ERROR_NOT_FOUND, model.Name);
                    _logger.LogWarningWithContext(ComLogEvents.UpdateItem, message);
                    return ServiceResult.Warning(message);
                }

                // カテゴリ情報の更新
                existingCategory.LargeCategoryName = model.Name;
                existingCategory.UpdatedAt = sysDateTime;

                // 既存のサブカテゴリを削除
                context.MSmallCategories.RemoveRange(existingCategory.MSmallCategories);

                // 新しいサブカテゴリを追加
                var newSubCategories = model.SubCategories.Select(x => new MSmallCategory
                {
                    LargeId = model.Id,
                    SmallId = x.Id,
                    SmallCategoryName = x.Name,
                    SortOrder = x.SortOrder,
                    CreatedAt = sysDateTime,
                    UpdatedAt = sysDateTime,
                    UserId = userId,
                }).ToList();

                context.MSmallCategories.AddRange(newSubCategories);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ServiceResult.Success(string.Format(ComMessage.MSG_I_SUCCESS_UPDATE_DETAIL, model.Name));
            }
            catch (Exception ex)
            {
                var message = string.Format(ComMessage.MSG_E_ERROR_UPDATE_DETAIL, model.Name);
                _logger.LogErrorWithContext(ComLogEvents.UpdateItem, ex, message);
                return ServiceResult.Failure(message);
            }
        }

        /// <summary>
        /// カテゴリを削除します
        /// </summary>
        /// <param name="largeCategoryId"></param>
        /// <returns></returns>
        public async Task<IServiceResult> DeleteCategoryAsync(Guid largeCategoryId)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            await using var transaction = await context.Database.BeginTransactionAsync();

            // 大分類名
            var largeCategoryName = string.Empty;
            try
            {
                // 大分類取得
                var largeCategory = await context.MLargeCategories.FirstOrDefaultAsync(x => x.LargeId == largeCategoryId);

                if (largeCategory == null)
                {
                    var message = string.Format(ComMessage.MSG_E_ERROR_NOT_FOUND, largeCategoryId);
                    _logger.LogWarningWithContext(ComLogEvents.DeleteItem, message);
                    return ServiceResult.Failure(message);
                }

                largeCategoryName = largeCategory.LargeCategoryName;
                // 小分類取得
                var smallCategories = await context.MSmallCategories
                    .Where(x => x.LargeId == largeCategoryId)
                    .ToListAsync();

                // 削除
                context.MSmallCategories.RemoveRange(smallCategories);
                context.MLargeCategories.Remove(largeCategory);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ServiceResult.Success(string.Format(ComMessage.MSG_I_SUCCESS_DELETE_DETAIL, largeCategoryName));

            }
            catch (Exception ex)
            {
                var message = string.Format(ComMessage.MSG_E_ERROR_DELETE_DETAIL, largeCategoryName);
                _logger.LogErrorWithContext(ComLogEvents.DeleteItem, ex, message);
                return ServiceResult.Failure(message);
            }
        }
    }
}
