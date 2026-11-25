using System.Security.Claims;
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
//   TABLERO PARTIDA   -> Tablero, Robar, Lanzar, ActivarHabilidad, PasarTurno
//                        CustomTransaction JugadorPartida.robarCarta / lanzarCarta, Partida.pasarTurno, Habilidad.activar
//   FIN DE PARTIDA    -> Fin (ganador)  CustomTransaction Partida.finalizar
//
// El controlador solo coordina: las reglas del juego (maná, turno activo, zonas...) las comprueban los casos de uso.
// Aquí solo se comprueba lo que es de la interfaz: que el usuario de la cookie juega esa partida.
// Las lecturas (GET) usan los servicios inyectados; cada escritura (POST) crea su Unit of Work para el caso de uso.
[Authorize(Roles = HariaWebAuthentication.RolCliente + "," + HariaWebAuthentication.RolAdmin)]
public sealed class JuegoController : Controller
{
    private readonly PartidaService _partidaService;
    private readonly UsuarioService _usuarioService;
    private readonly MazoService _mazoService;
    private readonly ZonaPartidaService _zonaPartidaService;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly ILogger<JuegoController> _logger;

    public JuegoController(
        PartidaService partidaService,
        UsuarioService usuarioService,
        MazoService mazoService,
        ZonaPartidaService zonaPartidaService,
        IUnitOfWorkFactory unitOfWorkFactory,
        ILogger<JuegoController> logger)
    {
        _partidaService = partidaService;
        _usuarioService = usuarioService;
        _mazoService = mazoService;
        _zonaPartidaService = zonaPartidaService;
        _unitOfWorkFactory = unitOfWorkFactory;
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

        // ReadFilter Partida.obtenerPartidasUsuario: la partida no finalizada en la que participa
        var misPartidas = await _partidaService.ObtenerPartidasUsuarioAsync(usuarioId);
        var activa = misPartidas.FirstOrDefault(p => p.Estado != EstadoPartida.Finalizada);

        var model = new SalaViewModel
        {
            UsuarioId = usuarioId,
            PartidaActiva = activa is null ? null : PartidaAssembler.ToViewModel(activa),
            SoyLocal = activa is not null && activa.Usuario1.Id == usuarioId,
            MazoConElQueJuego = (usuario.Mazos.FirstOrDefault(m => m.EstadoMazo == EstadoMazo.Jugable) ?? usuario.Mazos.FirstOrDefault())?.Nombre,
        };

        if (activa is null)
        {
            // ReadAll de Partida: las que están en espera y no son mías
            var enEspera = (await _partidaService.ReadAllAsync())
                // Solo partidas bien creadas (con el JugadorPartida del anfitrión); las creadas a mano por el CRUD no se pueden jugar
                .Where(p => p.Estado == EstadoPartida.Espera && p.Usuario2 is null && p.Usuario1.Id != usuarioId && p.Jugadores.Any())
                .OrderBy(p => p.FechaInicio);

            foreach (var partida in enEspera)
            {
                // ReadFilter Usuario.obtenerUsuarioLocal y Mazo.obtenerMazoJugador1 (asociaciones Usuario/Mazo - Partida)
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

        // iniciarPartida no comprueba si ya juega otra: se evita desde la interfaz
        if (await PartidaActivaDeAsync(usuarioId) is not null)
        {
            TempData["Error"] = "Ya estás en una partida. Abandónala antes de crear otra.";
            return RedirectToAction(nameof(Index));
        }

        var ok = await EjecutarCasoDeUsoAsync(uow =>
            new CraftCaster.Application.UseCases.Usuario.UsuarioUseCase(uow).IniciarPartidaAsync(usuarioId));
        if (ok) TempData["Mensaje"] = "Partida creada. Esperando a que se una un rival.";

        return RedirectToAction(nameof(Index));
    }

    // POST /Juego/Unirse  -> CustomTransaction Usuario.unirsePartida
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

        var ok = await EjecutarCasoDeUsoAsync(uow =>
            new CraftCaster.Application.UseCases.Usuario.UsuarioUseCase(uow).UnirsePartidaAsync(usuarioId, partidaId));
        if (!ok) return RedirectToAction(nameof(Index));

        TempData["Mensaje"] = "Te has unido a la partida.";
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

        // Si estaba en curso, el caso de uso la finaliza y da la victoria al rival; si estaba en espera, la borra
        await using var lectura = _unitOfWorkFactory.Create();
        var partida = await lectura.PartidaRepository.ReadAsync(partidaId);
        if (partida is not null && partida.Estado == EstadoPartida.Finalizada)
        {
            if (partida.Usuario1.Id == usuarioId) return RedirectToAction(nameof(Fin), new { id = partidaId });

            // El visitante deja de figurar en la partida, así que no puede ver la pantalla de fin
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

        var miNumero = partida.Usuario1.Id == usuarioId ? 1 : 2;
        var yo = partida.Jugadores.FirstOrDefault(j => j.NumJugador == miNumero);
        if (yo is null)
        {
            TempData["Error"] = $"La partida #{id} está incompleta: no tiene creado tu jugador (JugadorPartida {miNumero}). "
                + "Probablemente se creó con el CRUD y no con iniciarPartida/unirsePartida. Abandónala o pide a un administrador que la borre.";
            return RedirectToAction(nameof(Index));
        }
        var rival = partida.Jugadores.FirstOrDefault(j => j.NumJugador != miNumero);

        // Custom Partida.getActivo (0 = jugador 1, 1 = jugador 2) + Partida.getPlayer (id del JugadorPartida)
        var sinEmpezar = partida.Estado != EstadoPartida.EnCurso || rival is null || !partida.Jugadores.Any(j => j.Activo);
        int? idJugadorActivo = null;
        if (!sinEmpezar)
        {
            var indiceActivo = await _partidaService.GetActivoAsync(id);
            idJugadorActivo = await _partidaService.GetPlayerAsync(id, indiceActivo);
        }
        var esMiTurno = idJugadorActivo == yo.Id;

        // ReadFilter Mazo.obtenerMazoJugador1 / obtenerMazoJugador2
        var mazo1 = (await _mazoService.ObtenerMazoJugador1Async(id)).FirstOrDefault()?.Nombre;
        var mazo2 = (await _mazoService.ObtenerMazoJugador2Async(id)).FirstOrDefault()?.Nombre;

        var nombreLocal = partida.Usuario1.NombreUsuario;
        var nombreVisitante = partida.Usuario2?.NombreUsuario ?? "Esperando rival";

        var model = new TableroViewModel
        {
            PartidaId = partida.Id,
            Estado = partida.Estado,
            NumTurno = partida.Turnos.Count,
            PartidaSinEmpezar = sinEmpezar,
            EsMiTurno = esMiTurno,
            NombreJugadorActivo = idJugadorActivo is null ? null
                : (partida.Jugadores.First(j => j.Id == idJugadorActivo).NumJugador == 1 ? nombreLocal : nombreVisitante),
            Yo = await ToJugadorAsync(yo, miNumero == 1 ? nombreLocal : nombreVisitante, miNumero == 1 ? mazo1 : mazo2, soyYo: true, esMiTurno),
            Rival = rival is null ? null
                : await ToJugadorAsync(rival, miNumero == 1 ? nombreVisitante : nombreLocal, miNumero == 1 ? mazo2 : mazo1, soyYo: false, esMiTurno: false),
        };

        return View(model);
    }

    // POST /Juego/Robar/5  -> CustomTransaction JugadorPartida.robarCarta
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Robar(int id)
    {
        var (yo, error) = await CargarMiJugadorAsync(id, exigirMiTurno: true);
        if (error is not null) return error;

        var jugadorId = yo!.Id;
        if (await EjecutarCasoDeUsoAsync(uow =>
                new CraftCaster.Application.UseCases.JugadorPartida.JugadorPartidaUseCase(uow).RobarCartaAsync(jugadorId)))
        {
            TempData["Mensaje"] = "Has robado una carta.";
        }

        return await DespuesDeJugadaAsync(id);
    }

    // POST /Juego/Lanzar/5  -> CustomTransaction JugadorPartida.lanzarCarta
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lanzar(int id, [Bind(Prefix = nameof(TableroViewModel.Lanzar))] LanzarCartaFormViewModel lanzar)
    {
        var (yo, error) = await CargarMiJugadorAsync(id, exigirMiTurno: true);
        if (error is not null) return error;

        if (!ModelState.IsValid || lanzar.CartaPartidaId is not int cartaId)
        {
            TempData["Error"] = "Elige una carta de tu mano.";
            return RedirectToAction(nameof(Tablero), new { id });
        }

        // La carta tiene que ser mía (el caso de uso comprueba además que esté en la mano y el maná)
        var jugador = yo!;
        var esMia = jugador.Zonas.Any(z => z.TipoZona == TipoZona.Mano && z.Cartas.Any(c => c.Id == cartaId));
        if (!esMia)
        {
            TempData["Error"] = "Esa carta no está en tu mano.";
            return RedirectToAction(nameof(Tablero), new { id });
        }

        if (await EjecutarCasoDeUsoAsync(uow =>
                new CraftCaster.Application.UseCases.JugadorPartida.JugadorPartidaUseCase(uow).LanzarCartaAsync(jugador.Id, cartaId)))
        {
            TempData["Mensaje"] = "Carta lanzada. Si tiene habilidades, puedes activarlas en el tablero.";
        }

        return await DespuesDeJugadaAsync(id);
    }

    // POST /Juego/ActivarHabilidad/5  -> CustomTransaction Habilidad.activar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActivarHabilidad(int id, [Bind(Prefix = nameof(TableroViewModel.Activar))] ActivarHabilidadFormViewModel activar)
    {
        var (yo, error) = await CargarMiJugadorAsync(id, exigirMiTurno: true);
        if (error is not null) return error;

        if (!ModelState.IsValid || activar.HabilidadId is not int habilidadId)
        {
            TempData["Error"] = "Elige una habilidad.";
            return RedirectToAction(nameof(Tablero), new { id });
        }

        // Solo habilidades de mis criaturas en el campo o de mis hechizos ya lanzados (descarte)
        var habilidadesDisponibles =
            from zona in yo!.Zonas
            from carta in zona.Cartas
            where zona.TipoZona == TipoZona.Campo
               || (zona.TipoZona == TipoZona.Descarte && carta.Original.Tipo == TipoCarta.Hechizo)
            from estado in carta.Original.Estados
            from habilidad in estado.Habilidades
            select habilidad.Id;

        if (!habilidadesDisponibles.Contains(habilidadId))
        {
            TempData["Error"] = "Esa habilidad no pertenece a ninguna de tus cartas en juego.";
            return RedirectToAction(nameof(Tablero), new { id });
        }

        if (await EjecutarCasoDeUsoAsync(uow =>
                new CraftCaster.Application.UseCases.Habilidad.HabilidadUseCase(uow).ActivarAsync(habilidadId, id)))
        {
            TempData["Mensaje"] = "Habilidad activada.";
        }

        return await DespuesDeJugadaAsync(id);
    }

    // POST /Juego/PasarTurno/5  -> CustomTransaction Partida.pasarTurno
    // También empieza la partida: si nadie tiene el turno, pasarTurno se lo da al jugador 1.
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

        if (await EjecutarCasoDeUsoAsync(uow =>
                new CraftCaster.Application.UseCases.Partida.PartidaUseCase(uow).PasarTurnoAsync(id)))
        {
            TempData["Mensaje"] = sinEmpezar ? "¡Empieza la partida!" : "Has terminado tu turno.";
        }

        return await DespuesDeJugadaAsync(id);
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

    // Después de cada jugada: si un jugador se ha quedado sin vida o sin cartas en el mazo se invoca
    // la CustomTransaction Partida.finalizar (que decide el ganador) y se muestra la pantalla de fin.
    private async Task<IActionResult> DespuesDeJugadaAsync(int partidaId)
    {
        try
        {
            await using var unitOfWork = _unitOfWorkFactory.Create();
            var partida = await unitOfWork.PartidaRepository.ReadAsync(partidaId);
            if (partida is not null && partida.Estado == EstadoPartida.EnCurso && partida.Jugadores.Count >= 2)
            {
                var hayPerdedor = partida.Jugadores.Any(j =>
                    j.Vida <= 0 || (j.Zonas.FirstOrDefault(z => z.TipoZona == TipoZona.Mazo)?.Cartas.Count ?? 0) == 0);
                if (hayPerdedor)
                {
                    await new CraftCaster.Application.UseCases.Partida.PartidaUseCase(unitOfWork).FinalizarAsync(partidaId);
                    return RedirectToAction(nameof(Fin), new { id = partidaId });
                }
            }
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            _logger.LogError(error, "No se ha podido comprobar el fin de la partida {Id}.", partidaId);
            TempData["Error"] = MensajeDe(error);
        }

        return RedirectToAction(nameof(Tablero), new { id = partidaId });
    }

    // Carga la partida y el JugadorPartida del usuario de la cookie. Devuelve un IActionResult si no puede jugar.
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

    // Ejecuta un caso de uso con su propio Unit of Work (el caso de uso abre, confirma o deshace la transacción)
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

    private async Task<Partida?> PartidaActivaDeAsync(int usuarioId) =>
        (await _partidaService.ObtenerPartidasUsuarioAsync(usuarioId))
            .FirstOrDefault(p => p.Estado != EstadoPartida.Finalizada);

    private static bool Participa(Partida partida, int usuarioId) =>
        partida.Usuario1.Id == usuarioId || partida.Usuario2?.Id == usuarioId;

    private async Task<JugadorTableroViewModel> ToJugadorAsync(JugadorPartida jugador, string nombre, string? mazo, bool soyYo, bool esMiTurno)
    {
        ZonaPartida? Zona(TipoZona tipo) => jugador.Zonas.FirstOrDefault(z => z.TipoZona == tipo);

        var zonaMazo = Zona(TipoZona.Mazo);
        var mano = Zona(TipoZona.Mano)?.Cartas ?? new List<CartaPartida>();
        var campo = Zona(TipoZona.Campo)?.Cartas ?? new List<CartaPartida>();
        var descarte = Zona(TipoZona.Descarte)?.Cartas ?? new List<CartaPartida>();

        var model = new JugadorTableroViewModel
        {
            JugadorId = jugador.Id,
            NumJugador = jugador.NumJugador,
            Nombre = nombre,
            Mazo = mazo,
            Vida = jugador.Vida,
            Mana = jugador.Mana,
            Activo = jugador.Activo,
            // Custom ZonaPartida.getAllCartas
            CartasEnMazo = zonaMazo is null ? 0 : (await _zonaPartidaService.GetAllCartasAsync(zonaMazo.Id)).Count,
            CartasEnMano = mano.Count,
            // La mano del rival está oculta: solo se envía el número de cartas
            Mano = soyYo ? mano.Select(ToCarta).ToList() : new List<CartaTableroViewModel>(),
            Campo = campo.Select(ToCarta).ToList(),
            Descarte = descarte.Select(ToCarta).ToList(),
        };

        if (soyYo && esMiTurno)
        {
            foreach (var carta in model.Mano) carta.SePuedeLanzar = carta.CosteMana <= jugador.Mana;
            foreach (var carta in model.Campo) carta.SePuedenActivarHabilidades = carta.Habilidades.Any();
            foreach (var carta in model.Descarte.Where(c => c.Tipo == TipoCarta.Hechizo)) carta.SePuedenActivarHabilidades = carta.Habilidades.Any();
        }

        return model;
    }

    // CartaPartida -> Carta (original) -> Estado -> Habilidad, aplanado en un ViewModel
    private static CartaTableroViewModel ToCarta(CartaPartida carta) => new()
    {
        CartaPartidaId = carta.Id,
        Nombre = carta.Original.Nombre,
        Descripcion = carta.Original.Descripcion,
        Tipo = carta.Original.Tipo,
        Rareza = carta.Original.Rareza,
        CosteMana = carta.Original.CosteMana,
        VidaActual = carta.VidaActual,
        AtaqueActual = carta.AtaqueActual,
        PuedeAtacar = carta.PuedeAtacar,
        Habilidades = carta.Original.Estados
            .SelectMany(estado => estado.Habilidades.Select(h => new HabilidadTableroViewModel
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

    // Id del usuario de la cookie (HARIA Web guarda el id en ClaimTypes.Name)
    private int? UsuarioActualId() =>
        User.FindFirstValue("HariaUserClass") == "Usuario"
        && int.TryParse(User.FindFirstValue(ClaimTypes.Name), out var id)
            ? id
            : null;

    private const string ErrorGenericoCasoDeUso = "Application error";

    // Los casos de uso envuelven los errores ("Application error while executing...");
    // se muestra el mensaje de negocio más interno si lo hay.
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
