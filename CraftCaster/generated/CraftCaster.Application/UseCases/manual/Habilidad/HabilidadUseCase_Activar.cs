using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.UseCases_Habilidad_activar) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.UseCases.Habilidad;

public sealed partial class HabilidadUseCase
{
    public async Task ActivarAsync(int p_oid, int p_partida_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.UseCases_Habilidad_activar) ENABLED START*/
        // Resolve external infrastructure services with _utils!.GetRequiredService<TService>() when this use case receives IUnitOfWorkUtils.
        HabilidadService habilidadService = null!;

        try
        {
            await _unitOfWork.BeginTransactionAsync("HabilidadUseCase.ActivarAsync");
            habilidadService = new HabilidadService(_unitOfWork);

            // Write here your custom transaction code...
            // Use habilidadService, _unitOfWork.HabilidadRepository or _utils!.GetRequiredService<TService>() as the generated CP equivalent.

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            throw new ModelException("Application error while executing use case Habilidad.activar.", ex);
        }
        /*PROTECTED REGION END*/
    }
}
