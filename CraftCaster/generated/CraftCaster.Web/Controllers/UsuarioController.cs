// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(UsuarioController_usings) START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CraftCaster.Domain.Entities;
using CraftCaster.Application.Services;
using CraftCaster.Web.Assemblers;
using CraftCaster.Web.Utils.Security;

using CraftCaster.Domain.Enums;
/*PROTECTED REGION END*/

namespace CraftCaster.Web.Controllers;

/*PROTECTED REGION ID(UsuarioController_class_attributes) ENABLED START*/
[Authorize]
/*PROTECTED REGION END*/
public partial class UsuarioController : Controller
{
    /*PROTECTED REGION ID(UsuarioController_dependencies) START*/
    private readonly CraftCaster.Application.Abstractions.IUnitOfWorkFactory _useCaseUnitOfWorkFactory;
    private readonly UsuarioService _usuarioService;
    private readonly ILogger<UsuarioController> _logger;
    public UsuarioController(UsuarioService usuarioService, CraftCaster.Application.Abstractions.IUnitOfWorkFactory useCaseUnitOfWorkFactory, ILogger<UsuarioController>? logger = null)
    {
        _usuarioService = usuarioService;


        _useCaseUnitOfWorkFactory = useCaseUnitOfWorkFactory;
        _logger = logger ?? global::Microsoft.Extensions.Logging.Abstractions.NullLogger<UsuarioController>.Instance;
    }
    /*PROTECTED REGION END*/
    [HttpGet]
    /*PROTECTED REGION ID(UsuarioController_Index_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Index()
    {
    /*PROTECTED REGION ID(UsuarioController_Index_GET_body) START*/

        var entities = await _usuarioService.ReadAllAsync();
        return View(entities.Select(UsuarioAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(UsuarioController_Details_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Details(int id)
    {
    /*PROTECTED REGION ID(UsuarioController_Details_GET_body) START*/

        var entity = await _usuarioService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(UsuarioAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(UsuarioController_Register_GET_attributes) START*/
    [AllowAnonymous]
    /*PROTECTED REGION END*/
    public IActionResult Register()
    {
    /*PROTECTED REGION ID(UsuarioController_Register_GET_body) START*/

        return View(new CraftCaster.Web.ViewModels.UsuarioViewModel());
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(UsuarioController_Register_POST_attributes) START*/
    [AllowAnonymous]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Register(CraftCaster.Web.ViewModels.UsuarioViewModel model)
    {
    /*PROTECTED REGION ID(UsuarioController_Register_POST_body) START*/



        if (string.IsNullOrEmpty(model.Contrasenia)) ModelState.AddModelError(nameof(model.Contrasenia), "Debes indicar una contraseña.");
        if (!ModelState.IsValid) return View(model);
        Usuario createdUser;
        try
        {
            createdUser = await _usuarioService.NewAsync(model.NombreUsuario, model.Email, model.Contrasenia!, model.FotoPerfil, model.EstadoCuenta, model.TipoUsuario);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            _logger.LogError(error, "Could not create the record.");
            ModelState.AddModelError(string.Empty, "No se ha podido guardar el registro. Inténtalo de nuevo.");
            return View(model);
        }
        try
        {
            await HariaWebAuthentication.SignInAsync(HttpContext, "Usuario", createdUser.Id, createdUser);
            HttpContext.Session.SetUser(UsuarioAssembler.ToViewModel(createdUser));
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            _logger.LogError(error, "La cuenta se ha creado, pero no se ha podido iniciar la sesión.");
            ModelState.AddModelError(string.Empty, "Tu cuenta ya está creada. Inicia sesión desde la página de acceso.");
            return View(model);
        }
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(UsuarioController_Edit_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id)
    {
    /*PROTECTED REGION ID(UsuarioController_Edit_GET_body) START*/

        var entity = await _usuarioService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(UsuarioAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(UsuarioController_Edit_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, CraftCaster.Web.ViewModels.UsuarioViewModel model)
    {
    /*PROTECTED REGION ID(UsuarioController_Edit_POST_body) START*/



        if (!ModelState.IsValid) return View(model);
        var stored = await _usuarioService.ReadOIDAsync(id);
        if (stored is null) return NotFound();
        try
        {
            await _usuarioService.ModifyAsync(id, model.NombreUsuario, model.Email, (string.IsNullOrEmpty(model.Contrasenia) ? stored.Contrasenia : model.Contrasenia), model.FotoPerfil, model.EstadoCuenta, model.TipoUsuario);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            _logger.LogError(error, "Could not update the record.");
            ModelState.AddModelError(string.Empty, "No se ha podido guardar el registro. Inténtalo de nuevo.");
            return View(model);
        }
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(UsuarioController_Delete_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Delete(int id)
    {
    /*PROTECTED REGION ID(UsuarioController_Delete_GET_body) START*/

        var entity = await _usuarioService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(UsuarioAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(UsuarioController_DeleteConfirmed_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(UsuarioController_DeleteConfirmed_POST_body) START*/

        if (!ModelState.IsValid) return View("Delete", new CraftCaster.Web.ViewModels.UsuarioViewModel());
        try
        {
            await _usuarioService.DestroyAsync(id);
            TempData["Mensaje"] = "Usuario eliminado correctamente.";
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            _logger.LogWarning(error, "No se pudo eliminar el usuario {Id} por relaciones existentes.", id);
            TempData["Error"] = "No se puede eliminar este usuario porque tiene cartas, mazos o partidas asociadas. Puedes suspender su cuenta en su lugar.";
        }
        return User.IsInRole(CraftCaster.Web.Utils.Security.HariaWebAuthentication.RolAdmin)
            ? RedirectToAction("Index", "Admin")
            : RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(UsuarioController_AddCarta_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> AddCarta(int p_oid, IReadOnlyList<int> p_carta_oids)
    {
    /*PROTECTED REGION ID(UsuarioController_AddCarta_POST_body) START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _usuarioService.AddCartaAsync(p_oid, p_carta_oids);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(UsuarioController_AddMazo_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> AddMazo(int p_oid, IReadOnlyList<int> p_mazo_oids)
    {
    /*PROTECTED REGION ID(UsuarioController_AddMazo_POST_body) START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _usuarioService.AddMazoAsync(p_oid, p_mazo_oids);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(UsuarioController_AddPartidasLocal_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> AddPartidasLocal(int p_oid, IReadOnlyList<int> p_partida_oids)
    {
    /*PROTECTED REGION ID(UsuarioController_AddPartidasLocal_POST_body) START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _usuarioService.AddPartidasLocalAsync(p_oid, p_partida_oids);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(UsuarioController_AddPartidasVisitante_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> AddPartidasVisitante(int p_oid, IReadOnlyList<int> p_partida_oids)
    {
    /*PROTECTED REGION ID(UsuarioController_AddPartidasVisitante_POST_body) START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _usuarioService.AddPartidasVisitanteAsync(p_oid, p_partida_oids);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(UsuarioController_RemovePartidasVisitante_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> RemovePartidasVisitante(int p_oid, IReadOnlyList<int> p_partida_oids)
    {
    /*PROTECTED REGION ID(UsuarioController_RemovePartidasVisitante_POST_body) START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _usuarioService.RemovePartidasVisitanteAsync(p_oid, p_partida_oids);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(UsuarioController_ObtenerUsuarioLocal_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ObtenerUsuarioLocal(int p_oid)
    {
    /*PROTECTED REGION ID(UsuarioController_ObtenerUsuarioLocal_GET_body) START*/

        var result = await _usuarioService.ObtenerUsuarioLocalAsync(p_oid);
        return Ok(result.Select(UsuarioAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }
}
