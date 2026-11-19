// HARIA-WEB MVC managed manual-controller
/*PROTECTED REGION ID(PartidaController_manual_usings) ENABLED START*/
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
/*PROTECTED REGION ID(PartidaController_manual_class_attributes) ENABLED START*/
// Class-level authorization applies to both partial files.
// [Authorize(Roles = "Admin")]
/*PROTECTED REGION END*/
public partial class PartidaController
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(PartidaController_CambiarEstado_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> CambiarEstado(int p_oid, EstadoPartida p_estado)
    {
    /*PROTECTED REGION ID(PartidaController_CambiarEstado_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _partidaService.CambiarEstadoAsync(p_oid, p_estado);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(PartidaController_GetActivo_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> GetActivo(int p_oid)
    {
    /*PROTECTED REGION ID(PartidaController_GetActivo_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _partidaService.GetActivoAsync(p_oid);
        return Ok(result);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(PartidaController_GetCarta_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> GetCarta(int p_oid, int jugador, int zona, int carta)
    {
    /*PROTECTED REGION ID(PartidaController_GetCarta_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _partidaService.GetCartaAsync(p_oid, jugador, zona, carta);
        return Ok(result);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(PartidaController_GetPlayer_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> GetPlayer(int p_oid, int index)
    {
    /*PROTECTED REGION ID(PartidaController_GetPlayer_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _partidaService.GetPlayerAsync(p_oid, index);
        return Ok(result);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(PartidaController_Finalizar_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Finalizar(int p_oid)
    {
    /*PROTECTED REGION ID(PartidaController_Finalizar_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await using var unitOfWork = _useCaseUnitOfWorkFactory.Create();
        var useCase = new CraftCaster.Application.UseCases.Partida.PartidaUseCase(unitOfWork);
        await useCase.FinalizarAsync(p_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(PartidaController_PasarTurno_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> PasarTurno(int p_oid)
    {
    /*PROTECTED REGION ID(PartidaController_PasarTurno_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await using var unitOfWork = _useCaseUnitOfWorkFactory.Create();
        var useCase = new CraftCaster.Application.UseCases.Partida.PartidaUseCase(unitOfWork);
        await useCase.PasarTurnoAsync(p_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }
}
