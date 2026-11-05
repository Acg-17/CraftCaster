using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.Services_JugadorPartida_actualizarMana) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.Services;

public sealed partial class JugadorPartidaService
{
    public async Task ActualizarManaAsync(int p_oid, int p_mana)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.Services_JugadorPartida_actualizarMana) ENABLED START*/
        if (p_mana < 0)
        {
            throw new ModelException("El mana no puede ser negativo.");
        }

        JugadorPartida? jugador = await _repository.ReadAsync(p_oid);
        if (jugador == null)
        {
            throw new ModelException("No existe el jugador indicado.");
        }

        jugador.Mana = p_mana;
        await _repository.SaveAsync(jugador);
        /*PROTECTED REGION END*/
    }
}
