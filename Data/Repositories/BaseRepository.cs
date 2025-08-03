using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace PhrazorApp.Data.Repositories
{
    public abstract class BaseRepository<TEntity> where TEntity : class
    {


        public virtual async Task AddAsync(DbContext context, TEntity entity)
        {
            SetTimestamps(entity, isNew: true);
            context.Set<TEntity>().Add(entity);
            await context.SaveChangesAsync();
        }

        public virtual async Task AddRangeAsync(DbContext context, IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
                SetTimestamps(entity, isNew: true);

            context.Set<TEntity>().AddRange(entities);
            await context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(DbContext context, TEntity entity)
        {
            SetTimestamps(entity, isNew: false);
            context.Set<TEntity>().Update(entity);
            await context.SaveChangesAsync();
        }

        public virtual async Task UpdateRangeAsync(DbContext context, IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
                SetTimestamps(entity, isNew: false);

            context.Set<TEntity>().UpdateRange(entities);
            await context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(DbContext context, TEntity entity)
        {
            context.Set<TEntity>().Remove(entity);
            await context.SaveChangesAsync();
        }

        public virtual async Task DeleteRangeAsync(DbContext context, IEnumerable<TEntity> entities)
        {
            context.Set<TEntity>().RemoveRange(entities);
            await context.SaveChangesAsync();
        }

        private void SetTimestamps(object entity, bool isNew)
        {
            var now = DateTime.UtcNow;
            var type = entity.GetType();

            var createdAt = type.GetProperty("CreatedAt", BindingFlags.Public | BindingFlags.Instance);
            var updatedAt = type.GetProperty("UpdatedAt", BindingFlags.Public | BindingFlags.Instance);

            if (isNew && createdAt != null && createdAt.CanWrite)
                createdAt.SetValue(entity, now);

            if (updatedAt != null && updatedAt.CanWrite)
                updatedAt.SetValue(entity, now);
        }

    }
}
