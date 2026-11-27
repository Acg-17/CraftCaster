using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.UseCases_Partida_empezar) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.UseCases.Partida;

public sealed partial class PartidaUseCase
{
    public async Task<bool> EmpezarAsync(int p_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.UseCases_Partida_empezar) ENABLED START*/
        // Si la partida aún no ha empezado: reparte la mano inicial a cada jugador (con robarCartaDelMazo)
        // y da el primer turno al jugador 1 (maná 1 y roba una carta). Todo en una transacción.
        // Devuelve false si la partida ya había empezado (no hace nada).
        const int cartasInicialesMano = 4;
        JugadorPartidaService jugadorPartidaService = null!;
        bool result = false;

        try
        {
            await _unitOfWork.BeginTransactionAsync("PartidaUseCase.EmpezarAsync");
            jugadorPartidaService = new JugadorPartidaService(_unitOfWork);

            var partida = await _unitOfWork.PartidaRepository.ReadAsync(p_oid) ??
                throw new ModelException($"No existe la partida de id {p_oid}.");
            if (partida.Estado != EstadoPartida.EnCurso)
                throw new ModelException($"La partida de id {p_oid} no se encuentra en curso.");
            if (partida.Jugadores.Count < 2)
                throw new ModelException("La partida necesita dos jugadores para empezar.");

            if (partida.Turnos.Count == 0 && !partida.Jugadores.Any(j => j.Activo))
            {
                // Mano inicial de cada jugador
                foreach (var jugador in partida.Jugadores.OrderBy(j => j.NumJugador))
                {
                    var mano = jugador.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Mano);
                    var mazo = jugador.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Mazo);
                    if (mano == null || mazo == null || mano.Cartas.Count > 0) continue;

                    int porRobar = Math.Min(cartasInicialesMano, mazo.Cartas.Count);
                    for (int i = 0; i < porRobar; i++)
                    {
                        await jugadorPartidaService.RobarCartaDelMazoAsync(jugador.Id);
                    }
                }

                // Primer turno para el jugador 1
                var primero = partida.Jugadores.FirstOrDefault(j => j.NumJugador == 1) ??
                    throw new ModelException("La partida no tiene jugador 1.");
                Turno turno = new Turno
                {
                    Jugador = primero,
                    NumTurno = 1,
                    ManaTurno = 1,
                    Partida = partida,
                };
                await _unitOfWork.TurnoRepository.SaveAsync(turno);
                partida.Turnos.Add(turno);
                primero.Activo = true;
                primero.Mana = 1;
                await _unitOfWork.JugadorPartidaRepository.SaveAsync(primero);

                var mazoPrimero = primero.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Mazo);
                if (mazoPrimero?.Cartas != null && mazoPrimero.Cartas.Count > 0)
                {
                    await jugadorPartidaService.RobarCartaDelMazoAsync(primero.Id);
                }

                await _unitOfWork.PartidaRepository.SaveAsync(partida);
                result = true;
            }

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            if (ex is ModelException)
            {
                throw;
            }
            throw new ModelException("Application error while executing use case Partida.empezar.", ex);
        }

        return result;
        /*PROTECTED REGION END*/
    }
}
