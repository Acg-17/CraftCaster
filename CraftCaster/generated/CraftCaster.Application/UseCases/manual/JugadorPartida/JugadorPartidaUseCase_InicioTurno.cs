using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.UseCases_JugadorPartida_inicioTurno) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.UseCases.JugadorPartida;

using Usuario = CraftCaster.Domain.Entities.Usuario;
using Partida = CraftCaster.Domain.Entities.Partida;
using JugadorPartida = CraftCaster.Domain.Entities.JugadorPartida;
using ZonaPartida = CraftCaster.Domain.Entities.ZonaPartida;
using CartaPartida = CraftCaster.Domain.Entities.CartaPartida;
using Carta = CraftCaster.Domain.Entities.Carta;
using Mazo = CraftCaster.Domain.Entities.Mazo;
using Estado = CraftCaster.Domain.Entities.Estado;
using Habilidad = CraftCaster.Domain.Entities.Habilidad;
using Turno = CraftCaster.Domain.Entities.Turno;
using Accion = CraftCaster.Domain.Entities.Accion;


public sealed partial class JugadorPartidaUseCase
{
    public async Task InicioTurnoAsync(int p_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.UseCases_JugadorPartida_inicioTurno) ENABLED START*/
        // Resolve external infrastructure services with _utils!.GetRequiredService<TService>() when this use case receives IUnitOfWorkUtils.
        JugadorPartidaService jugadorPartidaService = null!;

        try
        {
            await _unitOfWork.BeginTransactionAsync("JugadorPartidaUseCase.InicioTurnoAsync");
            jugadorPartidaService = new JugadorPartidaService(_unitOfWork);

            JugadorPartida jugador = await _unitOfWork.JugadorPartidaRepository.ReadAsync(p_oid)
                ?? throw new ModelException("No existe el jugador indicado.");

            // 1. Establece activo en true
            jugador.Activo = true;

            // 2. Regenera y aumenta el mana (maximo 10) segun los turnos jugados
            int turnosPrevios = jugador.Partida?.Turnos?.Count(t => t.Jugador != null && t.Jugador.Id == jugador.Id) ?? 0;
            jugador.Mana = Math.Min(10, 1 + turnosPrevios);

            // 3. Reactiva el ataque para todas las criaturas en su zona de Campo
            ZonaPartida? zonaCampo = jugador.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Campo);
            if (zonaCampo?.Cartas != null)
            {
                foreach (var carta in zonaCampo.Cartas)
                {
                    carta.PuedeAtacar = true;
                    await _unitOfWork.CartaPartidaRepository.SaveAsync(carta);
                }
            }

            // 4. Roba una carta del mazo al iniciar el turno
            await jugadorPartidaService.RobarCartaAsync(jugador.Id);

            await _unitOfWork.JugadorPartidaRepository.SaveAsync(jugador);

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            if (ex is ModelException)
            {
                throw;
            }
            throw new ModelException("Application error while executing use case JugadorPartida.inicioTurno.", ex);
        }
        /*PROTECTED REGION END*/
    }
}
