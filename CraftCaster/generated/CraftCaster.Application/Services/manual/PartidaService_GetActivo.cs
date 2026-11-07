using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.Services_Partida_getActivo) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.Services;

public sealed partial class PartidaService
{
    public async Task<int> GetActivoAsync(int p_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.Services_Partida_getActivo) ENABLED START*/
        // Write here your custom code...
        Partida partida = await _repository.ReadAsync(p_oid) ?? throw new ModelException("No existe la partida indicada.");
        return partida.Jugadores[0].Activo ? 0 : 1;
        /*PROTECTED REGION END*/
    }
}
