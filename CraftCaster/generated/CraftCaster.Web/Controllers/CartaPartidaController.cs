// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(CartaPartidaController_usings) START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CraftCaster.Domain.Entities;
using CraftCaster.Application.Services;
using CraftCaster.Web.Assemblers;

using CraftCaster.Domain.Enums;
/*PROTECTED REGION END*/

namespace CraftCaster.Web.Controllers;

/*PROTECTED REGION ID(CartaPartidaController_class_attributes) ENABLED START*/
[Authorize]
/*PROTECTED REGION END*/
public partial class CartaPartidaController : Controller
{
    /*PROTECTED REGION ID(CartaPartidaController_dependencies) START*/
    private readonly CartaPartidaService _cartaPartidaService;
    private readonly ILogger<CartaPartidaController> _logger;
    private readonly CraftCaster.Application.Abstractions.IUnitOfWorkFactory _formUnitOfWorkFactory;
    public CartaPartidaController(CartaPartidaService cartaPartidaService, CraftCaster.Application.Abstractions.IUnitOfWorkFactory formUnitOfWorkFactory, ILogger<CartaPartidaController>? logger = null)
    {
        _cartaPartidaService = cartaPartidaService;

        _formUnitOfWorkFactory = formUnitOfWorkFactory;

        _logger = logger ?? global::Microsoft.Extensions.Logging.Abstractions.NullLogger<CartaPartidaController>.Instance;
    }
    /*PROTECTED REGION END*/
    [HttpGet]
    /*PROTECTED REGION ID(CartaPartidaController_Index_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Index()
    {
    /*PROTECTED REGION ID(CartaPartidaController_Index_GET_body) START*/

        var entities = await _cartaPartidaService.ReadAllAsync();
        return View(entities.Select(CartaPartidaAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(CartaPartidaController_Details_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Details(int id)
    {
    /*PROTECTED REGION ID(CartaPartidaController_Details_GET_body) START*/

        var entity = await _cartaPartidaService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(CartaPartidaAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(CartaPartidaController_Create_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create()
    {
    /*PROTECTED REGION ID(CartaPartidaController_Create_GET_body) START*/

        var model = new CraftCaster.Web.ViewModels.CartaPartidaViewModel();

        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(CartaPartidaController_Create_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(CraftCaster.Web.ViewModels.CartaPartidaViewModel model)
    {
    /*PROTECTED REGION ID(CartaPartidaController_Create_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::CraftCaster.Application.Services.CartaPartidaService(unitOfWork);





        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.ZonaId is int selectedZona &&
            !model.ZonaOptions.Any(option => option.Value == Convert.ToString(selectedZona, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("ZonaId", "Selecciona un registro existente de ZonaPartida.");
        if (model.ZonaId is null) ModelState.AddModelError("ZonaId", "Debes seleccionar una opción.");
        if (model.OriginalId is int selectedOriginal &&
            !model.OriginalOptions.Any(option => option.Value == Convert.ToString(selectedOriginal, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("OriginalId", "Selecciona un registro existente de Carta.");
        if (model.OriginalId is null) ModelState.AddModelError("OriginalId", "Debes seleccionar una opción.");




        if (!ModelState.IsValid) return View(model);


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.CartaPartida.Create");


            var entity = await service.NewAsync(model.VidaActual, model.AtaqueActual, model.PuedeAtacar, model.ZonaId.GetValueOrDefault(), model.OriginalId.GetValueOrDefault());

            await service.ApplyCartaPartidaRelationsAsync(entity, p_zona_id: model.ZonaId, p_original_id: model.OriginalId);

            await service.ModifierCartaPartidaDefaultAsync(entity);


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
    /*PROTECTED REGION ID(CartaPartidaController_Edit_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id)
    {
    /*PROTECTED REGION ID(CartaPartidaController_Edit_GET_body) START*/

        var entity = await _cartaPartidaService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        var model = CartaPartidaAssembler.ToViewModel(entity);
        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(CartaPartidaController_Edit_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, CraftCaster.Web.ViewModels.CartaPartidaViewModel model)
    {
    /*PROTECTED REGION ID(CartaPartidaController_Edit_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::CraftCaster.Application.Services.CartaPartidaService(unitOfWork);

        var entity = await service.ReadOIDCartaPartidaDefaultAsync(id);
        if (entity is null) return NotFound();



        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.ZonaId is int selectedZona &&
            !model.ZonaOptions.Any(option => option.Value == Convert.ToString(selectedZona, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("ZonaId", "Selecciona un registro existente de ZonaPartida.");
        if (model.ZonaId is null) ModelState.AddModelError("ZonaId", "Debes seleccionar una opción.");
        if (model.OriginalId is int selectedOriginal &&
            !model.OriginalOptions.Any(option => option.Value == Convert.ToString(selectedOriginal, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("OriginalId", "Selecciona un registro existente de Carta.");
        if (model.OriginalId is null) ModelState.AddModelError("OriginalId", "Debes seleccionar una opción.");




        if (!ModelState.IsValid) return View(model);


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.CartaPartida.Edit");


            await service.ModifyAsync(id, model.VidaActual, model.AtaqueActual, model.PuedeAtacar, model.ZonaId.GetValueOrDefault(), model.OriginalId.GetValueOrDefault());
            entity = await service.ReadOIDCartaPartidaDefaultAsync(id) ?? throw new InvalidOperationException("Entity disappeared during update.");
            await service.ApplyCartaPartidaRelationsAsync(entity, p_zona_id: model.ZonaId, p_original_id: model.OriginalId);

            await service.ModifierCartaPartidaDefaultAsync(entity);


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
    /*PROTECTED REGION ID(CartaPartidaController_Delete_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Delete(int id)
    {
    /*PROTECTED REGION ID(CartaPartidaController_Delete_GET_body) START*/

        var entity = await _cartaPartidaService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(CartaPartidaAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(CartaPartidaController_DeleteConfirmed_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(CartaPartidaController_DeleteConfirmed_POST_body) START*/

        if (!ModelState.IsValid) return View("Delete", new CraftCaster.Web.ViewModels.CartaPartidaViewModel());
        await _cartaPartidaService.DestroyAsync(id);
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    /*PROTECTED REGION ID(CartaPartidaController_method_LoadFormOptionsAsync_CraftCasterWebViewModelsCartaPartidaViewModel) START*/
    private async Task LoadFormOptionsAsync(CraftCaster.Web.ViewModels.CartaPartidaViewModel model)
    {
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        await LoadFormOptionsAsync(model, unitOfWork);
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(CartaPartidaController_method_LoadFormOptionsAsync_CraftCasterWebViewModelsCartaPartidaViewModel_CraftCasterApplicationAbstractionsIUnitOfWork) START*/
    private async Task LoadFormOptionsAsync(CraftCaster.Web.ViewModels.CartaPartidaViewModel model, CraftCaster.Application.Abstractions.IUnitOfWork unitOfWork)
    {

        var options0 = (await unitOfWork.ZonaPartidaRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        var options1 = (await unitOfWork.CartaRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Nombre, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        model.ZonaOptions = options0.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        model.OriginalOptions = options1.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();

    }
    /*PROTECTED REGION END*/

}
