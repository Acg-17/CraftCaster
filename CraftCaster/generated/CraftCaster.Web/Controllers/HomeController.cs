using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CraftCaster.Application.Services;
using CraftCaster.Web.Assemblers;
using CraftCaster.Web.Utils.Security;
using CraftCaster.Web.ViewModels;

namespace CraftCaster.Web.Controllers;

public sealed class HomeController : Controller
{
    private readonly MazoService _mazoService;

    public HomeController(MazoService mazoService)
    {
        _mazoService = mazoService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Mazo.obtenerMazoPorValoracion (mazos públicos valorados entre 0 y 5, ordenados por valoración)
        var mazos = await _mazoService.ObtenerMazoPorValoracionAsync(0, 5);
        var destacados = mazos
            .OrderByDescending(m => m.Valoracion)
            .ThenByDescending(m => m.NumCartas)
            .ThenBy(m => m.Nombre)
            .Take(6)
            .Select(MazoAssembler.ToViewModel)
            .ToList();

        return View(new HomeIndexViewModel { MazosDestacados = destacados });
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Desarrolladores() => View();

    [Authorize(Roles = HariaWebAuthentication.RolAdmin)]
    [HttpGet]
    public IActionResult Entidades() => View();

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
