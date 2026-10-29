using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;
using CraftCaster.Application.Abstractions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;
using CraftCaster.Domain.ValueObjects;
using CraftCaster.Infrastructure.Exceptions;

namespace CraftCaster.Infrastructure.Repositories;

public sealed class ZonaPartidaRepository : RepositoryBase<ZonaPartida>, IZonaPartidaRepository
{
    public ZonaPartidaRepository(ILogger<ZonaPartidaRepository>? logger = null)
        : base(logger)
    {
    }

    public ZonaPartidaRepository(ISession session, ILogger<ZonaPartidaRepository>? logger = null)
        : base(session, logger)
    {
    }

    public ZonaPartidaRepository(ISessionFactory sessionFactory, ILogger<ZonaPartidaRepository>? logger = null)
        : base(sessionFactory, logger)
    {
    }

    public async Task<IReadOnlyList<ZonaPartida>> ReadAllAsync()
    {
        try
        {
            using var context = OpenSession();
            return await context.Session.Query<ZonaPartida>().ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading all {Entity} entities.", EntityName);
            throw new DataException($"Error reading all {EntityName} entities.", ex);
        }
    }

    public async Task<ZonaPartida?> ReadAsync(object id)
    {
        try
        {
            using var context = OpenSession();
            return await context.Session.GetAsync<ZonaPartida>(id);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading {Entity} entity with id {EntityId}.", EntityName, id);
            throw new DataException($"Error reading {EntityName} entity.", ex);
        }
    }

    public async Task SaveAsync(ZonaPartida entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Save",
            session => session.SaveOrUpdateAsync(entity),
            $"Error saving {EntityName} entity.");
    }

    public async Task DeleteAsync(ZonaPartida entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Delete",
            session => session.DeleteAsync(entity),
            $"Error deleting {EntityName} entity.");
    }


}