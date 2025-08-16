// Services/DiaryTagService.cs
using PhrazorApp.Data.UnitOfWork;
using PhrazorApp.Models;

namespace PhrazorApp.Services
{


    public sealed class DiaryTagService
    {
        private readonly UnitOfWork _uow;
        private readonly UserService _user;

        public DiaryTagService(UnitOfWork uow, UserService user)
        {
            _uow = uow; _user = user;
        }

        public Task<ServiceResult<List<DropItemModel>>> GetDiaryTagDropItemModelListAsync()
              => _uow.ReadAsync(async r =>
              {
                  var uid = _user.GetUserId();
                  var tags = await r.DiaryTags.GetAllAsync();

                  var list = tags
                      .Select(t => new DropItemModel
                      {
                          Key1 = t.TagId,                  // TagId (Guid)
                          Key2 = Guid.Empty,            // タグは親なし
                          Name = t.TagName ?? string.Empty,
                          DropTarget = DropItemType.UnAssigned
                      })
                      .ToList();

                  return ServiceResult.Success(list);
              });
    }
}

