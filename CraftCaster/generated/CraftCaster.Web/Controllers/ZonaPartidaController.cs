// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(ZonaPartidaController_usings) START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CraftCaster.Domain.Entities;
using CraftCaster.Application.Services;
using CraftCaster.Web.Assemblers;

using CraftCaster.Domain.Enums;
/*PROTECTED REGION END*/

namespace CraftCaster.Web.Controllers;

/*PROTECTED REGION ID(ZonaPartidaController_class_attributes) ENABLED START*/
[Authorize]
/*PROTECTED REGION END*/
public partial class ZonaPartidaController : Controller
{
    /*PROTECTED REGION ID(ZonaPartidaController_dependencies) START*/
    private readonly CraftCaster.Application.Abstractions.IUnitOfWorkFactory _useCaseUnitOfWorkFactory;
    private readonly ZonaPartidaService _zonaPartidaService;
    private readonly ILogger<ZonaPartidaController> _logger;
    private readonly CraftCaster.Application.Abstractions.IUnitOfWorkFactory _formUnitOfWorkFactory;
    public ZonaPartidaController(ZonaPartidaService zonaPartidaService, CraftCaster.Application.Abstractions.IUnitOfWorkFactory formUnitOfWorkFactory, CraftCaster.Application.Abstractions.IUnitOfWorkFactory useCaseUnitOfWorkFactory, ILogger<ZonaPartidaController>? logger = null)
    {
        _zonaPartidaService = zonaPartidaService;

        _formUnitOfWorkFactory = formUnitOfWorkFactory;
        _useCaseUnitOfWorkFactory = useCaseUnitOfWorkFactory;
        _logger = logger ?? global::Microsoft.Extensions.Logging.Abstractions.NullLogger<ZonaPartidaController>.Instance;
    }
    /*PROTECTED REGION END*/
    [HttpGet]
    /*PROTECTED REGION ID(ZonaPartidaController_Index_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Index()
    {
    /*PROTECTED REGION ID(ZonaPartidaController_Index_GET_body) START*/

        var entities = await _zonaPartidaService.ReadAllAsync();
        return View(entities.Select(ZonaPartidaAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(ZonaPartidaController_Details_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Details(int id)
    {
    /*PROTECTED REGION ID(ZonaPartidaController_Details_GET_body) START*/

        var entity = await _zonaPartidaService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(ZonaPartidaAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(ZonaPartidaController_Create_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create()
    {
    /*PROTECTED REGION ID(ZonaPartidaController_Create_GET_body) START*/

        var model = new CraftCaster.Web.ViewModels.ZonaPartidaViewModel();

        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ZonaPartidaController_Create_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(CraftCaster.Web.ViewModels.ZonaPartidaViewModel model)
    {
    /*PROTECTED REGION ID(ZonaPartidaController_Create_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::CraftCaster.Application.Services.ZonaPartidaService(unitOfWork);



        model.Cartas ??= new();
        if (model.Cartas.Count > 500) return BadRequest("Número de líneas no válido (máximo 500).");
        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.JugadorId is int selectedJugador &&
            !model.JugadorOptions.Any(option => option.Value == Convert.ToString(selectedJugador, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("JugadorId", "Selecciona un registro existente de JugadorPartida.");
        if (model.JugadorId is null) ModelState.AddModelError("JugadorId", "Debes seleccionar una opción.");

        if (model.Cartas is null) { model.Cartas = new(); ModelState.AddModelError(nameof(model.Cartas), "Debes indicar las líneas."); }
        if (model.Cartas.Count < 0 || model.Cartas.Count > 500)
            ModelState.AddModelError(nameof(model.Cartas), "Número de líneas no válido (máximo 500).");

        for (var index = 0; index < model.Cartas.Count; index++)
        {
            var row = model.Cartas[index];
            if (!EqualityComparer<int>.Default.Equals(row.Id, default!))
                ModelState.AddModelError($"Cartas[{index}].Id", "La línea no pertenece a este registro o está repetida.");
        if (row.OriginalId is int selectedRowCartasOriginal &&
            !row.OriginalOptions.Any(option => option.Value == Convert.ToString(selectedRowCartasOriginal, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError($"Cartas[{index}].OriginalId", "Selecciona un registro existente de Carta.");
        if (row.OriginalId is null) ModelState.AddModelError($"Cartas[{index}].OriginalId", "Debes seleccionar una opción.");
        }


        if (!ModelState.IsValid) return View(model);


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.ZonaPartida.Create");
            var submittedCartas = await service.PrepareZonaPartidaCartasAsync(null,
                model.Cartas.Select(row => (row.Id, CartaPartidaAssembler.Create(row), row.OriginalId)).ToList());

            var entity = await service.NewAsync(model.TipoZona, model.JugadorId.GetValueOrDefault());

            await service.ApplyZonaPartidaRelationsAsync(entity, p_jugador_id: model.JugadorId, p_cartas: submittedCartas);

            await service.ModifierZonaPartidaDefaultAsync(entity);


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
    /*PROTECTED REGION ID(ZonaPartidaController_Edit_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id)
    {
    /*PROTECTED REGION ID(ZonaPartidaController_Edit_GET_body) START*/

        var entity = await _zonaPartidaService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        var model = ZonaPartidaAssembler.ToViewModel(entity);
        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ZonaPartidaController_Edit_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, CraftCaster.Web.ViewModels.ZonaPartidaViewModel model)
    {
    /*PROTECTED REGION ID(ZonaPartidaController_Edit_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::CraftCaster.Application.Services.ZonaPartidaService(unitOfWork);

        var entity = await service.ReadOIDZonaPartidaDefaultAsync(id);
        if (entity is null) return NotFound();

        model.Cartas ??= new();
        if (model.Cartas.Count > 500) return BadRequest("Número de líneas no válido (máximo 500).");
        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.JugadorId is int selectedJugador &&
            !model.JugadorOptions.Any(option => option.Value == Convert.ToString(selectedJugador, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("JugadorId", "Selecciona un registro existente de JugadorPartida.");
        if (model.JugadorId is null) ModelState.AddModelError("JugadorId", "Debes seleccionar una opción.");

        if (model.Cartas is null) { model.Cartas = new(); ModelState.AddModelError(nameof(model.Cartas), "Debes indicar las líneas."); }
        if (model.Cartas.Count < 0 || model.Cartas.Count > 500)
            ModelState.AddModelError(nameof(model.Cartas), "Número de líneas no válido (máximo 500).");
        var seenCartas = new HashSet<int>();
        for (var index = 0; index < model.Cartas.Count; index++)
        {
            var row = model.Cartas[index];
            if (!EqualityComparer<int>.Default.Equals(row.Id, default!) &&
                (!entity.Cartas.Any(child => child.Id.Equals(row.Id)) || !seenCartas.Add(row.Id)))
                ModelState.AddModelError($"Cartas[{index}].Id", "La línea no pertenece a este registro o está repetida.");
        if (row.OriginalId is int selectedRowCartasOriginal &&
            !row.OriginalOptions.Any(option => option.Value == Convert.ToString(selectedRowCartasOriginal, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError($"Cartas[{index}].OriginalId", "Selecciona un registro existente de Carta.");
        if (row.OriginalId is null) ModelState.AddModelError($"Cartas[{index}].OriginalId", "Debes seleccionar una opción.");
        }


        if (!ModelState.IsValid) return View(model);


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.ZonaPartida.Edit");
            var submittedCartas = await service.PrepareZonaPartidaCartasAsync(entity,
                model.Cartas.Select(row => (row.Id, CartaPartidaAssembler.Create(row), row.OriginalId)).ToList());

            await service.ModifyAsync(id, model.TipoZona, model.JugadorId.GetValueOrDefault());
            entity = await service.ReadOIDZonaPartidaDefaultAsync(id) ?? throw new InvalidOperationException("Entity disappeared during update.");
            await service.ApplyZonaPartidaRelationsAsync(entity, p_jugador_id: model.JugadorId, p_cartas: submittedCartas);

            await service.ModifierZonaPartidaDefaultAsync(entity);


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
    /*PROTECTED REGION ID(ZonaPartidaController_Delete_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Delete(int id)
    {
    /*PROTECTED REGION ID(ZonaPartidaController_Delete_GET_body) START*/

        var entity = await _zonaPartidaService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(ZonaPartidaAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ZonaPartidaController_DeleteConfirmed_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(ZonaPartidaController_DeleteConfirmed_POST_body) START*/

        if (!ModelState.IsValid) return View("Delete", new CraftCaster.Web.ViewModels.ZonaPartidaViewModel());
        await _zonaPartidaService.DestroyAsync(id);
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ZonaPartidaController_AddCarta_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> AddCarta(int p_oid, IReadOnlyList<int> p_cartaPartida_oids)
    {
    /*PROTECTED REGION ID(ZonaPartidaController_AddCarta_POST_body) START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _zonaPartidaService.AddCartaAsync(p_oid, p_cartaPartida_oids);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    /*PROTECTED REGION ID(ZonaPartidaController_method_LoadFormOptionsAsync_CraftCasterWebViewModelsZonaPartidaViewModel) START*/
    private async Task LoadFormOptionsAsync(CraftCaster.Web.ViewModels.ZonaPartidaViewModel model)
    {
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        await LoadFormOptionsAsync(model, unitOfWork);
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(ZonaPartidaController_method_LoadFormOptionsAsync_CraftCasterWebViewModelsZonaPartidaViewModel_CraftCasterApplicationAbstractionsIUnitOfWork) START*/
    private async Task LoadFormOptionsAsync(CraftCaster.Web.ViewModels.ZonaPartidaViewModel model, CraftCaster.Application.Abstractions.IUnitOfWork unitOfWork)
    {
        if (model.Cartas.Count > 500) { ModelState.AddModelError(nameof(model.Cartas), "Máximo 500 líneas."); return; }
        var options0 = (await unitOfWork.JugadorPartidaRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        var options1 = (await unitOfWork.CartaRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Nombre, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        model.JugadorOptions = options0.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        foreach (var row in model.Cartas)
        {
        row.OriginalOptions = options1.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        }
        model.CartasTemplate.OriginalOptions = options1.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
    }
    /*PROTECTED REGION END*/

}
