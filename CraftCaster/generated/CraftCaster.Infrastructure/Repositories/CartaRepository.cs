using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;
using CraftCaster.Application.Abstractions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;
using CraftCaster.Domain.ValueObjects;
using CraftCaster.Infrastructure.Exceptions;

namespace CraftCaster.Infrastructure.Repositories;

public sealed class CartaRepository : RepositoryBase<Carta>, ICartaRepository
{
    public CartaRepository(ILogger<CartaRepository>? logger = null)
        : base(logger)
    {
    }

    public CartaRepository(ISession session, ILogger<CartaRepository>? logger = null)
        : base(session, logger)
    {
    }

    public CartaRepository(ISessionFactory sessionFactory, ILogger<CartaRepository>? logger = null)
        : base(sessionFactory, logger)
    {
    }

    public async Task<IReadOnlyList<Carta>> ReadAllAsync()
    {
        try
        {
            using var context = OpenSession();
            return await context.Session.Query<Carta>().ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading all {Entity} entities.", EntityName);
            throw new DataException($"Error reading all {EntityName} entities.", ex);
        }
    }

    public async Task<Carta?> ReadAsync(object id)
    {
        try
        {
            using var context = OpenSession();
            return await context.Session.GetAsync<Carta>(id);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading {Entity} entity with id {EntityId}.", EntityName, id);
            throw new DataException($"Error reading {EntityName} entity.", ex);
        }
    }

    public async Task SaveAsync(Carta entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Save",
            session => session.SaveOrUpdateAsync(entity),
            $"Error saving {EntityName} entity.");
    }

    public async Task DeleteAsync(Carta entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Delete",
            session => session.DeleteAsync(entity),
            $"Error deleting {EntityName} entity.");
    }

    public async Task<IReadOnlyList<Carta>> ObtenerCartasUsuarioAsync(int p_oid)
    {
        try
        {
            using var context = OpenSession();
            return await context.Session.Query<Carta>()
                .Where(entity => entity.Usuario.Id == p_oid)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error executing ReadFilter Carta.obtenerCartasUsuario.");
            throw new DataException("Error executing ReadFilter Carta.obtenerCartasUsuario.", ex);
        }
    }

    public async Task<IReadOnlyList<Carta>> ObtenerCartaPorNombreAsync(string p_nombre)
    {
        try
        {
            using var context = OpenSession();
            return await context.Session.Query<Carta>()
                .Where(entity => entity.Nombre == p_nombre && entity.EsPublica == true)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error executing ReadFilter Carta.obtenerCartaPorNombre.");
            throw new DataException("Error executing ReadFilter Carta.obtenerCartaPorNombre.", ex);
        }
    }

    public async Task<IReadOnlyList<Carta>> ObtenerCartaPorCosteManaAsync(int p_mana)
    {
        try
        {
            using var context = OpenSession();
            return await context.Session.Query<Carta>()
                .Where(entity => entity.CosteMana == p_mana && entity.EsPublica == true)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error executing ReadFilter Carta.obtenerCartaPorCosteMana.");
            throw new DataException("Error executing ReadFilter Carta.obtenerCartaPorCosteMana.", ex);
        }
    }

    public async Task<IReadOnlyList<Carta>> ObtenerCartaPorRarezaAsync(Rareza p_rareza)
    {
        try
        {
            using var context = OpenSession();
            return await context.Session.Query<Carta>()
                .Where(entity => entity.Rareza == p_rareza && entity.EsPublica == true)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error executing ReadFilter Carta.obtenerCartaPorRareza.");
            throw new DataException("Error executing ReadFilter Carta.obtenerCartaPorRareza.", ex);
        }
    }

    public async Task<IReadOnlyList<Carta>> ObtenerCartaPorTipoAsync(TipoCarta p_tipo)
    {
        try
        {
            using var context = OpenSession();
            return await context.Session.Query<Carta>()
                .Where(entity => entity.Tipo == p_tipo && entity.EsPublica == true)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error executing ReadFilter Carta.obtenerCartaPorTipo.");
            throw new DataException("Error executing ReadFilter Carta.obtenerCartaPorTipo.", ex);
        }
    }
}