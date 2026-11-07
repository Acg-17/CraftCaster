using System.Linq;
using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.Services_Mazo_calcularEstadisticas) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.Services;

public sealed partial class MazoService
{
    public async Task CalcularEstadisticasAsync(int p_oid)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.Services_Mazo_calcularEstadisticas) ENABLED START*/
        Mazo mazo = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("No existe el mazo indicado.");

        if (mazo.Lineas == null || mazo.Lineas.Count == 0)
        {
            mazo.NumCartas = 0;
            mazo.CosteMedio = 0f;
            mazo.EstadoMazo = EstadoMazo.Incompleto;
        }
        else
        {
            int totalCartas = mazo.Lineas.Sum(l => l.Cantidad);
            int totalCoste = mazo.Lineas.Sum(l => l.Cantidad * (l.Carta != null ? l.Carta.CosteMana : 0));

            mazo.NumCartas = totalCartas;
            mazo.CosteMedio = totalCartas > 0 ? (float)totalCoste / totalCartas : 0f;

            if (totalCartas == 30)
            {
                mazo.EstadoMazo = EstadoMazo.Jugable;
            }
            else if (totalCartas > 30)
            {
                mazo.EstadoMazo = EstadoMazo.Completo;
            }
            else
            {
                mazo.EstadoMazo = EstadoMazo.Incompleto;
            }
        }

        await _repository.SaveAsync(mazo);
        /*PROTECTED REGION END*/
    }
}
