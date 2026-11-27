using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.UseCases_Partida_pasarTurno) ENABLED START*/
        // references to other libraries
        using System;
        using System.Linq;
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.UseCases.Partida;

public sealed partial class PartidaUseCase
{
    public async Task<string> PasarTurnoAsync(int p_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.UseCases_Partida_pasarTurno) ENABLED START*/
        // 1. Habilidades "Al terminar turno" de las criaturas del jugador activo
        // 2. Pasar el turno: crea el turno, da el maná, habilita el ataque de las criaturas y roba una carta
        // 3. Habilidades "Al empezar turno" de las criaturas del nuevo jugador activo
        // Las habilidades se ejecutan con Habilidad.ejecutar. Todo en una transacción. Devuelve el resumen de efectos.
        PartidaService partidaService = null!;
        JugadorPartidaService jugadorPartidaService = null!;
        string result = string.Empty;

        try
        {
            await _unitOfWork.BeginTransactionAsync("PartidaUseCase.PasarTurnoAsync");
            partidaService = new PartidaService(_unitOfWork);
            jugadorPartidaService = new JugadorPartidaService(_unitOfWork);
            var habilidadService = new HabilidadService(_unitOfWork);
            var resumen = new List<string>();

            // Comprobar existencia de partida
            var partida = await _unitOfWork.PartidaRepository.ReadAsync(p_oid) ??
                throw new ModelException($"No existe la partida de id {p_oid}.");
            // Comprobar partida no acabada
            if (partida.Estado != EstadoPartida.EnCurso)
                throw new ModelException($"La partida de id {p_oid} no se encuentra en curso.");
            // Comprobar dos jugadores
            if (partida.Jugadores.Count < 2)
                throw new ModelException("La partida necesita dos jugadores para pasar turno.");

            // Ejecuta las habilidades de un momento del turno de las criaturas en la mesa de un jugador
            async Task HabilidadesDeTurnoAsync(Domain.Entities.JugadorPartida jugador, TipoEstado momento)
            {
                var criaturas = jugador.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Campo)?.Cartas.ToList()
                    ?? new List<CartaPartida>();
                foreach (var criatura in criaturas)
                {
                    // Puede haber muerto por una habilidad anterior
                    if (criatura.Zona?.TipoZona != TipoZona.Campo) continue;
                    var habilidades = criatura.Original.Estados
                        .Where(e => e.Tipo == momento)
                        .SelectMany(e => e.Habilidades)
                        .Where(h => h.Tipo != TipoHabilidad.Defensa)
                        .ToList();
                    foreach (var habilidad in habilidades)
                    {
                        string efecto = await habilidadService.EjecutarAsync(habilidad.Id, criatura.Id, 0, false);
                        if (!string.IsNullOrWhiteSpace(efecto)) resumen.Add(efecto);
                    }
                }
            }

            var jugadorActivo = partida.Jugadores.FirstOrDefault(j => j.Activo);
            if (jugadorActivo != null)
            {
                // 1. Habilidades "Al terminar turno"
                await HabilidadesDeTurnoAsync(jugadorActivo, TipoEstado.FinTurno);

                // Si alguien se ha quedado sin vida no se pasa el turno: el controlador invoca Partida.finalizar
                if (partida.Jugadores.Any(j => j.Vida <= 0))
                {
                    await _unitOfWork.PartidaRepository.SaveAsync(partida);
                    await _unitOfWork.CommitAsync();
                    return string.Join(" ", resumen);
                }

                // 2. Pasar turno del jugador activo
                await jugadorPartidaService.PasarTurnoAsync(jugadorActivo.Id);
            }
            else
            {
                // No jugador activo es primer turno de partida
                Turno? ultimoTurno = partida.Turnos
                    .OrderByDescending(t => t.NumTurno)
                    .FirstOrDefault();
                // Determinar el siguiente jugador
                var siguiente = (ultimoTurno?.Jugador == null 
                    ? partida.Jugadores.FirstOrDefault(j => j.NumJugador == 1) 
                    : partida.Jugadores.FirstOrDefault(j => j.Id != ultimoTurno.Jugador.Id))
                    ?? throw new ModelException("No se pudo determinar el siguiente jugador.");
                int turnosPrevios = partida.Turnos.Count(t => t.Jugador != null && t.Jugador.Id == siguiente.Id);
                int manaTurno = 1 + turnosPrevios;
                // Crear un nuevo turno para el siguiente jugador
                Turno turno = new Turno
                {
                    Jugador = siguiente,
                    NumTurno = partida.Turnos.Count + 1,
                    ManaTurno = manaTurno,
                    Partida = partida,
                };
                await _unitOfWork.TurnoRepository.SaveAsync(turno);
                partida.Turnos.Add(turno);
                // Iniciar el turno del siguiente jugador
                siguiente.Activo = true;
                siguiente.Mana = manaTurno;
                // Habilitar ataque de criaturas en el campo del siguiente jugador
                var campo = siguiente.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Campo);
                if (campo?.Cartas != null)
                {
                    foreach (CartaPartida carta in campo.Cartas.Where(c => c.Original.Tipo == TipoCarta.Criatura))
                    {
                        carta.PuedeAtacar = true;
                        await _unitOfWork.CartaPartidaRepository.SaveAsync(carta);
                    }
                }
                await _unitOfWork.JugadorPartidaRepository.SaveAsync(siguiente);
                // Robar carta del mazo del siguiente jugador si tiene cartas disponibles
                var mazo = siguiente.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Mazo);
                if (mazo?.Cartas != null && mazo.Cartas.Count > 0)
                {
                    await jugadorPartidaService.RobarCartaDelMazoAsync(siguiente.Id);
                }
            }

            // 3. Habilidades "Al empezar turno" del nuevo jugador activo
            var nuevoActivo = partida.Jugadores.FirstOrDefault(j => j.Activo);
            if (nuevoActivo != null)
            {
                await HabilidadesDeTurnoAsync(nuevoActivo, TipoEstado.InicioTurno);
            }

            await _unitOfWork.PartidaRepository.SaveAsync(partida);
            result = string.Join(" ", resumen);
            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            if (ex is ModelException)
            {
                throw;
            }
            throw new ModelException("Application error while executing use case Partida.pasarTurno.", ex);
        }

        return result;
        /*PROTECTED REGION END*/
    }
}
