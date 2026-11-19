// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(TurnoController_usings) START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CraftCaster.Domain.Entities;
using CraftCaster.Application.Services;
using CraftCaster.Web.Assemblers;

using CraftCaster.Domain.Enums;
/*PROTECTED REGION END*/

namespace CraftCaster.Web.Controllers;

/*PROTECTED REGION ID(TurnoController_class_attributes) ENABLED START*/
[Authorize]
/*PROTECTED REGION END*/
public partial class TurnoController : Controller
{
    /*PROTECTED REGION ID(TurnoController_dependencies) START*/
    private readonly TurnoService _turnoService;
    private readonly ILogger<TurnoController> _logger;
    private readonly CraftCaster.Application.Abstractions.IUnitOfWorkFactory _formUnitOfWorkFactory;
    public TurnoController(TurnoService turnoService, CraftCaster.Application.Abstractions.IUnitOfWorkFactory formUnitOfWorkFactory, ILogger<TurnoController>? logger = null)
    {
        _turnoService = turnoService;

        _formUnitOfWorkFactory = formUnitOfWorkFactory;

        _logger = logger ?? global::Microsoft.Extensions.Logging.Abstractions.NullLogger<TurnoController>.Instance;
    }
    /*PROTECTED REGION END*/
    [HttpGet]
    /*PROTECTED REGION ID(TurnoController_Index_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Index()
    {
    /*PROTECTED REGION ID(TurnoController_Index_GET_body) START*/

        var entities = await _turnoService.ReadAllAsync();
        return View(entities.Select(TurnoAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(TurnoController_Details_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Details(int id)
    {
    /*PROTECTED REGION ID(TurnoController_Details_GET_body) START*/

        var entity = await _turnoService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(TurnoAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(TurnoController_Create_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create()
    {
    /*PROTECTED REGION ID(TurnoController_Create_GET_body) START*/

        var model = new CraftCaster.Web.ViewModels.TurnoViewModel();

        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(TurnoController_Create_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(CraftCaster.Web.ViewModels.TurnoViewModel model)
    {
    /*PROTECTED REGION ID(TurnoController_Create_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::CraftCaster.Application.Services.TurnoService(unitOfWork);



        model.Acciones ??= new();
        if (model.Acciones.Count > 500) return BadRequest("Número de líneas no válido (máximo 500).");
        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.PartidaId is int selectedPartida &&
            !model.PartidaOptions.Any(option => option.Value == Convert.ToString(selectedPartida, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("PartidaId", "Selecciona un registro existente de Partida.");
        if (model.PartidaId is null) ModelState.AddModelError("PartidaId", "Debes seleccionar una opción.");
        if (model.JugadorId is int selectedJugador &&
            !model.JugadorOptions.Any(option => option.Value == Convert.ToString(selectedJugador, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("JugadorId", "Selecciona un registro existente de JugadorPartida.");
        if (model.JugadorId is null) ModelState.AddModelError("JugadorId", "Debes seleccionar una opción.");

        if (model.Acciones is null) { model.Acciones = new(); ModelState.AddModelError(nameof(model.Acciones), "Debes indicar las líneas."); }
        if (model.Acciones.Count < 0 || model.Acciones.Count > 500)
            ModelState.AddModelError(nameof(model.Acciones), "Número de líneas no válido (máximo 500).");

        for (var index = 0; index < model.Acciones.Count; index++)
        {
            var row = model.Acciones[index];
            if (!EqualityComparer<int>.Default.Equals(row.Id, default!))
                ModelState.AddModelError($"Acciones[{index}].Id", "La línea no pertenece a este registro o está repetida.");
        if (row.OrigenId is int selectedRowAccionesOrigen &&
            !row.OrigenOptions.Any(option => option.Value == Convert.ToString(selectedRowAccionesOrigen, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError($"Acciones[{index}].OrigenId", "Selecciona un registro existente de CartaPartida.");

        }


        if (!ModelState.IsValid) return View(model);


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.Turno.Create");
            var submittedAcciones = await service.PrepareTurnoAccionesAsync(null,
                model.Acciones.Select(row => (row.Id, AccionAssembler.Create(row), row.OrigenId)).ToList());

            var entity = await service.NewAsync(model.NumTurno, model.ManaTurno, model.PartidaId.GetValueOrDefault(), model.JugadorId.GetValueOrDefault());

            await service.ApplyTurnoRelationsAsync(entity, p_partida_id: model.PartidaId, p_jugador_id: model.JugadorId, p_acciones: submittedAcciones);

            await service.ModifierTurnoDefaultAsync(entity);


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
    /*PROTECTED REGION ID(TurnoController_Edit_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id)
    {
    /*PROTECTED REGION ID(TurnoController_Edit_GET_body) START*/

        var entity = await _turnoService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        var model = TurnoAssembler.ToViewModel(entity);
        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(TurnoController_Edit_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, CraftCaster.Web.ViewModels.TurnoViewModel model)
    {
    /*PROTECTED REGION ID(TurnoController_Edit_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::CraftCaster.Application.Services.TurnoService(unitOfWork);

        var entity = await service.ReadOIDTurnoDefaultAsync(id);
        if (entity is null) return NotFound();

        model.Acciones ??= new();
        if (model.Acciones.Count > 500) return BadRequest("Número de líneas no válido (máximo 500).");
        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.PartidaId is int selectedPartida &&
            !model.PartidaOptions.Any(option => option.Value == Convert.ToString(selectedPartida, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("PartidaId", "Selecciona un registro existente de Partida.");
        if (model.PartidaId is null) ModelState.AddModelError("PartidaId", "Debes seleccionar una opción.");
        if (model.JugadorId is int selectedJugador &&
            !model.JugadorOptions.Any(option => option.Value == Convert.ToString(selectedJugador, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("JugadorId", "Selecciona un registro existente de JugadorPartida.");
        if (model.JugadorId is null) ModelState.AddModelError("JugadorId", "Debes seleccionar una opción.");

        if (model.Acciones is null) { model.Acciones = new(); ModelState.AddModelError(nameof(model.Acciones), "Debes indicar las líneas."); }
        if (model.Acciones.Count < 0 || model.Acciones.Count > 500)
            ModelState.AddModelError(nameof(model.Acciones), "Número de líneas no válido (máximo 500).");
        var seenAcciones = new HashSet<int>();
        for (var index = 0; index < model.Acciones.Count; index++)
        {
            var row = model.Acciones[index];
            if (!EqualityComparer<int>.Default.Equals(row.Id, default!) &&
                (!entity.Acciones.Any(child => child.Id.Equals(row.Id)) || !seenAcciones.Add(row.Id)))
                ModelState.AddModelError($"Acciones[{index}].Id", "La línea no pertenece a este registro o está repetida.");
        if (row.OrigenId is int selectedRowAccionesOrigen &&
            !row.OrigenOptions.Any(option => option.Value == Convert.ToString(selectedRowAccionesOrigen, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError($"Acciones[{index}].OrigenId", "Selecciona un registro existente de CartaPartida.");

        }


        if (!ModelState.IsValid) return View(model);


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.Turno.Edit");
            var submittedAcciones = await service.PrepareTurnoAccionesAsync(entity,
                model.Acciones.Select(row => (row.Id, AccionAssembler.Create(row), row.OrigenId)).ToList());

            await service.ModifyAsync(id, model.NumTurno, model.ManaTurno, model.PartidaId.GetValueOrDefault(), model.JugadorId.GetValueOrDefault());
            entity = await service.ReadOIDTurnoDefaultAsync(id) ?? throw new InvalidOperationException("Entity disappeared during update.");
            await service.ApplyTurnoRelationsAsync(entity, p_partida_id: model.PartidaId, p_jugador_id: model.JugadorId, p_acciones: submittedAcciones);

            await service.ModifierTurnoDefaultAsync(entity);


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
    /*PROTECTED REGION ID(TurnoController_Delete_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Delete(int id)
    {
    /*PROTECTED REGION ID(TurnoController_Delete_GET_body) START*/

        var entity = await _turnoService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(TurnoAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(TurnoController_DeleteConfirmed_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(TurnoController_DeleteConfirmed_POST_body) START*/

        if (!ModelState.IsValid) return View("Delete", new CraftCaster.Web.ViewModels.TurnoViewModel());
        await _turnoService.DestroyAsync(id);
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(TurnoController_AddAccione_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> AddAccione(int p_oid, IReadOnlyList<int> p_accion_oids)
    {
    /*PROTECTED REGION ID(TurnoController_AddAccione_POST_body) START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _turnoService.AddAccioneAsync(p_oid, p_accion_oids);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    /*PROTECTED REGION ID(TurnoController_method_LoadFormOptionsAsync_CraftCasterWebViewModelsTurnoViewModel) START*/
    private async Task LoadFormOptionsAsync(CraftCaster.Web.ViewModels.TurnoViewModel model)
    {
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        await LoadFormOptionsAsync(model, unitOfWork);
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(TurnoController_method_LoadFormOptionsAsync_CraftCasterWebViewModelsTurnoViewModel_CraftCasterApplicationAbstractionsIUnitOfWork) START*/
    private async Task LoadFormOptionsAsync(CraftCaster.Web.ViewModels.TurnoViewModel model, CraftCaster.Application.Abstractions.IUnitOfWork unitOfWork)
    {
        if (model.Acciones.Count > 500) { ModelState.AddModelError(nameof(model.Acciones), "Máximo 500 líneas."); return; }
        var options0 = (await unitOfWork.PartidaRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        var options1 = (await unitOfWork.JugadorPartidaRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        var options2 = (await unitOfWork.CartaPartidaRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        model.PartidaOptions = options0.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        model.JugadorOptions = options1.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        foreach (var row in model.Acciones)
        {
        row.OrigenOptions = options2.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        }
        model.AccionesTemplate.OrigenOptions = options2.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
    }
    /*PROTECTED REGION END*/

}
