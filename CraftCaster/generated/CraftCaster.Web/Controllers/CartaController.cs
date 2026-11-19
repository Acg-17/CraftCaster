// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(CartaController_usings) START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CraftCaster.Domain.Entities;
using CraftCaster.Application.Services;
using CraftCaster.Web.Assemblers;

using CraftCaster.Domain.Enums;
/*PROTECTED REGION END*/

namespace CraftCaster.Web.Controllers;

/*PROTECTED REGION ID(CartaController_class_attributes) ENABLED START*/
[Authorize]
/*PROTECTED REGION END*/
public partial class CartaController : Controller
{
    /*PROTECTED REGION ID(CartaController_dependencies) START*/
    private readonly CartaService _cartaService;
    private readonly ILogger<CartaController> _logger;
    private readonly CraftCaster.Application.Abstractions.IUnitOfWorkFactory _formUnitOfWorkFactory;
    public CartaController(CartaService cartaService, CraftCaster.Application.Abstractions.IUnitOfWorkFactory formUnitOfWorkFactory, ILogger<CartaController>? logger = null)
    {
        _cartaService = cartaService;

        _formUnitOfWorkFactory = formUnitOfWorkFactory;

        _logger = logger ?? global::Microsoft.Extensions.Logging.Abstractions.NullLogger<CartaController>.Instance;
    }
    /*PROTECTED REGION END*/
    [HttpGet]
    /*PROTECTED REGION ID(CartaController_Index_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Index()
    {
    /*PROTECTED REGION ID(CartaController_Index_GET_body) START*/

        var entities = await _cartaService.ReadAllAsync();
        return View(entities.Select(CartaAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(CartaController_Details_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Details(int id)
    {
    /*PROTECTED REGION ID(CartaController_Details_GET_body) START*/

        var entity = await _cartaService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(CartaAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(CartaController_Edit_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id)
    {
    /*PROTECTED REGION ID(CartaController_Edit_GET_body) START*/

        var entity = await _cartaService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        var model = CartaAssembler.ToViewModel(entity);
        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(CartaController_Edit_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, CraftCaster.Web.ViewModels.CartaViewModel model)
    {
    /*PROTECTED REGION ID(CartaController_Edit_POST_body) START*/

        ModelState.Remove(nameof(model.Vida)); // Campo fuera del contrato Edit de Application.
        ModelState.Remove(nameof(model.Ataque)); // Campo fuera del contrato Edit de Application.
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::CraftCaster.Application.Services.CartaService(unitOfWork);

        var entity = await service.ReadOIDCartaDefaultAsync(id);
        if (entity is null) return NotFound();



        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.UsuarioId is int selectedUsuario &&
            !model.UsuarioOptions.Any(option => option.Value == Convert.ToString(selectedUsuario, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("UsuarioId", "Selecciona un registro existente de Usuario.");
        if (model.UsuarioId is null) ModelState.AddModelError("UsuarioId", "Debes seleccionar una opción.");




        if (!ModelState.IsValid) return View(model);


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.Carta.Edit");


            await service.ModifyAsync(id, model.Nombre, model.Rareza, model.CosteMana, model.Tipo, model.Descripcion, model.Imagen, model.EsPublica, model.UsuarioId.GetValueOrDefault());
            entity = await service.ReadOIDCartaDefaultAsync(id) ?? throw new InvalidOperationException("Entity disappeared during update.");
            await service.ApplyCartaRelationsAsync(entity, p_usuario_id: model.UsuarioId);

            await service.ModifierCartaDefaultAsync(entity);


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
    /*PROTECTED REGION ID(CartaController_Delete_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Delete(int id)
    {
    /*PROTECTED REGION ID(CartaController_Delete_GET_body) START*/

        var entity = await _cartaService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(CartaAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(CartaController_DeleteConfirmed_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(CartaController_DeleteConfirmed_POST_body) START*/

        if (!ModelState.IsValid) return View("Delete", new CraftCaster.Web.ViewModels.CartaViewModel());
        await _cartaService.DestroyAsync(id);
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(CartaController_AddEstado_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> AddEstado(int p_oid, IReadOnlyList<int> p_estado_oids)
    {
    /*PROTECTED REGION ID(CartaController_AddEstado_POST_body) START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _cartaService.AddEstadoAsync(p_oid, p_estado_oids);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(CartaController_ObtenerCartasUsuario_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ObtenerCartasUsuario(int p_oid)
    {
    /*PROTECTED REGION ID(CartaController_ObtenerCartasUsuario_GET_body) START*/

        var result = await _cartaService.ObtenerCartasUsuarioAsync(p_oid);
        return Ok(result.Select(CartaAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(CartaController_ObtenerCartaPorNombre_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ObtenerCartaPorNombre(string p_nombre)
    {
    /*PROTECTED REGION ID(CartaController_ObtenerCartaPorNombre_GET_body) START*/

        var result = await _cartaService.ObtenerCartaPorNombreAsync(p_nombre);
        return Ok(result.Select(CartaAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(CartaController_ObtenerCartaPorCosteMana_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ObtenerCartaPorCosteMana(int p_mana)
    {
    /*PROTECTED REGION ID(CartaController_ObtenerCartaPorCosteMana_GET_body) START*/

        var result = await _cartaService.ObtenerCartaPorCosteManaAsync(p_mana);
        return Ok(result.Select(CartaAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(CartaController_ObtenerCartaPorRareza_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ObtenerCartaPorRareza(Rareza p_rareza)
    {
    /*PROTECTED REGION ID(CartaController_ObtenerCartaPorRareza_GET_body) START*/

        var result = await _cartaService.ObtenerCartaPorRarezaAsync(p_rareza);
        return Ok(result.Select(CartaAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(CartaController_ObtenerCartaPorTipo_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ObtenerCartaPorTipo(TipoCarta p_tipo)
    {
    /*PROTECTED REGION ID(CartaController_ObtenerCartaPorTipo_GET_body) START*/

        var result = await _cartaService.ObtenerCartaPorTipoAsync(p_tipo);
        return Ok(result.Select(CartaAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    /*PROTECTED REGION ID(CartaController_method_LoadFormOptionsAsync_CraftCasterWebViewModelsCartaViewModel) START*/
    private async Task LoadFormOptionsAsync(CraftCaster.Web.ViewModels.CartaViewModel model)
    {
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        await LoadFormOptionsAsync(model, unitOfWork);
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(CartaController_method_LoadFormOptionsAsync_CraftCasterWebViewModelsCartaViewModel_CraftCasterApplicationAbstractionsIUnitOfWork) START*/
    private async Task LoadFormOptionsAsync(CraftCaster.Web.ViewModels.CartaViewModel model, CraftCaster.Application.Abstractions.IUnitOfWork unitOfWork)
    {

        var options0 = (await unitOfWork.UsuarioRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.NombreUsuario, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        model.UsuarioOptions = options0.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();

    }
    /*PROTECTED REGION END*/

}
