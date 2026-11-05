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
       Partida? partida = await _repository.ReadAsync(p_oid);
        if (partida == null)
        {
            throw new ModelException("No existe la partida indicada.");
        }

        if (partida.Estado == EstadoPartida.Finalizada)
        {
            throw new ModelException("La partida ya ha finalizado y no puede cambiar de estado.");
        }
        if (p_estado == partida.Estado)
        {
            throw new ModelException("La partida ya se encuentra en el estado indicado.");
        }
        
        // El ciclo de vida solo avanza: Espera -> EnCurso -> Finalizada
        if ((int)p_estado < (int)partida.Estado)
        {
            throw new ModelException("No se puede volver a un estado anterior de la partida.");
        }

        partida.Estado = p_estado;
        await _repository.SaveAsync(partida);
        /*PROTECTED REGION END*/
    }
}
