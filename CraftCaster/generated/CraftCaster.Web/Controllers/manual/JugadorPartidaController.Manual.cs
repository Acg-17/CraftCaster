// HARIA-WEB MVC managed manual-controller
/*PROTECTED REGION ID(JugadorPartidaController_manual_usings) ENABLED START*/
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
/*PROTECTED REGION ID(JugadorPartidaController_manual_class_attributes) ENABLED START*/
// Class-level authorization applies to both partial files.
// [Authorize(Roles = "Admin")]
/*PROTECTED REGION END*/
public partial class JugadorPartidaController
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(JugadorPartidaController_ActualizarVida_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ActualizarVida(int p_oid, int p_vida)
    {
    /*PROTECTED REGION ID(JugadorPartidaController_ActualizarVida_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _jugadorPartidaService.ActualizarVidaAsync(p_oid, p_vida);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(JugadorPartidaController_ActualizarMana_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ActualizarMana(int p_oid, int p_mana)
    {
    /*PROTECTED REGION ID(JugadorPartidaController_ActualizarMana_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _jugadorPartidaService.ActualizarManaAsync(p_oid, p_mana);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(JugadorPartidaController_RobarCartaDelMazo_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> RobarCartaDelMazo(int p_oid)
    {
    /*PROTECTED REGION ID(JugadorPartidaController_RobarCartaDelMazo_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _jugadorPartidaService.RobarCartaDelMazoAsync(p_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(JugadorPartidaController_PasarTurno_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> PasarTurno(int p_oid)
    {
    /*PROTECTED REGION ID(JugadorPartidaController_PasarTurno_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _jugadorPartidaService.PasarTurnoAsync(p_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(JugadorPartidaController_LanzarCarta_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> LanzarCarta(int p_oid, int p_carta_oid)
    {
    /*PROTECTED REGION ID(JugadorPartidaController_LanzarCarta_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await using var unitOfWork = _useCaseUnitOfWorkFactory.Create();
        var useCase = new CraftCaster.Application.UseCases.JugadorPartida.JugadorPartidaUseCase(unitOfWork);
        await useCase.LanzarCartaAsync(p_oid, p_carta_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(JugadorPartidaController_RobarCarta_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> RobarCarta(int p_oid)
    {
    /*PROTECTED REGION ID(JugadorPartidaController_RobarCarta_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await using var unitOfWork = _useCaseUnitOfWorkFactory.Create();
        var useCase = new CraftCaster.Application.UseCases.JugadorPartida.JugadorPartidaUseCase(unitOfWork);
        await useCase.RobarCartaAsync(p_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(JugadorPartidaController_InicioTurno_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> InicioTurno(int p_oid)
    {
    /*PROTECTED REGION ID(JugadorPartidaController_InicioTurno_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await using var unitOfWork = _useCaseUnitOfWorkFactory.Create();
        var useCase = new CraftCaster.Application.UseCases.JugadorPartida.JugadorPartidaUseCase(unitOfWork);
        await useCase.InicioTurnoAsync(p_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(JugadorPartidaController_FinalTurno_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> FinalTurno(int p_oid)
    {
    /*PROTECTED REGION ID(JugadorPartidaController_FinalTurno_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await using var unitOfWork = _useCaseUnitOfWorkFactory.Create();
        var useCase = new CraftCaster.Application.UseCases.JugadorPartida.JugadorPartidaUseCase(unitOfWork);
        await useCase.FinalTurnoAsync(p_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }
}
