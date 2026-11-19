// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(MazoController_usings) START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CraftCaster.Domain.Entities;
using CraftCaster.Application.Services;
using CraftCaster.Web.Assemblers;

using CraftCaster.Domain.Enums;
/*PROTECTED REGION END*/

namespace CraftCaster.Web.Controllers;

/*PROTECTED REGION ID(MazoController_class_attributes) ENABLED START*/
[Authorize]
/*PROTECTED REGION END*/
public partial class MazoController : Controller
{
    /*PROTECTED REGION ID(MazoController_dependencies) START*/
    private readonly MazoService _mazoService;
    private readonly ILogger<MazoController> _logger;
    private readonly CraftCaster.Application.Abstractions.IUnitOfWorkFactory _formUnitOfWorkFactory;
    public MazoController(MazoService mazoService, CraftCaster.Application.Abstractions.IUnitOfWorkFactory formUnitOfWorkFactory, ILogger<MazoController>? logger = null)
    {
        _mazoService = mazoService;

        _formUnitOfWorkFactory = formUnitOfWorkFactory;

        _logger = logger ?? global::Microsoft.Extensions.Logging.Abstractions.NullLogger<MazoController>.Instance;
    }
    /*PROTECTED REGION END*/
    [HttpGet]
    /*PROTECTED REGION ID(MazoController_Index_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Index()
    {
    /*PROTECTED REGION ID(MazoController_Index_GET_body) START*/

        var entities = await _mazoService.ReadAllAsync();
        return View(entities.Select(MazoAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(MazoController_Details_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Details(int id)
    {
    /*PROTECTED REGION ID(MazoController_Details_GET_body) START*/

        var entity = await _mazoService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(MazoAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(MazoController_Edit_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id)
    {
    /*PROTECTED REGION ID(MazoController_Edit_GET_body) START*/

        var entity = await _mazoService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        var model = MazoAssembler.ToViewModel(entity);
        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(MazoController_Edit_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, CraftCaster.Web.ViewModels.MazoViewModel model)
    {
    /*PROTECTED REGION ID(MazoController_Edit_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::CraftCaster.Application.Services.MazoService(unitOfWork);

        var entity = await service.ReadOIDMazoDefaultAsync(id);
        if (entity is null) return NotFound();

        model.Lineas ??= new();
        if (model.Lineas.Count > 500) return BadRequest("Número de líneas no válido (máximo 500).");
        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.UsuarioId is int selectedUsuario &&
            !model.UsuarioOptions.Any(option => option.Value == Convert.ToString(selectedUsuario, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("UsuarioId", "Selecciona un registro existente de Usuario.");
        if (model.UsuarioId is null) ModelState.AddModelError("UsuarioId", "Debes seleccionar una opción.");

        if (model.Lineas is null) { model.Lineas = new(); ModelState.AddModelError(nameof(model.Lineas), "Debes indicar las líneas."); }
        if (model.Lineas.Count < 0 || model.Lineas.Count > 500)
            ModelState.AddModelError(nameof(model.Lineas), "Número de líneas no válido (máximo 500).");
        var seenLineas = new HashSet<int>();
        for (var index = 0; index < model.Lineas.Count; index++)
        {
            var row = model.Lineas[index];
            if (!EqualityComparer<int>.Default.Equals(row.Id, default!) &&
                (!entity.Lineas.Any(child => child.Id.Equals(row.Id)) || !seenLineas.Add(row.Id)))
                ModelState.AddModelError($"Lineas[{index}].Id", "La línea no pertenece a este registro o está repetida.");
        if (row.CartaId is int selectedRowLineasCarta &&
            !row.CartaOptions.Any(option => option.Value == Convert.ToString(selectedRowLineasCarta, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError($"Lineas[{index}].CartaId", "Selecciona un registro existente de Carta.");
        if (row.CartaId is null) ModelState.AddModelError($"Lineas[{index}].CartaId", "Debes seleccionar una opción.");
        }


        if (!ModelState.IsValid) return View(model);


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.Mazo.Edit");
            var submittedLineas = await service.PrepareMazoLineasAsync(entity,
                model.Lineas.Select(row => (row.Id, LineaMazoAssembler.Create(row), row.CartaId)).ToList());

            await service.ModifyAsync(id, model.Nombre, model.CosteMedio, model.NumCartas, model.EstadoMazo, model.EsPublico, model.Valoracion, model.Imagen, model.UsuarioId.GetValueOrDefault());
            entity = await service.ReadOIDMazoDefaultAsync(id) ?? throw new InvalidOperationException("Entity disappeared during update.");
            await service.ApplyMazoRelationsAsync(entity, p_usuario_id: model.UsuarioId, p_lineas: submittedLineas);

            await service.ModifierMazoDefaultAsync(entity);


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
    /*PROTECTED REGION ID(MazoController_Delete_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Delete(int id)
    {
    /*PROTECTED REGION ID(MazoController_Delete_GET_body) START*/

        var entity = await _mazoService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(MazoAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(MazoController_DeleteConfirmed_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(MazoController_DeleteConfirmed_POST_body) START*/

        if (!ModelState.IsValid) return View("Delete", new CraftCaster.Web.ViewModels.MazoViewModel());
        await _mazoService.DestroyAsync(id);
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(MazoController_AddLinea_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> AddLinea(int p_oid, IReadOnlyList<int> p_lineaMazo_oids)
    {
    /*PROTECTED REGION ID(MazoController_AddLinea_POST_body) START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _mazoService.AddLineaAsync(p_oid, p_lineaMazo_oids);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(MazoController_AddPartidasLocal_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> AddPartidasLocal(int p_oid, IReadOnlyList<int> p_partida_oids)
    {
    /*PROTECTED REGION ID(MazoController_AddPartidasLocal_POST_body) START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _mazoService.AddPartidasLocalAsync(p_oid, p_partida_oids);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(MazoController_AddPartidasVisitante_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> AddPartidasVisitante(int p_oid, IReadOnlyList<int> p_partida_oids)
    {
    /*PROTECTED REGION ID(MazoController_AddPartidasVisitante_POST_body) START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _mazoService.AddPartidasVisitanteAsync(p_oid, p_partida_oids);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(MazoController_ObtenerMazosUsuario_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ObtenerMazosUsuario(int p_oid)
    {
    /*PROTECTED REGION ID(MazoController_ObtenerMazosUsuario_GET_body) START*/

        var result = await _mazoService.ObtenerMazosUsuarioAsync(p_oid);
        return Ok(result.Select(MazoAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(MazoController_ObtenerMazosPorNombre_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ObtenerMazosPorNombre(string p_nombre)
    {
    /*PROTECTED REGION ID(MazoController_ObtenerMazosPorNombre_GET_body) START*/

        var result = await _mazoService.ObtenerMazosPorNombreAsync(p_nombre);
        return Ok(result.Select(MazoAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(MazoController_ObtenerMazoPorCosteMedio_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ObtenerMazoPorCosteMedio(int p_min_mana, int p_max_mana)
    {
    /*PROTECTED REGION ID(MazoController_ObtenerMazoPorCosteMedio_GET_body) START*/

        var result = await _mazoService.ObtenerMazoPorCosteMedioAsync(p_min_mana, p_max_mana);
        return Ok(result.Select(MazoAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(MazoController_ObtenerMazoPorValoracion_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ObtenerMazoPorValoracion(int p_min_val, int p_max_val)
    {
    /*PROTECTED REGION ID(MazoController_ObtenerMazoPorValoracion_GET_body) START*/

        var result = await _mazoService.ObtenerMazoPorValoracionAsync(p_min_val, p_max_val);
        return Ok(result.Select(MazoAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(MazoController_ObtenerMazoJugador1_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ObtenerMazoJugador1(int p_oid)
    {
    /*PROTECTED REGION ID(MazoController_ObtenerMazoJugador1_GET_body) START*/

        var result = await _mazoService.ObtenerMazoJugador1Async(p_oid);
        return Ok(result.Select(MazoAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(MazoController_ObtenerMazoJugador2_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ObtenerMazoJugador2(int p_oid)
    {
    /*PROTECTED REGION ID(MazoController_ObtenerMazoJugador2_GET_body) START*/

        var result = await _mazoService.ObtenerMazoJugador2Async(p_oid);
        return Ok(result.Select(MazoAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    /*PROTECTED REGION ID(MazoController_method_LoadFormOptionsAsync_CraftCasterWebViewModelsMazoViewModel) START*/
    private async Task LoadFormOptionsAsync(CraftCaster.Web.ViewModels.MazoViewModel model)
    {
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        await LoadFormOptionsAsync(model, unitOfWork);
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(MazoController_method_LoadFormOptionsAsync_CraftCasterWebViewModelsMazoViewModel_CraftCasterApplicationAbstractionsIUnitOfWork) START*/
    private async Task LoadFormOptionsAsync(CraftCaster.Web.ViewModels.MazoViewModel model, CraftCaster.Application.Abstractions.IUnitOfWork unitOfWork)
    {
        if (model.Lineas.Count > 500) { ModelState.AddModelError(nameof(model.Lineas), "Máximo 500 líneas."); return; }
        var options0 = (await unitOfWork.UsuarioRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.NombreUsuario, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        var options1 = (await unitOfWork.CartaRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Nombre, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        model.UsuarioOptions = options0.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        foreach (var row in model.Lineas)
        {
        row.CartaOptions = options1.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        }
        model.LineasTemplate.CartaOptions = options1.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
    }
    /*PROTECTED REGION END*/

}
