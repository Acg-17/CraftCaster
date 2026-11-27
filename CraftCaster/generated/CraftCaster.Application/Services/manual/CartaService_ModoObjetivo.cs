using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

        /*PROTECTED REGION ID(usingCraftCaster.Application.Services_Carta_modoObjetivo) ENABLED START*/
        // references to other libraries
        using System.Linq;
        /*PROTECTED REGION END*/

namespace CraftCaster.Application.Services;

public sealed partial class CartaService
{
    public async Task<int> ModoObjetivoAsync(int p_oid, int p_criaturasEnemigas, int p_criaturasPropias)
    {
        /*PROTECTED REGION ID(CraftCaster.Application.Services_Carta_modoObjetivo) ENABLED START*/
        // Objetivo que hay que pedir al jugador al lanzar la carta:
        //   0 = ninguno, 1 = enemigo o criatura, 2 = criatura enemiga, 3 = criatura propia, 4 = jugador enemigo
        const int Ninguno = 0, EnemigoOCriatura = 1, SoloCriaturaEnemiga = 2, SoloCriaturaPropia = 3, SoloJugadorEnemigo = 4;

        Carta carta = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("No existe la carta indicada.");

        // Habilidades que se ejecutan al lanzarla: todas en un hechizo, las de "Entrar" en una criatura
        var habilidades = (carta.Estados ?? new List<Estado>())
            .Where(e => carta.Tipo == TipoCarta.Hechizo || e.Tipo == TipoEstado.Entrar)
            .SelectMany(e => e.Habilidades ?? new List<Habilidad>())
            .Where(h => h.Tipo != TipoHabilidad.Defensa)
            .ToList();
        if (habilidades.Count == 0) return Ninguno;

        // Prioridad 1: daño dirigido a enemigo / libre
        if (habilidades.Any(h => h.Tipo == TipoHabilidad.Danyo && h.TipoObjetivo is TipoObjetivo.Enemigo or TipoObjetivo.Libre))
        {
            return EnemigoOCriatura;
        }

        // Prioridad 2: destruir una criatura concreta (propia si el objetivo es Jugador)
        var destruir = habilidades.FirstOrDefault(h => h.Tipo == TipoHabilidad.Destruir);
        if (destruir is not null)
        {
            if (destruir.TipoObjetivo == TipoObjetivo.Jugador)
            {
                return p_criaturasPropias > 0 ? SoloCriaturaPropia : Ninguno;
            }
            return p_criaturasEnemigas > 0 ? SoloCriaturaEnemiga : Ninguno;
        }

        // Prioridad 3: hechizo de daño directo al jugador enemigo
        if (carta.Tipo == TipoCarta.Hechizo &&
            habilidades.Any(h => h.Tipo == TipoHabilidad.Danyo && h.TipoObjetivo == TipoObjetivo.Jugador))
        {
            return SoloJugadorEnemigo;
        }

        return Ninguno;
        /*PROTECTED REGION END*/
    }
}
