using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;
using CraftCaster.Application.Abstractions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;
using CraftCaster.Domain.ValueObjects;
using CraftCaster.Infrastructure.Exceptions;

namespace CraftCaster.Infrastructure.Repositories;

public sealed class MazoRepository : RepositoryBase<Mazo>, IMazoRepository
{
    public MazoRepository(ILogger<MazoRepository>? logger = null)
        : base(logger)
    {
    }

    public MazoRepository(ISession session, ILogger<MazoRepository>? logger = null)
        : base(session, logger)
    {
    }

    public MazoRepository(ISessionFactory sessionFactory, ILogger<MazoRepository>? logger = null)
        : base(sessionFactory, logger)
    {
    }

    public async Task<IReadOnlyList<Mazo>> ReadAllAsync()
    {
        try
        {
            using var context = OpenSession();
            return await context.Session.Query<Mazo>().ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading all {Entity} entities.", EntityName);
            throw new DataException($"Error reading all {EntityName} entities.", ex);
        }
    }

    public async Task<Mazo?> ReadAsync(object id)
    {
        try
        {
            using var context = OpenSession();
            return await context.Session.GetAsync<Mazo>(id);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading {Entity} entity with id {EntityId}.", EntityName, id);
            throw new DataException($"Error reading {EntityName} entity.", ex);
        }
    }

    public async Task SaveAsync(Mazo entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Save",
            session => session.SaveOrUpdateAsync(entity),
            $"Error saving {EntityName} entity.");
    }

    public async Task DeleteAsync(Mazo entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Delete",
            session => session.DeleteAsync(entity),
            $"Error deleting {EntityName} entity.");
    }

    public async Task<IReadOnlyList<Mazo>> ObtenerMazosUsuarioAsync(int p_oid)
    {
        try
        {
            using var context = OpenSession();
            return await context.Session.Query<Mazo>()
                .Where(entity => entity.Usuario.Id == p_oid)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error executing ReadFilter Mazo.obtenerMazosUsuario.");
            throw new DataException("Error executing ReadFilter Mazo.obtenerMazosUsuario.", ex);
        }
    }

    public async Task<IReadOnlyList<Mazo>> ObtenerMazosPorNombreAsync(string p_nombre)
    {
        try
        {
            using var context = OpenSession();
            return await context.Session.Query<Mazo>()
                .Where(entity => entity.Nombre == p_nombre && entity.EsPublico == true)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error executing ReadFilter Mazo.obtenerMazosPorNombre.");
            throw new DataException("Error executing ReadFilter Mazo.obtenerMazosPorNombre.", ex);
        }
    }

    public async Task<IReadOnlyList<Mazo>> ObtenerMazoPorCosteMedioAsync(int p_min_mana, int p_max_mana)
    {
        try
        {
            using var context = OpenSession();
            return await context.Session.Query<Mazo>()
                .Where(entity => entity.CosteMedio >= p_min_mana && entity.CosteMedio <= p_max_mana && entity.EsPublico == true)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error executing ReadFilter Mazo.obtenerMazoPorCosteMedio.");
            throw new DataException("Error executing ReadFilter Mazo.obtenerMazoPorCosteMedio.", ex);
        }
    }

    public async Task<IReadOnlyList<Mazo>> ObtenerMazoPorValoracionAsync(int p_min_val, int p_max_val)
    {
        try
        {
            using var context = OpenSession();
            return await context.Session.Query<Mazo>()
                .Where(entity => entity.Valoracion >= p_min_val && entity.Valoracion <= p_max_val && entity.EsPublico == true)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error executing ReadFilter Mazo.obtenerMazoPorValoracion.");
            throw new DataException("Error executing ReadFilter Mazo.obtenerMazoPorValoracion.", ex);
        }
    }

    public async Task<IReadOnlyList<Mazo>> ObtenerMazoPorCreadorAsync(string p_creador)
    {
        try
        {
            using var context = OpenSession();
            return await context.Session.Query<Mazo>()
                .Where(entity => entity.Creador == p_creador && entity.EsPublico == true)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error executing ReadFilter Mazo.obtenerMazoPorCreador.");
            throw new DataException("Error executing ReadFilter Mazo.obtenerMazoPorCreador.", ex);
        }
    }

    public async Task<IReadOnlyList<Mazo>> ObtenerMazoJugador1Async(int p_oid)
    {
        try
        {
            using var context = OpenSession();
            return await context.Session.Query<Mazo>()
                .Where(entity => entity.PartidasLocal.Any(p => p.Id == p_oid))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error executing ReadFilter Mazo.obtenerMazoJugador1.");
            throw new DataException("Error executing ReadFilter Mazo.obtenerMazoJugador1.", ex);
        }
    }

    public async Task<IReadOnlyList<Mazo>> ObtenerMazoJugador2Async(int p_oid)
    {
        try
        {
            using var context = OpenSession();
            return await context.Session.Query<Mazo>()
                .Where(entity => entity.PartidasVisitante.Any(p => p.Id == p_oid))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error executing ReadFilter Mazo.obtenerMazoJugador2.");
            throw new DataException("Error executing ReadFilter Mazo.obtenerMazoJugador2.", ex);
        }
    }
}