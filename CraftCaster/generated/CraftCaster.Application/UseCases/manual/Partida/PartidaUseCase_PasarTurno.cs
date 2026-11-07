using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.UseCases_Partida_pasarTurno) ENABLED START*/
// references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.UseCases.Partida;

public sealed partial class PartidaUseCase
{
    public async Task PasarTurnoAsync(int p_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.UseCases_Partida_pasarTurno) ENABLED START*/
        // Resolve external infrastructure services with _utils!.GetRequiredService<TService>() when this use case receives IUnitOfWorkUtils.
        PartidaService partidaService = null!;
        JugadorPartidaService jugadorPartidaService = null!;

        try
        {
            await _unitOfWork.BeginTransactionAsync("PartidaUseCase.PasarTurnoAsync");

            partidaService = new PartidaService(_unitOfWork);
            jugadorPartidaService = new JugadorPartidaService(_unitOfWork);

            // Comprobar existencia de partida
            var partida = await _unitOfWork.PartidaRepository.ReadAsync(p_oid) ??
                throw new ModelException($"No existe la partida de id {p_oid}.");

            // Comprobar partida no acabada
            if (partida.Estado != EstadoPartida.EnCurso)
                throw new ModelException($"La partida de id {p_oid} no se encuentra en curso.");

            // Comprobar dos jugadores
            if (partida.Jugadores.Count < 2)
                throw new ModelException("La partida necesita dos jugadores para pasar turno.");

            var jugadorActivo = partida.Jugadores.FirstOrDefault(j => j.Activo);

            // Pasar turno del jugador activo
            if (jugadorActivo != null)
            {
                await jugadorPartidaService.PasarTurnoAsync(jugadorActivo.Id);
            }
            else
            {
                // No jugador activo es primer turno de partida
                Turno? ultimoTurno = partida.Turnos
                    .OrderByDescending(t => t.NumTurno)
                    .FirstOrDefault();

                // Determinar el siguiente jugador
                var siguiente = (ultimoTurno == null ? partida.Jugadores.FirstOrDefault(j => j.NumJugador == 1) : partida.Jugadores.FirstOrDefault(j => j.Id != ultimoTurno.Jugador.Id))
                    ?? throw new ModelException("No se pudo determinar el siguiente jugador.");

                int turnosPrevios = partida.Turnos.Count(t => t.Jugador.Id == siguiente.Id);
                int manaTurno = Math.Min(10, 1 + turnosPrevios);

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
                    await jugadorPartidaService.RobarCartaAsync(siguiente.Id);
                }
            }

            await _unitOfWork.PartidaRepository.SaveAsync(partida);
            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            throw new ModelException("Application error while executing use case Partida.pasarTurno.", ex);
        }
        /*PROTECTED REGION END*/
    }
}
