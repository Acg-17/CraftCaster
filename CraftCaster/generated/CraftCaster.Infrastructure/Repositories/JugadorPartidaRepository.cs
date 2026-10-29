using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;
using CraftCaster.Application.Abstractions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;
using CraftCaster.Domain.ValueObjects;
using CraftCaster.Infrastructure.Exceptions;

namespace CraftCaster.Infrastructure.Repositories;

public sealed class JugadorPartidaRepository : RepositoryBase<JugadorPartida>, IJugadorPartidaRepository
{
    public JugadorPartidaRepository(ILogger<JugadorPartidaRepository>? logger = null)
        : base(logger)
    {
    }

    public JugadorPartidaRepository(ISession session, ILogger<JugadorPartidaRepository>? logger = null)
        : base(session, logger)
    {
    }

    public JugadorPartidaRepository(ISessionFactory sessionFactory, ILogger<JugadorPartidaRepository>? logger = null)
        : base(sessionFactory, logger)
    {
    }

    public async Task<IReadOnlyList<JugadorPartida>> ReadAllAsync()
    {
        try
        {
            using var context = OpenSession();
            return await context.Session.Query<JugadorPartida>().ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading all {Entity} entities.", EntityName);
            throw new DataException($"Error reading all {EntityName} entities.", ex);
        }
    }

    public async Task<JugadorPartida?> ReadAsync(object id)
    {
        try
        {
            using var context = OpenSession();
            return await context.Session.GetAsync<JugadorPartida>(id);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading {Entity} entity with id {EntityId}.", EntityName, id);
            throw new DataException($"Error reading {EntityName} entity.", ex);
        }
    }

    public async Task SaveAsync(JugadorPartida entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Save",
            session => session.SaveOrUpdateAsync(entity),
            $"Error saving {EntityName} entity.");
    }

    public async Task DeleteAsync(JugadorPartida entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Delete",
            session => session.DeleteAsync(entity),
            $"Error deleting {EntityName} entity.");
    }


}