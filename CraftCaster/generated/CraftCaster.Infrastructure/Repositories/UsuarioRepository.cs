using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;
using CraftCaster.Application.Abstractions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;
using CraftCaster.Domain.ValueObjects;
using CraftCaster.Infrastructure.Exceptions;

namespace CraftCaster.Infrastructure.Repositories;

public sealed class UsuarioRepository : RepositoryBase<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(ILogger<UsuarioRepository>? logger = null)
        : base(logger)
    {
    }

    public UsuarioRepository(ISession session, ILogger<UsuarioRepository>? logger = null)
        : base(session, logger)
    {
    }

    public UsuarioRepository(ISessionFactory sessionFactory, ILogger<UsuarioRepository>? logger = null)
        : base(sessionFactory, logger)
    {
    }

    public async Task<IReadOnlyList<Usuario>> ReadAllAsync()
    {
        try
        {
            using var context = OpenSession();
            return await context.Session.Query<Usuario>().ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading all {Entity} entities.", EntityName);
            throw new DataException($"Error reading all {EntityName} entities.", ex);
        }
    }

    public async Task<Usuario?> ReadAsync(object id)
    {
        try
        {
            using var context = OpenSession();
            return await context.Session.GetAsync<Usuario>(id);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading {Entity} entity with id {EntityId}.", EntityName, id);
            throw new DataException($"Error reading {EntityName} entity.", ex);
        }
    }

    public async Task SaveAsync(Usuario entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Save",
            session => session.SaveOrUpdateAsync(entity),
            $"Error saving {EntityName} entity.");
    }

    public async Task DeleteAsync(Usuario entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Delete",
            session => session.DeleteAsync(entity),
            $"Error deleting {EntityName} entity.");
    }

    public async Task<IReadOnlyList<Usuario>> ObtenerUsuarioLocalAsync(int p_oid)
    {
        try
        {
            using var context = OpenSession();
            return await context.Session.Query<Usuario>()
                .Where(entity => entity.PartidasLocal.Any(p => p.Id == p_oid))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error executing ReadFilter Usuario.obtenerUsuarioLocal.");
            throw new DataException("Error executing ReadFilter Usuario.obtenerUsuarioLocal.", ex);
        }
    }
}