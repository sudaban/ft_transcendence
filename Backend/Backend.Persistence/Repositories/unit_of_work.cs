using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Backend.Domain.Repositories;

namespace Backend.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext db_context;
    private readonly ConcurrentDictionary<string, object> repositories;
    private bool disposed;

    public UnitOfWork(ApplicationDbContext db_context)
    {
        this.db_context = db_context;
        this.repositories = new ConcurrentDictionary<string, object>();
    }

    public IGenericRepository<T> repository<T>() where T : class
    {
        var type_name = typeof(T).Name;
        return (IGenericRepository<T>)repositories.GetOrAdd(type_name, _ => new GenericRepository<T>(db_context));
    }

    public async Task<int> save_changes_async()
    {
        return await db_context.SaveChangesAsync();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                db_context.Dispose();
            }
            disposed = true;
        }
    }
}
