using System.Collections.Generic;
using System.Linq;
using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.Services_ZonaPartida_getAllCartas) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.Services;

public sealed partial class ZonaPartidaService
{
    public async Task<IReadOnlyList<int>> GetAllCartasAsync(int p_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.Services_ZonaPartida_getAllCartas) ENABLED START*/
        ZonaPartida zonaPartida = await _repository.ReadAsync(p_oid) 
            ?? throw new ModelException("No existe la zona indicada.");

        List<int> result = new();
        if (zonaPartida.Cartas != null)
        {
            foreach (CartaPartida carta in zonaPartida.Cartas)
            {
                result.Add(carta.Id);
            }
        }

        return result;
        /*PROTECTED REGION END*/
    }
}
