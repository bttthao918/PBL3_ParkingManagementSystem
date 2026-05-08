using ParkingManagement.DAL.Data;
using ParkingManagement.DAL.Interfaces;

namespace ParkingManagement.DAL.Repositories;

public sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private readonly Dictionary<Type, object> _repositories = [];

    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity);
        if (!_repositories.TryGetValue(type, out var repo))
        {
            repo = new GenericRepository<TEntity>(context);
            _repositories[type] = repo;
        }
        return (IGenericRepository<TEntity>)repo;
    }

    public async Task<int> SaveChangesAsync() =>
        await context.SaveChangesAsync();

    public async ValueTask DisposeAsync() =>
        await context.DisposeAsync();
}
