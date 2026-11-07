using System.Linq;
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
        Partida partida = await _repository.ReadAsync(p_oid) 
            ?? throw new ModelException("No existe la partida indicada.");

        if (partida.Jugadores == null || partida.Jugadores.Count == 0)
        {
            throw new ModelException("La partida no tiene jugadores registrados.");
        }

        // Si el jugador 1 está activo devuelve 0, en caso contrario 1
        JugadorPartida jugador1 = partida.Jugadores.FirstOrDefault(j => j.NumJugador == 1) 
            ?? partida.Jugadores[0];

        return jugador1.Activo ? 0 : 1;
        /*PROTECTED REGION END*/
    }
}
