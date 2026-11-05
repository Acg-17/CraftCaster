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

        try
        {
            await _unitOfWork.BeginTransactionAsync("PartidaUseCase.PasarTurnoAsync");
            partidaService = new PartidaService(_unitOfWork);

            // Write here your custom transaction code...
            // Use partidaService, _unitOfWork.PartidaRepository or _utils!.GetRequiredService<TService>() as the generated CP equivalent.

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
