namespace CraftCaster.Application.Abstractions;

public interface IRepository<TEntity>
    where TEntity : class
{
    Task<IReadOnlyList<TEntity>> ReadAllAsync();
    Task<TEntity?> ReadAsync(object id);
    Task SaveAsync(TEntity entity);
    Task DeleteAsync(TEntity entity);
}