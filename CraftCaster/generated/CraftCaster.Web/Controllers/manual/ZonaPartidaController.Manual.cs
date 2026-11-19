// HARIA-WEB MVC managed manual-controller
/*PROTECTED REGION ID(ZonaPartidaController_manual_usings) ENABLED START*/
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
/*PROTECTED REGION ID(ZonaPartidaController_manual_class_attributes) ENABLED START*/
// Class-level authorization applies to both partial files.
// [Authorize(Roles = "Admin")]
/*PROTECTED REGION END*/
public partial class ZonaPartidaController
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ZonaPartidaController_GetAllCartas_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> GetAllCartas(int p_oid)
    {
    /*PROTECTED REGION ID(ZonaPartidaController_GetAllCartas_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _zonaPartidaService.GetAllCartasAsync(p_oid);
        return Ok(result);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ZonaPartidaController_OperationTransaction_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> OperationTransaction(int p_oid, int p_destino_oid, int index)
    {
    /*PROTECTED REGION ID(ZonaPartidaController_OperationTransaction_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await using var unitOfWork = _useCaseUnitOfWorkFactory.Create();
        var useCase = new CraftCaster.Application.UseCases.ZonaPartida.ZonaPartidaUseCase(unitOfWork);
        await useCase.OperationTransactionAsync(p_oid, p_destino_oid, index);
        return NoContent();
    /*PROTECTED REGION END*/
    }
}
