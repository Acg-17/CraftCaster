// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(EstadoController_usings) START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CraftCaster.Domain.Entities;
using CraftCaster.Application.Services;
using CraftCaster.Web.Assemblers;

using CraftCaster.Domain.Enums;
/*PROTECTED REGION END*/

namespace CraftCaster.Web.Controllers;

/*PROTECTED REGION ID(EstadoController_class_attributes) ENABLED START*/
[Authorize]
/*PROTECTED REGION END*/
public partial class EstadoController : Controller
{
    /*PROTECTED REGION ID(EstadoController_dependencies) START*/
    private readonly EstadoService _estadoService;
    private readonly ILogger<EstadoController> _logger;
    private readonly CraftCaster.Application.Abstractions.IUnitOfWorkFactory _formUnitOfWorkFactory;
    public EstadoController(EstadoService estadoService, CraftCaster.Application.Abstractions.IUnitOfWorkFactory formUnitOfWorkFactory, ILogger<EstadoController>? logger = null)
    {
        _estadoService = estadoService;

        _formUnitOfWorkFactory = formUnitOfWorkFactory;

        _logger = logger ?? global::Microsoft.Extensions.Logging.Abstractions.NullLogger<EstadoController>.Instance;
    }
    /*PROTECTED REGION END*/
    [HttpGet]
    /*PROTECTED REGION ID(EstadoController_Index_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Index()
    {
    /*PROTECTED REGION ID(EstadoController_Index_GET_body) START*/

        var entities = await _estadoService.ReadAllAsync();
        return View(entities.Select(EstadoAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(EstadoController_Details_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Details(int id)
    {
    /*PROTECTED REGION ID(EstadoController_Details_GET_body) START*/

        var entity = await _estadoService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(EstadoAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(EstadoController_Create_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create()
    {
    /*PROTECTED REGION ID(EstadoController_Create_GET_body) START*/

        var model = new CraftCaster.Web.ViewModels.EstadoViewModel();

        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(EstadoController_Create_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(CraftCaster.Web.ViewModels.EstadoViewModel model)
    {
    /*PROTECTED REGION ID(EstadoController_Create_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::CraftCaster.Application.Services.EstadoService(unitOfWork);



        model.Habilidades ??= new();
        if (model.Habilidades.Count > 500) return BadRequest("Número de líneas no válido (máximo 500).");
        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.CartaId is int selectedCarta &&
            !model.CartaOptions.Any(option => option.Value == Convert.ToString(selectedCarta, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("CartaId", "Selecciona un registro existente de Carta.");
        if (model.CartaId is null) ModelState.AddModelError("CartaId", "Debes seleccionar una opción.");

        if (model.Habilidades is null) { model.Habilidades = new(); ModelState.AddModelError(nameof(model.Habilidades), "Debes indicar las líneas."); }
        if (model.Habilidades.Count < 0 || model.Habilidades.Count > 500)
            ModelState.AddModelError(nameof(model.Habilidades), "Número de líneas no válido (máximo 500).");

        for (var index = 0; index < model.Habilidades.Count; index++)
        {
            var row = model.Habilidades[index];
            if (!EqualityComparer<int>.Default.Equals(row.Id, default!))
                ModelState.AddModelError($"Habilidades[{index}].Id", "La línea no pertenece a este registro o está repetida.");

        }


        if (!ModelState.IsValid) return View(model);


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.Estado.Create");
            var submittedHabilidades = await service.PrepareEstadoHabilidadesAsync(null,
                model.Habilidades.Select(row => (row.Id, HabilidadAssembler.Create(row))).ToList());

            var entity = await service.NewAsync(model.Tipo, model.CartaId.GetValueOrDefault());

            await service.ApplyEstadoRelationsAsync(entity, p_carta_id: model.CartaId, p_habilidades: submittedHabilidades);

            await service.ModifierEstadoDefaultAsync(entity);


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
    /*PROTECTED REGION ID(EstadoController_Edit_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id)
    {
    /*PROTECTED REGION ID(EstadoController_Edit_GET_body) START*/

        var entity = await _estadoService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        var model = EstadoAssembler.ToViewModel(entity);
        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(EstadoController_Edit_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, CraftCaster.Web.ViewModels.EstadoViewModel model)
    {
    /*PROTECTED REGION ID(EstadoController_Edit_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::CraftCaster.Application.Services.EstadoService(unitOfWork);

        var entity = await service.ReadOIDEstadoDefaultAsync(id);
        if (entity is null) return NotFound();

        model.Habilidades ??= new();
        if (model.Habilidades.Count > 500) return BadRequest("Número de líneas no válido (máximo 500).");
        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.CartaId is int selectedCarta &&
            !model.CartaOptions.Any(option => option.Value == Convert.ToString(selectedCarta, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("CartaId", "Selecciona un registro existente de Carta.");
        if (model.CartaId is null) ModelState.AddModelError("CartaId", "Debes seleccionar una opción.");

        if (model.Habilidades is null) { model.Habilidades = new(); ModelState.AddModelError(nameof(model.Habilidades), "Debes indicar las líneas."); }
        if (model.Habilidades.Count < 0 || model.Habilidades.Count > 500)
            ModelState.AddModelError(nameof(model.Habilidades), "Número de líneas no válido (máximo 500).");
        var seenHabilidades = new HashSet<int>();
        for (var index = 0; index < model.Habilidades.Count; index++)
        {
            var row = model.Habilidades[index];
            if (!EqualityComparer<int>.Default.Equals(row.Id, default!) &&
                (!entity.Habilidades.Any(child => child.Id.Equals(row.Id)) || !seenHabilidades.Add(row.Id)))
                ModelState.AddModelError($"Habilidades[{index}].Id", "La línea no pertenece a este registro o está repetida.");

        }


        if (!ModelState.IsValid) return View(model);


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.Estado.Edit");
            var submittedHabilidades = await service.PrepareEstadoHabilidadesAsync(entity,
                model.Habilidades.Select(row => (row.Id, HabilidadAssembler.Create(row))).ToList());

            await service.ModifyAsync(id, model.Tipo, model.CartaId.GetValueOrDefault());
            entity = await service.ReadOIDEstadoDefaultAsync(id) ?? throw new InvalidOperationException("Entity disappeared during update.");
            await service.ApplyEstadoRelationsAsync(entity, p_carta_id: model.CartaId, p_habilidades: submittedHabilidades);

            await service.ModifierEstadoDefaultAsync(entity);


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
    /*PROTECTED REGION ID(EstadoController_Delete_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Delete(int id)
    {
    /*PROTECTED REGION ID(EstadoController_Delete_GET_body) START*/

        var entity = await _estadoService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(EstadoAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(EstadoController_DeleteConfirmed_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(EstadoController_DeleteConfirmed_POST_body) START*/

        if (!ModelState.IsValid) return View("Delete", new CraftCaster.Web.ViewModels.EstadoViewModel());
        await _estadoService.DestroyAsync(id);
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(EstadoController_AddHabilidade_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> AddHabilidade(int p_oid, IReadOnlyList<int> p_habilidad_oids)
    {
    /*PROTECTED REGION ID(EstadoController_AddHabilidade_POST_body) START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _estadoService.AddHabilidadeAsync(p_oid, p_habilidad_oids);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    /*PROTECTED REGION ID(EstadoController_method_LoadFormOptionsAsync_CraftCasterWebViewModelsEstadoViewModel) START*/
    private async Task LoadFormOptionsAsync(CraftCaster.Web.ViewModels.EstadoViewModel model)
    {
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        await LoadFormOptionsAsync(model, unitOfWork);
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(EstadoController_method_LoadFormOptionsAsync_CraftCasterWebViewModelsEstadoViewModel_CraftCasterApplicationAbstractionsIUnitOfWork) START*/
    private async Task LoadFormOptionsAsync(CraftCaster.Web.ViewModels.EstadoViewModel model, CraftCaster.Application.Abstractions.IUnitOfWork unitOfWork)
    {
        if (model.Habilidades.Count > 500) { ModelState.AddModelError(nameof(model.Habilidades), "Máximo 500 líneas."); return; }
        var options0 = (await unitOfWork.CartaRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Nombre, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        model.CartaOptions = options0.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        foreach (var row in model.Habilidades)
        {

        }

    }
    /*PROTECTED REGION END*/

}
