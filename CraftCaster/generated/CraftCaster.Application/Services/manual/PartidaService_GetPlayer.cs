using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.Services_Partida_getPlayer) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.Services;

public sealed partial class PartidaService
{
    public async Task<int> GetPlayerAsync(int p_oid, int index)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.Services_Partida_getPlayer) ENABLED START*/
        // Write here your custom code...
        Partida partida = await _repository.ReadAsync(p_oid) ?? throw new ModelException("No existe la partida indicada.");
        return partida.Jugadores[index].Id;

        /*PROTECTED REGION END*/
    }
}
