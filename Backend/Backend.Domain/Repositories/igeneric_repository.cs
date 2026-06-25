using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Domain.Repositories;

public interface IGenericRepository<T> where T : class
{
    Task<IEnumerable<T>> get_all_async();
    Task<T?> get_by_id_async(int id);
    Task add_async(T entity);
    void update(T entity);
    void delete(T entity);
}
