using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;
using CraftCaster.Application.Abstractions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;
using CraftCaster.Domain.ValueObjects;
using CraftCaster.Infrastructure.Exceptions;

namespace CraftCaster.Infrastructure.Repositories;

public sealed class AccionRepository : RepositoryBase<Accion>, IAccionRepository
{
    public AccionRepository(ILogger<AccionRepository>? logger = null)
        : base(logger)
    {
    }

    public AccionRepository(ISession session, ILogger<AccionRepository>? logger = null)
        : base(session, logger)
    {
    }

    public AccionRepository(ISessionFactory sessionFactory, ILogger<AccionRepository>? logger = null)
        : base(sessionFactory, logger)
    {
    }

    public async Task<IReadOnlyList<Accion>> ReadAllAsync()
    {
        try
        {
            using var context = OpenSession();
            return await context.Session.Query<Accion>().ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading all {Entity} entities.", EntityName);
            throw new DataException($"Error reading all {EntityName} entities.", ex);
        }
    }

    public async Task<Accion?> ReadAsync(object id)
    {
        try
        {
            using var context = OpenSession();
            return await context.Session.GetAsync<Accion>(id);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading {Entity} entity with id {EntityId}.", EntityName, id);
            throw new DataException($"Error reading {EntityName} entity.", ex);
        }
    }

    public async Task SaveAsync(Accion entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Save",
            session => session.SaveOrUpdateAsync(entity),
            $"Error saving {EntityName} entity.");
    }

    public async Task DeleteAsync(Accion entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Delete",
            session => session.DeleteAsync(entity),
            $"Error deleting {EntityName} entity.");
    }


}