using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;
using CraftCaster.Web.Assemblers;
using CraftCaster.Web.Utils.Security;
using CraftCaster.Web.ViewModels;

namespace CraftCaster.Web.Controllers;

// Partidas desde el punto de vista del jugador:
//   OPCIONES PARTIDA  -> Index (sala), Crear, Unirse, Abandonar, Historial
//                        CustomTransaction Usuario.iniciarPartida / unirsePartida / abandonarPartida
//   TABLERO PARTIDA   -> Tablero, EstadoTablero (polling ligero), Lanzar, Atacar, PasarTurno
//                        CustomTransaction JugadorPartida.lanzarCarta / atacar, Partida.pasarTurno / empezar.
//                        Dentro de su transacción, cada habilidad se ejecuta con la Custom Habilidad.ejecutar.
//   FIN DE PARTIDA    -> Fin (ganador)  CustomTransaction Partida.finalizar
[Authorize(Roles = HariaWebAuthentication.RolCliente + "," + HariaWebAuthentication.RolAdmin)]
public sealed class JuegoController : Controller
{
    private readonly PartidaService _partidaService;
    private readonly UsuarioService _usuarioService;
    private readonly MazoService _mazoService;
    private readonly ZonaPartidaService _zonaPartidaService;
    private readonly CartaService _cartaService;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<JuegoController> _logger;

    public JuegoController(
        PartidaService partidaService,
        UsuarioService usuarioService,
        MazoService mazoService,
        ZonaPartidaService zonaPartidaService,
        CartaService cartaService,
        IUnitOfWorkFactory unitOfWorkFactory,
        IWebHostEnvironment env,
        ILogger<JuegoController> logger)
    {
        _partidaService = partidaService;
        _usuarioService = usuarioService;
        _mazoService = mazoService;
        _zonaPartidaService = zonaPartidaService;
        _cartaService = cartaService;
        _unitOfWorkFactory = unitOfWorkFactory;
        _env = env;
        _logger = logger;
    }

    // =====================================================================
    // OPCIONES PARTIDA
    // =====================================================================

    // GET /Juego  (sala de partidas)
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (UsuarioActualId() is not int usuarioId) return Forbid();

        var usuario = await _usuarioService.ReadOIDAsync(usuarioId);
        if (usuario is null) return Forbid();

        var misPartidas = await _partidaService.ObtenerPartidasUsuarioAsync(usuarioId);
        var activa = misPartidas.FirstOrDefault(p => p.Estado != EstadoPartida.Finalizada);

        var mazoJugable = BuscarMazoJugable(usuario);
        var mazoMostrar = mazoJugable ?? usuario.Mazos.FirstOrDefault();

        var model = new SalaViewModel
        {
            UsuarioId = usuarioId,
            PartidaActiva = activa is null ? null : PartidaAssembler.ToViewModel(activa),
            SoyLocal = activa is not null && activa.Usuario1.Id == usuarioId,
            MazoConElQueJuego = mazoMostrar?.Nombre,
            MazoNumCartas = mazoMostrar?.Lineas.Sum(l => l.Cantidad) ?? mazoMostrar?.NumCartas,
            TieneMazoJugable = mazoJugable is not null,
        };

        if (activa is null)
        {
            var enEspera = (await _partidaService.ReadAllAsync())
                .Where(p => p.Estado == EstadoPartida.Espera && p.Usuario2 is null && p.Usuario1.Id != usuarioId && p.Jugadores.Any())
                .OrderBy(p => p.FechaInicio);

            foreach (var partida in enEspera)
            {
                var anfitrion = (await _usuarioService.ObtenerUsuarioLocalAsync(partida.Id)).FirstOrDefault();
                var mazoAnfitrion = (await _mazoService.ObtenerMazoJugador1Async(partida.Id)).FirstOrDefault();
                model.PartidasEnEspera.Add(new PartidaEnEsperaViewModel
                {
                    Id = partida.Id,
                    Anfitrion = anfitrion?.NombreUsuario ?? "-",
                    Mazo = mazoAnfitrion?.Nombre ?? "-",
                    FechaInicio = partida.FechaInicio,
                });
            }
        }

        return View(model);
    }

    // POST /Juego/Crear  -> CustomTransaction Usuario.iniciarPartida
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear()
    {
        if (UsuarioActualId() is not int usuarioId) return Forbid();

        if (await PartidaActivaDeAsync(usuarioId) is not null)
        {
            TempData["Error"] = "Ya estás en una partida. Abandónala antes de crear otra.";
            return RedirectToAction(nameof(Index));
        }

        var usuario = await _usuarioService.ReadOIDAsync(usuarioId);
        if (usuario is null) return Forbid();
        if (BuscarMazoJugable(usuario) is null)
        {
            TempData["Error"] = "Necesitas un mazo jugable de exactamente 30 cartas para iniciar una partida.";
            return RedirectToAction(nameof(Index));
        }

        var ok = await EjecutarCasoDeUsoAsync(uow =>
            new CraftCaster.Application.UseCases.Usuario.UsuarioUseCase(uow).IniciarPartidaAsync(usuarioId));

        if (ok)
        {
            var creada = await PartidaActivaDeAsync(usuarioId);
            TempData["Mensaje"] = "Partida creada con tu mazo de 30 cartas. Esperando a que se una un rival.";
            if (creada is not null)
            {
                return RedirectToAction(nameof(Tablero), new { id = creada.Id });
            }
        }

        return RedirectToAction(nameof(Index));
    }

    // POST /Juego/Unirse  -> CustomTransaction Usuario.unirsePartida + reparto inicial y Turno 1
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unirse(UnirsePartidaFormViewModel form)
    {
        if (UsuarioActualId() is not int usuarioId) return Forbid();

        if (!ModelState.IsValid || form.PartidaId is not int partidaId)
        {
            TempData["Error"] = "Elige una partida válida.";
            return RedirectToAction(nameof(Index));
        }

        if (await PartidaActivaDeAsync(usuarioId) is not null)
        {
            TempData["Error"] = "Ya estás en una partida. Abandónala antes de unirte a otra.";
            return RedirectToAction(nameof(Index));
        }

        var usuario = await _usuarioService.ReadOIDAsync(usuarioId);
        if (usuario is null) return Forbid();
        if (BuscarMazoJugable(usuario) is null)
        {
            TempData["Error"] = "Necesitas un mazo jugable de exactamente 30 cartas para unirte a una partida.";
            return RedirectToAction(nameof(Index));
        }

        var ok = await EjecutarCasoDeUsoAsync(uow =>
            new CraftCaster.Application.UseCases.Usuario.UsuarioUseCase(uow).UnirsePartidaAsync(usuarioId, partidaId));
        if (!ok) return RedirectToAction(nameof(Index));

        // CustomTransaction Partida.empezar: mano inicial y primer turno, en una transacción
        if (await EjecutarCasoDeUsoAsync(uow =>
                new CraftCaster.Application.UseCases.Partida.PartidaUseCase(uow).EmpezarAsync(partidaId)))
        {
            TempData["Mensaje"] = "¡Te has unido a la partida! Se han repartido las cartas iniciales y comienza el turno 1.";
        }

        return RedirectToAction(nameof(Tablero), new { id = partidaId });
    }

    // POST /Juego/Abandonar  -> CustomTransaction Usuario.abandonarPartida
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Abandonar()
    {
        if (UsuarioActualId() is not int usuarioId) return Forbid();

        var activa = await PartidaActivaDeAsync(usuarioId);
        if (activa is null)
        {
            TempData["Error"] = "No estás en ninguna partida.";
            return RedirectToAction(nameof(Index));
        }
        var partidaId = activa.Id;

        var ok = await EjecutarCasoDeUsoAsync(uow =>
            new CraftCaster.Application.UseCases.Usuario.UsuarioUseCase(uow).AbandonarPartidaAsync(usuarioId));
        if (!ok) return RedirectToAction(nameof(Index));

        await using var lectura = _unitOfWorkFactory.Create();
        var partida = await lectura.PartidaRepository.ReadAsync(partidaId);
        if (partida is not null && partida.Estado == EstadoPartida.Finalizada)
        {
            if (partida.Usuario1.Id == usuarioId) return RedirectToAction(nameof(Fin), new { id = partidaId });

            TempData["Mensaje"] = $"Has abandonado la partida. La victoria es para {partida.Usuario1.NombreUsuario}.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Mensaje"] = "Has abandonado la partida.";
        return RedirectToAction(nameof(Index));
    }

    // GET /Juego/Historial  -> ReadFilter Partida.obtenerPartidasUsuario
    [HttpGet]
    public async Task<IActionResult> Historial()
    {
        if (UsuarioActualId() is not int usuarioId) return Forbid();

        var partidas = await _partidaService.ObtenerPartidasUsuarioAsync(usuarioId);
        var model = new HistorialViewModel
        {
            UsuarioId = usuarioId,
            Partidas = partidas
                .OrderByDescending(p => p.FechaInicio)
                .Select(p =>
                {
                    var soyLocal = p.Usuario1.Id == usuarioId;
                    return new PartidaHistorialViewModel
                    {
                        Id = p.Id,
                        FechaInicio = p.FechaInicio,
                        Estado = p.Estado,
                        Rival = soyLocal ? (p.Usuario2?.NombreUsuario ?? "Esperando rival") : p.Usuario1.NombreUsuario,
                        MiMazo = soyLocal ? p.Mazo1?.Nombre : p.Mazo2?.Nombre,
                        Resultado = p.Estado != EstadoPartida.Finalizada ? "En juego"
                            : p.Ganador is null ? "Sin ganador"
                            : p.Ganador == usuarioId ? "Victoria" : "Derrota",
                    };
                })
                .ToList(),
        };

        return View(model);
    }

    // =====================================================================
    // TABLERO PARTIDA
    // =====================================================================

    // GET /Juego/Tablero/5
    [HttpGet]
    public async Task<IActionResult> Tablero(int id)
    {
        if (UsuarioActualId() is not int usuarioId) return Forbid();

        var partida = await _partidaService.ReadOIDAsync(id);
        if (partida is null) return NotFound();
        if (!Participa(partida, usuarioId)) return Forbid();
        if (partida.Estado == EstadoPartida.Finalizada) return RedirectToAction(nameof(Fin), new { id });

        // Si ambos jugadores ya están unidos pero la partida no ha repartido mano ni iniciado turno 1, la arrancamos
        if (partida.Estado == EstadoPartida.EnCurso && partida.Jugadores.Count >= 2 &&
            partida.Turnos.Count == 0 && !partida.Jugadores.Any(j => j.Activo))
        {
            // CustomTransaction Partida.empezar: mano inicial y primer turno, en una transacción.
            // Si falla no se redirige (evita un bucle de redirecciones): se muestra el tablero con el error.
            if (await EjecutarCasoDeUsoAsync(uow =>
                    new CraftCaster.Application.UseCases.Partida.PartidaUseCase(uow).EmpezarAsync(id)))
            {
                return RedirectToAction(nameof(Tablero), new { id });
            }
        }

        var miNumero = partida.Usuario1.Id == usuarioId ? 1 : 2;
        var yo = partida.Jugadores.FirstOrDefault(j => j.NumJugador == miNumero);
        if (yo is null)
        {
            TempData["Error"] = $"La partida #{id} está incompleta: no tiene creado tu jugador (JugadorPartida {miNumero}).";
            return RedirectToAction(nameof(Index));
        }
        var rival = partida.Jugadores.FirstOrDefault(j => j.NumJugador != miNumero);

        var sinEmpezar = partida.Estado != EstadoPartida.EnCurso || rival is null || !partida.Jugadores.Any(j => j.Activo);
        int? idJugadorActivo = null;
        if (!sinEmpezar)
        {
            var indiceActivo = await _partidaService.GetActivoAsync(id);
            idJugadorActivo = await _partidaService.GetPlayerAsync(id, indiceActivo);
        }
        var esMiTurno = idJugadorActivo == yo.Id;

        var mazo1 = (await _mazoService.ObtenerMazoJugador1Async(id)).FirstOrDefault()?.Nombre;
        var mazo2 = (await _mazoService.ObtenerMazoJugador2Async(id)).FirstOrDefault()?.Nombre;

        var nombreLocal = partida.Usuario1.NombreUsuario;
        var nombreVisitante = partida.Usuario2?.NombreUsuario ?? "Esperando rival";

        int criaturasEnemigas = rival?.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Campo)?.Cartas.Count ?? 0;
        int criaturasPropias = yo.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Campo)?.Cartas.Count ?? 0;

        var yoVm = await ToJugadorAsync(
            partida, yo, miNumero == 1 ? nombreLocal : nombreVisitante, miNumero == 1 ? mazo1 : mazo2,
            soyYo: true, esMiTurno, criaturasEnemigas, criaturasPropias);

        var rivalVm = rival is null
            ? null
            : await ToJugadorAsync(
                partida, rival, miNumero == 1 ? nombreVisitante : nombreLocal, miNumero == 1 ? mazo2 : mazo1,
                soyYo: false, esMiTurno: false, criaturasPropias, criaturasEnemigas);

        var jugadorActivoEnt = idJugadorActivo is int actId
            ? partida.Jugadores.FirstOrDefault(j => j.Id == actId)
            : null;
        int rondaActual = jugadorActivoEnt is null
            ? 0
            : Math.Max(1, partida.Turnos.Count(t => t.Jugador?.Id == jugadorActivoEnt.Id));

        var model = new TableroViewModel
        {
            PartidaId = partida.Id,
            Estado = partida.Estado,
            NumTurno = partida.Turnos.Count,
            RondaActual = rondaActual,
            PartidaSinEmpezar = sinEmpezar,
            EsMiTurno = esMiTurno,
            NombreJugadorActivo = jugadorActivoEnt is null
                ? null
                : (jugadorActivoEnt.NumJugador == 1 ? nombreLocal : nombreVisitante),
            FirmaEstado = CalcularFirmaEstado(partida),
            Yo = yoVm,
            Rival = rivalVm,
        };

        return View(model);
    }

    // GET /Juego/EstadoTablero/5 -> Polling ligero para detectar cambios sin recargar innecesariamente
    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> EstadoTablero(int id)
    {
        if (UsuarioActualId() is not int usuarioId) return Forbid();

        var partida = await _partidaService.ReadOIDAsync(id);
        if (partida is null) return NotFound();
        if (!Participa(partida, usuarioId)) return Forbid();

        var miNumero = partida.Usuario1.Id == usuarioId ? 1 : 2;
        var yo = partida.Jugadores.FirstOrDefault(j => j.NumJugador == miNumero);

        return Json(new
        {
            estado = partida.Estado.ToString(),
            numTurno = partida.Turnos.Count,
            esMiTurno = yo?.Activo == true,
            firma = CalcularFirmaEstado(partida),
        });
    }

    // POST /Juego/Robar/5 -> Sin robo manual: se roba al repartir la mano inicial (Partida.empezar), al empezar
    // cada turno (Partida.pasarTurno) y con la habilidad Robar (Habilidad.ejecutar).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Robar(int id)
    {
        TempData["Mensaje"] = "El robo es automático: robas 1 carta al comenzar cada uno de tus turnos.";
        return RedirectToAction(nameof(Tablero), new { id });
    }

    // POST /Juego/Lanzar/5 -> Lanza Criatura a mesa o Instantáneo con objetivo y resuelve habilidades automáticamente
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lanzar(int id, [Bind(Prefix = nameof(TableroViewModel.Lanzar))] LanzarCartaFormViewModel lanzar)
    {
        var (yo, error) = await CargarMiJugadorAsync(id, exigirMiTurno: true);
        if (error is not null) return error;

        if (!ModelState.IsValid || lanzar.CartaPartidaId is not int cartaId)
        {
            TempData["Error"] = "Elige una carta válida de tu mano.";
            return RedirectToAction(nameof(Tablero), new { id });
        }

        try
        {
            // CustomTransaction JugadorPartida.lanzarCarta (objetivo 0 = sin carta objetivo)
            string resumen;
            await using (var uow = _unitOfWorkFactory.Create())
            {
                resumen = await new CraftCaster.Application.UseCases.JugadorPartida.JugadorPartidaUseCase(uow).LanzarCartaAsync(
                    yo!.Id, cartaId, lanzar.ObjetivoCartaId ?? 0, lanzar.ObjetivoJugador);
            }

            TempData["Mensaje"] = resumen;
            if (await FinalizarSiHayPerdedorAsync(id)) return RedirectToAction(nameof(Fin), new { id });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (!EsErrorDeNegocio(ex)) _logger.LogError(ex, "Error al lanzar carta {CartaId} en partida {Id}.", cartaId, id);
            TempData["Error"] = MensajeDe(ex);
        }

        return RedirectToAction(nameof(Tablero), new { id });
    }

    // POST /Juego/Atacar/5 -> Combate de criatura en mesa apuntando a criatura rival (o jugador si mesa rival vacía)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Atacar(int id, [Bind(Prefix = nameof(TableroViewModel.Atacar))] AtacarFormViewModel atacar)
    {
        var (yo, error) = await CargarMiJugadorAsync(id, exigirMiTurno: true);
        if (error is not null) return error;

        if (!ModelState.IsValid || atacar.AtacanteId is not int atacanteId)
        {
            TempData["Error"] = "Elige una criatura de tu mesa para atacar.";
            return RedirectToAction(nameof(Tablero), new { id });
        }

        try
        {
            // CustomTransaction JugadorPartida.atacar (objetivo 0 = sin carta objetivo)
            string resumen;
            await using (var uow = _unitOfWorkFactory.Create())
            {
                resumen = await new CraftCaster.Application.UseCases.JugadorPartida.JugadorPartidaUseCase(uow).AtacarAsync(
                    yo!.Id, atacanteId, atacar.ObjetivoCartaId ?? 0, atacar.ObjetivoJugador);
            }

            TempData["Mensaje"] = resumen;
            if (await FinalizarSiHayPerdedorAsync(id)) return RedirectToAction(nameof(Fin), new { id });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (!EsErrorDeNegocio(ex)) _logger.LogError(ex, "Error al atacar con {AtacanteId} en partida {Id}.", atacanteId, id);
            TempData["Error"] = MensajeDe(ex);
        }

        return RedirectToAction(nameof(Tablero), new { id });
    }

    // POST /Juego/PasarTurno/5 -> Termina turno (FinTurno -> PasarTurno (+1 maná sin tope, robo auto) -> InicioTurno)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PasarTurno(int id)
    {
        var (yo, error) = await CargarMiJugadorAsync(id, exigirMiTurno: false);
        if (error is not null) return error;

        var partida = yo!.Partida;
        var sinEmpezar = !partida.Jugadores.Any(j => j.Activo);
        if (!sinEmpezar && !yo.Activo)
        {
            TempData["Error"] = "No es tu turno.";
            return RedirectToAction(nameof(Tablero), new { id });
        }

        try
        {
            await using var uow = _unitOfWorkFactory.Create();
            var partidaUseCase = new CraftCaster.Application.UseCases.Partida.PartidaUseCase(uow);

            if (sinEmpezar)
            {
                await partidaUseCase.EmpezarAsync(id);
                TempData["Mensaje"] = "¡Empieza la partida!";
                return RedirectToAction(nameof(Tablero), new { id });
            }

            // CustomTransaction Partida.pasarTurno: devuelve el resumen de las habilidades de fin e inicio de turno
            var resumen = await partidaUseCase.PasarTurnoAsync(id);
            TempData["Mensaje"] = string.IsNullOrWhiteSpace(resumen)
                ? "Has terminado tu turno."
                : $"Has terminado tu turno. {resumen}";
            if (await FinalizarSiHayPerdedorAsync(id)) return RedirectToAction(nameof(Fin), new { id });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (!EsErrorDeNegocio(ex)) _logger.LogError(ex, "Error al pasar turno en partida {Id}.", id);
            TempData["Error"] = MensajeDe(ex);
        }

        return RedirectToAction(nameof(Tablero), new { id });
    }

    // =====================================================================
    // FIN DE PARTIDA
    // =====================================================================

    // GET /Juego/Fin/5
    [HttpGet]
    public async Task<IActionResult> Fin(int id)
    {
        if (UsuarioActualId() is not int usuarioId) return Forbid();

        var partida = await _partidaService.ReadOIDAsync(id);
        if (partida is null) return NotFound();
        if (partida.Estado != EstadoPartida.Finalizada) return RedirectToAction(nameof(Tablero), new { id });

        var participa = Participa(partida, usuarioId) || partida.Ganador == usuarioId;
        if (!participa && !User.IsInRole(HariaWebAuthentication.RolAdmin)) return Forbid();

        var ganador = partida.Ganador is int ganadorId ? await _usuarioService.ReadOIDAsync(ganadorId) : null;
        var local = partida.Jugadores.FirstOrDefault(j => j.NumJugador == 1);
        var visitante = partida.Jugadores.FirstOrDefault(j => j.NumJugador == 2);

        return View(new FinPartidaViewModel
        {
            PartidaId = partida.Id,
            FechaInicio = partida.FechaInicio,
            NumTurnos = partida.Turnos.Count,
            GanadorId = partida.Ganador,
            GanadorNombre = ganador?.NombreUsuario,
            HeGanado = partida.Ganador == usuarioId,
            SoyParticipante = participa,
            NombreLocal = partida.Usuario1.NombreUsuario,
            VidaLocal = local?.Vida,
            NombreVisitante = partida.Usuario2?.NombreUsuario,
            VidaVisitante = visitante?.Vida,
        });
    }

    // =====================================================================
    // Auxiliares
    // =====================================================================

    private static Mazo? BuscarMazoJugable(Usuario usuario) =>
        usuario.Mazos.FirstOrDefault(m =>
            m.EstadoMazo == EstadoMazo.Jugable || m.Lineas.Sum(l => l.Cantidad) == 30);

    private async Task<(JugadorPartida? Yo, IActionResult? Error)> CargarMiJugadorAsync(int partidaId, bool exigirMiTurno)
    {
        if (UsuarioActualId() is not int usuarioId) return (null, Forbid());

        var partida = await _partidaService.ReadOIDAsync(partidaId);
        if (partida is null) return (null, NotFound());
        if (!Participa(partida, usuarioId)) return (null, Forbid());

        if (partida.Estado == EstadoPartida.Finalizada) return (null, RedirectToAction(nameof(Fin), new { id = partidaId }));
        if (partida.Estado != EstadoPartida.EnCurso)
        {
            TempData["Error"] = "La partida todavía no ha empezado: falta un rival.";
            return (null, RedirectToAction(nameof(Tablero), new { id = partidaId }));
        }

        var miNumero = partida.Usuario1.Id == usuarioId ? 1 : 2;
        var yo = partida.Jugadores.FirstOrDefault(j => j.NumJugador == miNumero);
        if (yo is null)
        {
            TempData["Error"] = $"La partida #{partidaId} está incompleta: no tiene creado tu jugador.";
            return (null, RedirectToAction(nameof(Index)));
        }

        if (exigirMiTurno && !yo.Activo)
        {
            TempData["Error"] = "No es tu turno.";
            return (null, RedirectToAction(nameof(Tablero), new { id = partidaId }));
        }

        return (yo, null);
    }

    private async Task<bool> EjecutarCasoDeUsoAsync(Func<IUnitOfWork, Task> casoDeUso)
    {
        try
        {
            await using var unitOfWork = _unitOfWorkFactory.Create();
            await casoDeUso(unitOfWork);
            return true;
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            if (!EsErrorDeNegocio(error)) _logger.LogError(error, "Error al ejecutar un caso de uso de partida.");
            TempData["Error"] = MensajeDe(error);
            return false;
        }
    }

    // FIN DE PARTIDA: si tras la jugada algún jugador se ha quedado sin vida, CustomTransaction Partida.finalizar
    // (decide el ganador). Va en su propia transacción, después de confirmar la jugada.
    private async Task<bool> FinalizarSiHayPerdedorAsync(int partidaId)
    {
        await using var uow = _unitOfWorkFactory.Create();
        var partida = await uow.PartidaRepository.ReadAsync(partidaId);
        if (partida is null) return false;
        if (partida.Estado == EstadoPartida.Finalizada) return true;
        if (partida.Estado != EstadoPartida.EnCurso || partida.Jugadores.Count < 2 || !partida.Jugadores.Any(j => j.Vida <= 0))
        {
            return false;
        }

        await new CraftCaster.Application.UseCases.Partida.PartidaUseCase(uow).FinalizarAsync(partidaId);
        return true;
    }

    private async Task<Partida?> PartidaActivaDeAsync(int usuarioId) =>
        (await _partidaService.ObtenerPartidasUsuarioAsync(usuarioId))
            .FirstOrDefault(p => p.Estado != EstadoPartida.Finalizada);

    private static bool Participa(Partida partida, int usuarioId) =>
        partida.Usuario1.Id == usuarioId || partida.Usuario2?.Id == usuarioId;

    private async Task<JugadorTableroViewModel> ToJugadorAsync(
        Partida partida,
        JugadorPartida jugador,
        string nombre,
        string? mazo,
        bool soyYo,
        bool esMiTurno,
        int criaturasEnemigas,
        int criaturasPropias)
    {
        ZonaPartida? Zona(TipoZona tipo) => jugador.Zonas.FirstOrDefault(z => z.TipoZona == tipo);

        var zonaMazo = Zona(TipoZona.Mazo);
        var mano = Zona(TipoZona.Mano)?.Cartas ?? new List<CartaPartida>();
        var campo = Zona(TipoZona.Campo)?.Cartas ?? new List<CartaPartida>();
        var descarte = Zona(TipoZona.Descarte)?.Cartas ?? new List<CartaPartida>();

        var turnosJugador = partida.Turnos.Where(t => t.Jugador?.Id == jugador.Id).OrderByDescending(t => t.NumTurno).ToList();
        int manaMaximo = turnosJugador.FirstOrDefault()?.ManaTurno ?? jugador.Mana;

        var model = new JugadorTableroViewModel
        {
            JugadorId = jugador.Id,
            NumJugador = jugador.NumJugador,
            Nombre = nombre,
            Mazo = mazo,
            Vida = jugador.Vida,
            Mana = jugador.Mana,
            ManaMaximo = Math.Max(manaMaximo, jugador.Mana),
            TurnosJugados = turnosJugador.Count,
            Activo = jugador.Activo,
            CartasEnMazo = zonaMazo is null ? 0 : (await _zonaPartidaService.GetAllCartasAsync(zonaMazo.Id)).Count,
            CartasEnMano = mano.Count,
            // La mano del rival está oculta: solo se envía el número de cartas
            Mano = soyYo ? mano.Select(c => ToCarta(c, criaturasEnemigas, criaturasPropias)).ToList() : new List<CartaTableroViewModel>(),
            Campo = campo.Select(c => ToCarta(c, criaturasEnemigas, criaturasPropias)).ToList(),
            Descarte = descarte.Select(c => ToCarta(c, criaturasEnemigas, criaturasPropias)).ToList(),
        };

        // Custom Carta.modoObjetivo: objetivo que pide cada carta de la mano al lanzarla
        foreach (var carta in model.Mano)
        {
            carta.ModoObjetivo = (ModoObjetivoCarta)await _cartaService.ModoObjetivoAsync(
                carta.CartaOriginalId, criaturasEnemigas, criaturasPropias);
        }

        if (soyYo && esMiTurno)
        {
            foreach (var carta in model.Mano)
            {
                carta.SePuedeLanzar = carta.CosteMana <= jugador.Mana;
            }
            foreach (var carta in model.Campo)
            {
                carta.PuedeAtacarAhora = carta.Tipo == TipoCarta.Criatura && carta.PuedeAtacar && carta.AtaqueActual > 0;
            }
        }

        return model;
    }

    private CartaTableroViewModel ToCarta(CartaPartida carta, int criaturasEnemigas, int criaturasPropias) => new()
    {
        CartaPartidaId = carta.Id,
        CartaOriginalId = carta.Original.Id,
        Nombre = carta.Original.Nombre,
        Descripcion = carta.Original.Descripcion,
        ImagenUrl = BuscarImagenCarta(carta.Original.Id),
        Tipo = carta.Original.Tipo,
        Rareza = carta.Original.Rareza,
        CosteMana = carta.Original.CosteMana,
        VidaBase = carta.Original.Vida ?? 0,
        AtaqueBase = carta.Original.Ataque ?? 0,
        VidaActual = carta.VidaActual,
        AtaqueActual = carta.AtaqueActual,
        PuedeAtacar = carta.PuedeAtacar,
        Habilidades = (carta.Original.Estados ?? new List<Estado>())
            .SelectMany(estado => (estado.Habilidades ?? new List<Habilidad>()).Select(h => new HabilidadTableroViewModel
            {
                Id = h.Id,
                Momento = estado.Tipo,
                Tipo = h.Tipo,
                Cantidad = h.Cantidad,
                TipoObjetivo = h.TipoObjetivo,
                Objetivo = h.Objetivo,
            }))
            .ToList(),
    };

    private string? BuscarImagenCarta(int cartaId)
    {
        var dir = Path.Combine(_env.WebRootPath, "uploads", "cartas");
        if (!Directory.Exists(dir)) return null;
        var file = Directory.GetFiles(dir, $"carta_{cartaId}.*").FirstOrDefault();
        return file is null ? null : $"/uploads/cartas/{Path.GetFileName(file)}";
    }

    private static string CalcularFirmaEstado(Partida partida)
    {
        var sb = new StringBuilder();
        sb.Append(partida.Estado).Append('|').Append(partida.Turnos.Count).Append('|').Append(partida.Ganador ?? 0);
        foreach (var j in partida.Jugadores.OrderBy(x => x.NumJugador))
        {
            sb.Append(';').Append(j.Id).Append(':').Append(j.Activo ? 1 : 0)
              .Append(':').Append(j.Vida).Append(':').Append(j.Mana);
            foreach (var z in j.Zonas.OrderBy(x => (int)x.TipoZona))
            {
                sb.Append('/').Append((int)z.TipoZona).Append('=');
                if (z.TipoZona == TipoZona.Campo)
                {
                    foreach (var c in z.Cartas.OrderBy(x => x.Id))
                    {
                        sb.Append(c.Id).Append('.').Append(c.VidaActual).Append('.').Append(c.AtaqueActual).Append('.').Append(c.PuedeAtacar ? 1 : 0).Append(',');
                    }
                }
                else
                {
                    sb.Append(z.Cartas.Count);
                }
            }
        }
        return sb.ToString();
    }

    private int? UsuarioActualId() =>
        User.FindFirstValue("HariaUserClass") == "Usuario"
        && int.TryParse(User.FindFirstValue(ClaimTypes.Name), out var id)
            ? id
            : null;

    private const string ErrorGenericoCasoDeUso = "Application error";

    private static string MensajeDe(Exception error)
    {
        string? mensaje = null;
        for (var actual = error; actual is not null; actual = actual.InnerException)
        {
            if (actual is ModelException && !actual.Message.StartsWith(ErrorGenericoCasoDeUso, StringComparison.Ordinal))
            {
                mensaje = actual.Message;
            }
        }
        return mensaje ?? "No se ha podido completar la acción. Inténtalo de nuevo.";
    }

    private static bool EsErrorDeNegocio(Exception error)
    {
        for (var actual = error; actual is not null; actual = actual.InnerException)
        {
            if (actual is ModelException && !actual.Message.StartsWith(ErrorGenericoCasoDeUso, StringComparison.Ordinal)) return true;
        }
        return false;
    }
}
