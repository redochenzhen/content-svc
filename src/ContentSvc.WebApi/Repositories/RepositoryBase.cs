using ContentSvc.Model.Entities;
using ContentSvc.WebApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ContentSvc.WebApi.Repositories
{
    public abstract class RepositoryBase<TEntity> : IRepository<TEntity>
        where TEntity : class, new()
    {
        public DbSet<TEntity> DbSet => _entitySet;

        protected readonly DbContext _context;
        protected readonly DbSet<TEntity> _entitySet;

        protected bool TryAttach<T>(T entity)
        {
            if (_context.Entry(entity).State != EntityState.Detached) return false;
            _context.Attach(entity);
            return true;
        }

        protected bool TryAttachRange(params object[] entities)
        {
            bool flag = true;
            foreach (var e in entities)
            {
                flag = TryAttach(e) && false;
            }
            return flag;
        }

        public RepositoryBase(DbContext context,
            Func<DbContext, DbSet<TEntity>> getEntity)
        {
            _context = context;
            _entitySet = getEntity(context);
        }

        public void Add(TEntity entity)
        {
            _entitySet.Add(entity);
        }

        public void AddRange(IList<TEntity> entities)
        {
            _entitySet.AddRange(entities.ToArray());
        }

        public virtual async Task<IList<TEntity>> GetAllAsync()
        {
            return await _entitySet.ToListAsync();
        }

        public void Remove(TEntity entity)
        {
            _entitySet.Remove(entity);
        }

        public void RemoveRange(IList<TEntity> entities)
        {
            _entitySet.RemoveRange(entities.ToList());
        }

        public void Update(TEntity entity)
        {
            _entitySet.Update(entity);
        }

        public void UpdateRange(IList<TEntity> entities)
        {
            _entitySet.UpdateRange(entities.ToList());
        }

        public void PartialUpdate(TEntity entity, Func<string, bool> isModified)
        {
            TryAttach(entity);
            var type = entity.GetType();
            var properties = type.GetProperties();
            foreach (var p in properties)
            {
                if (!isModified(p.Name)) continue;
                _context.Entry(entity)
                .Property(p.Name)
                .IsModified = true;
            }
        }
    }

    public class RepositoryBase<TEntity, TId> : RepositoryBase<TEntity>, IRepository<TEntity, TId>
        where TEntity : class, IEntity<TId>, new()
    {
        public RepositoryBase(DbContext context, Func<DbContext, DbSet<TEntity>> getEntity) :
            base(context, getEntity)
        {
        }

        public async Task<int> ArchiveAsync(TId id)
        {
            var tableAtt = this.GetType()
            .GetCustomAttributes(false)
            .Cast<TableAttribute>()
            .Where(a => a != null)
            .FirstOrDefault();
            if (tableAtt == null) goto THROW;
            var tableName = tableAtt.Name;
            // var repositoryName = this.GetType().Name;
            // var match = Regex.Match(repositoryName, @"(?<TableName>\w+)Repository");
            // if (match == null) goto THROW;
            // var tableName = match.Groups["TableName"].Value;
            if (string.IsNullOrEmpty(tableName)) goto THROW;
            return await _context.Database.ExecuteSqlRawAsync(
                $"INSERT INTO `{tableName}History` SELECT * FROM `{tableName}` WHERE `Id` = @p0", id);
        THROW:
            throw new Exception($"无法获取TableAttribute");
        }

        public async Task<int> ArchiveAsync(params TId[] ids)
        {
            if (ids.Length == 0) return 0;
            var tableAtt = this.GetType()
            .GetCustomAttributes(false)
            .Cast<TableAttribute>()
            .Where(a => a != null)
            .FirstOrDefault();
            if (tableAtt == null) goto THROW;
            var tableName = tableAtt.Name;
            if (string.IsNullOrEmpty(tableName)) goto THROW;
            var paramsLst = string.Join(',', ids.Select((_, idx) => $"@p{idx}"));
            var @params = ids.Cast<object>();
            return await _context.Database.ExecuteSqlRawAsync(
                $"INSERT INTO `{tableName}History` SELECT * FROM `{tableName}` WHERE `Id` IN ({paramsLst})", @params);
        THROW:
            throw new Exception($"无法获取TableAttribute");
        }

        public virtual async Task<bool> ExistsAsync(TId id)
        {
            return await _entitySet
            .AnyAsync(e => e.Id.Equals(id));
        }

        public async Task<TEntity> GetAsync(TId id)
        {
            return await _entitySet
            .Where(e => e.Id.Equals(id))
            .SingleOrDefaultAsync();
        }

        public void Remove(TId id)
        {
            this.Remove(new TEntity { Id = id });
        }

    }

    public class RepositoryBase<TEntity, TId1, TId2> : RepositoryBase<TEntity>, IRepository<TEntity, TId1, TId2>
        where TEntity : class, IEntity<TId1, TId2>, new()
    {
        public RepositoryBase(DbContext context, Func<DbContext, DbSet<TEntity>> getEntity) :
            base(context, getEntity)
        {
        }

        public virtual async Task<bool> ExistsAsync(TId1 id1, TId2 id2)
        {
            return await _entitySet
            .AnyAsync(e => e.Id1.Equals(id1) && e.Id2.Equals(id2));
        }

        public async Task<TEntity> GetAsync(TId1 id1, TId2 id2)
        {
            return await _entitySet
            .Where(e => e.Id1.Equals(id1) && e.Id2.Equals(2))
            .SingleOrDefaultAsync();
        }

        public void Remove(TId1 id1, TId2 id2)
        {
            this.Remove(
                new TEntity
                {
                    Id1 = id1,
                    Id2 = id2
                });
        }
    }
}