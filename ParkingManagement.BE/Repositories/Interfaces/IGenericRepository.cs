using System.Linq.Expressions;

namespace ParkingManagement.DAL.Interfaces;

public interface IGenericRepository<T> where T : class
{
    // Read
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    // Write
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);

    // Aggregate
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
}
