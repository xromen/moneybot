using Microsoft.EntityFrameworkCore;
using MoneyBotTelegram.Infrasctructure;
using MoneyBotTelegram.Infrasctructure.Entities;

namespace MoneyBotTelegram.Services;
public interface IEntityCacheService<TEntity>
    where TEntity : BaseEntity
{
    Task<TEntity?> FindAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task RefreshAsync(CancellationToken cancellationToken = default);
}

public class EntityCacheService<TEntity>(IDbContextFactory<ApplicationDbContext> contextFactory) : IEntityCacheService<TEntity>
    where TEntity : BaseEntity
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private List<TEntity> _cache = new();
    private bool _isInitialized;

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
            return _cache;

        await RefreshAsync(cancellationToken);
        return _cache;
    }

    public async Task<TEntity?> FindAsync(long id, CancellationToken cancellationToken = default)
    {
        if (!_isInitialized)
            await RefreshAsync(cancellationToken);

        return _cache.SingleOrDefault(c => c.Id == id);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
            db.Set<TEntity>().Add(entity);
            await db.SaveChangesAsync(cancellationToken);

            _cache.Add(entity);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
            _cache = await db.Set<TEntity>().ToListAsync(cancellationToken);
            _isInitialized = true;
        }
        finally
        {
            _lock.Release();
        }
    }
}
