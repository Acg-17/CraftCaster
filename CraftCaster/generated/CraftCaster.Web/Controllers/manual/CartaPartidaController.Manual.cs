// HARIA-WEB MVC managed manual-controller
/*PROTECTED REGION ID(CartaPartidaController_manual_usings) ENABLED START*/
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
/*PROTECTED REGION ID(CartaPartidaController_manual_class_attributes) ENABLED START*/
// Datos internos de las partidas: su CRUD (vistas generadas) solo lo usa el administrador.
// Los jugadores juegan desde JuegoController (sala, tablero y fin de partida).
[Authorize(Roles = "Admin")]
/*PROTECTED REGION END*/
public partial class CartaPartidaController
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(CartaPartidaController_RecibirDanyo_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> RecibirDanyo(int p_oid, int p_danyo)
    {
    /*PROTECTED REGION ID(CartaPartidaController_RecibirDanyo_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _cartaPartidaService.RecibirDanyoAsync(p_oid, p_danyo);
        return NoContent();
    /*PROTECTED REGION END*/
    }
}
