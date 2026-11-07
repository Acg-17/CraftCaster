using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.Services_Estado_activar) ENABLED START*/
        using System.Collections.Generic;
        using System.Linq;
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.Services;

public sealed partial class EstadoService
{
    public async Task ActivarAsync(int p_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.Services_Estado_activar) ENABLED START*/
        Estado estado = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException($"No existe el estado con id {p_oid}.");

        if (estado.Habilidades != null)
        {
            foreach (var habilidad in estado.Habilidades)
            {
                await _unitOfWork.HabilidadRepository.SaveAsync(habilidad);
            }
        }

        await _repository.SaveAsync(estado);
        /*PROTECTED REGION END*/
    }
}
