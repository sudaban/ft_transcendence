using System;
using System.Threading.Tasks;

namespace Backend.Domain.Repositories;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<T> repository<T>() where T : class;
    Task<int> save_changes_async();
}
