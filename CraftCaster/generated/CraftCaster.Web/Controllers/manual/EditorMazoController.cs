using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Domain.Enums;
using CraftCaster.Web.Utils.Security;
using CraftCaster.Web.ViewModels;

namespace CraftCaster.Web.Controllers;

// Editor de mazos: Mis mazos, crear (Mazo.newCustomized), añadir/quitar cartas (LineaMazo.new/destroy),
// recuento 30/30 con cuotas de rareza, curva de maná (Mazo.calcularEstadisticas) y compartir (Mazo.compartir).
[Authorize]
public sealed class EditorMazoController : Controller
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IWebHostEnvironment _env;

    public EditorMazoController(IUnitOfWorkFactory unitOfWorkFactory, IWebHostEnvironment env)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _env = env;
    }

    // GET /EditorMazo -> Lista de mazos del usuario (ReadFilter ObtenerMazosUsuario)
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var usuarioId = UsuarioActualId();
        if (usuarioId is null) return RedirectToAction("Acceso", "Cuenta");

        await using var uow = _unitOfWorkFactory.Create();
        var mazoService = new MazoService(uow);
        var mazos = await mazoService.ObtenerMazosUsuarioAsync(usuarioId.Value);

        ViewBag.Imagenes = mazos.ToDictionary(m => m.Id, m => BuscarImagenMazo(m.Id));
        return View(mazos);
    }

    // GET /EditorMazo/Crear
    [HttpGet]
    public IActionResult Crear()
    {
        return View(new CrearMazoViewModel());
    }

    // POST /EditorMazo/Crear -> Mazo.newCustomized + Mazo.compartir (si público) + subida de imagen
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CrearMazoViewModel model)
    {
        var usuarioId = UsuarioActualId();
        if (usuarioId is null) return RedirectToAction("Acceso", "Cuenta");

        if (!ModelState.IsValid) return View(model);

        await using var uow = _unitOfWorkFactory.Create();
        var mazoService = new MazoService(uow);

        try
        {
            await uow.BeginTransactionAsync("MVC.EditorMazo.Crear");

            byte tieneImagen = model.ImagenArchivo is { Length: > 0 } ? (byte)1 : (byte)0;
            var mazo = await mazoService.NewCustomizedAsync(
                model.Nombre.Trim(),
                false,
                tieneImagen,
                usuarioId.Value);

            if (model.EsPublico)
            {
                await mazoService.CompartirAsync(mazo.Id);
            }

            await uow.CommitAsync();

            if (model.ImagenArchivo is { Length: > 0 })
            {
                await GuardarImagenMazoAsync(mazo.Id, model.ImagenArchivo);
            }

            TempData["Exito"] = $"Mazo \"{mazo.Nombre}\" creado. Ahora puedes añadir hasta 30 cartas.";
            return RedirectToAction(nameof(Editar), new { id = mazo.Id });
        }
        catch (ModelException ex)
        {
            await uow.RollbackAsync();
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    // GET /EditorMazo/Editar/5 -> Colección (filtrada con ReadFilter de Carta), cartas del mazo, 30/30 y curva de maná
    [HttpGet]
    public async Task<IActionResult> Editar(int id, string? buscar = null, TipoCarta? tipo = null, Rareza? rareza = null, int? mana = null)
    {
        var usuarioId = UsuarioActualId();
        if (usuarioId is null) return RedirectToAction("Acceso", "Cuenta");

        await using var uow = _unitOfWorkFactory.Create();
        var mazoService = new MazoService(uow);
        var cartaService = new CartaService(uow);

        var mazo = await mazoService.ReadOIDAsync(id);
        if (mazo is null) return NotFound();
        if (!PuedeGestionar(mazo.Usuario?.Id)) return Forbid();

        var lineas = mazo.Lineas?.ToList() ?? new();
        foreach (var l in lineas)
        {
            _ = l.Carta?.Nombre;
        }

        // Colección disponible: cartas propias + cartas públicas de la comunidad
        var propias = await cartaService.ObtenerCartasUsuarioAsync(usuarioId.Value);
        var todas = await cartaService.ReadAllAsync();
        var coleccionBase = propias
            .Concat(todas.Where(c => c.EsPublica))
            .GroupBy(c => c.Id)
            .Select(g => g.First())
            .OrderBy(c => c.CosteMana)
            .ThenBy(c => c.Nombre)
            .ToList();

        // Aplicar los ReadFilter modelados en HARIA sobre Carta
        var idsValidos = coleccionBase.Select(c => c.Id).ToHashSet();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var porNombre = await cartaService.ObtenerCartaPorNombreAsync(buscar.Trim());
            idsValidos.IntersectWith(porNombre.Select(c => c.Id));
        }
        if (tipo is TipoCarta tipoFiltro)
        {
            var porTipo = await cartaService.ObtenerCartaPorTipoAsync(tipoFiltro);
            idsValidos.IntersectWith(porTipo.Select(c => c.Id));
        }
        if (rareza is Rareza rarezaFiltro)
        {
            var porRareza = await cartaService.ObtenerCartaPorRarezaAsync(rarezaFiltro);
            idsValidos.IntersectWith(porRareza.Select(c => c.Id));
        }
        if (mana is int manaFiltro && manaFiltro >= 0)
        {
            var porMana = await cartaService.ObtenerCartaPorCosteManaAsync(manaFiltro);
            idsValidos.IntersectWith(porMana.Select(c => c.Id));
        }

        var coleccion = coleccionBase.Where(c => idsValidos.Contains(c.Id)).ToList();

        // Cargar estados y habilidades para mostrar iconos y texto rico en la colección
        foreach (var c in coleccion)
        {
            _ = c.Estados?.Select(e => e.Habilidades?.Count).ToList();
        }

        // Calcular curva de maná (0..7+) y recuento por rareza
        var curva = Enumerable.Range(0, 8).ToDictionary(k => k, _ => 0);
        int totalCartas = 0, comunes = 0, especiales = 0, epicas = 0, legendarias = 0;

        foreach (var linea in lineas)
        {
            totalCartas += linea.Cantidad;
            int coste = Math.Clamp(linea.Carta?.CosteMana ?? 0, 0, 7);
            curva[coste] += linea.Cantidad;

            switch (linea.Carta?.Rareza)
            {
                case Rareza.Legendario: legendarias += linea.Cantidad; break;
                case Rareza.Epico: epicas += linea.Cantidad; break;
                case Rareza.Especial: especiales += linea.Cantidad; break;
                default: comunes += linea.Cantidad; break;
            }
        }

        var vm = new EditorMazoViewModel
        {
            Mazo = mazo,
            ImagenUrl = BuscarImagenMazo(mazo.Id),
            Coleccion = coleccion,
            ImagenesCartas = coleccion.ToDictionary(c => c.Id, c => BuscarImagenCarta(c.Id)),
            Buscar = buscar,
            FiltroTipo = tipo,
            FiltroRareza = rareza,
            FiltroMana = mana,
            CurvaMana = curva,
            TotalCartas = totalCartas,
            MaxCurva = Math.Max(1, curva.Values.Max()),
            NumComunes = comunes,
            NumEspeciales = especiales,
            NumEpicas = epicas,
            NumLegendarias = legendarias,
        };

        return View(vm);
    }

    // POST /EditorMazo/AnadirCarta -> valida cuotas de rareza + LineaMazo.new + Mazo.addLinea + Mazo.calcularEstadisticas
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AnadirCarta(int mazoId, int cartaId, string? buscar, TipoCarta? tipo, Rareza? rareza, int? mana)
    {
        await using var uow = _unitOfWorkFactory.Create();
        var mazoService = new MazoService(uow);
        var cartaService = new CartaService(uow);
        var lineaService = new LineaMazoService(uow);

        var mazo = await mazoService.ReadOIDAsync(mazoId);
        if (mazo is null) return NotFound();
        if (!PuedeGestionar(mazo.Usuario?.Id)) return Forbid();

        var carta = await cartaService.ReadOIDAsync(cartaId);
        if (carta is null) return NotFound();

        // Validar cupo 30/30 y reglas acumulativas de rareza:
        // Máx 2 Legendarias | Leg+Épicas <= 6 | Leg+Épicas+Especiales <= 14 | Total <= 30
        int leg = 0, epi = 0, esp = 0, com = 0;
        foreach (var l in mazo.Lineas ?? Enumerable.Empty<CraftCaster.Domain.Entities.LineaMazo>())
        {
            switch (l.Carta?.Rareza)
            {
                case Rareza.Legendario: leg += l.Cantidad; break;
                case Rareza.Epico: epi += l.Cantidad; break;
                case Rareza.Especial: esp += l.Cantidad; break;
                default: com += l.Cantidad; break;
            }
        }
        switch (carta.Rareza)
        {
            case Rareza.Legendario: leg++; break;
            case Rareza.Epico: epi++; break;
            case Rareza.Especial: esp++; break;
            default: com++; break;
        }

        if (leg + epi + esp + com > 30)
        {
            TempData["Error"] = "El mazo ya tiene 30 cartas (máximo permitido).";
            return RedirectToAction(nameof(Editar), new { id = mazoId, buscar, tipo, rareza, mana });
        }
        if (leg > 2)
        {
            TempData["Error"] = "Máximo 2 cartas Legendarias por mazo.";
            return RedirectToAction(nameof(Editar), new { id = mazoId, buscar, tipo, rareza, mana });
        }
        if (leg + epi > 6)
        {
            TempData["Error"] = "Límite de calidad superado: máximo 6 cartas entre Épicas y Legendarias.";
            return RedirectToAction(nameof(Editar), new { id = mazoId, buscar, tipo, rareza, mana });
        }
        if (leg + epi + esp > 14)
        {
            TempData["Error"] = "Límite de calidad superado: máximo 14 cartas entre Especiales, Épicas y Legendarias.";
            return RedirectToAction(nameof(Editar), new { id = mazoId, buscar, tipo, rareza, mana });
        }

        try
        {
            await uow.BeginTransactionAsync("MVC.EditorMazo.AnadirCarta");

            var lineaExistente = mazo.Lineas?.FirstOrDefault(l => l.Carta?.Id == cartaId);
            if (lineaExistente is not null)
            {
                lineaExistente.Cantidad += 1;
                await uow.LineaMazoRepository.SaveAsync(lineaExistente);
            }
            else
            {
                var nuevaLinea = await lineaService.NewAsync(1, mazoId, cartaId);
                await mazoService.AddLineaAsync(mazoId, new[] { nuevaLinea.Id });
            }

            await mazoService.CalcularEstadisticasAsync(mazoId);
            await uow.CommitAsync();
        }
        catch (ModelException ex)
        {
            await uow.RollbackAsync();
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Editar), new { id = mazoId, buscar, tipo, rareza, mana });
    }

    // POST /EditorMazo/QuitarCarta -> LineaMazo.destroy (si cantidad == 1) + Mazo.calcularEstadisticas
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuitarCarta(int mazoId, int lineaId, string? buscar, TipoCarta? tipo, Rareza? rareza, int? mana)
    {
        await using var uow = _unitOfWorkFactory.Create();
        var mazoService = new MazoService(uow);
        var lineaService = new LineaMazoService(uow);

        var mazo = await mazoService.ReadOIDAsync(mazoId);
        if (mazo is null) return NotFound();
        if (!PuedeGestionar(mazo.Usuario?.Id)) return Forbid();

        var linea = mazo.Lineas?.FirstOrDefault(l => l.Id == lineaId);
        if (linea is null) return RedirectToAction(nameof(Editar), new { id = mazoId, buscar, tipo, rareza, mana });

        try
        {
            await uow.BeginTransactionAsync("MVC.EditorMazo.QuitarCarta");

            if (linea.Cantidad > 1)
            {
                linea.Cantidad -= 1;
                await uow.LineaMazoRepository.SaveAsync(linea);
            }
            else
            {
                mazo.Lineas!.Remove(linea);
                await uow.MazoRepository.SaveAsync(mazo);
                await lineaService.DestroyAsync(lineaId);
            }

            await mazoService.CalcularEstadisticasAsync(mazoId);
            await uow.CommitAsync();
        }
        catch (ModelException ex)
        {
            await uow.RollbackAsync();
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Editar), new { id = mazoId, buscar, tipo, rareza, mana });
    }

    // POST /EditorMazo/Vaciar -> elimina todas las LineaMazo del mazo + Mazo.calcularEstadisticas
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Vaciar(int mazoId)
    {
        await using var uow = _unitOfWorkFactory.Create();
        var mazoService = new MazoService(uow);
        var lineaService = new LineaMazoService(uow);

        var mazo = await mazoService.ReadOIDAsync(mazoId);
        if (mazo is null) return NotFound();
        if (!PuedeGestionar(mazo.Usuario?.Id)) return Forbid();

        var lineasIds = mazo.Lineas?.Select(l => l.Id).ToList() ?? new();
        if (lineasIds.Count > 0)
        {
            await uow.BeginTransactionAsync("MVC.EditorMazo.Vaciar");
            mazo.Lineas!.Clear();
            await uow.MazoRepository.SaveAsync(mazo);
            foreach (var lid in lineasIds)
            {
                await lineaService.DestroyAsync(lid);
            }
            await mazoService.CalcularEstadisticasAsync(mazoId);
            await uow.CommitAsync();
        }

        TempData["Exito"] = "Mazo vaciado.";
        return RedirectToAction(nameof(Editar), new { id = mazoId });
    }

    // POST /EditorMazo/SubirPortada -> Sube o actualiza la portada del mazo (type="file")
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubirPortada(int mazoId, IFormFile? imagenArchivo)
    {
        await using var uow = _unitOfWorkFactory.Create();
        var mazoService = new MazoService(uow);
        var mazo = await mazoService.ReadOIDAsync(mazoId);
        if (mazo is null) return NotFound();
        if (!PuedeGestionar(mazo.Usuario?.Id)) return Forbid();

        if (imagenArchivo is { Length: > 0 })
        {
            await uow.BeginTransactionAsync("MVC.EditorMazo.SubirPortada");
            mazo.Imagen = 1;
            await uow.MazoRepository.SaveAsync(mazo);
            await uow.CommitAsync();

            await GuardarImagenMazoAsync(mazoId, imagenArchivo);
            TempData["Exito"] = "Portada del mazo actualizada.";
        }
        else
        {
            TempData["Error"] = "Selecciona una imagen para actualizar la portada.";
        }

        return RedirectToAction(nameof(Editar), new { id = mazoId });
    }

    // POST /EditorMazo/Compartir/5 -> Mazo.compartir
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Compartir(int id)
    {
        await using var uow = _unitOfWorkFactory.Create();
        var mazoService = new MazoService(uow);
        var mazo = await mazoService.ReadOIDAsync(id);
        if (mazo is null) return NotFound();
        if (!PuedeGestionar(mazo.Usuario?.Id)) return Forbid();

        await uow.BeginTransactionAsync("MVC.EditorMazo.Compartir");
        await mazoService.CompartirAsync(id);
        await uow.CommitAsync();

        TempData["Exito"] = $"Mazo \"{mazo.Nombre}\" compartido con la comunidad.";
        return RedirectToAction(nameof(Editar), new { id });
    }

    // POST /EditorMazo/Borrar/5 -> Mazo.destroy
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Borrar(int id)
    {
        await using var uow = _unitOfWorkFactory.Create();
        var mazoService = new MazoService(uow);
        var mazo = await mazoService.ReadOIDAsync(id);
        if (mazo is null) return NotFound();
        if (!PuedeGestionar(mazo.Usuario?.Id)) return Forbid();

        try
        {
            await uow.BeginTransactionAsync("MVC.EditorMazo.Borrar");
            await mazoService.DestroyAsync(id);
            await uow.CommitAsync();
            TempData["Exito"] = "Mazo eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            await uow.RollbackAsync();
            TempData["Error"] = "No se puede borrar este mazo porque está en uso en alguna partida.";
            return RedirectToAction(nameof(Index));
        }
    }

    // --- Helpers simples ---
    private int? UsuarioActualId() =>
        User.FindFirstValue("HariaUserClass") == "Usuario"
        && int.TryParse(User.FindFirstValue(ClaimTypes.Name), out var id)
            ? id
            : null;

    private bool PuedeGestionar(int? ownerId) =>
        (ownerId is not null && ownerId == UsuarioActualId())
        || User.IsInRole(HariaWebAuthentication.RolAdmin);

    private string? BuscarImagenMazo(int mazoId)
    {
        var dir = Path.Combine(_env.WebRootPath, "uploads", "mazos");
        if (!Directory.Exists(dir)) return null;
        var file = Directory.GetFiles(dir, $"mazo_{mazoId}.*").FirstOrDefault();
        return file is null ? null : $"/uploads/mazos/{Path.GetFileName(file)}";
    }

    private string? BuscarImagenCarta(int cartaId)
    {
        var dir = Path.Combine(_env.WebRootPath, "uploads", "cartas");
        if (!Directory.Exists(dir)) return null;
        var file = Directory.GetFiles(dir, $"carta_{cartaId}.*").FirstOrDefault();
        return file is null ? null : $"/uploads/cartas/{Path.GetFileName(file)}";
    }

    private async Task GuardarImagenMazoAsync(int mazoId, IFormFile archivo)
    {
        var ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (ext is not (".png" or ".jpg" or ".jpeg" or ".webp" or ".gif")) ext = ".png";

        var dir = Path.Combine(_env.WebRootPath, "uploads", "mazos");
        Directory.CreateDirectory(dir);

        foreach (var antiguo in Directory.GetFiles(dir, $"mazo_{mazoId}.*"))
        {
            System.IO.File.Delete(antiguo);
        }

        var destino = Path.Combine(dir, $"mazo_{mazoId}{ext}");
        await using var stream = new FileStream(destino, FileMode.Create);
        await archivo.CopyToAsync(stream);
    }
}
