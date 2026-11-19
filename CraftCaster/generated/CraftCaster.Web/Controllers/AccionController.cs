// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(AccionController_usings) START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CraftCaster.Domain.Entities;
using CraftCaster.Application.Services;
using CraftCaster.Web.Assemblers;

using CraftCaster.Domain.Enums;
/*PROTECTED REGION END*/

namespace CraftCaster.Web.Controllers;

/*PROTECTED REGION ID(AccionController_class_attributes) ENABLED START*/
[Authorize]
/*PROTECTED REGION END*/
public partial class AccionController : Controller
{
    /*PROTECTED REGION ID(AccionController_dependencies) START*/
    private readonly AccionService _accionService;
    private readonly ILogger<AccionController> _logger;
    private readonly CraftCaster.Application.Abstractions.IUnitOfWorkFactory _formUnitOfWorkFactory;
    public AccionController(AccionService accionService, CraftCaster.Application.Abstractions.IUnitOfWorkFactory formUnitOfWorkFactory, ILogger<AccionController>? logger = null)
    {
        _accionService = accionService;

        _formUnitOfWorkFactory = formUnitOfWorkFactory;

        _logger = logger ?? global::Microsoft.Extensions.Logging.Abstractions.NullLogger<AccionController>.Instance;
    }
    /*PROTECTED REGION END*/
    [HttpGet]
    /*PROTECTED REGION ID(AccionController_Index_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Index()
    {
    /*PROTECTED REGION ID(AccionController_Index_GET_body) START*/

        var entities = await _accionService.ReadAllAsync();
        return View(entities.Select(AccionAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(AccionController_Details_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Details(int id)
    {
    /*PROTECTED REGION ID(AccionController_Details_GET_body) START*/

        var entity = await _accionService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(AccionAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(AccionController_Create_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create()
    {
    /*PROTECTED REGION ID(AccionController_Create_GET_body) START*/

        var model = new CraftCaster.Web.ViewModels.AccionViewModel();

        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(AccionController_Create_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(CraftCaster.Web.ViewModels.AccionViewModel model)
    {
    /*PROTECTED REGION ID(AccionController_Create_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::CraftCaster.Application.Services.AccionService(unitOfWork);





        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.TurnoId is int selectedTurno &&
            !model.TurnoOptions.Any(option => option.Value == Convert.ToString(selectedTurno, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("TurnoId", "Selecciona un registro existente de Turno.");
        if (model.TurnoId is null) ModelState.AddModelError("TurnoId", "Debes seleccionar una opción.");
        if (model.OrigenId is int selectedOrigen &&
            !model.OrigenOptions.Any(option => option.Value == Convert.ToString(selectedOrigen, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("OrigenId", "Selecciona un registro existente de CartaPartida.");





        if (!ModelState.IsValid) return View(model);


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.Accion.Create");


            var entity = await service.NewAsync(model.Name, model.TurnoId.GetValueOrDefault());

            await service.ApplyAccionRelationsAsync(entity, p_turno_id: model.TurnoId, p_origen_id: model.OrigenId);

            await service.ModifierAccionDefaultAsync(entity);


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
    /*PROTECTED REGION ID(AccionController_Edit_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id)
    {
    /*PROTECTED REGION ID(AccionController_Edit_GET_body) START*/

        var entity = await _accionService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        var model = AccionAssembler.ToViewModel(entity);
        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(AccionController_Edit_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, CraftCaster.Web.ViewModels.AccionViewModel model)
    {
    /*PROTECTED REGION ID(AccionController_Edit_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::CraftCaster.Application.Services.AccionService(unitOfWork);

        var entity = await service.ReadOIDAccionDefaultAsync(id);
        if (entity is null) return NotFound();



        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.TurnoId is int selectedTurno &&
            !model.TurnoOptions.Any(option => option.Value == Convert.ToString(selectedTurno, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("TurnoId", "Selecciona un registro existente de Turno.");
        if (model.TurnoId is null) ModelState.AddModelError("TurnoId", "Debes seleccionar una opción.");
        if (model.OrigenId is int selectedOrigen &&
            !model.OrigenOptions.Any(option => option.Value == Convert.ToString(selectedOrigen, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("OrigenId", "Selecciona un registro existente de CartaPartida.");





        if (!ModelState.IsValid) return View(model);


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.Accion.Edit");


            await service.ModifyAsync(id, model.Name, model.TurnoId.GetValueOrDefault());
            entity = await service.ReadOIDAccionDefaultAsync(id) ?? throw new InvalidOperationException("Entity disappeared during update.");
            await service.ApplyAccionRelationsAsync(entity, p_turno_id: model.TurnoId, p_origen_id: model.OrigenId);

            await service.ModifierAccionDefaultAsync(entity);


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
    /*PROTECTED REGION ID(AccionController_Delete_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Delete(int id)
    {
    /*PROTECTED REGION ID(AccionController_Delete_GET_body) START*/

        var entity = await _accionService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(AccionAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(AccionController_DeleteConfirmed_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(AccionController_DeleteConfirmed_POST_body) START*/

        if (!ModelState.IsValid) return View("Delete", new CraftCaster.Web.ViewModels.AccionViewModel());
        await _accionService.DestroyAsync(id);
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(AccionController_AddOrigen_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> AddOrigen(int p_oid, int p_cartaPartida_oid)
    {
    /*PROTECTED REGION ID(AccionController_AddOrigen_POST_body) START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _accionService.AddOrigenAsync(p_oid, p_cartaPartida_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(AccionController_RemoveOrigen_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> RemoveOrigen(int p_oid, int p_cartaPartida_oid)
    {
    /*PROTECTED REGION ID(AccionController_RemoveOrigen_POST_body) START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _accionService.RemoveOrigenAsync(p_oid, p_cartaPartida_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(AccionController_AddObjetivo_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> AddObjetivo(int p_oid, IReadOnlyList<int> p_cartaPartida_oids)
    {
    /*PROTECTED REGION ID(AccionController_AddObjetivo_POST_body) START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _accionService.AddObjetivoAsync(p_oid, p_cartaPartida_oids);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(AccionController_RemoveObjetivo_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> RemoveObjetivo(int p_oid, IReadOnlyList<int> p_cartaPartida_oids)
    {
    /*PROTECTED REGION ID(AccionController_RemoveObjetivo_POST_body) START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _accionService.RemoveObjetivoAsync(p_oid, p_cartaPartida_oids);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    /*PROTECTED REGION ID(AccionController_method_LoadFormOptionsAsync_CraftCasterWebViewModelsAccionViewModel) START*/
    private async Task LoadFormOptionsAsync(CraftCaster.Web.ViewModels.AccionViewModel model)
    {
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        await LoadFormOptionsAsync(model, unitOfWork);
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(AccionController_method_LoadFormOptionsAsync_CraftCasterWebViewModelsAccionViewModel_CraftCasterApplicationAbstractionsIUnitOfWork) START*/
    private async Task LoadFormOptionsAsync(CraftCaster.Web.ViewModels.AccionViewModel model, CraftCaster.Application.Abstractions.IUnitOfWork unitOfWork)
    {

        var options0 = (await unitOfWork.TurnoRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        var options1 = (await unitOfWork.CartaPartidaRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        model.TurnoOptions = options0.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        model.OrigenOptions = options1.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();

    }
    /*PROTECTED REGION END*/

}
