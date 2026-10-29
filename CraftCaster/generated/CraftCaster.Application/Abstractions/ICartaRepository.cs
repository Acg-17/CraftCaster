using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Application.Abstractions;

public interface ICartaRepository : IRepository<Carta>
{
    Task<IReadOnlyList<Carta>> ObtenerCartasUsuarioAsync(int p_oid);
    Task<IReadOnlyList<Carta>> ObtenerCartaPorNombreAsync(string p_nombre);
    Task<IReadOnlyList<Carta>> ObtenerCartaPorCosteManaAsync(int p_mana);
    Task<IReadOnlyList<Carta>> ObtenerCartaPorRarezaAsync(Rareza p_rareza);
    Task<IReadOnlyList<Carta>> ObtenerCartaPorTipoAsync(TipoCarta p_tipo);
}
