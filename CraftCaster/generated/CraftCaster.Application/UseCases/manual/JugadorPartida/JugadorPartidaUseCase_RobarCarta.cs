using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.UseCases_JugadorPartida_robarCarta) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.UseCases.JugadorPartida;

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
