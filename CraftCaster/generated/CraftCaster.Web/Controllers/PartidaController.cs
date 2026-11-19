// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(PartidaController_usings) START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CraftCaster.Domain.Entities;
using CraftCaster.Application.Services;
using CraftCaster.Web.Assemblers;

using CraftCaster.Domain.Enums;
/*PROTECTED REGION END*/

namespace CraftCaster.Web.Controllers;

/*PROTECTED REGION ID(PartidaController_class_attributes) ENABLED START*/
[Authorize]
/*PROTECTED REGION END*/
public partial class PartidaController : Controller
{
    /*PROTECTED REGION ID(PartidaController_dependencies) START*/
    private readonly CraftCaster.Application.Abstractions.IUnitOfWorkFactory _useCaseUnitOfWorkFactory;
    private readonly PartidaService _partidaService;
    private readonly ILogger<PartidaController> _logger;
    private readonly CraftCaster.Application.Abstractions.IUnitOfWorkFactory _formUnitOfWorkFactory;
    public PartidaController(PartidaService partidaService, CraftCaster.Application.Abstractions.IUnitOfWorkFactory formUnitOfWorkFactory, CraftCaster.Application.Abstractions.IUnitOfWorkFactory useCaseUnitOfWorkFactory, ILogger<PartidaController>? logger = null)
    {
        _partidaService = partidaService;

        _formUnitOfWorkFactory = formUnitOfWorkFactory;
        _useCaseUnitOfWorkFactory = useCaseUnitOfWorkFactory;
        _logger = logger ?? global::Microsoft.Extensions.Logging.Abstractions.NullLogger<PartidaController>.Instance;
    }
    /*PROTECTED REGION END*/
    [HttpGet]
    /*PROTECTED REGION ID(PartidaController_Index_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Index()
    {
    /*PROTECTED REGION ID(PartidaController_Index_GET_body) START*/

        var entities = await _partidaService.ReadAllAsync();
        return View(entities.Select(PartidaAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(PartidaController_Details_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Details(int id)
    {
    /*PROTECTED REGION ID(PartidaController_Details_GET_body) START*/

        var entity = await _partidaService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(PartidaAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(PartidaController_Create_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create()
    {
    /*PROTECTED REGION ID(PartidaController_Create_GET_body) START*/

        var model = new CraftCaster.Web.ViewModels.PartidaViewModel();

        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(PartidaController_Create_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(CraftCaster.Web.ViewModels.PartidaViewModel model)
    {
    /*PROTECTED REGION ID(PartidaController_Create_POST_body) START*/

        ModelState.Remove(nameof(model.Ganador)); // Campo fuera del contrato Create de Application.
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::CraftCaster.Application.Services.PartidaService(unitOfWork);





        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.Usuario1Id is int selectedUsuario1 &&
            !model.Usuario1Options.Any(option => option.Value == Convert.ToString(selectedUsuario1, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("Usuario1Id", "Selecciona un registro existente de Usuario.");
        if (model.Usuario1Id is null) ModelState.AddModelError("Usuario1Id", "Debes seleccionar una opción.");
        if (model.Usuario2Id is int selectedUsuario2 &&
            !model.Usuario2Options.Any(option => option.Value == Convert.ToString(selectedUsuario2, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("Usuario2Id", "Selecciona un registro existente de Usuario.");

        if (model.Mazo1Id is int selectedMazo1 &&
            !model.Mazo1Options.Any(option => option.Value == Convert.ToString(selectedMazo1, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("Mazo1Id", "Selecciona un registro existente de Mazo.");
        if (model.Mazo1Id is null) ModelState.AddModelError("Mazo1Id", "Debes seleccionar una opción.");
        if (model.Mazo2Id is int selectedMazo2 &&
            !model.Mazo2Options.Any(option => option.Value == Convert.ToString(selectedMazo2, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("Mazo2Id", "Selecciona un registro existente de Mazo.");





        if (!ModelState.IsValid) return View(model);


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.Partida.Create");


            var entity = await service.NewAsync(model.Estado, model.FechaInicio, model.Usuario1Id.GetValueOrDefault(), model.Mazo1Id.GetValueOrDefault());

            await service.ApplyPartidaRelationsAsync(entity, p_usuario1_id: model.Usuario1Id, p_usuario2_id: model.Usuario2Id, p_mazo1_id: model.Mazo1Id, p_mazo2_id: model.Mazo2Id);

            await service.ModifierPartidaDefaultAsync(entity);


            await unitOfWork.CommitAsync();
        }
        catch (Exception error)
        {
            await unitOfWork.RollbackAsync();
            if (error is OperationCanceledException) throw;
            _logger.LogError(error, "Could not save the record and its lines.");
            ModelState.AddModelError(string.Empty, "No se han podido guardar el registro y sus líneas.");
            return View(model);
        }


        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(PartidaController_Edit_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id)
    {
    /*PROTECTED REGION ID(PartidaController_Edit_GET_body) START*/

        var entity = await _partidaService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        var model = PartidaAssembler.ToViewModel(entity);
        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(PartidaController_Edit_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, CraftCaster.Web.ViewModels.PartidaViewModel model)
    {
    /*PROTECTED REGION ID(PartidaController_Edit_POST_body) START*/

        ModelState.Remove(nameof(model.Ganador)); // Campo fuera del contrato Edit de Application.
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::CraftCaster.Application.Services.PartidaService(unitOfWork);

        var entity = await service.ReadOIDPartidaDefaultAsync(id);
        if (entity is null) return NotFound();



        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.Usuario1Id is int selectedUsuario1 &&
            !model.Usuario1Options.Any(option => option.Value == Convert.ToString(selectedUsuario1, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("Usuario1Id", "Selecciona un registro existente de Usuario.");
        if (model.Usuario1Id is null) ModelState.AddModelError("Usuario1Id", "Debes seleccionar una opción.");
        if (model.Usuario2Id is int selectedUsuario2 &&
            !model.Usuario2Options.Any(option => option.Value == Convert.ToString(selectedUsuario2, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("Usuario2Id", "Selecciona un registro existente de Usuario.");

        if (model.Mazo1Id is int selectedMazo1 &&
            !model.Mazo1Options.Any(option => option.Value == Convert.ToString(selectedMazo1, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("Mazo1Id", "Selecciona un registro existente de Mazo.");
        if (model.Mazo1Id is null) ModelState.AddModelError("Mazo1Id", "Debes seleccionar una opción.");
        if (model.Mazo2Id is int selectedMazo2 &&
            !model.Mazo2Options.Any(option => option.Value == Convert.ToString(selectedMazo2, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("Mazo2Id", "Selecciona un registro existente de Mazo.");





        if (!ModelState.IsValid) return View(model);


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.Partida.Edit");


            await service.ModifyAsync(id, model.Estado, model.FechaInicio, model.Usuario1Id.GetValueOrDefault(), model.Mazo1Id.GetValueOrDefault());
            entity = await service.ReadOIDPartidaDefaultAsync(id) ?? throw new InvalidOperationException("Entity disappeared during update.");
            await service.ApplyPartidaRelationsAsync(entity, p_usuario1_id: model.Usuario1Id, p_usuario2_id: model.Usuario2Id, p_mazo1_id: model.Mazo1Id, p_mazo2_id: model.Mazo2Id);

            await service.ModifierPartidaDefaultAsync(entity);


            await unitOfWork.CommitAsync();
        }
        catch (Exception error)
        {
            await unitOfWork.RollbackAsync();
            if (error is OperationCanceledException) throw;
            _logger.LogError(error, "Could not save the record and its lines.");
            ModelState.AddModelError(string.Empty, "No se han podido guardar el registro y sus líneas.");
            return View(model);
        }


        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(PartidaController_Delete_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Delete(int id)
    {
    /*PROTECTED REGION ID(PartidaController_Delete_GET_body) START*/

        var entity = await _partidaService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(PartidaAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(PartidaController_DeleteConfirmed_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(PartidaController_DeleteConfirmed_POST_body) START*/

        if (!ModelState.IsValid) return View("Delete", new CraftCaster.Web.ViewModels.PartidaViewModel());
        await _partidaService.DestroyAsync(id);
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(PartidaController_AddTurno_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> AddTurno(int p_oid, IReadOnlyList<int> p_turno_oids)
    {
    /*PROTECTED REGION ID(PartidaController_AddTurno_POST_body) START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _partidaService.AddTurnoAsync(p_oid, p_turno_oids);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(PartidaController_AddJugadores_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> AddJugadores(int p_oid, IReadOnlyList<int> p_jugadorPartida_oids)
    {
    /*PROTECTED REGION ID(PartidaController_AddJugadores_POST_body) START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _partidaService.AddJugadoresAsync(p_oid, p_jugadorPartida_oids);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(PartidaController_AddUsuario2_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> AddUsuario2(int p_oid, int p_usuario_oid)
    {
    /*PROTECTED REGION ID(PartidaController_AddUsuario2_POST_body) START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _partidaService.AddUsuario2Async(p_oid, p_usuario_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(PartidaController_RemoveUsuario2_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> RemoveUsuario2(int p_oid, int p_usuario_oid)
    {
    /*PROTECTED REGION ID(PartidaController_RemoveUsuario2_POST_body) START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _partidaService.RemoveUsuario2Async(p_oid, p_usuario_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(PartidaController_AddMazo2_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> AddMazo2(int p_oid, int p_mazo_oid)
    {
    /*PROTECTED REGION ID(PartidaController_AddMazo2_POST_body) START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _partidaService.AddMazo2Async(p_oid, p_mazo_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(PartidaController_RemoveMazo2_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> RemoveMazo2(int p_oid, int p_mazo_oid)
    {
    /*PROTECTED REGION ID(PartidaController_RemoveMazo2_POST_body) START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _partidaService.RemoveMazo2Async(p_oid, p_mazo_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(PartidaController_ObtenerPartidasUsuario_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ObtenerPartidasUsuario(int p_oid)
    {
    /*PROTECTED REGION ID(PartidaController_ObtenerPartidasUsuario_GET_body) START*/

        var result = await _partidaService.ObtenerPartidasUsuarioAsync(p_oid);
        return Ok(result.Select(PartidaAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    /*PROTECTED REGION ID(PartidaController_method_LoadFormOptionsAsync_CraftCasterWebViewModelsPartidaViewModel) START*/
    private async Task LoadFormOptionsAsync(CraftCaster.Web.ViewModels.PartidaViewModel model)
    {
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        await LoadFormOptionsAsync(model, unitOfWork);
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(PartidaController_method_LoadFormOptionsAsync_CraftCasterWebViewModelsPartidaViewModel_CraftCasterApplicationAbstractionsIUnitOfWork) START*/
    private async Task LoadFormOptionsAsync(CraftCaster.Web.ViewModels.PartidaViewModel model, CraftCaster.Application.Abstractions.IUnitOfWork unitOfWork)
    {

        var options0 = (await unitOfWork.UsuarioRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.NombreUsuario, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        var options1 = (await unitOfWork.MazoRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Nombre, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        model.Usuario1Options = options0.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        model.Usuario2Options = options0.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        model.Mazo1Options = options1.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        model.Mazo2Options = options1.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();

    }
    /*PROTECTED REGION END*/

}
