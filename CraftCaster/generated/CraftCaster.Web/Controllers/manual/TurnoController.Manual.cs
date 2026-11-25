// HARIA-WEB MVC managed manual-controller
/*PROTECTED REGION ID(TurnoController_manual_usings) ENABLED START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CraftCaster.Domain.Entities;
using CraftCaster.Application.Services;
using CraftCaster.Web.Assemblers;

using CraftCaster.Domain.Enums;
/*PROTECTED REGION END*/

namespace CraftCaster.Web.Controllers;

/*PROTECTED REGION ID(TurnoController_manual_class_attributes) ENABLED START*/
// Datos internos de las partidas: su CRUD (vistas generadas) solo lo usa el administrador.
// Los jugadores juegan desde JuegoController (sala, tablero y fin de partida).
[Authorize(Roles = "Admin")]
/*PROTECTED REGION END*/
public partial class TurnoController
{

}
