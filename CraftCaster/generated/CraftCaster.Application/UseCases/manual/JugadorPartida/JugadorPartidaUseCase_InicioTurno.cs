using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.UseCases_JugadorPartida_inicioTurno) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.UseCases.JugadorPartida;

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

            Domain.Entities.JugadorPartida jugador = await _unitOfWork.JugadorPartidaRepository.ReadAsync(p_oid)
                ?? throw new ModelException("No existe el jugador indicado.");

            // 1. Establece activo en true
            jugador.Activo = true;

            // 2. Regenera y aumenta el mana (maximo 10) segun los turnos jugados
            int turnosPrevios = jugador.Partida?.Turnos?.Count(t => t.Jugador != null && t.Jugador.Id == jugador.Id) ?? 0;
            jugador.Mana = Math.Min(10, 1 + turnosPrevios);

            // 3. Reactiva el ataque para todas las criaturas en su zona de Campo
            Domain.Entities.ZonaPartida? zonaCampo = jugador.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Campo);
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
