// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(LineaMazoController_usings) START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CraftCaster.Domain.Entities;
using CraftCaster.Application.Services;
using CraftCaster.Web.Assemblers;

using CraftCaster.Domain.Enums;
/*PROTECTED REGION END*/

namespace CraftCaster.Web.Controllers;

/*PROTECTED REGION ID(LineaMazoController_class_attributes) ENABLED START*/
[Authorize]
/*PROTECTED REGION END*/
public partial class LineaMazoController : Controller
{
    /*PROTECTED REGION ID(LineaMazoController_dependencies) START*/
    private readonly LineaMazoService _lineaMazoService;
    private readonly ILogger<LineaMazoController> _logger;
    private readonly CraftCaster.Application.Abstractions.IUnitOfWorkFactory _formUnitOfWorkFactory;
    public LineaMazoController(LineaMazoService lineaMazoService, CraftCaster.Application.Abstractions.IUnitOfWorkFactory formUnitOfWorkFactory, ILogger<LineaMazoController>? logger = null)
    {
        _lineaMazoService = lineaMazoService;

        _formUnitOfWorkFactory = formUnitOfWorkFactory;

        _logger = logger ?? global::Microsoft.Extensions.Logging.Abstractions.NullLogger<LineaMazoController>.Instance;
    }
    /*PROTECTED REGION END*/
    [HttpGet]
    /*PROTECTED REGION ID(LineaMazoController_Index_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Index()
    {
    /*PROTECTED REGION ID(LineaMazoController_Index_GET_body) START*/

        var entities = await _lineaMazoService.ReadAllAsync();
        return View(entities.Select(LineaMazoAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(LineaMazoController_Details_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Details(int id)
    {
    /*PROTECTED REGION ID(LineaMazoController_Details_GET_body) START*/

        var entity = await _lineaMazoService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(LineaMazoAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(LineaMazoController_Create_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create()
    {
    /*PROTECTED REGION ID(LineaMazoController_Create_GET_body) START*/

        var model = new CraftCaster.Web.ViewModels.LineaMazoViewModel();

        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(LineaMazoController_Create_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(CraftCaster.Web.ViewModels.LineaMazoViewModel model)
    {
    /*PROTECTED REGION ID(LineaMazoController_Create_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::CraftCaster.Application.Services.LineaMazoService(unitOfWork);





        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.MazoId is int selectedMazo &&
            !model.MazoOptions.Any(option => option.Value == Convert.ToString(selectedMazo, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("MazoId", "Selecciona un registro existente de Mazo.");
        if (model.MazoId is null) ModelState.AddModelError("MazoId", "Debes seleccionar una opción.");
        if (model.CartaId is int selectedCarta &&
            !model.CartaOptions.Any(option => option.Value == Convert.ToString(selectedCarta, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("CartaId", "Selecciona un registro existente de Carta.");
        if (model.CartaId is null) ModelState.AddModelError("CartaId", "Debes seleccionar una opción.");




        if (!ModelState.IsValid) return View(model);


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.LineaMazo.Create");


            var entity = await service.NewAsync(model.Cantidad, model.MazoId.GetValueOrDefault(), model.CartaId.GetValueOrDefault());

            await service.ApplyLineaMazoRelationsAsync(entity, p_mazo_id: model.MazoId, p_carta_id: model.CartaId);

            await service.ModifierLineaMazoDefaultAsync(entity);


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
    /*PROTECTED REGION ID(LineaMazoController_Edit_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id)
    {
    /*PROTECTED REGION ID(LineaMazoController_Edit_GET_body) START*/

        var entity = await _lineaMazoService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        var model = LineaMazoAssembler.ToViewModel(entity);
        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(LineaMazoController_Edit_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, CraftCaster.Web.ViewModels.LineaMazoViewModel model)
    {
    /*PROTECTED REGION ID(LineaMazoController_Edit_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::CraftCaster.Application.Services.LineaMazoService(unitOfWork);

        var entity = await service.ReadOIDLineaMazoDefaultAsync(id);
        if (entity is null) return NotFound();



        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.MazoId is int selectedMazo &&
            !model.MazoOptions.Any(option => option.Value == Convert.ToString(selectedMazo, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("MazoId", "Selecciona un registro existente de Mazo.");
        if (model.MazoId is null) ModelState.AddModelError("MazoId", "Debes seleccionar una opción.");
        if (model.CartaId is int selectedCarta &&
            !model.CartaOptions.Any(option => option.Value == Convert.ToString(selectedCarta, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("CartaId", "Selecciona un registro existente de Carta.");
        if (model.CartaId is null) ModelState.AddModelError("CartaId", "Debes seleccionar una opción.");




        if (!ModelState.IsValid) return View(model);


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.LineaMazo.Edit");


            await service.ModifyAsync(id, model.Cantidad, model.MazoId.GetValueOrDefault(), model.CartaId.GetValueOrDefault());
            entity = await service.ReadOIDLineaMazoDefaultAsync(id) ?? throw new InvalidOperationException("Entity disappeared during update.");
            await service.ApplyLineaMazoRelationsAsync(entity, p_mazo_id: model.MazoId, p_carta_id: model.CartaId);

            await service.ModifierLineaMazoDefaultAsync(entity);


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
    /*PROTECTED REGION ID(LineaMazoController_Delete_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Delete(int id)
    {
    /*PROTECTED REGION ID(LineaMazoController_Delete_GET_body) START*/

        var entity = await _lineaMazoService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(LineaMazoAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(LineaMazoController_DeleteConfirmed_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(LineaMazoController_DeleteConfirmed_POST_body) START*/

        if (!ModelState.IsValid) return View("Delete", new CraftCaster.Web.ViewModels.LineaMazoViewModel());
        await _lineaMazoService.DestroyAsync(id);
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    /*PROTECTED REGION ID(LineaMazoController_method_LoadFormOptionsAsync_CraftCasterWebViewModelsLineaMazoViewModel) START*/
    private async Task LoadFormOptionsAsync(CraftCaster.Web.ViewModels.LineaMazoViewModel model)
    {
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        await LoadFormOptionsAsync(model, unitOfWork);
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(LineaMazoController_method_LoadFormOptionsAsync_CraftCasterWebViewModelsLineaMazoViewModel_CraftCasterApplicationAbstractionsIUnitOfWork) START*/
    private async Task LoadFormOptionsAsync(CraftCaster.Web.ViewModels.LineaMazoViewModel model, CraftCaster.Application.Abstractions.IUnitOfWork unitOfWork)
    {

        var options0 = (await unitOfWork.MazoRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Nombre, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        var options1 = (await unitOfWork.CartaRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Nombre, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        model.MazoOptions = options0.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        model.CartaOptions = options1.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();

    }
    /*PROTECTED REGION END*/

}
