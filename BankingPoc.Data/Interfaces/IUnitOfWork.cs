namespace BankingPoc.Data.Interfaces;

public interface IUnitOfWork: IDisposable
{
    Task SaveChanges();
    IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
}