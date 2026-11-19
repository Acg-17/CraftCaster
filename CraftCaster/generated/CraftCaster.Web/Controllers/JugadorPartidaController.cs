// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(JugadorPartidaController_usings) START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CraftCaster.Domain.Entities;
using CraftCaster.Application.Services;
using CraftCaster.Web.Assemblers;

using CraftCaster.Domain.Enums;
/*PROTECTED REGION END*/

namespace CraftCaster.Web.Controllers;

/*PROTECTED REGION ID(JugadorPartidaController_class_attributes) ENABLED START*/
[Authorize]
/*PROTECTED REGION END*/
public partial class JugadorPartidaController : Controller
{
    /*PROTECTED REGION ID(JugadorPartidaController_dependencies) START*/
    private readonly CraftCaster.Application.Abstractions.IUnitOfWorkFactory _useCaseUnitOfWorkFactory;
    private readonly JugadorPartidaService _jugadorPartidaService;
    private readonly ILogger<JugadorPartidaController> _logger;
    private readonly CraftCaster.Application.Abstractions.IUnitOfWorkFactory _formUnitOfWorkFactory;
    public JugadorPartidaController(JugadorPartidaService jugadorPartidaService, CraftCaster.Application.Abstractions.IUnitOfWorkFactory formUnitOfWorkFactory, CraftCaster.Application.Abstractions.IUnitOfWorkFactory useCaseUnitOfWorkFactory, ILogger<JugadorPartidaController>? logger = null)
    {
        _jugadorPartidaService = jugadorPartidaService;

        _formUnitOfWorkFactory = formUnitOfWorkFactory;
        _useCaseUnitOfWorkFactory = useCaseUnitOfWorkFactory;
        _logger = logger ?? global::Microsoft.Extensions.Logging.Abstractions.NullLogger<JugadorPartidaController>.Instance;
    }
    /*PROTECTED REGION END*/
    [HttpGet]
    /*PROTECTED REGION ID(JugadorPartidaController_Index_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Index()
    {
    /*PROTECTED REGION ID(JugadorPartidaController_Index_GET_body) START*/

        var entities = await _jugadorPartidaService.ReadAllAsync();
        return View(entities.Select(JugadorPartidaAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(JugadorPartidaController_Details_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Details(int id)
    {
    /*PROTECTED REGION ID(JugadorPartidaController_Details_GET_body) START*/

        var entity = await _jugadorPartidaService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(JugadorPartidaAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(JugadorPartidaController_Create_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create()
    {
    /*PROTECTED REGION ID(JugadorPartidaController_Create_GET_body) START*/

        var model = new CraftCaster.Web.ViewModels.JugadorPartidaViewModel();

        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(JugadorPartidaController_Create_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(CraftCaster.Web.ViewModels.JugadorPartidaViewModel model)
    {
    /*PROTECTED REGION ID(JugadorPartidaController_Create_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::CraftCaster.Application.Services.JugadorPartidaService(unitOfWork);





        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.PartidaId is int selectedPartida &&
            !model.PartidaOptions.Any(option => option.Value == Convert.ToString(selectedPartida, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("PartidaId", "Selecciona un registro existente de Partida.");
        if (model.PartidaId is null) ModelState.AddModelError("PartidaId", "Debes seleccionar una opción.");




        if (!ModelState.IsValid) return View(model);


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.JugadorPartida.Create");


            var entity = await service.NewAsync(model.Vida, model.NumJugador, model.Mana, model.Activo, model.PartidaId.GetValueOrDefault());

            await service.ApplyJugadorPartidaRelationsAsync(entity, p_partida_id: model.PartidaId);

            await service.ModifierJugadorPartidaDefaultAsync(entity);


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
    /*PROTECTED REGION ID(JugadorPartidaController_Edit_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id)
    {
    /*PROTECTED REGION ID(JugadorPartidaController_Edit_GET_body) START*/

        var entity = await _jugadorPartidaService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        var model = JugadorPartidaAssembler.ToViewModel(entity);
        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(JugadorPartidaController_Edit_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, CraftCaster.Web.ViewModels.JugadorPartidaViewModel model)
    {
    /*PROTECTED REGION ID(JugadorPartidaController_Edit_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::CraftCaster.Application.Services.JugadorPartidaService(unitOfWork);

        var entity = await service.ReadOIDJugadorPartidaDefaultAsync(id);
        if (entity is null) return NotFound();



        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.PartidaId is int selectedPartida &&
            !model.PartidaOptions.Any(option => option.Value == Convert.ToString(selectedPartida, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("PartidaId", "Selecciona un registro existente de Partida.");
        if (model.PartidaId is null) ModelState.AddModelError("PartidaId", "Debes seleccionar una opción.");




        if (!ModelState.IsValid) return View(model);


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.JugadorPartida.Edit");


            await service.ModifyAsync(id, model.Vida, model.NumJugador, model.Mana, model.Activo, model.PartidaId.GetValueOrDefault());
            entity = await service.ReadOIDJugadorPartidaDefaultAsync(id) ?? throw new InvalidOperationException("Entity disappeared during update.");
            await service.ApplyJugadorPartidaRelationsAsync(entity, p_partida_id: model.PartidaId);

            await service.ModifierJugadorPartidaDefaultAsync(entity);


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
    /*PROTECTED REGION ID(JugadorPartidaController_Delete_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Delete(int id)
    {
    /*PROTECTED REGION ID(JugadorPartidaController_Delete_GET_body) START*/

        var entity = await _jugadorPartidaService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(JugadorPartidaAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(JugadorPartidaController_DeleteConfirmed_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(JugadorPartidaController_DeleteConfirmed_POST_body) START*/

        if (!ModelState.IsValid) return View("Delete", new CraftCaster.Web.ViewModels.JugadorPartidaViewModel());
        await _jugadorPartidaService.DestroyAsync(id);
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(JugadorPartidaController_AddZona_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> AddZona(int p_oid, IReadOnlyList<int> p_zonaPartida_oids)
    {
    /*PROTECTED REGION ID(JugadorPartidaController_AddZona_POST_body) START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _jugadorPartidaService.AddZonaAsync(p_oid, p_zonaPartida_oids);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    /*PROTECTED REGION ID(JugadorPartidaController_method_LoadFormOptionsAsync_CraftCasterWebViewModelsJugadorPartidaViewModel) START*/
    private async Task LoadFormOptionsAsync(CraftCaster.Web.ViewModels.JugadorPartidaViewModel model)
    {
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        await LoadFormOptionsAsync(model, unitOfWork);
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(JugadorPartidaController_method_LoadFormOptionsAsync_CraftCasterWebViewModelsJugadorPartidaViewModel_CraftCasterApplicationAbstractionsIUnitOfWork) START*/
    private async Task LoadFormOptionsAsync(CraftCaster.Web.ViewModels.JugadorPartidaViewModel model, CraftCaster.Application.Abstractions.IUnitOfWork unitOfWork)
    {

        var options0 = (await unitOfWork.PartidaRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        model.PartidaOptions = options0.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();

    }
    /*PROTECTED REGION END*/

}
