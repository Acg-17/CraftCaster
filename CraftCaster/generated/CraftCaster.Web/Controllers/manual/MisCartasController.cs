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

// Creador de cartas, Mis Cartas y Detalle de carta (editar, borrar y compartir).
[Authorize]
public sealed class MisCartasController : Controller
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IWebHostEnvironment _env;

    public MisCartasController(IUnitOfWorkFactory unitOfWorkFactory, IWebHostEnvironment env)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _env = env;
    }

    // GET /MisCartas -> Lista y filtra las cartas propias + cartas de todos tus mazos (incluyendo mazos copiados)
    [HttpGet]
    public async Task<IActionResult> Index(string? buscar = null, TipoCarta? tipo = null, Rareza? rareza = null, int? mana = null, string? origen = null)
    {
        var usuarioId = UsuarioActualId();
        if (usuarioId is null) return RedirectToAction("Acceso", "Cuenta");

        await using var uow = _unitOfWorkFactory.Create();
        var cartaService = new CartaService(uow);
        var mazoService = new MazoService(uow);

        var propias = await cartaService.ObtenerCartasUsuarioAsync(usuarioId.Value);
        var misMazos = await mazoService.ObtenerMazosUsuarioAsync(usuarioId.Value);

        // Combinar cartas creadas por el usuario + cartas presentes en cualquiera de sus mazos (sin duplicados)
        var coleccion = new Dictionary<int, CraftCaster.Domain.Entities.Carta>();
        foreach (var c in propias)
        {
            coleccion[c.Id] = c;
        }
        foreach (var mazo in misMazos)
        {
            if (mazo.Lineas is null) continue;
            foreach (var linea in mazo.Lineas)
            {
                if (linea.Carta is not null && !coleccion.ContainsKey(linea.Carta.Id))
                {
                    coleccion[linea.Carta.Id] = linea.Carta;
                }
            }
        }

        // Asegurar carga de usuario/autor para filtros y visualización
        foreach (var c in coleccion.Values)
        {
            _ = c.Usuario?.NombreUsuario;
        }

        var idsValidos = coleccion.Keys.ToHashSet();

        if (string.Equals(origen, "propias", StringComparison.OrdinalIgnoreCase))
        {
            idsValidos.IntersectWith(coleccion.Values.Where(c => c.Usuario?.Id == usuarioId.Value).Select(c => c.Id));
        }
        else if (string.Equals(origen, "externas", StringComparison.OrdinalIgnoreCase))
        {
            idsValidos.IntersectWith(coleccion.Values.Where(c => c.Usuario?.Id != usuarioId.Value).Select(c => c.Id));
        }

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var termino = buscar.Trim();
            if (termino.StartsWith('@'))
            {
                var autorBuscado = termino.TrimStart('@').Trim();
                if (!string.IsNullOrEmpty(autorBuscado))
                {
                    var porAutor = coleccion.Values
                        .Where(c => c.Usuario?.NombreUsuario is not null
                            && c.Usuario.NombreUsuario.Contains(autorBuscado, StringComparison.OrdinalIgnoreCase))
                        .Select(c => c.Id);
                    idsValidos.IntersectWith(porAutor);
                }
            }
            else
            {
                var porNombre = (await cartaService.ObtenerCartaPorNombreAsync(termino)).Select(c => c.Id).ToHashSet();
                var porAutor = coleccion.Values
                    .Where(c => c.Usuario?.NombreUsuario is not null
                        && c.Usuario.NombreUsuario.Contains(termino, StringComparison.OrdinalIgnoreCase))
                    .Select(c => c.Id);
                porNombre.UnionWith(porAutor);
                idsValidos.IntersectWith(porNombre);
            }
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

        var cartas = coleccion.Values
            .Where(c => idsValidos.Contains(c.Id))
            .OrderByDescending(c => c.Usuario?.Id == usuarioId.Value)
            .ThenBy(c => c.CosteMana)
            .ThenBy(c => c.Nombre)
            .ToList();

        // Asegurar carga de estados y habilidades para mostrar iconos y lineas en Mis Cartas
        foreach (var c in cartas)
        {
            _ = c.Estados?.Select(e => e.Habilidades?.Count).ToList();
        }

        ViewBag.Imagenes = cartas.ToDictionary(c => c.Id, c => BuscarImagenCarta(c.Id));
        ViewBag.Autores = cartas.ToDictionary(c => c.Id, c => c.Usuario?.NombreUsuario ?? "Usuario");
        ViewBag.UsuarioActualId = usuarioId.Value;
        ViewBag.Buscar = buscar;
        ViewBag.Tipo = tipo;
        ViewBag.Rareza = rareza;
        ViewBag.Mana = mana;
        ViewBag.Origen = origen;
        return View(cartas);
    }

    // GET /MisCartas/Crear -> Creador de cartas con bloques de eventos/efectos y vista previa
    [HttpGet]
    public IActionResult Crear()
    {
        var model = new CreadorCartaViewModel
        {
            Bloques = new List<BloqueEventoInput> { new() },
        };
        return View(model);
    }

    // POST /MisCartas/Crear -> Carta.new customized + Estado.new + Habilidad.new + Carta.addEstado + Carta.compartir
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CreadorCartaViewModel model)
    {
        var usuarioId = UsuarioActualId();
        if (usuarioId is null) return RedirectToAction("Acceso", "Cuenta");

        var bloques = model.Bloques ?? new List<BloqueEventoInput>();

        // Validar reglas de Criatura vs Hechizo
        int maxBloques = model.Tipo == TipoCarta.Criatura ? 2 : 3;
        if (bloques.Count > maxBloques)
        {
            ModelState.AddModelError(string.Empty,
                model.Tipo == TipoCarta.Criatura
                    ? "Una criatura solo puede tener como máximo 2 habilidades."
                    : "Un hechizo solo puede tener como máximo 3 efectos.");
        }

        if (model.Tipo == TipoCarta.Hechizo && bloques.Any(b => b.TipoHabilidad is TipoHabilidad.Ataque or TipoHabilidad.Defensa))
        {
            ModelState.AddModelError(string.Empty, "Un hechizo no puede tener Daño al atacar ni Daño al defender (solo Criaturas).");
        }

        // Normalizar reglas por habilidad
        foreach (var b in bloques)
        {
            if (model.Tipo == TipoCarta.Hechizo || b.TipoHabilidad == TipoHabilidad.Defensa)
            {
                b.TipoEstado = TipoEstado.Entrar;
            }

            switch (b.TipoHabilidad)
            {
                case TipoHabilidad.Robar:
                    b.TipoObjetivo = TipoObjetivo.Jugador;
                    break;
                case TipoHabilidad.Descartar:
                case TipoHabilidad.Defensa:
                    b.TipoObjetivo = TipoObjetivo.Enemigo;
                    break;
                case TipoHabilidad.Destruir:
                    b.Cantidad = 1;
                    if (b.TipoObjetivo is not (TipoObjetivo.Enemigo or TipoObjetivo.Jugador))
                        b.TipoObjetivo = TipoObjetivo.Enemigo;
                    break;
                case TipoHabilidad.Cura:
                    if (model.Tipo == TipoCarta.Hechizo || b.TipoObjetivo is not (TipoObjetivo.Jugador or TipoObjetivo.Self))
                        b.TipoObjetivo = TipoObjetivo.Jugador;
                    break;
                case TipoHabilidad.Danyo:
                    if (b.TipoObjetivo is not (TipoObjetivo.Enemigo or TipoObjetivo.Jugador or TipoObjetivo.TodosEnemigos))
                        b.TipoObjetivo = TipoObjetivo.Enemigo;
                    break;
            }
        }

        // Calcular coste de maná equilibrado en servidor y bloquear cartas rotas (> 10 maná)
        int costeCalculado = CalcularCosteEquilibrado(model, bloques);
        model.CosteMana = costeCalculado;
        if (costeCalculado > 10)
        {
            ModelState.AddModelError(string.Empty,
                $"La carta está desequilibrada ({costeCalculado}/10 maná). Reduce sus estadísticas o habilidades para que no supere 10 de maná.");
        }

        if (!ModelState.IsValid) return View(model);

        await using var uow = _unitOfWorkFactory.Create();
        var cartaService = new CartaService(uow);
        var estadoService = new EstadoService(uow);
        var habilidadService = new HabilidadService(uow);

        try
        {
            await uow.BeginTransactionAsync("MVC.MisCartas.Crear");

            // 1. Crear carta usando la operación customized (Carta.new customized)
            int ataque = model.Tipo == TipoCarta.Criatura ? model.Ataque : 0;
            int vida = model.Tipo == TipoCarta.Criatura ? model.Vida : 0;
            byte tieneImagen = model.ImagenArchivo is { Length: > 0 } ? (byte)1 : (byte)0;

            var carta = await cartaService.NewAsync(
                model.Nombre.Trim(),
                model.Rareza,
                model.CosteMana,
                model.Tipo,
                model.Descripcion.Trim(),
                tieneImagen,
                false,
                ataque,
                vida,
                usuarioId.Value);

            // 2. Crear bloques de eventos (Estado.new) y efectos (Habilidad.new) y asociarlos (Carta.addEstado)
            var estadosIds = new List<int>();
            foreach (var bloque in model.Bloques ?? new List<BloqueEventoInput>())
            {
                var estado = await estadoService.NewAsync(bloque.TipoEstado, carta.Id);
                var habilidad = await habilidadService.NewAsync(
                    bloque.TipoHabilidad,
                    bloque.Cantidad,
                    bloque.TipoObjetivo,
                    bloque.Objetivo,
                    estado.Id);

                await estadoService.AddHabilidadeAsync(estado.Id, new[] { habilidad.Id });
                estadosIds.Add(estado.Id);
            }

            if (estadosIds.Count > 0)
            {
                await cartaService.AddEstadoAsync(carta.Id, estadosIds);
            }

            // 3. Si marcó compartir, invocamos Carta.compartir
            if (model.EsPublica)
            {
                await cartaService.CompartirAsync(carta.Id);
            }

            await uow.CommitAsync();

            // 4. Guardar fichero de imagen si se subió (atributo type="file")
            if (model.ImagenArchivo is { Length: > 0 })
            {
                await GuardarImagenCartaAsync(carta.Id, model.ImagenArchivo);
            }

            TempData["Exito"] = $"Carta \"{carta.Nombre}\" creada correctamente.";
            return RedirectToAction(nameof(Detalle), new { id = carta.Id });
        }
        catch (ModelException ex)
        {
            await uow.RollbackAsync();
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    // GET /MisCartas/Detalle/5 -> Carta.readOID
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        await using var uow = _unitOfWorkFactory.Create();
        var cartaService = new CartaService(uow);
        var carta = await cartaService.ReadOIDAsync(id);

        if (carta is null) return NotFound();

        var usuarioId = UsuarioActualId();
        bool esPropia = (carta.Usuario is not null && carta.Usuario.Id == usuarioId)
            || User.IsInRole(HariaWebAuthentication.RolAdmin);

        bool enMazoDelUsuario = false;
        if (!carta.EsPublica && !esPropia && usuarioId is int uid)
        {
            var mazoService = new MazoService(uow);
            var misMazos = await mazoService.ObtenerMazosUsuarioAsync(uid);
            enMazoDelUsuario = misMazos.Any(m => m.Lineas is not null && m.Lineas.Any(l => l.Carta?.Id == carta.Id));
        }

        if (!carta.EsPublica && !esPropia && !enMazoDelUsuario) return Forbid();

        // Asegurar carga de usuario, estados y habilidades dentro de la sesión activa
        _ = carta.Usuario?.NombreUsuario;
        _ = carta.Estados?.Select(e => e.Habilidades?.Count).ToList();

        return View(new DetalleCartaViewModel
        {
            Carta = carta,
            ImagenUrl = BuscarImagenCarta(carta.Id),
            EsPropia = esPropia,
        });
    }

    // GET /MisCartas/Editar/5 -> Abre la carta dentro del propio Creador de cartas
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        await using var uow = _unitOfWorkFactory.Create();
        var cartaService = new CartaService(uow);
        var carta = await cartaService.ReadOIDAsync(id);
        if (carta is null) return NotFound();
        if (!PuedeGestionar(carta.Usuario?.Id)) return Forbid();

        var bloques = (carta.Estados ?? Enumerable.Empty<CraftCaster.Domain.Entities.Estado>())
            .SelectMany(e => (e.Habilidades ?? Enumerable.Empty<CraftCaster.Domain.Entities.Habilidad>())
                .Select(h => new BloqueEventoInput
                {
                    TipoEstado = e.Tipo,
                    TipoHabilidad = h.Tipo,
                    Cantidad = h.Cantidad,
                    TipoObjetivo = h.TipoObjetivo,
                    Objetivo = h.Objetivo,
                }))
            .ToList();

        var model = new CreadorCartaViewModel
        {
            Id = carta.Id,
            Nombre = carta.Nombre,
            Rareza = carta.Rareza,
            CosteMana = carta.CosteMana,
            Tipo = carta.Tipo,
            Ataque = carta.Ataque ?? 1,
            Vida = carta.Vida ?? 1,
            Descripcion = carta.Descripcion,
            EsPublica = carta.EsPublica,
            ImagenUrlActual = BuscarImagenCarta(carta.Id),
            Bloques = bloques,
        };
        return View("Crear", model);
    }

    // POST /MisCartas/Editar/5 -> Carta.modify + actualizar bloques (Estado/Habilidad)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, CreadorCartaViewModel model)
    {
        model.Id = id;
        var bloques = model.Bloques ?? new List<BloqueEventoInput>();

        ValidarYNormalizarCarta(model, bloques);

        if (!ModelState.IsValid)
        {
            model.ImagenUrlActual = BuscarImagenCarta(id);
            return View("Crear", model);
        }

        await using var uow = _unitOfWorkFactory.Create();
        var cartaService = new CartaService(uow);
        var estadoService = new EstadoService(uow);
        var habilidadService = new HabilidadService(uow);

        var carta = await cartaService.ReadOIDAsync(id);
        if (carta is null) return NotFound();
        if (!PuedeGestionar(carta.Usuario?.Id)) return Forbid();

        try
        {
            await uow.BeginTransactionAsync("MVC.MisCartas.Editar");

            byte imagenFlag = model.ImagenArchivo is { Length: > 0 } ? (byte)1 : carta.Imagen;
            await cartaService.ModifyAsync(
                id,
                model.Nombre.Trim(),
                model.Rareza,
                model.CosteMana,
                model.Tipo,
                model.Descripcion.Trim(),
                imagenFlag,
                carta.EsPublica,
                carta.Usuario!.Id);

            // Actualizar ataque y vida si es Criatura
            carta.Ataque = model.Tipo == TipoCarta.Criatura ? model.Ataque : null;
            carta.Vida = model.Tipo == TipoCarta.Criatura ? model.Vida : null;

            // Reemplazar bloques previos (Estados y Habilidades) por los editados
            var estadosAnteriores = carta.Estados?.ToList() ?? new();
            if (estadosAnteriores.Count > 0)
            {
                carta.Estados!.Clear();
                await uow.CartaRepository.SaveAsync(carta);
                foreach (var estOld in estadosAnteriores)
                {
                    var habsOld = estOld.Habilidades?.ToList() ?? new();
                    estOld.Habilidades?.Clear();
                    foreach (var hOld in habsOld)
                    {
                        await habilidadService.DestroyAsync(hOld.Id);
                    }
                    await estadoService.DestroyAsync(estOld.Id);
                }
            }

            var nuevosEstadosIds = new List<int>();
            foreach (var bloque in bloques)
            {
                var estado = await estadoService.NewAsync(bloque.TipoEstado, carta.Id);
                var habilidad = await habilidadService.NewAsync(
                    bloque.TipoHabilidad,
                    bloque.Cantidad,
                    bloque.TipoObjetivo,
                    bloque.Objetivo,
                    estado.Id);

                await estadoService.AddHabilidadeAsync(estado.Id, new[] { habilidad.Id });
                nuevosEstadosIds.Add(estado.Id);
            }

            if (nuevosEstadosIds.Count > 0)
            {
                await cartaService.AddEstadoAsync(carta.Id, nuevosEstadosIds);
            }

            await uow.CartaRepository.SaveAsync(carta);
            await uow.CommitAsync();

            if (model.ImagenArchivo is { Length: > 0 })
            {
                await GuardarImagenCartaAsync(id, model.ImagenArchivo);
            }

            TempData["Exito"] = "Cambios guardados correctamente.";
            return RedirectToAction(nameof(Detalle), new { id });
        }
        catch (ModelException ex)
        {
            await uow.RollbackAsync();
            ModelState.AddModelError(string.Empty, ex.Message);
            model.ImagenUrlActual = BuscarImagenCarta(id);
            return View("Crear", model);
        }
    }

    // POST /MisCartas/Compartir/5 -> Carta.compartir
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Compartir(int id)
    {
        await using var uow = _unitOfWorkFactory.Create();
        var cartaService = new CartaService(uow);
        var carta = await cartaService.ReadOIDAsync(id);
        if (carta is null) return NotFound();
        if (!PuedeGestionar(carta.Usuario?.Id)) return Forbid();

        await uow.BeginTransactionAsync("MVC.MisCartas.Compartir");
        await cartaService.CompartirAsync(id);
        await uow.CommitAsync();

        TempData["Exito"] = $"Carta \"{carta.Nombre}\" compartida con la comunidad.";
        return RedirectToAction(nameof(Detalle), new { id });
    }

    // POST /MisCartas/Borrar/5 -> Carta.destroy
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Borrar(int id)
    {
        await using var uow = _unitOfWorkFactory.Create();
        var cartaService = new CartaService(uow);
        var carta = await cartaService.ReadOIDAsync(id);
        if (carta is null) return NotFound();
        if (!PuedeGestionar(carta.Usuario?.Id)) return Forbid();

        try
        {
            await uow.BeginTransactionAsync("MVC.MisCartas.Borrar");
            await cartaService.DestroyAsync(id);
            await uow.CommitAsync();
            TempData["Exito"] = "Carta eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            await uow.RollbackAsync();
            TempData["Error"] = "No se puede borrar la carta porque está en uso en algún mazo o partida.";
            return RedirectToAction(nameof(Detalle), new { id });
        }
    }

    // --- Helpers simples ---
    private void ValidarYNormalizarCarta(CreadorCartaViewModel model, List<BloqueEventoInput> bloques)
    {
        int maxBloques = model.Tipo == TipoCarta.Criatura ? 2 : 3;
        if (bloques.Count > maxBloques)
        {
            ModelState.AddModelError(string.Empty,
                model.Tipo == TipoCarta.Criatura
                    ? "Una criatura solo puede tener como máximo 2 habilidades."
                    : "Un hechizo solo puede tener como máximo 3 efectos.");
        }

        if (model.Tipo == TipoCarta.Hechizo && bloques.Any(b => b.TipoHabilidad is TipoHabilidad.Ataque or TipoHabilidad.Defensa))
        {
            ModelState.AddModelError(string.Empty, "Un hechizo no puede tener Daño al atacar ni Daño al defender (solo Criaturas).");
        }

        foreach (var b in bloques)
        {
            if (model.Tipo == TipoCarta.Hechizo || b.TipoHabilidad == TipoHabilidad.Defensa)
                b.TipoEstado = TipoEstado.Entrar;

            switch (b.TipoHabilidad)
            {
                case TipoHabilidad.Robar:
                    b.TipoObjetivo = TipoObjetivo.Jugador;
                    break;
                case TipoHabilidad.Descartar:
                case TipoHabilidad.Defensa:
                    b.TipoObjetivo = TipoObjetivo.Enemigo;
                    break;
                case TipoHabilidad.Destruir:
                    b.Cantidad = 1;
                    if (b.TipoObjetivo is not (TipoObjetivo.Enemigo or TipoObjetivo.Jugador))
                        b.TipoObjetivo = TipoObjetivo.Enemigo;
                    break;
                case TipoHabilidad.Cura:
                    if (model.Tipo == TipoCarta.Hechizo || b.TipoObjetivo is not (TipoObjetivo.Jugador or TipoObjetivo.Self))
                        b.TipoObjetivo = TipoObjetivo.Jugador;
                    break;
                case TipoHabilidad.Danyo:
                    if (b.TipoObjetivo is not (TipoObjetivo.Enemigo or TipoObjetivo.Jugador or TipoObjetivo.TodosEnemigos))
                        b.TipoObjetivo = TipoObjetivo.Enemigo;
                    break;
            }
        }

        int costeCalculado = CalcularCosteEquilibrado(model, bloques);
        model.CosteMana = costeCalculado;
        if (costeCalculado > 10)
        {
            ModelState.AddModelError(string.Empty,
                $"La carta está desequilibrada ({costeCalculado}/10 maná). Reduce sus estadísticas o habilidades para que no supere 10 de maná.");
        }
    }

    private static int CalcularCosteEquilibrado(CreadorCartaViewModel model, List<BloqueEventoInput> bloques)
    {
        double total = 0;

        if (model.Tipo == TipoCarta.Criatura)
        {
            total += (Math.Max(1, model.Ataque) * 0.6) + (Math.Max(1, model.Vida) * 0.5);
        }

        foreach (var b in bloques)
        {
            bool repetible = model.Tipo == TipoCarta.Criatura
                && b.TipoHabilidad != TipoHabilidad.Defensa
                && b.TipoEstado is TipoEstado.Atacar or TipoEstado.InicioTurno or TipoEstado.FinTurno;

            double unit = b.TipoHabilidad switch
            {
                TipoHabilidad.Danyo => 1.0,
                TipoHabilidad.Cura => 0.6,
                TipoHabilidad.Robar => 1.4,
                TipoHabilidad.Descartar => 2.0,
                TipoHabilidad.Destruir => 3.5,
                TipoHabilidad.Defensa => 0.9,
                _ => 1.0,
            };

            double multCuando = !repetible ? 1.0 : b.TipoHabilidad switch
            {
                TipoHabilidad.Destruir => 2.5,
                TipoHabilidad.Descartar => 2.0,
                _ => 1.8,
            };

            double multObjetivo = (b.TipoHabilidad == TipoHabilidad.Danyo && b.TipoObjetivo == TipoObjetivo.TodosEnemigos)
                ? 2.5
                : 1.0;

            int cant = b.TipoHabilidad == TipoHabilidad.Destruir ? 1 : Math.Max(1, b.Cantidad);
            total += unit * cant * multCuando * multObjetivo;
        }

        // Penalización por combo (múltiples habilidades en una misma carta)
        if (bloques.Count == 2) total += 0.5;
        else if (bloques.Count >= 3) total += 1.5;

        // Descuento por rareza (Épico -1, Legendario -4)
        total += model.Rareza switch
        {
            Rareza.Epico => -1,
            Rareza.Legendario => -4,
            _ => 0,
        };

        return Math.Max(0, (int)Math.Round(total, MidpointRounding.AwayFromZero));
    }

    private int? UsuarioActualId() =>
        User.FindFirstValue("HariaUserClass") == "Usuario"
        && int.TryParse(User.FindFirstValue(ClaimTypes.Name), out var id)
            ? id
            : null;

    private bool PuedeGestionar(int? ownerId) =>
        (ownerId is not null && ownerId == UsuarioActualId())
        || User.IsInRole(HariaWebAuthentication.RolAdmin);

    private string? BuscarImagenCarta(int cartaId)
    {
        var dir = Path.Combine(_env.WebRootPath, "uploads", "cartas");
        if (!Directory.Exists(dir)) return null;
        var file = Directory.GetFiles(dir, $"carta_{cartaId}.*").FirstOrDefault();
        return file is null ? null : $"/uploads/cartas/{Path.GetFileName(file)}";
    }

    private async Task GuardarImagenCartaAsync(int cartaId, IFormFile archivo)
    {
        var ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (ext is not (".png" or ".jpg" or ".jpeg" or ".webp" or ".gif")) ext = ".png";

        var dir = Path.Combine(_env.WebRootPath, "uploads", "cartas");
        Directory.CreateDirectory(dir);

        // Borrar imagen previa si cambió la extensión
        foreach (var antiguo in Directory.GetFiles(dir, $"carta_{cartaId}.*"))
        {
            System.IO.File.Delete(antiguo);
        }

        var destino = Path.Combine(dir, $"carta_{cartaId}{ext}");
        await using var stream = new FileStream(destino, FileMode.Create);
        await archivo.CopyToAsync(stream);
    }
}
