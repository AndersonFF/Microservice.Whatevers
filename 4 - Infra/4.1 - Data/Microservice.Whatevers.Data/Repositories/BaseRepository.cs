using Microservice.Whatevers.Domain.Entities;
using Microservice.Whatevers.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System;

namespace Microservice.Whatevers.Data.Repositories
{
    public abstract class BaseRepository<TEntity> : IRepository<TEntity> 
        where TEntity : BaseEntity
    {
        private readonly IWhateverContext _context;
        private readonly DbSet<TEntity> _dbSet;

        protected BaseRepository(IWhateverContext whateverContext)
        {
            _context = whateverContext;
            _dbSet = whateverContext.Set<TEntity>();
        }

        public virtual void Delete(Guid id)
        {
            _dbSet.Remove(SelectById(id));
            _context.SaveChanges();
        }

        public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            _dbSet.Remove(await SelectByIdAsync(id, cancellationToken));
            await _context.SaveChangesAsync(true, cancellationToken);
        }

        public virtual bool Exists(Guid id) => _dbSet.AsNoTracking().Any(x => x.Id == id);

        public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken) => 
            await _dbSet.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken);

        public virtual void Insert(TEntity entity)
        {
            if (Exists(entity.Id)) return;

            _dbSet.Add(entity);
            _context.SaveChanges();
        }

        public virtual async Task InsertAsync(TEntity entity, CancellationToken cancellationToken)
        {
            if (await ExistsAsync(entity.Id, cancellationToken)) return;
            
            await _dbSet.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(true, cancellationToken);
        }

        public virtual TEntity SelectById(Guid id) => 
            _dbSet.AsNoTracking().FirstOrDefault(x => x.Id == id);

        public virtual async Task<TEntity> SelectByIdAsync(Guid id, CancellationToken cancellationToken) => 
            await _dbSet.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        public IQueryable<TEntity> SelectAll() => _dbSet.AsNoTracking();

        public void Update(TEntity entity)
        {
            if (!Exists(entity.Id)) return;

            _context.Entry(entity).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken)
        {
            if (!await ExistsAsync(entity.Id, cancellationToken)) return;
            
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync(true, cancellationToken);
        }
    }
}