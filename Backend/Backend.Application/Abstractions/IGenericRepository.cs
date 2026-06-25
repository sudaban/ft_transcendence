namespace Backend.Application.Abstractions;

public interface IGenericRepository<T> where T : class
{

    IQueryable<T> GetQueryable(bool asNoTracking = false);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}