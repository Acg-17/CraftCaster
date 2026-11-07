using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.Services_Partida_getPlayer) ENABLED START*/
        // references to other libraries
        using System.Linq;
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.Services;

public sealed partial class PartidaService
{
    public async Task<int> GetPlayerAsync(int p_oid, int index)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.Services_Partida_getPlayer) ENABLED START*/
        Partida partida = await _repository.ReadAsync(p_oid) 
            ?? throw new ModelException("No existe la partida indicada.");

        if (partida.Jugadores == null || partida.Jugadores.Count == 0)
        {
            throw new ModelException("La partida no tiene jugadores registrados.");
        }

        // Se localiza por NumJugador si index corresponde al número de jugador (1 o 2), o por posición de lista
        JugadorPartida? jugador = partida.Jugadores.FirstOrDefault(j => j.NumJugador == (index + 1))
            ?? (index >= 0 && index < partida.Jugadores.Count ? partida.Jugadores[index] : null);

        if (jugador == null)
        {
            throw new ModelException($"No se encontro ningun jugador para el indice o numero {index}.");
        }

        return jugador.Id;
        /*PROTECTED REGION END*/
    }
}
