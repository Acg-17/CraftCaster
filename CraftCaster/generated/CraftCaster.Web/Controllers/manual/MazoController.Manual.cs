// HARIA-WEB MVC managed manual-controller
/*PROTECTED REGION ID(MazoController_manual_usings) ENABLED START*/
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
/*PROTECTED REGION ID(MazoController_manual_class_attributes) ENABLED START*/
// Class-level authorization applies to both partial files.
// [Authorize(Roles = "Admin")]
/*PROTECTED REGION END*/
public partial class MazoController
{
    [HttpGet]
    /*PROTECTED REGION ID(MazoController_Create_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create()
    {
    /*PROTECTED REGION ID(MazoController_Create_GET_body) ENABLED START*/

        var model = new CraftCaster.Web.ViewModels.MazoViewModel();

        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(MazoController_Create_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(CraftCaster.Web.ViewModels.MazoViewModel model)
    {
    /*PROTECTED REGION ID(MazoController_Create_POST_body) ENABLED START*/

        ModelState.Remove(nameof(model.CosteMedio)); // Campo fuera del contrato Create de Application.
        ModelState.Remove(nameof(model.NumCartas)); // Campo fuera del contrato Create de Application.
        ModelState.Remove(nameof(model.EstadoMazo)); // Campo fuera del contrato Create de Application.
        ModelState.Remove(nameof(model.Valoracion)); // Campo fuera del contrato Create de Application.
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::CraftCaster.Application.Services.MazoService(unitOfWork);



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

        for (var index = 0; index < model.Lineas.Count; index++)
        {
            var row = model.Lineas[index];
            if (!EqualityComparer<int>.Default.Equals(row.Id, default!))
                ModelState.AddModelError($"Lineas[{index}].Id", "La línea no pertenece a este registro o está repetida.");
        if (row.CartaId is int selectedRowLineasCarta &&
            !row.CartaOptions.Any(option => option.Value == Convert.ToString(selectedRowLineasCarta, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError($"Lineas[{index}].CartaId", "Selecciona un registro existente de Carta.");
        if (row.CartaId is null) ModelState.AddModelError($"Lineas[{index}].CartaId", "Debes seleccionar una opción.");
        }


        if (!ModelState.IsValid) return View(model);


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.Mazo.Create");
            var submittedLineas = await service.PrepareMazoLineasAsync(null,
                model.Lineas.Select(row => (row.Id, LineaMazoAssembler.Create(row), row.CartaId)).ToList());

            var entity = await service.NewCustomizedAsync(model.Nombre, model.EsPublico, model.Imagen, model.UsuarioId.GetValueOrDefault());

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(MazoController_Compartir_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Compartir(int p_oid)
    {
    /*PROTECTED REGION ID(MazoController_Compartir_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _mazoService.CompartirAsync(p_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(MazoController_CalcularEstadisticas_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> CalcularEstadisticas(int p_oid)
    {
    /*PROTECTED REGION ID(MazoController_CalcularEstadisticas_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _mazoService.CalcularEstadisticasAsync(p_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(MazoController_ValorarMazo_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ValorarMazo(int p_oid, float p_valoracion)
    {
    /*PROTECTED REGION ID(MazoController_ValorarMazo_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _mazoService.ValorarMazoAsync(p_oid, p_valoracion);
        return NoContent();
    /*PROTECTED REGION END*/
    }
}
