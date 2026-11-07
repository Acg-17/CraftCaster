using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.UseCases_JugadorPartida_robarCarta) ENABLED START*/
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
    public async Task RobarCartaAsync(int p_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.UseCases_JugadorPartida_robarCarta) ENABLED START*/
        JugadorPartidaService jugadorPartidaService = null!;

        try
        {
            await _unitOfWork.BeginTransactionAsync("JugadorPartidaUseCase.RobarCartaAsync");
            jugadorPartidaService = new JugadorPartidaService(_unitOfWork);

            await jugadorPartidaService.RobarCartaAsync(p_oid);

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            if (ex is ModelException)
            {
                throw;
            }
            throw new ModelException("Application error while executing use case JugadorPartida.robarCarta.", ex);
        }
        /*PROTECTED REGION END*/
    }
}
