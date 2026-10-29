using CraftCaster.Domain.Entities;

namespace CraftCaster.Application.Abstractions;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<IReadOnlyList<Usuario>> ObtenerUsuarioLocalAsync(int p_oid);
}
