// HARIA-WEB MVC managed manual-controller
/*PROTECTED REGION ID(LineaMazoController_manual_usings) ENABLED START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CraftCaster.Domain.Entities;
using CraftCaster.Application.Services;
using CraftCaster.Web.Assemblers;

using CraftCaster.Domain.Enums;
/*PROTECTED REGION END*/

namespace CraftCaster.Web.Controllers;

/*PROTECTED REGION ID(LineaMazoController_manual_class_attributes) ENABLED START*/
// CRUD generado: solo para el administrador. Los jugadores usan MisCartas, EditorMazo y Explorar,
// que comprueban que la carta o el mazo es suyo. Afecta también a las acciones JSON (Compartir, ValorarMazo...).
[Authorize(Roles = "Admin")]
/*PROTECTED REGION END*/
public partial class LineaMazoController
{

}
