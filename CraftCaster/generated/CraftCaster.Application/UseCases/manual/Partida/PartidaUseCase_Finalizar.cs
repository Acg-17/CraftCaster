using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.UseCases_Partida_finalizar) ENABLED START*/
        using System.Linq;
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.UseCases.Partida;

public sealed partial class PartidaUseCase
{
    public async Task FinalizarAsync(int p_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.UseCases_Partida_finalizar) ENABLED START*/
        // Resolve external infrastructure services with _utils!.GetRequiredService<TService>() when this use case receives IUnitOfWorkUtils.
        PartidaService partidaService = null!;

        try
        {
            await _unitOfWork.BeginTransactionAsync("PartidaUseCase.FinalizarAsync");
            partidaService = new PartidaService(_unitOfWork);

            // Custom code:
            Partida partida = await _unitOfWork.PartidaRepository.ReadAsync(p_oid) 
                ?? throw new ModelException($"No existe la partida con id {p_oid}.");

            if (partida.Estado == EstadoPartida.Finalizada)
            {
                throw new ModelException($"La partida con id {p_oid} ya se encuentra finalizada.");
            }

            JugadorPartida? jugador1 = partida.Jugadores.FirstOrDefault(j => j.NumJugador == 1);
            JugadorPartida? jugador2 = partida.Jugadores.FirstOrDefault(j => j.NumJugador == 2);

            if (jugador1 == null || jugador2 == null)
            {
                throw new ModelException("La partida no cuenta con ambos jugadores para determinar un ganador.");
            }

            ZonaPartida? mazoJugador1 = jugador1.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Mazo);
            ZonaPartida? mazoJugador2 = jugador2.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Mazo);
            
            bool jugador1SinCartas = mazoJugador1 == null || mazoJugador1.Cartas == null || mazoJugador1.Cartas.Count == 0;
            bool jugador2SinCartas = mazoJugador2 == null || mazoJugador2.Cartas == null || mazoJugador2.Cartas.Count == 0;

            // Caso jugador 1 Pierde
            if (jugador1.Vida <= 0 || jugador1SinCartas)
            {
                partida.Ganador = partida.Usuario2?.Id;
            }
            // Caso jugador 2 Pierde
            else if (jugador2.Vida <= 0 || jugador2SinCartas)
            {
                partida.Ganador = partida.Usuario1.Id;
            }
            // Caso pierde por abandono o tiempo
            else
            {
                partida.Ganador = jugador1.Vida >= jugador2.Vida ? partida.Usuario1.Id : partida.Usuario2?.Id;
            }

            // Marcamos partida como finalizada y guardamos
            partida.Estado = EstadoPartida.Finalizada;
            await _unitOfWork.PartidaRepository.SaveAsync(partida);

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            throw new ModelException("Application error while executing use case Partida.finalizar.", ex);
        }
        /*PROTECTED REGION END*/
    }
}
