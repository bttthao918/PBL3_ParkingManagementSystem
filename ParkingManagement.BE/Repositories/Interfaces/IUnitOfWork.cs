namespace ParkingManagement.DAL.Interfaces;

/// <summary>
/// Unit of Work pattern to coordinate multiple repositories within a single transaction.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync();
}
