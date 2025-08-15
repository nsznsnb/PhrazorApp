using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data.UnitOfWork;
using PhrazorApp.Models;
using PhrazorApp.Models.Mappings;

namespace PhrazorApp.Services
{
    public sealed class ReviewTypeService
    {
        private readonly UnitOfWork _uow;
        private const string MSG_PREFIX = "復習種別";

        public ReviewTypeService(UnitOfWork uow) => _uow = uow;

        public async Task<ServiceResult<List<ReviewTypeModel>>> GetListAsync()
        {
            var list = await _uow.ReadAsync(async repos =>
            {
                return await repos.ReviewTypes.Queryable()
                .OrderBy(x => x.ReviewTypeName)
                .SelectModel()
                .ToListAsync();
            });
            return ServiceResult.Success(list);
        }

        public async Task<ServiceResult<ReviewTypeModel?>> GetAsync(Guid id)
        {
            var model = await _uow.ReadAsync(async repos =>
            {
                var data = await repos.ReviewTypes.GetByIdAsync(id);
                return data?.ToModel();
            });
            return ServiceResult.Success(model);
        }

        public async Task<ServiceResult<Unit>> CreateAsync(ReviewTypeModel model)
        {
            try
            {
                await _uow.ExecuteInTransactionAsync(async repos =>
                {
                    if (model.Id == Guid.Empty) model.Id = Guid.NewGuid();

                    var dup = await repos.ReviewTypes.Queryable(true)
                        .AnyAsync(x => x.ReviewTypeName == model.Name);
                    if (dup) throw new InvalidOperationException("同名の復習種別が既に存在します。");

                    await repos.ReviewTypes.AddAsync(model.ToEntity());
                });

                return ServiceResult.None.Success(string.Format(AppMessages.MSG_I_SUCCESS_CREATE_DETAIL, MSG_PREFIX));
            }
            catch
            {
                return ServiceResult.None.Error(string.Format(AppMessages.MSG_E_FAILURE_CREATE_DETAIL, MSG_PREFIX));
            }
        }

        public async Task<ServiceResult<Unit>> UpdateAsync(ReviewTypeModel model)
        {
            try
            {
                await _uow.ExecuteInTransactionAsync(async r =>
                {
                    var e = await r.ReviewTypes.GetByIdAsync(model.Id)
                        ?? throw new InvalidOperationException("対象が見つかりません。");

                    var dup = await r.ReviewTypes.Queryable(true)
                        .AnyAsync(x => x.ReviewTypeName == model.Name && x.ReviewTypeId != model.Id);
                    if (dup) throw new InvalidOperationException("同名の復習種別が既に存在します。");

                    model.ApplyTo(e);
                    await r.ReviewTypes.UpdateAsync(e);
                });

                return ServiceResult.None.Success(string.Format(AppMessages.MSG_I_SUCCESS_UPDATE_DETAIL, MSG_PREFIX));
            }
            catch
            {
                return ServiceResult.None.Error(string.Format(AppMessages.MSG_E_FAILURE_UPDATE_DETAIL, MSG_PREFIX));
            }
        }

        public async Task<ServiceResult<Unit>> DeleteAsync(Guid id)
        {
            try
            {
                var hasRef = await _uow.ReadAsync(async repos =>
                    await repos.ReviewLogs.Queryable().AnyAsync(x => x.ReviewTypeId == id));
                if (hasRef)
                    return ServiceResult.None.Warning("この復習種別は復習履歴で使用されています。削除できません。");

                await _uow.ExecuteInTransactionAsync(async repos =>
                {
                    var e = await repos.ReviewTypes.GetByIdAsync(id)
                        ?? throw new InvalidOperationException("対象が見つかりません。");
                    await repos.ReviewTypes.DeleteAsync(e);
                });

                return ServiceResult.None.Success(string.Format(AppMessages.MSG_I_SUCCESS_DELETE_DETAIL, MSG_PREFIX));
            }
            catch
            {
                return ServiceResult.None.Error(string.Format(AppMessages.MSG_E_FAILURE_DELETE_DETAIL, MSG_PREFIX));
            }
        }
    }
}
