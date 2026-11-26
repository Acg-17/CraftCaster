using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Application.UseCases.JugadorPartida;
using CraftCaster.Application.UseCases.Partida;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Web.Services;

public enum ModoObjetivoCarta
{
    Ninguno = 0,
    EnemigoOCriatura = 1,
    SoloCriaturaEnemiga = 2,
    SoloCriaturaPropia = 3,
    SoloJugadorEnemigo = 4,
}

public sealed record ResultadoAccionPartida(bool Finalizada, string Mensaje);

/// <summary>
/// Orquesta el inicio de partida (mano inicial de 4 cartas), el combate de criaturas
/// con protección de mesa y la ejecución automática de hechizos y habilidades por evento,
/// apoyándose en los servicios y casos de uso existentes sin alterar el código generado por HARIA.
/// </summary>
public sealed class MotorPartidaService
{
    public const int CartasInicialesMano = 4;
    private const int ProfundidadMaximaTriggers = 6;

    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public MotorPartidaService(IUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    /// <summary>
    /// Determina si una carta en mano requiere que el jugador elija un objetivo al lanzarla.
    /// </summary>
    public static ModoObjetivoCarta DeterminarModoObjetivo(Carta carta, int criaturasEnemigasEnMesa, int criaturasPropiasEnMesa)
    {
        var habilidades = ObtenerHabilidadesDeEntrada(carta);
        if (habilidades.Count == 0) return ModoObjetivoCarta.Ninguno;

        // Prioridad 1: daño dirigido a enemigo / libre (en instantáneos o criaturas al entrar)
        foreach (var hab in habilidades)
        {
            if (hab.Tipo == TipoHabilidad.Danyo && hab.TipoObjetivo is TipoObjetivo.Enemigo or TipoObjetivo.Libre)
            {
                return ModoObjetivoCarta.EnemigoOCriatura;
            }
        }

        // Prioridad 2: destruir criatura concreta
        foreach (var hab in habilidades)
        {
            if (hab.Tipo == TipoHabilidad.Destruir)
            {
                if (hab.TipoObjetivo == TipoObjetivo.Jugador)
                {
                    return criaturasPropiasEnMesa > 0 ? ModoObjetivoCarta.SoloCriaturaPropia : ModoObjetivoCarta.Ninguno;
                }

                return criaturasEnemigasEnMesa > 0 ? ModoObjetivoCarta.SoloCriaturaEnemiga : ModoObjetivoCarta.Ninguno;
            }
        }

        // Prioridad 3: hechizo de daño directo al jugador enemigo (permite apuntar al rival)
        if (carta.Tipo == TipoCarta.Hechizo &&
            habilidades.Any(h => h.Tipo == TipoHabilidad.Danyo && h.TipoObjetivo == TipoObjetivo.Jugador))
        {
            return ModoObjetivoCarta.SoloJugadorEnemigo;
        }

        return ModoObjetivoCarta.Ninguno;
    }

    /// <summary>
    /// Reparte la mano inicial de 4 cartas a ambos jugadores (desde sus mazos de 30 barajados)
    /// y arranca automáticamente el Turno 1 del jugador 1 si la partida aún no había empezado.
    /// </summary>
    public async Task PrepararInicioPartidaAsync(int partidaId)
    {
        bool debeIniciarPrimerTurno = false;

        await using (var uow = _unitOfWorkFactory.Create())
        {
            var partida = await uow.PartidaRepository.ReadAsync(partidaId);
            if (partida is null || partida.Estado != EstadoPartida.EnCurso || partida.Jugadores.Count < 2)
            {
                return;
            }

            if (partida.Turnos.Count > 0 || partida.Jugadores.Any(j => j.Activo))
            {
                return;
            }

            debeIniciarPrimerTurno = true;

            await uow.BeginTransactionAsync("MotorPartida.RepartirManoInicial");
            try
            {
                var jugadorService = new JugadorPartidaService(uow);
                foreach (var jugador in partida.Jugadores.OrderBy(j => j.NumJugador))
                {
                    var mano = ZonaDe(jugador, TipoZona.Mano);
                    var mazo = ZonaDe(jugador, TipoZona.Mazo);
                    if (mano is null || mazo is null || mano.Cartas.Count > 0) continue;

                    int porRobar = Math.Min(CartasInicialesMano, mazo.Cartas.Count);
                    for (int i = 0; i < porRobar; i++)
                    {
                        await jugadorService.RobarCartaDelMazoAsync(jugador.Id);
                    }
                }

                await uow.CommitAsync();
            }
            catch
            {
                await uow.RollbackAsync();
                throw;
            }
        }

        if (debeIniciarPrimerTurno)
        {
            await using var uowTurno = _unitOfWorkFactory.Create();
            await new PartidaUseCase(uowTurno).PasarTurnoAsync(partidaId);
        }
    }

    /// <summary>
    /// Lanza una carta de la mano (Criatura a la mesa o Hechizo instantáneo al descarte)
    /// y resuelve automáticamente sus habilidades de entrada con el objetivo seleccionado.
    /// </summary>
    public async Task<ResultadoAccionPartida> LanzarCartaConEfectosAsync(
        int partidaId,
        int jugadorId,
        int cartaPartidaId,
        int? objetivoCartaId,
        bool objetivoJugador)
    {
        string nombreCarta;
        TipoCarta tipoCarta;

        // 1. Validación previa antes de gastar maná
        await using (var uowVal = _unitOfWorkFactory.Create())
        {
            var (partida, jugador, rival) = await CargarJugadoresActivosAsync(uowVal, partidaId, jugadorId);
            var mano = ZonaDe(jugador, TipoZona.Mano)
                ?? throw new ModelException("No dispones de zona de mano.");
            var carta = mano.Cartas.FirstOrDefault(c => c.Id == cartaPartidaId)
                ?? throw new ModelException("Esa carta no está en tu mano.");

            if (carta.Original.CosteMana > jugador.Mana)
            {
                throw new ModelException($"Maná insuficiente: necesitas {carta.Original.CosteMana} y tienes {jugador.Mana}.");
            }

            var campoRival = ZonaDe(rival, TipoZona.Campo)?.Cartas ?? new List<CartaPartida>();
            var campoPropio = ZonaDe(jugador, TipoZona.Campo)?.Cartas ?? new List<CartaPartida>();
            var modo = DeterminarModoObjetivo(carta.Original, campoRival.Count, campoPropio.Count);

            ValidarObjetivoEntrada(modo, carta.Original, campoRival, campoPropio, objetivoCartaId, objetivoJugador);

            nombreCarta = carta.Original.Nombre;
            tipoCarta = carta.Original.Tipo;
        }

        // 2. Ejecutar el caso de uso oficial JugadorPartida.lanzarCarta
        await using (var uowLanzar = _unitOfWorkFactory.Create())
        {
            await new JugadorPartidaUseCase(uowLanzar).LanzarCartaAsync(jugadorId, cartaPartidaId);
        }

        // 3. Resolver habilidades automáticas al entrar (hechizos y criaturas)
        var resumen = new List<string>();
        await using (var uowEf = _unitOfWorkFactory.Create())
        {
            await uowEf.BeginTransactionAsync("MotorPartida.EfectosEntrada");
            try
            {
                var (partida, jugador, rival) = await CargarJugadoresActivosAsync(uowEf, partidaId, jugadorId);
                var carta = await uowEf.CartaPartidaRepository.ReadAsync(cartaPartidaId);
                if (carta is not null)
                {
                    var habilidades = ObtenerHabilidadesDeEntrada(carta.Original);
                    foreach (var hab in habilidades)
                    {
                        await EjecutarHabilidadAsync(
                            uowEf, partida, jugador, rival, carta, hab,
                            objetivoCartaId, objetivoJugador, resumen, profundidad: 0);
                    }
                }

                await uowEf.CommitAsync();
            }
            catch
            {
                await uowEf.RollbackAsync();
                throw;
            }
        }

        bool finalizada = await ComprobarYFinalizarSiMurioAlguienAsync(partidaId);
        string accionBase = tipoCarta == TipoCarta.Criatura
            ? $"Has invocado a {nombreCarta} en la mesa."
            : $"Has lanzado el instantáneo {nombreCarta}.";

        string mensaje = resumen.Count > 0
            ? $"{accionBase} {string.Join(" ", resumen)}"
            : accionBase;

        return new ResultadoAccionPartida(finalizada, mensaje);
    }

    /// <summary>
    /// Ataca con una criatura propia en la mesa hacia una criatura rival o,
    /// únicamente si la mesa rival está vacía, hacia el jugador enemigo.
    /// Solo la atacante inflige daño salvo que la defensora tenga la habilidad Defensa ("Daño al defender").
    /// </summary>
    public async Task<ResultadoAccionPartida> AtacarConCriaturaAsync(
        int partidaId,
        int jugadorId,
        int atacanteId,
        int? objetivoCartaId,
        bool objetivoJugador)
    {
        var resumen = new List<string>();

        await using (var uow = _unitOfWorkFactory.Create())
        {
            await uow.BeginTransactionAsync("MotorPartida.AtacarConCriatura");
            try
            {
                var (partida, jugador, rival) = await CargarJugadoresActivosAsync(uow, partidaId, jugadorId);
                var campoPropio = ZonaDe(jugador, TipoZona.Campo)
                    ?? throw new ModelException("No dispones de zona de mesa.");
                var campoRival = ZonaDe(rival, TipoZona.Campo)
                    ?? throw new ModelException("El rival no dispone de zona de mesa.");

                var atacante = campoPropio.Cartas.FirstOrDefault(c => c.Id == atacanteId)
                    ?? throw new ModelException("La criatura atacante no está en tu mesa.");

                if (atacante.Original.Tipo != TipoCarta.Criatura)
                {
                    throw new ModelException("Solo las criaturas pueden atacar desde la mesa.");
                }

                if (!atacante.PuedeAtacar)
                {
                    throw new ModelException($"{atacante.Original.Nombre} no puede atacar en este turno.");
                }

                if (atacante.AtaqueActual <= 0)
                {
                    throw new ModelException($"{atacante.Original.Nombre} tiene 0 de ataque y no puede atacar.");
                }

                CartaPartida? defensor = null;
                bool atacaAlJugador = false;

                if (campoRival.Cartas.Count > 0)
                {
                    if (objetivoJugador || objetivoCartaId is null)
                    {
                        throw new ModelException("No puedes atacar al jugador rival mientras aún tenga criaturas en la mesa.");
                    }

                    defensor = campoRival.Cartas.FirstOrDefault(c => c.Id == objetivoCartaId.Value)
                        ?? throw new ModelException("La carta objetivo no está en la mesa del rival.");
                }
                else
                {
                    if (objetivoCartaId is not null && !objetivoJugador)
                    {
                        throw new ModelException("No hay criaturas rivales en mesa; apunta al jugador enemigo.");
                    }

                    atacaAlJugador = true;
                }

                // Marcar que ya ha atacado este turno
                atacante.PuedeAtacar = false;
                await uow.CartaPartidaRepository.SaveAsync(atacante);

                // Registrar acción de ataque en el turno actual
                var turnoActual = partida.Turnos.OrderByDescending(t => t.NumTurno).FirstOrDefault();
                if (turnoActual is not null && turnoActual.Jugador?.Id == jugador.Id)
                {
                    var accion = new Accion
                    {
                        Name = TipoAccion.Ataque,
                        Turno = turnoActual,
                        Origen = atacante,
                    };
                    if (defensor is not null)
                    {
                        accion.Objetivo.Add(defensor);
                    }
                    await uow.AccionRepository.SaveAsync(accion);
                    turnoActual.Acciones.Add(accion);
                }

                // 1. Disparar habilidades "Al atacar" de la criatura atacante
                var habilidadesAtacar = ObtenerHabilidadesPorEstado(atacante.Original, TipoEstado.Atacar);
                foreach (var hab in habilidadesAtacar)
                {
                    await EjecutarHabilidadAsync(
                        uow, partida, jugador, rival, atacante, hab,
                        defensor?.Id, atacaAlJugador, resumen, profundidad: 0);
                }

                // 2. Resolver el ataque principal
                if (atacaAlJugador)
                {
                    var jugadorService = new JugadorPartidaService(uow);
                    await jugadorService.ActualizarVidaAsync(rival.Id, atacante.AtaqueActual);
                    resumen.Insert(0, $"{atacante.Original.Nombre} ataca al rival por {atacante.AtaqueActual} de daño.");
                }
                else if (defensor is not null)
                {
                    // Solo si el defensor sigue vivo en mesa tras las habilidades "Al atacar"
                    if (defensor.Zona?.TipoZona == TipoZona.Campo && defensor.VidaActual > 0)
                    {
                        int danyoDefensa = ObtenerDanyoAlDefender(defensor.Original);
                        string nombreDefensor = defensor.Original.Nombre;

                        bool murioDefensor = await AplicarDanyoACriaturaAsync(
                            uow, partida, rival, jugador, defensor, atacante.AtaqueActual, resumen, profundidad: 0);

                        resumen.Insert(0, murioDefensor
                            ? $"{atacante.Original.Nombre} inflige {atacante.AtaqueActual} de daño a {nombreDefensor} y la destruye."
                            : $"{atacante.Original.Nombre} inflige {atacante.AtaqueActual} de daño a {nombreDefensor} (queda con {defensor.VidaActual} de vida).");

                        // Solo devuelve daño si la defensora tiene la habilidad Defensa ("Daño al defender")
                        if (danyoDefensa > 0 && atacante.Zona?.TipoZona == TipoZona.Campo && atacante.VidaActual > 0)
                        {
                            bool murioAtacante = await AplicarDanyoACriaturaAsync(
                                uow, partida, jugador, rival, atacante, danyoDefensa, resumen, profundidad: 0);

                            resumen.Add(murioAtacante
                                ? $"{nombreDefensor} devuelve {danyoDefensa} de daño al defender y destruye a {atacante.Original.Nombre}."
                                : $"{nombreDefensor} devuelve {danyoDefensa} de daño al defender a {atacante.Original.Nombre}.");
                        }
                    }
                }

                await uow.CommitAsync();
            }
            catch
            {
                await uow.RollbackAsync();
                throw;
            }
        }

        bool finalizada = await ComprobarYFinalizarSiMurioAlguienAsync(partidaId);
        return new ResultadoAccionPartida(finalizada, string.Join(" ", resumen));
    }

    /// <summary>
    /// Finaliza el turno actual disparando habilidades de FinTurno, pasa el turno
    /// (sube +1 maná sin tope, regenera al máximo y roba 1 carta) y dispara habilidades de InicioTurno.
    /// </summary>
    public async Task<ResultadoAccionPartida> PasarTurnoConEfectosAsync(int partidaId, int jugadorId)
    {
        var resumen = new List<string>();

        // 1. Disparar efectos "Al finalizar turno" del jugador que termina
        await using (var uowFin = _unitOfWorkFactory.Create())
        {
            await uowFin.BeginTransactionAsync("MotorPartida.FinTurnoEfectos");
            try
            {
                var (partida, jugador, rival) = await CargarJugadoresActivosAsync(uowFin, partidaId, jugadorId);
                var criaturasCampo = ZonaDe(jugador, TipoZona.Campo)?.Cartas.ToList() ?? new List<CartaPartida>();

                foreach (var criatura in criaturasCampo)
                {
                    if (criatura.Zona?.TipoZona != TipoZona.Campo) continue;
                    foreach (var hab in ObtenerHabilidadesPorEstado(criatura.Original, TipoEstado.FinTurno))
                    {
                        await EjecutarHabilidadAsync(
                            uowFin, partida, jugador, rival, criatura, hab,
                            objetivoCartaId: null, objetivoJugador: false, resumen, profundidad: 0);
                    }
                }

                await uowFin.CommitAsync();
            }
            catch
            {
                await uowFin.RollbackAsync();
                throw;
            }
        }

        if (await ComprobarYFinalizarSiMurioAlguienAsync(partidaId))
        {
            return new ResultadoAccionPartida(true, resumen.Count > 0 ? string.Join(" ", resumen) : "La partida ha finalizado.");
        }

        // 2. Ejecutar el caso de uso oficial Partida.pasarTurno
        await using (var uowPasar = _unitOfWorkFactory.Create())
        {
            await new PartidaUseCase(uowPasar).PasarTurnoAsync(partidaId);
        }

        // 3. Disparar efectos "Al iniciar turno" del nuevo jugador activo
        await using (var uowInicio = _unitOfWorkFactory.Create())
        {
            await uowInicio.BeginTransactionAsync("MotorPartida.InicioTurnoEfectos");
            try
            {
                var partida = await uowInicio.PartidaRepository.ReadAsync(partidaId);
                var nuevoActivo = partida?.Jugadores.FirstOrDefault(j => j.Activo);
                var otro = partida?.Jugadores.FirstOrDefault(j => nuevoActivo != null && j.Id != nuevoActivo.Id);

                if (partida is not null && nuevoActivo is not null && otro is not null)
                {
                    var criaturasCampo = ZonaDe(nuevoActivo, TipoZona.Campo)?.Cartas.ToList() ?? new List<CartaPartida>();
                    foreach (var criatura in criaturasCampo)
                    {
                        if (criatura.Zona?.TipoZona != TipoZona.Campo) continue;
                        foreach (var hab in ObtenerHabilidadesPorEstado(criatura.Original, TipoEstado.InicioTurno))
                        {
                            await EjecutarHabilidadAsync(
                                uowInicio, partida, nuevoActivo, otro, criatura, hab,
                                objetivoCartaId: null, objetivoJugador: false, resumen, profundidad: 0);
                        }
                    }
                }

                await uowInicio.CommitAsync();
            }
            catch
            {
                await uowInicio.RollbackAsync();
                throw;
            }
        }

        bool finalizada = await ComprobarYFinalizarSiMurioAlguienAsync(partidaId);
        string mensaje = resumen.Count > 0
            ? $"Has terminado tu turno. {string.Join(" ", resumen)}"
            : "Has terminado tu turno.";

        return new ResultadoAccionPartida(finalizada, mensaje);
    }

    /// <summary>
    /// Solo finaliza automáticamente la partida cuando algún jugador llega a 0 o menos de vida,
    /// permitiendo que ambos jugadores puedan alcanzar el turno 30+ si siguen vivos.
    /// </summary>
    public async Task<bool> ComprobarYFinalizarSiMurioAlguienAsync(int partidaId)
    {
        await using var uow = _unitOfWorkFactory.Create();
        var partida = await uow.PartidaRepository.ReadAsync(partidaId);
        if (partida is null || partida.Estado != EstadoPartida.EnCurso || partida.Jugadores.Count < 2)
        {
            return partida?.Estado == EstadoPartida.Finalizada;
        }

        bool hayMuerto = partida.Jugadores.Any(j => j.Vida <= 0);
        if (!hayMuerto) return false;

        await new PartidaUseCase(uow).FinalizarAsync(partidaId);
        return true;
    }

    // =====================================================================
    // Ejecución interna de habilidades y muerte
    // =====================================================================

    private static async Task EjecutarHabilidadAsync(
        IUnitOfWork uow,
        Partida partida,
        JugadorPartida dueno,
        JugadorPartida rival,
        CartaPartida origen,
        Habilidad hab,
        int? objetivoCartaId,
        bool objetivoJugador,
        List<string> resumen,
        int profundidad)
    {
        if (profundidad > ProfundidadMaximaTriggers) return;
        if (hab.Tipo == TipoHabilidad.Defensa) return; // Pasiva al recibir daño

        var jugadorService = new JugadorPartidaService(uow);
        var campoRival = ZonaDe(rival, TipoZona.Campo);
        var campoPropio = ZonaDe(dueno, TipoZona.Campo);
        var manoRival = ZonaDe(rival, TipoZona.Mano);
        var descarteRival = ZonaDe(rival, TipoZona.Descarte);

        switch (hab.Tipo)
        {
            case TipoHabilidad.Danyo:
            case TipoHabilidad.Ataque:
                if (hab.TipoObjetivo == TipoObjetivo.TodosEnemigos)
                {
                    var objetivos = campoRival?.Cartas.ToList() ?? new List<CartaPartida>();
                    foreach (var c in objetivos)
                    {
                        if (c.Zona?.TipoZona == TipoZona.Campo && c.VidaActual > 0)
                        {
                            await AplicarDanyoACriaturaAsync(uow, partida, rival, dueno, c, hab.Cantidad, resumen, profundidad + 1);
                        }
                    }
                    if (objetivos.Count > 0)
                    {
                        resumen.Add($"Inflige {hab.Cantidad} de daño a todas las criaturas rivales.");
                    }
                }
                else if (hab.TipoObjetivo == TipoObjetivo.Jugador)
                {
                    await jugadorService.ActualizarVidaAsync(rival.Id, hab.Cantidad);
                    resumen.Add($"Inflige {hab.Cantidad} de daño al rival.");
                }
                else
                {
                    // Enemigo o Libre: respeta objetivo elegido o cae a criatura aleatoria / jugador rival
                    if (objetivoJugador)
                    {
                        await jugadorService.ActualizarVidaAsync(rival.Id, hab.Cantidad);
                        resumen.Add($"Inflige {hab.Cantidad} de daño al rival.");
                    }
                    else
                    {
                        CartaPartida? target = objetivoCartaId is int idObj
                            ? campoRival?.Cartas.FirstOrDefault(c => c.Id == idObj)
                            : null;

                        if (target is null && campoRival is not null && campoRival.Cartas.Count > 0)
                        {
                            target = campoRival.Cartas[Random.Shared.Next(campoRival.Cartas.Count)];
                        }

                        if (target is not null && target.Zona?.TipoZona == TipoZona.Campo && target.VidaActual > 0)
                        {
                            string nombreObj = target.Original.Nombre;
                            bool murio = await AplicarDanyoACriaturaAsync(uow, partida, rival, dueno, target, hab.Cantidad, resumen, profundidad + 1);
                            resumen.Add(murio
                                ? $"Inflige {hab.Cantidad} de daño a {nombreObj} y la destruye."
                                : $"Inflige {hab.Cantidad} de daño a {nombreObj}.");
                        }
                        else
                        {
                            await jugadorService.ActualizarVidaAsync(rival.Id, hab.Cantidad);
                            resumen.Add($"Inflige {hab.Cantidad} de daño al rival.");
                        }
                    }
                }
                break;

            case TipoHabilidad.Cura:
                if (hab.TipoObjetivo == TipoObjetivo.Self && origen.Original.Tipo == TipoCarta.Criatura && origen.Zona?.TipoZona == TipoZona.Campo)
                {
                    origen.VidaActual += hab.Cantidad;
                    await uow.CartaPartidaRepository.SaveAsync(origen);
                    resumen.Add($"{origen.Original.Nombre} recupera {hab.Cantidad} de vida.");
                }
                else
                {
                    await jugadorService.ActualizarVidaAsync(dueno.Id, -hab.Cantidad);
                    resumen.Add($"Suma {hab.Cantidad} de vida al jugador.");
                }
                break;

            case TipoHabilidad.Robar:
                var mazoDueno = ZonaDe(dueno, TipoZona.Mazo);
                int robadas = 0;
                for (int i = 0; i < hab.Cantidad; i++)
                {
                    if (mazoDueno is null || mazoDueno.Cartas.Count == 0) break;
                    await jugadorService.RobarCartaDelMazoAsync(dueno.Id);
                    robadas++;
                }
                if (robadas > 0)
                {
                    resumen.Add($"Roba {robadas} carta(s).");
                }
                break;

            case TipoHabilidad.Descartar:
                if (manoRival is not null && descarteRival is not null && manoRival.Cartas.Count > 0)
                {
                    int descartadas = 0;
                    for (int i = 0; i < hab.Cantidad && manoRival.Cartas.Count > 0; i++)
                    {
                        var elegida = manoRival.Cartas[Random.Shared.Next(manoRival.Cartas.Count)];
                        manoRival.Cartas.Remove(elegida);
                        elegida.Zona = descarteRival;
                        descarteRival.Cartas.Add(elegida);
                        await uow.CartaPartidaRepository.SaveAsync(elegida);
                        descartadas++;
                    }
                    await uow.ZonaPartidaRepository.SaveAsync(manoRival);
                    await uow.ZonaPartidaRepository.SaveAsync(descarteRival);
                    if (descartadas > 0)
                    {
                        resumen.Add($"El rival descarta {descartadas} carta(s) de su mano.");
                    }
                }
                break;

            case TipoHabilidad.Destruir:
                bool destruirPropia = hab.TipoObjetivo == TipoObjetivo.Jugador;
                var zonaObjetivo = destruirPropia ? campoPropio : campoRival;
                var duenoObjetivo = destruirPropia ? dueno : rival;
                var rivalObjetivo = destruirPropia ? rival : dueno;

                if (zonaObjetivo is not null && zonaObjetivo.Cartas.Count > 0)
                {
                    CartaPartida? victima = objetivoCartaId is int idVictima
                        ? zonaObjetivo.Cartas.FirstOrDefault(c => c.Id == idVictima)
                        : null;
                    victima ??= zonaObjetivo.Cartas[Random.Shared.Next(zonaObjetivo.Cartas.Count)];

                    string nombreVictima = victima.Original.Nombre;
                    await DestruirCriaturaAsync(uow, partida, duenoObjetivo, rivalObjetivo, victima, resumen, profundidad + 1);
                    resumen.Add($"Destruye a {nombreVictima}.");
                }
                break;
        }
    }

    private static async Task<bool> AplicarDanyoACriaturaAsync(
        IUnitOfWork uow,
        Partida partida,
        JugadorPartida duenoCriatura,
        JugadorPartida oponenteCriatura,
        CartaPartida criatura,
        int danyo,
        List<string> resumen,
        int profundidad)
    {
        if (danyo <= 0) return false;

        var cartaService = new CartaPartidaService(uow);
        await cartaService.RecibirDanyoAsync(criatura.Id, danyo);

        if (criatura.VidaActual <= 0)
        {
            await DispararAlMorirAsync(uow, partida, duenoCriatura, oponenteCriatura, criatura, resumen, profundidad + 1);
            return true;
        }

        return false;
    }

    private static async Task DestruirCriaturaAsync(
        IUnitOfWork uow,
        Partida partida,
        JugadorPartida duenoCriatura,
        JugadorPartida oponenteCriatura,
        CartaPartida criatura,
        List<string> resumen,
        int profundidad)
    {
        criatura.VidaActual = 0;
        criatura.PuedeAtacar = false;

        var zonaActual = criatura.Zona;
        var zonaDescarte = ZonaDe(duenoCriatura, TipoZona.Descarte);

        if (zonaActual is not null && zonaDescarte is not null && zonaActual.Id != zonaDescarte.Id)
        {
            zonaActual.Cartas.Remove(criatura);
            criatura.Zona = zonaDescarte;
            if (!zonaDescarte.Cartas.Contains(criatura))
            {
                zonaDescarte.Cartas.Add(criatura);
            }
            await uow.ZonaPartidaRepository.SaveAsync(zonaActual);
            await uow.ZonaPartidaRepository.SaveAsync(zonaDescarte);
        }

        await uow.CartaPartidaRepository.SaveAsync(criatura);
        await DispararAlMorirAsync(uow, partida, duenoCriatura, oponenteCriatura, criatura, resumen, profundidad + 1);
    }

    private static async Task DispararAlMorirAsync(
        IUnitOfWork uow,
        Partida partida,
        JugadorPartida duenoCriatura,
        JugadorPartida oponenteCriatura,
        CartaPartida criatura,
        List<string> resumen,
        int profundidad)
    {
        if (profundidad > ProfundidadMaximaTriggers) return;

        foreach (var hab in ObtenerHabilidadesPorEstado(criatura.Original, TipoEstado.Morir))
        {
            await EjecutarHabilidadAsync(
                uow, partida, duenoCriatura, oponenteCriatura, criatura, hab,
                objetivoCartaId: null, objetivoJugador: false, resumen, profundidad + 1);
        }
    }

    // =====================================================================
    // Helpers
    // =====================================================================

    private static void ValidarObjetivoEntrada(
        ModoObjetivoCarta modo,
        Carta carta,
        IList<CartaPartida> campoRival,
        IList<CartaPartida> campoPropio,
        int? objetivoCartaId,
        bool objetivoJugador)
    {
        switch (modo)
        {
            case ModoObjetivoCarta.EnemigoOCriatura:
                if (!objetivoJugador && objetivoCartaId is null)
                {
                    throw new ModelException($"Debes elegir un objetivo para lanzar {carta.Nombre}.");
                }
                if (!objetivoJugador && objetivoCartaId is int idEnemiga && !campoRival.Any(c => c.Id == idEnemiga))
                {
                    throw new ModelException("La criatura objetivo ya no está en la mesa rival.");
                }
                break;

            case ModoObjetivoCarta.SoloCriaturaEnemiga:
                if (objetivoCartaId is not int idObj || !campoRival.Any(c => c.Id == idObj))
                {
                    throw new ModelException($"Debes elegir una criatura rival en mesa para {carta.Nombre}.");
                }
                break;

            case ModoObjetivoCarta.SoloCriaturaPropia:
                if (objetivoCartaId is not int idProp || !campoPropio.Any(c => c.Id == idProp))
                {
                    throw new ModelException($"Debes elegir una de tus criaturas en mesa para {carta.Nombre}.");
                }
                break;
        }
    }

    private static async Task<(Partida Partida, JugadorPartida Jugador, JugadorPartida Rival)> CargarJugadoresActivosAsync(
        IUnitOfWork uow, int partidaId, int jugadorId)
    {
        var partida = await uow.PartidaRepository.ReadAsync(partidaId)
            ?? throw new ModelException("No existe la partida indicada.");

        if (partida.Estado != EstadoPartida.EnCurso)
        {
            throw new ModelException("La partida no está en curso.");
        }

        var jugador = partida.Jugadores.FirstOrDefault(j => j.Id == jugadorId)
            ?? throw new ModelException("No perteneces a esta partida.");

        if (!jugador.Activo)
        {
            throw new ModelException("No es tu turno.");
        }

        var rival = partida.Jugadores.FirstOrDefault(j => j.Id != jugadorId)
            ?? throw new ModelException("Falta el rival en la partida.");

        return (partida, jugador, rival);
    }

    private static List<Habilidad> ObtenerHabilidadesDeEntrada(Carta carta)
    {
        var estados = carta.Estados ?? new List<Estado>();
        if (carta.Tipo == TipoCarta.Hechizo)
        {
            return estados
                .SelectMany(e => e.Habilidades ?? new List<Habilidad>())
                .Where(h => h.Tipo != TipoHabilidad.Defensa)
                .ToList();
        }

        return estados
            .Where(e => e.Tipo == TipoEstado.Entrar)
            .SelectMany(e => e.Habilidades ?? new List<Habilidad>())
            .Where(h => h.Tipo != TipoHabilidad.Defensa)
            .ToList();
    }

    private static List<Habilidad> ObtenerHabilidadesPorEstado(Carta carta, TipoEstado tipoEstado) =>
        (carta.Estados ?? new List<Estado>())
            .Where(e => e.Tipo == tipoEstado)
            .SelectMany(e => e.Habilidades ?? new List<Habilidad>())
            .Where(h => h.Tipo != TipoHabilidad.Defensa)
            .ToList();

    private static int ObtenerDanyoAlDefender(Carta carta) =>
        (carta.Estados ?? new List<Estado>())
            .SelectMany(e => e.Habilidades ?? new List<Habilidad>())
            .Where(h => h.Tipo == TipoHabilidad.Defensa)
            .Sum(h => Math.Max(0, h.Cantidad));

    private static ZonaPartida? ZonaDe(JugadorPartida jugador, TipoZona tipo) =>
        jugador.Zonas?.FirstOrDefault(z => z.TipoZona == tipo);
}
