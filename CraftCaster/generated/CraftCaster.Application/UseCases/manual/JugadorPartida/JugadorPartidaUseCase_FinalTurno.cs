using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.UseCases_JugadorPartida_finalTurno) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.UseCases.JugadorPartida;

public sealed partial class JugadorPartidaUseCase
{
    public async Task FinalTurnoAsync(int p_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.UseCases_JugadorPartida_finalTurno) ENABLED START*/
        // Resolve external infrastructure services with _utils!.GetRequiredService<TService>() when this use case receives IUnitOfWorkUtils.
        JugadorPartidaService jugadorPartidaService = null!;

        try
        {
            await _unitOfWork.BeginTransactionAsync("JugadorPartidaUseCase.FinalTurnoAsync");
            jugadorPartidaService = new JugadorPartidaService(_unitOfWork);

            Domain.Entities.JugadorPartida jugador = await _unitOfWork.JugadorPartidaRepository.ReadAsync(p_oid)
                ?? throw new ModelException("No existe el jugador indicado.");

            // 1. Establece activo en false
            jugador.Activo = false;

            // 2. Desactiva la posibilidad de atacar para sus cartas en campo hasta el siguiente turno
            Domain.Entities.ZonaPartida? zonaCampo = jugador.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Campo);
            if (zonaCampo?.Cartas != null)
            {
                foreach (var carta in zonaCampo.Cartas)
                {
                    carta.PuedeAtacar = false;
                    await _unitOfWork.CartaPartidaRepository.SaveAsync(carta);
                }
            }

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
            throw new ModelException("Application error while executing use case JugadorPartida.finalTurno.", ex);
        }
        /*PROTECTED REGION END*/
    }
}
