using System.Linq;
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
        Partida partida = await _repository.ReadAsync(p_oid) 
            ?? throw new ModelException("No existe la partida indicada.");

        if (partida.Jugadores == null || partida.Jugadores.Count == 0)
        {
            throw new ModelException("La partida no tiene jugadores registrados.");
        }

        // Búsqueda del jugador por índice (0 o 1) o por NumJugador (1 o 2)
        JugadorPartida? j = (jugador >= 0 && jugador < partida.Jugadores.Count)
            ? partida.Jugadores[jugador]
            : partida.Jugadores.FirstOrDefault(x => x.NumJugador == jugador);

        if (j == null)
        {
            throw new ModelException($"Jugador no encontrado para el parametro {jugador}.");
        }

        // Búsqueda de la zona por enum TipoZona o por índice
        ZonaPartida? z = j.Zonas.FirstOrDefault(x => (int)x.TipoZona == zona)
            ?? (zona >= 0 && zona < j.Zonas.Count ? j.Zonas[zona] : null);

        if (z == null)
        {
            throw new ModelException($"Zona no encontrada para el parametro {zona}.");
        }

        if (z.Cartas == null || carta < 0 || carta >= z.Cartas.Count)
        {
            throw new ModelException($"No existe ninguna carta en la posicion {carta} de la zona indicada.");
        }

        return z.Cartas[carta].Id;
        /*PROTECTED REGION END*/
    }
}
