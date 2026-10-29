using CraftCaster.Domain.Entities;

namespace CraftCaster.Application.Abstractions;

public interface IPartidaRepository : IRepository<Partida>
{
    Task<IReadOnlyList<Partida>> ObtenerPartidasUsuarioAsync(int p_oid);
}
