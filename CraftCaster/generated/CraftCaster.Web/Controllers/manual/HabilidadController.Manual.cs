// HARIA-WEB MVC managed manual-controller
/*PROTECTED REGION ID(HabilidadController_manual_usings) ENABLED START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CraftCaster.Domain.Entities;
using CraftCaster.Application.Services;
using CraftCaster.Web.Assemblers;

using CraftCaster.Domain.Enums;
/*PROTECTED REGION END*/

namespace CraftCaster.Web.Controllers;

// Class-level authorization applies to both partial files.
// [Authorize(Roles = "Admin")]
/*PROTECTED REGION ID(HabilidadController_manual_class_attributes) ENABLED START*/
// CRUD generado: solo para el administrador. Los jugadores usan MisCartas, EditorMazo y Explorar,
// que comprueban que la carta o el mazo es suyo. Afecta también a las acciones JSON (Compartir, ValorarMazo...).
[Authorize(Roles = "Admin")]
/*PROTECTED REGION END*/
public partial class HabilidadController
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(HabilidadController_Activar_POST_attributes) ENABLED START*/
    [Authorize(Roles = "Admin")] // Recibe ids arbitrarios: los jugadores usan JuegoController, que toma el usuario de la cookie
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Activar(int p_oid, int p_partida_oid)
    {
    /*PROTECTED REGION ID(HabilidadController_Activar_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await using var unitOfWork = _useCaseUnitOfWorkFactory.Create();
        var useCase = new CraftCaster.Application.UseCases.Habilidad.HabilidadUseCase(unitOfWork);
        await useCase.ActivarAsync(p_oid, p_partida_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }
}
