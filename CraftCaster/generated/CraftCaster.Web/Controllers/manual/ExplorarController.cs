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
        var termino = filtro.Nombre?.Trim();
        IReadOnlyList<CraftCaster.Domain.Entities.Mazo> mazos;
        if (string.IsNullOrWhiteSpace(termino))
        {
            mazos = await _mazoService.ObtenerMazosPublicosAsync();
        }
        else if (termino.StartsWith('@'))
        {
            var nombreUsuario = termino.TrimStart('@').Trim();
            var encontrados = new List<CraftCaster.Domain.Entities.Mazo>();
            if (!string.IsNullOrWhiteSpace(nombreUsuario))
            {
                await using var uowUser = _unitOfWorkFactory.Create();
                var usuarios = await new UsuarioService(uowUser).ObtenerUsuarioPorEmailONombreAsync(nombreUsuario);
                var publicos = await _mazoService.ObtenerMazosPublicosAsync();
                var idsUsuarios = usuarios.Select(u => u.Id).ToHashSet();
                encontrados = publicos.Where(m => m.Usuario is not null && idsUsuarios.Contains(m.Usuario.Id)).ToList();
            }
            mazos = encontrados;
        }
        else
        {
            mazos = await _mazoService.ObtenerMazosPorNombreAsync(termino);
        }

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

    // POST /Explorar/Copiar/5 -> Copia un mazo público a Mis Mazos conservando "(por @creador)" en el nombre
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Copiar(int id)
    {
        var usuarioId = UsuarioActualId();
        if (usuarioId is null) return RedirectToAction("Acceso", "Cuenta");

        await using var unitOfWork = _unitOfWorkFactory.Create();
        var mazoService = new MazoService(unitOfWork);
        var lineaService = new LineaMazoService(unitOfWork);

        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.Explorar.Copiar");

            var original = await mazoService.ReadOIDAsync(id);
            if (original is null || (!original.EsPublico && original.Usuario?.Id != usuarioId.Value))
            {
                await unitOfWork.RollbackAsync();
                return NotFound();
            }

            var creador = original.Usuario?.NombreUsuario;
            var sufijo = !string.IsNullOrWhiteSpace(creador) && !original.Nombre.Contains($"(por @{creador})", StringComparison.OrdinalIgnoreCase)
                ? $" (por @{creador})"
                : string.Empty;
            var nombreCopia = $"{original.Nombre}{sufijo}";
            if (nombreCopia.Length > 60) nombreCopia = nombreCopia[..60];

            var copia = await mazoService.NewCustomizedAsync(nombreCopia, false, original.Imagen, usuarioId.Value);

            var lineasOriginales = original.Lineas?.Where(l => l.Carta is not null).ToList() ?? new();
            var nuevasLineasIds = new List<int>();
            foreach (var linea in lineasOriginales)
            {
                var nuevaLinea = await lineaService.NewAsync(linea.Cantidad, copia.Id, linea.Carta.Id);
                nuevasLineasIds.Add(nuevaLinea.Id);
            }

            if (nuevasLineasIds.Count > 0)
            {
                await mazoService.AddLineaAsync(copia.Id, nuevasLineasIds);
            }

            await mazoService.CalcularEstadisticasAsync(copia.Id);
            await unitOfWork.CommitAsync();

            TempData["Exito"] = $"Mazo \"{nombreCopia}\" copiado a tus mazos. Ya puedes editarlo o jugar con él.";
            return RedirectToAction("Editar", "EditorMazo", new { id = copia.Id });
        }
        catch (ModelException error)
        {
            await unitOfWork.RollbackAsync();
            TempData["Error"] = error.Message;
            return RedirectToAction(nameof(Detalle), new { id });
        }
    }

    // POST /Explorar/Reportar/5 -> Invoca Usuario.reportarUsuario directo contra la BD
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reportar(int id, string? motivo)
    {
        var usuarioId = UsuarioActualId();
        if (usuarioId is null) return RedirectToAction("Acceso", "Cuenta");

        await using var unitOfWork = _unitOfWorkFactory.Create();
        var mazoService = new MazoService(unitOfWork);
        var usuarioService = new UsuarioService(unitOfWork);

        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.Explorar.Reportar");

            var mazo = await mazoService.ReadOIDAsync(id);
            if (mazo is null || !mazo.EsPublico || mazo.Usuario is null)
            {
                await unitOfWork.RollbackAsync();
                return NotFound();
            }

            if (mazo.Usuario.Id == usuarioId.Value)
            {
                await unitOfWork.RollbackAsync();
                TempData["Error"] = "No puedes reportar tu propio mazo.";
                return RedirectToAction(nameof(Detalle), new { id });
            }

            if (mazo.Usuario.TipoUsuario == CraftCaster.Domain.Enums.TipoUsuario.Admin)
            {
                await unitOfWork.RollbackAsync();
                TempData["Error"] = "No se puede reportar una cuenta de administrador.";
                return RedirectToAction(nameof(Detalle), new { id });
            }

            await usuarioService.ReportarUsuarioAsync(mazo.Usuario.Id);
            await unitOfWork.CommitAsync();

            var motivoTexto = string.IsNullOrWhiteSpace(motivo) ? "Contenido inadecuado" : motivo.Trim();
            TempData["Mensaje"] = $"Reporte registrado ({motivoTexto}). La cuenta del autor ha pasado a revisión en el Panel de Admin.";
        }
        catch (ModelException error)
        {
            await unitOfWork.RollbackAsync();
            TempData["Error"] = error.Message;
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
