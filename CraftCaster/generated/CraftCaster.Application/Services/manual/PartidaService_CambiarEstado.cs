using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.Services_Partida_cambiarEstado) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.Services;

public sealed partial class PartidaService
{
    public async Task CambiarEstadoAsync(int p_oid, EstadoPartida p_estado)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.Services_Partida_cambiarEstado) ENABLED START*/
        // Write here your custom code...

        throw new ModelException("Application service generation for Partida.cambiarEstado is pending.", new NotImplementedException("Method CambiarEstadoAsync() not yet implemented."));
        /*PROTECTED REGION END*/
    }
}
