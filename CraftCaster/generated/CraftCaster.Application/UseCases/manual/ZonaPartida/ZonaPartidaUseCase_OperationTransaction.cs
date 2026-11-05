using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.UseCases_ZonaPartida_operationTransaction) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.UseCases.ZonaPartida;

public sealed partial class ZonaPartidaUseCase
{
    public async Task OperationTransactionAsync(int p_oid, int p_destino_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.UseCases_ZonaPartida_operationTransaction) ENABLED START*/
        // Resolve external infrastructure services with _utils!.GetRequiredService<TService>() when this use case receives IUnitOfWorkUtils.
        ZonaPartidaService zonaPartidaService = null!;

        try
        {
            await _unitOfWork.BeginTransactionAsync("ZonaPartidaUseCase.OperationTransactionAsync");
            zonaPartidaService = new ZonaPartidaService(_unitOfWork);

            // Write here your custom transaction code...
            // Use zonaPartidaService, _unitOfWork.ZonaPartidaRepository or _utils!.GetRequiredService<TService>() as the generated CP equivalent.

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            throw new ModelException("Application error while executing use case ZonaPartida.operationTransaction.", ex);
        }
        /*PROTECTED REGION END*/
    }
}
