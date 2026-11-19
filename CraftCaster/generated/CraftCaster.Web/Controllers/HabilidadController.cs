// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(HabilidadController_usings) START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CraftCaster.Domain.Entities;
using CraftCaster.Application.Services;
using CraftCaster.Web.Assemblers;

using CraftCaster.Domain.Enums;
/*PROTECTED REGION END*/

namespace CraftCaster.Web.Controllers;

/*PROTECTED REGION ID(HabilidadController_class_attributes) ENABLED START*/
[Authorize]
/*PROTECTED REGION END*/
public partial class HabilidadController : Controller
{
    /*PROTECTED REGION ID(HabilidadController_dependencies) START*/
    private readonly CraftCaster.Application.Abstractions.IUnitOfWorkFactory _useCaseUnitOfWorkFactory;
    private readonly HabilidadService _habilidadService;
    private readonly ILogger<HabilidadController> _logger;
    private readonly CraftCaster.Application.Abstractions.IUnitOfWorkFactory _formUnitOfWorkFactory;
    public HabilidadController(HabilidadService habilidadService, CraftCaster.Application.Abstractions.IUnitOfWorkFactory formUnitOfWorkFactory, CraftCaster.Application.Abstractions.IUnitOfWorkFactory useCaseUnitOfWorkFactory, ILogger<HabilidadController>? logger = null)
    {
        _habilidadService = habilidadService;

        _formUnitOfWorkFactory = formUnitOfWorkFactory;
        _useCaseUnitOfWorkFactory = useCaseUnitOfWorkFactory;
        _logger = logger ?? global::Microsoft.Extensions.Logging.Abstractions.NullLogger<HabilidadController>.Instance;
    }
    /*PROTECTED REGION END*/
    [HttpGet]
    /*PROTECTED REGION ID(HabilidadController_Index_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Index()
    {
    /*PROTECTED REGION ID(HabilidadController_Index_GET_body) START*/

        var entities = await _habilidadService.ReadAllAsync();
        return View(entities.Select(HabilidadAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(HabilidadController_Details_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Details(int id)
    {
    /*PROTECTED REGION ID(HabilidadController_Details_GET_body) START*/

        var entity = await _habilidadService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(HabilidadAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(HabilidadController_Create_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create()
    {
    /*PROTECTED REGION ID(HabilidadController_Create_GET_body) START*/

        var model = new CraftCaster.Web.ViewModels.HabilidadViewModel();

        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(HabilidadController_Create_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(CraftCaster.Web.ViewModels.HabilidadViewModel model)
    {
    /*PROTECTED REGION ID(HabilidadController_Create_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::CraftCaster.Application.Services.HabilidadService(unitOfWork);





        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.EstadoId is int selectedEstado &&
            !model.EstadoOptions.Any(option => option.Value == Convert.ToString(selectedEstado, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("EstadoId", "Selecciona un registro existente de Estado.");
        if (model.EstadoId is null) ModelState.AddModelError("EstadoId", "Debes seleccionar una opción.");




        if (!ModelState.IsValid) return View(model);


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.Habilidad.Create");


            var entity = await service.NewAsync(model.Tipo, model.Cantidad, model.TipoObjetivo, model.Objetivo, model.EstadoId.GetValueOrDefault());

            await service.ApplyHabilidadRelationsAsync(entity, p_estado_id: model.EstadoId);

            await service.ModifierHabilidadDefaultAsync(entity);


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
    /*PROTECTED REGION ID(HabilidadController_Edit_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id)
    {
    /*PROTECTED REGION ID(HabilidadController_Edit_GET_body) START*/

        var entity = await _habilidadService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        var model = HabilidadAssembler.ToViewModel(entity);
        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(HabilidadController_Edit_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, CraftCaster.Web.ViewModels.HabilidadViewModel model)
    {
    /*PROTECTED REGION ID(HabilidadController_Edit_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::CraftCaster.Application.Services.HabilidadService(unitOfWork);

        var entity = await service.ReadOIDHabilidadDefaultAsync(id);
        if (entity is null) return NotFound();



        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.EstadoId is int selectedEstado &&
            !model.EstadoOptions.Any(option => option.Value == Convert.ToString(selectedEstado, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("EstadoId", "Selecciona un registro existente de Estado.");
        if (model.EstadoId is null) ModelState.AddModelError("EstadoId", "Debes seleccionar una opción.");




        if (!ModelState.IsValid) return View(model);


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.Habilidad.Edit");


            await service.ModifyAsync(id, model.Tipo, model.Cantidad, model.TipoObjetivo, model.Objetivo, model.EstadoId.GetValueOrDefault());
            entity = await service.ReadOIDHabilidadDefaultAsync(id) ?? throw new InvalidOperationException("Entity disappeared during update.");
            await service.ApplyHabilidadRelationsAsync(entity, p_estado_id: model.EstadoId);

            await service.ModifierHabilidadDefaultAsync(entity);


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
    /*PROTECTED REGION ID(HabilidadController_Delete_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Delete(int id)
    {
    /*PROTECTED REGION ID(HabilidadController_Delete_GET_body) START*/

        var entity = await _habilidadService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(HabilidadAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(HabilidadController_DeleteConfirmed_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(HabilidadController_DeleteConfirmed_POST_body) START*/

        if (!ModelState.IsValid) return View("Delete", new CraftCaster.Web.ViewModels.HabilidadViewModel());
        await _habilidadService.DestroyAsync(id);
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    /*PROTECTED REGION ID(HabilidadController_method_LoadFormOptionsAsync_CraftCasterWebViewModelsHabilidadViewModel) START*/
    private async Task LoadFormOptionsAsync(CraftCaster.Web.ViewModels.HabilidadViewModel model)
    {
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        await LoadFormOptionsAsync(model, unitOfWork);
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(HabilidadController_method_LoadFormOptionsAsync_CraftCasterWebViewModelsHabilidadViewModel_CraftCasterApplicationAbstractionsIUnitOfWork) START*/
    private async Task LoadFormOptionsAsync(CraftCaster.Web.ViewModels.HabilidadViewModel model, CraftCaster.Application.Abstractions.IUnitOfWork unitOfWork)
    {

        var options0 = (await unitOfWork.EstadoRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        model.EstadoOptions = options0.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();

    }
    /*PROTECTED REGION END*/

}
