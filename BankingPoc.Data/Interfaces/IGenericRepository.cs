using System.Linq.Expressions;

namespace BankingPoc.Data.Interfaces;

public interface IGenericRepository<TEntity> where TEntity : class
{
    Task<TEntity> Insert(TEntity entity);
    Task Update(TEntity entityToUpdate);
    Task<TEntity> Get(object id);
    Task Delete(TEntity entityToDelete);
    Task DeleteRange(IEnumerable<TEntity> entitiesToDelete);
    Task<IEnumerable<TEntity>> Get(
        Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        string includeProperties = "");
}