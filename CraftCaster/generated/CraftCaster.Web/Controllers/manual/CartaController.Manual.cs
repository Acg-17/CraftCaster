// HARIA-WEB MVC managed manual-controller
/*PROTECTED REGION ID(CartaController_manual_usings) ENABLED START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CraftCaster.Domain.Entities;
using CraftCaster.Application.Services;
using CraftCaster.Web.Assemblers;

using CraftCaster.Domain.Enums;
/*PROTECTED REGION END*/

namespace CraftCaster.Web.Controllers;

// Class-level authorization applies to both partial files.
// [Authorize(Roles = "Admin")]
/*PROTECTED REGION ID(CartaController_manual_class_attributes) ENABLED START*/
// Class-level authorization applies to both partial files.
// [Authorize(Roles = "Admin")]
/*PROTECTED REGION END*/
public partial class CartaController
{
    [HttpGet]
    /*PROTECTED REGION ID(CartaController_Create_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create()
    {
    /*PROTECTED REGION ID(CartaController_Create_GET_body) ENABLED START*/

        var model = new CraftCaster.Web.ViewModels.CartaViewModel();

        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(CartaController_Create_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(CraftCaster.Web.ViewModels.CartaViewModel model)
    {
    /*PROTECTED REGION ID(CartaController_Create_POST_body) ENABLED START*/

        ModelState.Remove(nameof(model.Vida)); // Campo fuera del contrato Create de Application.
        ModelState.Remove(nameof(model.Ataque)); // Campo fuera del contrato Create de Application.
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::CraftCaster.Application.Services.CartaService(unitOfWork);





        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.UsuarioId is int selectedUsuario &&
            !model.UsuarioOptions.Any(option => option.Value == Convert.ToString(selectedUsuario, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("UsuarioId", "Selecciona un registro existente de Usuario.");
        if (model.UsuarioId is null) ModelState.AddModelError("UsuarioId", "Debes seleccionar una opción.");




        if (!ModelState.IsValid) return View(model);


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.Carta.Create");


            var entity = await service.NewAsync(model.Nombre, model.Rareza, model.CosteMana, model.Tipo, model.Descripcion, model.Imagen, model.EsPublica, model.UsuarioId.GetValueOrDefault());

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(CartaController_Compartir_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Compartir(int p_oid)
    {
    /*PROTECTED REGION ID(CartaController_Compartir_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _cartaService.CompartirAsync(p_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }
}
