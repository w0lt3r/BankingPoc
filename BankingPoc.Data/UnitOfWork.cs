using BankingPoc.Data.Interfaces;

namespace BankingPoc.Data;

public class UnitOfWork : IUnitOfWork
{
    private BankingContext context;
    private Dictionary<Type, object> repositories;

    public UnitOfWork(
        BankingContext context
    )
    {
        this.context = context;
        this.context.Database.EnsureCreated();
        repositories = new Dictionary<Type, object>();
    }
    
    public async Task SaveChanges()
    {
        await context.SaveChangesAsync();
    }

    private bool disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            if (disposing)
            {
                context.Dispose();
            }
        }
        this.disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class
    {
        if (repositories.ContainsKey(typeof(TEntity)))
        {
            return (IGenericRepository<TEntity>)repositories[typeof(TEntity)];
        }

        var repository = new GenericRepository<TEntity>(context);
        repositories.Add(typeof(TEntity), repository);
        return repository;
    }
}