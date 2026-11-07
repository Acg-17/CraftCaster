using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.Services_JugadorPartida_pasarTurno) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.Services;

public sealed partial class JugadorPartidaService
{
    public async Task PasarTurnoAsync(int p_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.Services_JugadorPartida_pasarTurno) ENABLED START*/
        
        const int manaMaximo = 10;

        // Comprobar que el jugador existe
        JugadorPartida jugador = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("No existe el jugador indicado.");

        // Comprobar que el jugador pertenece a una partida
        Partida partida = jugador.Partida
            ?? throw new ModelException("El jugador no pertenece a ninguna partida.");

        // Comprobar que la partida está en curso
        if (partida.Estado != EstadoPartida.EnCurso)
            throw new ModelException("Solo se puede pasar turno en una partida en curso.");

        // Comprobar que el jugador es el activo
        if (!jugador.Activo)
            throw new ModelException("Solo el jugador activo puede pasar el turno.");

        // Comprobar que la partida tiene un rival
        JugadorPartida rival = partida.Jugadores.FirstOrDefault(j => j.Id != jugador.Id)
            ?? throw new ModelException("La partida no tiene rival al que pasar el turno.");

        jugador.Activo = false;

        // Comprobar si el jugador tiene cartas en el campo
        ZonaPartida? campoJugador = jugador.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Campo);
        if (campoJugador?.Cartas != null)
        {
            foreach (CartaPartida carta in campoJugador.Cartas)
            {
                carta.PuedeAtacar = false;
                await _unitOfWork.CartaPartidaRepository.SaveAsync(carta);
            }
        }
        await _repository.SaveAsync(jugador);

        int turnosPreviosRival = partida.Turnos.Count(t => t.Jugador != null && t.Jugador.Id == rival.Id);
        int manaTurno = Math.Min(manaMaximo, turnosPreviosRival + 1);

        // Crear un nuevo turno para el rival
        Turno turno = new Turno
        {
            NumTurno = partida.Turnos.Count + 1,
            ManaTurno = manaTurno,
            Partida = partida,
            Jugador = rival,
        };
        await _unitOfWork.TurnoRepository.SaveAsync(turno);
        partida.Turnos.Add(turno);

        // Comprobar si el rival tiene un turno activo
        rival.Activo = true;
        rival.Mana = manaTurno;

        // Comprobar si el rival tiene cartas en el campo
        ZonaPartida? campoRival = rival.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Campo);
        if (campoRival?.Cartas != null)
        {
            foreach (CartaPartida carta in campoRival.Cartas)
            {
                carta.PuedeAtacar = true;
                await _unitOfWork.CartaPartidaRepository.SaveAsync(carta);
            }
        }
        await _repository.SaveAsync(rival);

        // Comprobar si el mazo del rival tiene cartas para robar
        ZonaPartida? mazoRival = rival.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Mazo);
        if (mazoRival?.Cartas != null && mazoRival.Cartas.Count > 0)
            await RobarCartaAsync(rival.Id);
            
        /*PROTECTED REGION END*/
    }
}
