using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.Services_Partida_finalizar) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.Services;

public sealed partial class PartidaService
{
    public async Task FinalizarAsync(int p_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.Services_Partida_finalizar) ENABLED START*/
        Partida partida = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("No existe la partida indicada.");

        partida.Estado = EstadoPartida.Finalizada;
        await _repository.SaveAsync(partida);
        /*PROTECTED REGION END*/
    }
}
