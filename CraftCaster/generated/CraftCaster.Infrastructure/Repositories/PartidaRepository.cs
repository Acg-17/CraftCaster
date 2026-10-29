using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;
using CraftCaster.Application.Abstractions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;
using CraftCaster.Domain.ValueObjects;
using CraftCaster.Infrastructure.Exceptions;

namespace CraftCaster.Infrastructure.Repositories;

public sealed class PartidaRepository : RepositoryBase<Partida>, IPartidaRepository
{
    public PartidaRepository(ILogger<PartidaRepository>? logger = null)
        : base(logger)
    {
    }

    public PartidaRepository(ISession session, ILogger<PartidaRepository>? logger = null)
        : base(session, logger)
    {
    }

    public PartidaRepository(ISessionFactory sessionFactory, ILogger<PartidaRepository>? logger = null)
        : base(sessionFactory, logger)
    {
    }

    public async Task<IReadOnlyList<Partida>> ReadAllAsync()
    {
        try
        {
            using var context = OpenSession();
            return await context.Session.Query<Partida>().ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading all {Entity} entities.", EntityName);
            throw new DataException($"Error reading all {EntityName} entities.", ex);
        }
    }

    public async Task<Partida?> ReadAsync(object id)
    {
        try
        {
            using var context = OpenSession();
            return await context.Session.GetAsync<Partida>(id);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading {Entity} entity with id {EntityId}.", EntityName, id);
            throw new DataException($"Error reading {EntityName} entity.", ex);
        }
    }

    public async Task SaveAsync(Partida entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Save",
            session => session.SaveOrUpdateAsync(entity),
            $"Error saving {EntityName} entity.");
    }

    public async Task DeleteAsync(Partida entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Delete",
            session => session.DeleteAsync(entity),
            $"Error deleting {EntityName} entity.");
    }

    public async Task<IReadOnlyList<Partida>> ObtenerPartidasUsuarioAsync(int p_oid)
    {
        try
        {
            using var context = OpenSession();
            return await context.Session.Query<Partida>()
                .Where(entity => entity.Usuario1.Id == p_oid || entity.Usuario2.Id == p_oid)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error executing ReadFilter Partida.obtenerPartidasUsuario.");
            throw new DataException("Error executing ReadFilter Partida.obtenerPartidasUsuario.", ex);
        }
    }
}