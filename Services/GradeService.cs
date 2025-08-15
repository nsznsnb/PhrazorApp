using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data.UnitOfWork;
using PhrazorApp.Models;
using PhrazorApp.Models.Mappings;

namespace PhrazorApp.Services
{
    public sealed class GradeService
    {
        private readonly UnitOfWork _uow;
        private const string MSG_PREFIX = "成績";

        public GradeService(UnitOfWork uow) => _uow = uow;

        public Task<ServiceResult<List<GradeModel>>> GetListAsync()
        {
            return _uow.ReadAsync(async repos =>
            {
                var list = await repos.Grades.Queryable()
                    .OrderBy(x => x.GradeName)
                    .SelectModel()
                    .ToListAsync();
                return ServiceResult.Success(list, message: "");
            });

        }


        public Task<ServiceResult<GradeModel?>> GetAsync(Guid id)
        {
            return _uow.ReadAsync(async repos =>
            {
                var e = await repos.Grades.GetByIdAsync(id);
                return ServiceResult.Success(e?.ToModel(), message: "");
            });
        }

        public async Task<ServiceResult<Unit>> CreateAsync(GradeModel model)
        {
            try
            {
                await _uow.ExecuteInTransactionAsync(async repos =>
                {
                    if (model.Id == Guid.Empty) model.Id = Guid.NewGuid();

                    var exists = await repos.Grades.Queryable()
                        .AnyAsync(x => x.GradeName == model.Name);
                    if (exists) throw new InvalidOperationException("同名の成績が既に存在します。");

                    await repos.Grades.AddAsync(model.ToEntity());
                });

                return ServiceResult.None.Success(string.Format(AppMessages.MSG_I_SUCCESS_CREATE_DETAIL, MSG_PREFIX));
            }
            catch
            {
                return ServiceResult.None.Error(string.Format(AppMessages.MSG_E_FAILURE_CREATE_DETAIL, MSG_PREFIX));
            }
        }

        public async Task<ServiceResult<Unit>> UpdateAsync(GradeModel model)
        {
            try
            {
                await _uow.ExecuteInTransactionAsync(async repos =>
                {
                    var e = await repos.Grades.GetByIdAsync(model.Id)  
                        ?? throw new InvalidOperationException("対象が見つかりません。");

                    var exists = await repos.Grades.Queryable()
                        .AnyAsync(x => x.GradeName == model.Name && x.GradeId != model.Id);
                    if (exists) throw new InvalidOperationException("同名の成績が既に存在します。");

                    model.ApplyTo(e);
                    await repos.Grades.UpdateAsync(e);
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
                var hasRef = await _uow.ReadAsync(async r =>
                    await r.TestResults.Queryable().AnyAsync(x => x.GradeId == id));
                if (hasRef)
                    return ServiceResult.None.Warning("この成績はテスト結果で使用されています。削除できません。");

                await _uow.ExecuteInTransactionAsync(async r =>
                {
                    var e = await r.Grades.GetByIdAsync(id)
                        ?? throw new InvalidOperationException("対象が見つかりません。");
                    await r.Grades.DeleteAsync(e);
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
