using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.Services_Mazo_valorarMazo) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.Services;

public sealed partial class MazoService
{
    public async Task ValorarMazoAsync(int p_oid, float p_valoracion)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.Services_Mazo_valorarMazo) ENABLED START*/
        if (p_valoracion < 0.5f || p_valoracion > 5.0f)
        {
            throw new ModelException("La valoracion del mazo debe estar entre 0.5 y 5 estrellas.");
        }

        Mazo? mazo = await _repository.ReadAsync(p_oid);
        if (mazo == null)
        {
            throw new ModelException("No existe el mazo indicado.");
        }

        mazo.Valoracion = p_valoracion;
        await _repository.SaveAsync(mazo);
        /*PROTECTED REGION END*/
    }
}
