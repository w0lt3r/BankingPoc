using System.Linq.Expressions;
using BankingPoc.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankingPoc.Data;

public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
{
    internal BankingContext context;
    internal DbSet<TEntity> dbSet;

    public GenericRepository(BankingContext context)
    {
        this.context = context;
        this.dbSet = context.Set<TEntity>();
    }

    public async Task<IEnumerable<TEntity>> Get(
        Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        string includeProperties = "")
    {
        IQueryable<TEntity> query = dbSet;

        if (filter != null)
        {
            query = query.Where(filter);
        }

        foreach (var includeProperty in includeProperties.Split
                     (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }

        if (orderBy != null)
        {
            return await orderBy(query).ToListAsync();
        }
        else
        {
            return await query.ToListAsync();
        }
    }

    public virtual async Task<TEntity> Insert(TEntity entity)
    {
        var entityEntry = await dbSet.AddAsync(entity);
        return entityEntry.Entity;
    }

    public virtual async Task Update(TEntity entityToUpdate)
    {
        dbSet.Attach(entityToUpdate);
        context.Entry(entityToUpdate).State = EntityState.Modified;
    }
    
    public virtual async Task<TEntity> Get(object id)
    {
        return await dbSet.FindAsync(id);
    }
    
    public virtual async Task Delete(TEntity entityToDelete)
    {
        dbSet.Remove(entityToDelete);
    }
    
    public virtual async Task DeleteRange(IEnumerable<TEntity> entitiesToDelete)
    {
        dbSet.RemoveRange(entitiesToDelete);
    }
}