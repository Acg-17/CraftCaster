using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.Services_Partida_getCarta) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.Services;

public sealed partial class PartidaService
{
    public async Task<int> GetCartaAsync(int p_oid, int jugador, int zona, int carta)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.Services_Partida_getCarta) ENABLED START*/
        // Write here your custom code...
        Partida partida = await _repository.ReadAsync(p_oid) ?? throw new ModelException("No existe la partida indicada.");
        return partida.Jugadores[jugador].Zonas[zona].Cartas[carta].Id;

        throw new ModelException("Application service generation for Partida.getCarta is pending.", new NotImplementedException("Method GetCartaAsync() not yet implemented."));
        /*PROTECTED REGION END*/
    }
}
