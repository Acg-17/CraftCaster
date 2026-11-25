// HARIA-WEB MVC managed manual-controller
/*PROTECTED REGION ID(UsuarioController_manual_usings) ENABLED START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CraftCaster.Domain.Entities;
using CraftCaster.Application.Services;
using CraftCaster.Web.Assemblers;
using CraftCaster.Web.Utils.Security;

using CraftCaster.Domain.Enums;
/*PROTECTED REGION END*/

namespace CraftCaster.Web.Controllers;

// Class-level authorization applies to both partial files.
// [Authorize(Roles = "Admin")]
/*PROTECTED REGION ID(UsuarioController_manual_class_attributes) ENABLED START*/
// Class-level authorization applies to both partial files.
[Authorize(Roles = HariaWebAuthentication.RolAdmin)]
/*PROTECTED REGION END*/
public partial class UsuarioController
{
    [HttpGet]
    /*PROTECTED REGION ID(UsuarioController_Create_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public IActionResult Create()
    {
    /*PROTECTED REGION ID(UsuarioController_Create_GET_body) ENABLED START*/

        return View(new CraftCaster.Web.ViewModels.UsuarioViewModel());
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(UsuarioController_Create_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(CraftCaster.Web.ViewModels.UsuarioViewModel model)
    {
    /*PROTECTED REGION ID(UsuarioController_Create_POST_body) ENABLED START*/



        if (string.IsNullOrEmpty(model.Contrasenia)) ModelState.AddModelError(nameof(model.Contrasenia), "Debes indicar una contraseña.");
        if (!ModelState.IsValid) return View(model);

        try
        {
            await _usuarioService.NewAsync(model.NombreUsuario, model.Email, model.Contrasenia!, model.FotoPerfil, model.EstadoCuenta, model.TipoUsuario);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            _logger.LogError(error, "Could not create the record.");
            ModelState.AddModelError(string.Empty, "No se ha podido guardar el registro. Inténtalo de nuevo.");
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(UsuarioController_CambiarEstadoCuenta_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> CambiarEstadoCuenta(int p_oid, EstadoCuenta p_estadoCuenta)
    {
    /*PROTECTED REGION ID(UsuarioController_CambiarEstadoCuenta_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _usuarioService.CambiarEstadoCuentaAsync(p_oid, p_estadoCuenta);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(UsuarioController_ReportarUsuario_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ReportarUsuario(int p_oid)
    {
    /*PROTECTED REGION ID(UsuarioController_ReportarUsuario_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _usuarioService.ReportarUsuarioAsync(p_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(UsuarioController_RegistrarUsuario_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> RegistrarUsuario(string p_nombreUsuario, string p_email, string p_contrasenia, byte p_fotoPerfil)
    {
    /*PROTECTED REGION ID(UsuarioController_RegistrarUsuario_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await using var unitOfWork = _useCaseUnitOfWorkFactory.Create();
        var useCase = new CraftCaster.Application.UseCases.Usuario.UsuarioUseCase(unitOfWork);
        var result = await useCase.RegistrarUsuarioAsync(p_nombreUsuario, p_email, p_contrasenia, p_fotoPerfil);
        return Ok(result);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(UsuarioController_IniciarPartida_POST_attributes) ENABLED START*/
    [Authorize(Roles = "Admin")] // Recibe ids arbitrarios: los jugadores usan JuegoController, que toma el usuario de la cookie
    /*PROTECTED REGION END*/
    public async Task<IActionResult> IniciarPartida(int p_oid)
    {
    /*PROTECTED REGION ID(UsuarioController_IniciarPartida_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await using var unitOfWork = _useCaseUnitOfWorkFactory.Create();
        var useCase = new CraftCaster.Application.UseCases.Usuario.UsuarioUseCase(unitOfWork);
        await useCase.IniciarPartidaAsync(p_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(UsuarioController_UnirsePartida_POST_attributes) ENABLED START*/
    [Authorize(Roles = "Admin")] // Recibe ids arbitrarios: los jugadores usan JuegoController, que toma el usuario de la cookie
    /*PROTECTED REGION END*/
    public async Task<IActionResult> UnirsePartida(int p_oid, int p_partida_oid)
    {
    /*PROTECTED REGION ID(UsuarioController_UnirsePartida_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await using var unitOfWork = _useCaseUnitOfWorkFactory.Create();
        var useCase = new CraftCaster.Application.UseCases.Usuario.UsuarioUseCase(unitOfWork);
        await useCase.UnirsePartidaAsync(p_oid, p_partida_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(UsuarioController_AbandonarPartida_POST_attributes) ENABLED START*/
    [Authorize(Roles = "Admin")] // Recibe ids arbitrarios: los jugadores usan JuegoController, que toma el usuario de la cookie
    /*PROTECTED REGION END*/
    public async Task<IActionResult> AbandonarPartida(int p_oid)
    {
    /*PROTECTED REGION ID(UsuarioController_AbandonarPartida_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await using var unitOfWork = _useCaseUnitOfWorkFactory.Create();
        var useCase = new CraftCaster.Application.UseCases.Usuario.UsuarioUseCase(unitOfWork);
        await useCase.AbandonarPartidaAsync(p_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }
}
