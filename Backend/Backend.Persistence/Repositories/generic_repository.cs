using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Persistence.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly ApplicationDbContext db_context;
    private readonly DbSet<T> db_set;

    public GenericRepository(ApplicationDbContext db_context)
    {
        this.db_context = db_context;
        this.db_set = db_context.Set<T>();
    }

    public async Task<IEnumerable<T>> get_all_async()
    {
        return await db_set.ToListAsync();
    }

    public async Task<T?> get_by_id_async(int id)
    {
        return await db_set.FindAsync(id);
    }

    public async Task add_async(T entity)
    {
        await db_set.AddAsync(entity);
    }

    public void update(T entity)
    {
        db_set.Update(entity);
    }

    public void delete(T entity)
    {
        db_set.Remove(entity);
    }
}
