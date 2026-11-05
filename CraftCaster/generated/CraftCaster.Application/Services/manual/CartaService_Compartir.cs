using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.Services_Carta_compartir) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.Services;

public sealed partial class CartaService
{
    public async Task CompartirAsync(int p_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.Services_Carta_compartir) ENABLED START*/

        // el ? para que pueda ser null 
       Carta? carta = await _repository.ReadAsync(p_oid);
        if (carta == null)
        {
            throw new ModelException("No existe la carta indicada.");
        }

        // para que sea visible en la comunidad
        carta.EsPublica = true;
        await _repository.SaveAsync(carta);
        /*PROTECTED REGION END*/
    }
}
