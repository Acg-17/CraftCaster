using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Web.Assemblers;
using CraftCaster.Web.ViewModels;

namespace CraftCaster.Web.Controllers;

// Catálogo de mazos públicos (Explorar) y detalle de un mazo con su valoración.
public sealed class ExplorarController : Controller
{
    private readonly MazoService _mazoService;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly ILogger<ExplorarController> _logger;

    public ExplorarController(MazoService mazoService, IUnitOfWorkFactory unitOfWorkFactory, ILogger<ExplorarController> logger)
    {
        _mazoService = mazoService;
        _unitOfWorkFactory = unitOfWorkFactory;
        _logger = logger;
    }

    // GET /Explorar?Nombre=...&CosteMin=...&CosteMax=...&ValoracionMin=...&Orden=...
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ExplorarFiltroViewModel filtro)
    {
        if (filtro.CosteMin is int min && filtro.CosteMax is int max && min > max)
        {
            ModelState.AddModelError(nameof(filtro.CosteMax), "El coste medio máximo no puede ser menor que el mínimo.");
        }

        if (!ModelState.IsValid)
        {
            // Con filtros no válidos se muestran todos los mazos públicos y el error
            var todos = await _mazoService.ObtenerMazosPublicosAsync();
            return View(new ExplorarViewModel { Filtro = filtro, Mazos = Ordenar(todos.Select(MazoAssembler.ToViewModel), filtro.Orden) });
        }

        // Cada criterio usa su ReadFilter del modelo; los resultados se cruzan por id
        var mazos = string.IsNullOrWhiteSpace(filtro.Nombre)
            ? await _mazoService.ObtenerMazosPublicosAsync()
            : await _mazoService.ObtenerMazosPorNombreAsync(filtro.Nombre.Trim());

        var resultado = mazos.AsEnumerable();

        if (filtro.CosteMin is not null || filtro.CosteMax is not null)
        {
            var porCoste = await _mazoService.ObtenerMazoPorCosteMedioAsync(filtro.CosteMin ?? 0, filtro.CosteMax ?? int.MaxValue);
            var ids = porCoste.Select(m => m.Id).ToHashSet();
            resultado = resultado.Where(m => ids.Contains(m.Id));
        }

        if (filtro.ValoracionMin is int valoracionMin)
        {
            var porValoracion = await _mazoService.ObtenerMazoPorValoracionAsync(valoracionMin, int.MaxValue);
            var ids = porValoracion.Select(m => m.Id).ToHashSet();
            resultado = resultado.Where(m => ids.Contains(m.Id));
        }

        return View(new ExplorarViewModel
        {
            Filtro = filtro,
            Mazos = Ordenar(resultado.Select(MazoAssembler.ToViewModel), filtro.Orden),
        });
    }

    // GET /Explorar/Detalle/5
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var mazo = await _mazoService.ReadOIDAsync(id);
        var usuarioId = UsuarioActualId();
        var esPropio = mazo?.Usuario is not null && mazo.Usuario.Id == usuarioId;

        // Un mazo privado solo lo ve su dueño
        if (mazo is null || (!mazo.EsPublico && !esPropio))
        {
            return NotFound();
        }

        return View(new DetalleMazoViewModel
        {
            Mazo = MazoAssembler.ToViewModel(mazo),
            Cartas = mazo.Lineas
                .Where(l => l.Carta is not null)
                .Select(l => new CartaEnMazoViewModel
                {
                    CartaId = l.Carta.Id,
                    Nombre = l.Carta.Nombre,
                    CosteMana = l.Carta.CosteMana,
                    Tipo = l.Carta.Tipo,
                    Rareza = l.Carta.Rareza,
                    Cantidad = l.Cantidad,
                })
                .OrderBy(c => c.CosteMana)
                .ThenBy(c => c.Nombre)
                .ToList(),
            EsPropio = esPropio,
            PuedeValorar = usuarioId is not null && !esPropio,
        });
    }

    // POST /Explorar/Valorar/5
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Valorar(int id, [Bind(Prefix = nameof(DetalleMazoViewModel.Valorar))] ValorarMazoFormViewModel valorar)
    {
        if (!ModelState.IsValid || valorar.Valoracion is null)
        {
            TempData["Error"] = "Elige una valoración entre 1 y 5 estrellas.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        await using var unitOfWork = _unitOfWorkFactory.Create();
        var service = new MazoService(unitOfWork);
        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.Explorar.Valorar");
            var mazo = await service.ReadOIDAsync(id);
            if (mazo is null || !mazo.EsPublico)
            {
                await unitOfWork.RollbackAsync();
                return NotFound();
            }

            if (mazo.Usuario?.Id == UsuarioActualId())
            {
                await unitOfWork.RollbackAsync();
                TempData["Error"] = "No puedes valorar tu propio mazo.";
                return RedirectToAction(nameof(Detalle), new { id });
            }

            // Custom valorarMazo
            await service.ValorarMazoAsync(id, valorar.Valoracion.Value);
            await unitOfWork.CommitAsync();
            TempData["Mensaje"] = "¡Gracias! Tu valoración se ha guardado.";
        }
        catch (ModelException error)
        {
            await unitOfWork.RollbackAsync();
            TempData["Error"] = error.Message;
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            await unitOfWork.RollbackAsync();
            _logger.LogError(error, "No se ha podido valorar el mazo {Id}.", id);
            TempData["Error"] = "No se ha podido guardar la valoración. Inténtalo de nuevo.";
        }

        return RedirectToAction(nameof(Detalle), new { id });
    }

    private static List<MazoViewModel> Ordenar(IEnumerable<MazoViewModel> mazos, string? orden) => orden switch
    {
        "coste-asc" => mazos.OrderBy(m => m.CosteMedio).ThenBy(m => m.Nombre).ToList(),
        "coste-desc" => mazos.OrderByDescending(m => m.CosteMedio).ThenBy(m => m.Nombre).ToList(),
        "nombre" => mazos.OrderBy(m => m.Nombre).ToList(),
        _ => mazos.OrderByDescending(m => m.Valoracion).ThenBy(m => m.Nombre).ToList(),
    };

    private int? UsuarioActualId() =>
        User.FindFirstValue("HariaUserClass") == "Usuario"
        && int.TryParse(User.FindFirstValue(ClaimTypes.Name), out var id)
            ? id
            : null;
}
