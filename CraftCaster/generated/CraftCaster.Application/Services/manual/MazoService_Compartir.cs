using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.Services_Mazo_compartir) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.Services;

public sealed partial class MazoService
{
    public async Task CompartirAsync(int p_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.Services_Mazo_compartir) ENABLED START*/
        Mazo? mazo = await _repository.ReadAsync(p_oid);
        if (mazo == null)
        {
            throw new ModelException("No existe el mazo indicado.");
        }

        // publicar el mazo para que sea visible en la comunidad
        mazo.EsPublico = true;
        await _repository.SaveAsync(mazo);
        /*PROTECTED REGION END*/
    }
}
