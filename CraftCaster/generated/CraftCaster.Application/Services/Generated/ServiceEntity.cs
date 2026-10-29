using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;

namespace CraftCaster.Application.Services;

public abstract class ServiceEntity<TEntity> : IAsyncDisposable
    where TEntity : class
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<TEntity> _repository;
    private readonly bool _ownsUnitOfWork;

    protected ServiceEntity(IUnitOfWork unitOfWork, IRepository<TEntity> repository, bool ownsUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = repository;
        _ownsUnitOfWork = ownsUnitOfWork;
    }

    protected async Task<TEntity> AddDefaultAsync(TEntity entity)
    {
        await _repository.SaveAsync(entity);
        return entity;
    }

    protected async Task DeleteDefaultAsync(object id)
    {
        var entity = await _repository.ReadAsync(id)
            ?? throw new ModelException($"{typeof(TEntity).Name} not found.");
        await _repository.DeleteAsync(entity);
    }

    protected Task<TEntity?> ReadOIDDefaultAsync(object id) =>
        _repository.ReadAsync(id);

    protected Task<IReadOnlyList<TEntity>> ReadAllDefaultAsync() =>
        _repository.ReadAllAsync();

    protected async Task<TEntity> ModifierDefaultAsync(TEntity entity)
    {
        await _repository.SaveAsync(entity);
        return entity;
    }

    public async ValueTask DisposeAsync()
    {
        if (_ownsUnitOfWork)
        {
            await _unitOfWork.DisposeAsync();
        }
    }
}