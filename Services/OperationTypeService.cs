using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data.UnitOfWork;
using PhrazorApp.Models;
using PhrazorApp.Models.Mappings;

namespace PhrazorApp.Services
{
    public sealed class OperationTypeService
    {
        private readonly UnitOfWork _uow;
        private const string MSG_PREFIX = "操作種別";

        public OperationTypeService(UnitOfWork uow) => _uow = uow;

        public async Task<ServiceResult<List<OperationTypeModel>>> GetListAsync()
        {
            var list = await _uow.ReadAsync(async (UowRepos repos) =>
                await repos.OperationTypes
                    .Queryable(asNoTracking: true)
                    .OrderBy(x => x.OperationTypeCode)          // ★ コード順
                    .ThenBy(x => x.OperationTypeName)           //   安定ソート（任意）
                    .SelectModel()
                    .ToListAsync());
            return ServiceResult.Success(list);
        }

        public async Task<ServiceResult<OperationTypeModel?>> GetAsync(Guid id)
        {
            var data = await _uow.ReadAsync(async (UowRepos repos) =>
            {
                var e = await repos.OperationTypes.GetByIdAsync(id);
                return e?.ToModel();

            });
            return ServiceResult.Success(data);
        }

        public async Task<ServiceResult<Unit>> CreateAsync(OperationTypeModel model)
        {
            try
            {
                await _uow.ExecuteInTransactionAsync(async (UowRepos repos) =>
                {
                    if (model.Id == Guid.Empty) model.Id = Guid.NewGuid();

                    var dup = await repos.OperationTypes.Queryable(true)
                        .AnyAsync(x => x.OperationTypeName == model.Name || x.OperationTypeCode == model.Code);
                    if (dup) throw new InvalidOperationException("同名または同コードの操作種別が既に存在します。");

                    await repos.OperationTypes.AddAsync(model.ToEntity());
                });

                return ServiceResult.None.Success(string.Format(AppMessages.MSG_I_SUCCESS_CREATE_DETAIL, MSG_PREFIX));
            }
            catch
            {
                return ServiceResult.None.Error(string.Format(AppMessages.MSG_E_FAILURE_CREATE_DETAIL, MSG_PREFIX));
            }
        }

        public async Task<ServiceResult<Unit>> UpdateAsync(OperationTypeModel model)
        {
            try
            {
                await _uow.ExecuteInTransactionAsync(async (UowRepos repos) =>
                {
                    var e = await repos.OperationTypes.GetByIdAsync(model.Id)
                        ?? throw new InvalidOperationException("対象が見つかりません。");

                    var dup = await repos.OperationTypes.Queryable(true)
                        .AnyAsync(x => (x.OperationTypeName == model.Name || x.OperationTypeCode == model.Code)
                                     && x.OperationTypeId != model.Id);
                    if (dup) throw new InvalidOperationException("同名または同コードの操作種別が既に存在します。");

                    model.ApplyTo(e);
                    await repos.OperationTypes.UpdateAsync(e);
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
                var hasRef = await _uow.ReadAsync(async (UowRepos repos) =>
                    await repos.DailyUsages.Queryable(true).AnyAsync(x => x.OperationTypeId == id));
                if (hasRef)
                    return ServiceResult.None.Warning("この操作種別は利用状況に使用されています。削除できません。");

                await _uow.ExecuteInTransactionAsync(async (UowRepos repos) =>
                {
                    var e = await repos.OperationTypes.GetByIdAsync(id)
                        ?? throw new InvalidOperationException("対象が見つかりません。");
                    await repos.OperationTypes.DeleteAsync(e);
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
