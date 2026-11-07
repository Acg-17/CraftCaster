using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.Services_JugadorPartida_actualizarVida) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.Services;

public sealed partial class JugadorPartidaService
{
    public async Task ActualizarVidaAsync(int p_oid, int p_vida)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.Services_JugadorPartida_actualizarVida) ENABLED START*/
        JugadorPartida? jugador = await _repository.ReadAsync(p_oid);
        if (jugador == null)
        {
            throw new ModelException("No existe el jugador indicado.");
        }


        jugador.Vida = p_vida;
        await _repository.SaveAsync(jugador);
        /*PROTECTED REGION END*/
    }
}
