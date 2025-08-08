using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace PhrazorApp.Data.Repositories
{
    /// <summary>
    /// UoW前提のベースリポジトリ（SaveChangesは呼ばない）
    /// </summary>
    public abstract class BaseRepository<TEntity> where TEntity : class
    {
        protected readonly DbContext _context;
        protected DbSet<TEntity> Set => _context.Set<TEntity>();

        protected BaseRepository(DbContext context) => _context = context;

        public virtual Task AddAsync(TEntity entity)
        {
            Stamp(entity, isNew: true);
            Set.Add(entity);
            return Task.CompletedTask;
        }

        public virtual Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            foreach (var e in entities) Stamp(e, isNew: true);
            Set.AddRange(entities);
            return Task.CompletedTask;
        }

        public virtual Task UpdateAsync(TEntity entity)
        {
            Stamp(entity, isNew: false);
            Set.Update(entity);
            return Task.CompletedTask;
        }

        public virtual Task UpdateRangeAsync(IEnumerable<TEntity> entities)
        {
            foreach (var e in entities) Stamp(e, isNew: false);
            Set.UpdateRange(entities);
            return Task.CompletedTask;
        }

        public virtual Task DeleteAsync(TEntity entity)
        {
            Set.Remove(entity);
            return Task.CompletedTask;
        }

        public virtual Task DeleteRangeAsync(IEnumerable<TEntity> entities)
        {
            Set.RemoveRange(entities);
            return Task.CompletedTask;
        }

        protected static void Stamp(object entity, bool isNew)
        {
            var now = DateTime.UtcNow;
            var type = entity.GetType();

            var createdAt = type.GetProperty("CreatedAt", BindingFlags.Public | BindingFlags.Instance);
            var updatedAt = type.GetProperty("UpdatedAt", BindingFlags.Public | BindingFlags.Instance);

            if (isNew && createdAt?.CanWrite == true) createdAt.SetValue(entity, now);
            if (updatedAt?.CanWrite == true) updatedAt.SetValue(entity, now);
        }
    }
}
