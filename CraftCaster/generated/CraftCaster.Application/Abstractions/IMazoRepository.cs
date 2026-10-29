using CraftCaster.Domain.Entities;

namespace CraftCaster.Application.Abstractions;

public interface IMazoRepository : IRepository<Mazo>
{
    Task<IReadOnlyList<Mazo>> ObtenerMazosUsuarioAsync(int p_oid);
    Task<IReadOnlyList<Mazo>> ObtenerMazosPorNombreAsync(string p_nombre);
    Task<IReadOnlyList<Mazo>> ObtenerMazoPorCosteMedioAsync(int p_min_mana, int p_max_mana);
    Task<IReadOnlyList<Mazo>> ObtenerMazoPorValoracionAsync(int p_min_val, int p_max_val);
    Task<IReadOnlyList<Mazo>> ObtenerMazoPorCreadorAsync(string p_creador);
    Task<IReadOnlyList<Mazo>> ObtenerMazoJugador1Async(int p_oid);
    Task<IReadOnlyList<Mazo>> ObtenerMazoJugador2Async(int p_oid);
}
