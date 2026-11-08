using NHibernate.Linq;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.UseCases.Habilidad;
using CraftCaster.Application.UseCases.JugadorPartida;
using CraftCaster.Application.UseCases.Partida;
using CraftCaster.Application.UseCases.Usuario;
using CraftCaster.Application.UseCases.ZonaPartida;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Tests.Manual;

/// <summary>
/// Pruebas de las operaciones CustomTransaction (UseCases/manual).
/// Se comprueba el camino exitoso, la reversion (rollback) tras un fallo y
/// que los cambios se persisten, releyendo siempre con una sesion nueva.
/// </summary>
public sealed class CustomTransactionTests
{
    private const int IdInexistente = 9999;

    // ==================================================================
    // Usuario.registrarUsuario
    // ==================================================================

    [Fact]
    public async Task RegistrarUsuario_DatosValidos_CreaClienteActivo()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var useCase = new UsuarioUseCase(db.UnitOfWork);

        // Act
        Usuario usuario = await useCase.RegistrarUsuarioAsync("ana", "ana@test.com", "secreto123", 1);

        // Assert
        var guardado = db.Verify(s =>
        {
            Usuario u = s.Get<Usuario>(usuario.Id);
            return (u.NombreUsuario, u.TipoUsuario, u.EstadoCuenta, u.Contrasenia);
        });
        Assert.Equal("ana", guardado.NombreUsuario);
        Assert.Equal(TipoUsuario.Cliente, guardado.TipoUsuario);
        Assert.Equal(EstadoCuenta.Activa, guardado.EstadoCuenta);
        Assert.NotEqual("secreto123", guardado.Contrasenia);
    }

    [Fact]
    public async Task RegistrarUsuario_EmailDuplicado_LanzaModelExceptionYNoCreaUsuario()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        await db.CrearUsuarioAsync("ana"); // email ana@test.com
        var useCase = new UsuarioUseCase(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() =>
            useCase.RegistrarUsuarioAsync("otra", "ANA@test.com", "secreto123", 1));
        Assert.Equal(1, db.Contar<Usuario>());
    }

    [Fact]
    public async Task RegistrarUsuario_NombreDuplicado_LanzaModelExceptionYNoCreaUsuario()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        await db.CrearUsuarioAsync("ana");
        var useCase = new UsuarioUseCase(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() =>
            useCase.RegistrarUsuarioAsync("Ana", "otro@test.com", "secreto123", 1));
        Assert.Equal(1, db.Contar<Usuario>());
    }

    [Theory]
    [InlineData("", "ana@test.com", "secreto")]
    [InlineData("ana", "  ", "secreto")]
    [InlineData("ana", "ana@test.com", "")]
    public async Task RegistrarUsuario_CampoVacio_LanzaModelException(string nombre, string email, string contrasenia)
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var useCase = new UsuarioUseCase(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => useCase.RegistrarUsuarioAsync(nombre, email, contrasenia, 1));
        Assert.Equal(0, db.Contar<Usuario>());
    }

    // ==================================================================
    // Usuario.iniciarPartida
    // ==================================================================

    [Fact]
    public async Task IniciarPartida_UsuarioConMazoJugable_CreaPartidaEnEsperaConJugadorZonasYCartas()
    {
        // Arrange: mazo con 2 criaturas + 1 hechizo
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario usuario = await db.CrearUsuarioAsync();
        Carta criatura = await db.CrearCartaAsync(usuario, "Golem");
        Carta hechizo = await db.CrearCartaAsync(usuario, "Rayo", TipoCarta.Hechizo);
        Mazo mazo = await db.CrearMazoAsync(usuario, estado: EstadoMazo.Jugable);
        await db.AnyadirLineaAsync(mazo, criatura, 2);
        await db.AnyadirLineaAsync(mazo, hechizo, 1);
        var useCase = new UsuarioUseCase(db.UnitOfWork);

        // Act
        await useCase.IniciarPartidaAsync(usuario.Id);

        // Assert
        Partida creada = Assert.Single(usuario.PartidasLocal);
        var guardada = db.Verify(s =>
        {
            Partida p = s.Get<Partida>(creada.Id);
            JugadorPartida j = p.Jugadores.Single();
            return (p.Estado, MazoId: p.Mazo1.Id, UsuarioId: p.Usuario1.Id, TieneVisitante: p.Usuario2 != null,
                    j.NumJugador, j.Vida, j.Activo, Zonas: j.Zonas.Count, JugadorId: j.Id);
        });
        Assert.Equal(EstadoPartida.Espera, guardada.Estado);
        Assert.Equal(mazo.Id, guardada.MazoId);
        Assert.Equal(usuario.Id, guardada.UsuarioId);
        Assert.False(guardada.TieneVisitante);
        Assert.Equal(1, guardada.NumJugador);
        Assert.Equal(30, guardada.Vida);
        Assert.False(guardada.Activo);
        Assert.Equal(4, guardada.Zonas);
        Assert.Equal(3, db.ContarCartas(guardada.JugadorId, TipoZona.Mazo));
        Assert.Equal(0, db.ContarCartas(guardada.JugadorId, TipoZona.Mano));
    }

    [Fact]
    public async Task IniciarPartida_UsuarioSinMazos_LanzaModelExceptionYNoCreaPartida()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario usuario = await db.CrearUsuarioAsync();
        var useCase = new UsuarioUseCase(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => useCase.IniciarPartidaAsync(usuario.Id));
        Assert.Equal(0, db.Contar<Partida>());
        Assert.Equal(0, db.Contar<JugadorPartida>());
    }

    [Fact]
    public async Task IniciarPartida_UsuarioInexistente_LanzaModelException()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var useCase = new UsuarioUseCase(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => useCase.IniciarPartidaAsync(IdInexistente));
    }

    // ==================================================================
    // Usuario.unirsePartida
    // ==================================================================

    private static async Task<(Usuario Local, Usuario Visitante, Mazo MazoVisitante, Partida Partida)>
        CrearPartidaEnEsperaYVisitanteAsync(CraftCasterTestDb db)
    {
        Usuario local = await db.CrearUsuarioAsync("local");
        Carta carta = await db.CrearCartaAsync(local, "Golem");
        Mazo mazoLocal = await db.CrearMazoAsync(local);
        await db.AnyadirLineaAsync(mazoLocal, carta, 3);
        await new UsuarioUseCase(db.UnitOfWork).IniciarPartidaAsync(local.Id);

        Usuario visitante = await db.CrearUsuarioAsync("visitante");
        Mazo mazoVisitante = await db.CrearMazoAsync(visitante);
        await db.AnyadirLineaAsync(mazoVisitante, carta, 2);

        return (local, visitante, mazoVisitante, local.PartidasLocal.Single());
    }

    [Fact]
    public async Task UnirsePartida_PartidaEnEspera_UneAlVisitanteYPoneLaPartidaEnCurso()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var (_, visitante, mazoVisitante, partida) = await CrearPartidaEnEsperaYVisitanteAsync(db);
        var useCase = new UsuarioUseCase(db.UnitOfWork);

        // Act
        await useCase.UnirsePartidaAsync(visitante.Id, partida.Id);

        // Assert
        var guardada = db.Verify(s =>
        {
            Partida p = s.Get<Partida>(partida.Id);
            JugadorPartida j2 = p.Jugadores.Single(j => j.NumJugador == 2);
            return (p.Estado, Usuario2Id: p.Usuario2!.Id, Mazo2Id: p.Mazo2!.Id, Jugadores: p.Jugadores.Count,
                    Jugador2Id: j2.Id, Zonas: j2.Zonas.Count);
        });
        Assert.Equal(EstadoPartida.EnCurso, guardada.Estado);
        Assert.Equal(visitante.Id, guardada.Usuario2Id);
        Assert.Equal(mazoVisitante.Id, guardada.Mazo2Id);
        Assert.Equal(2, guardada.Jugadores);
        Assert.Equal(4, guardada.Zonas);
        Assert.Equal(2, db.ContarCartas(guardada.Jugador2Id, TipoZona.Mazo));
    }

    [Fact]
    public async Task UnirsePartida_PartidaPropia_LanzaModelExceptionYNoModificaLaPartida()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var (local, _, _, partida) = await CrearPartidaEnEsperaYVisitanteAsync(db);
        var useCase = new UsuarioUseCase(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => useCase.UnirsePartidaAsync(local.Id, partida.Id));
        var guardada = db.Verify(s =>
        {
            Partida p = s.Get<Partida>(partida.Id);
            return (p.Estado, TieneVisitante: p.Usuario2 != null, Jugadores: p.Jugadores.Count);
        });
        Assert.Equal(EstadoPartida.Espera, guardada.Estado);
        Assert.False(guardada.TieneVisitante);
        Assert.Equal(1, guardada.Jugadores);
    }

    [Fact]
    public async Task UnirsePartida_PartidaYaEnCurso_LanzaModelException()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var (_, visitante, _, partida) = await CrearPartidaEnEsperaYVisitanteAsync(db);
        var useCase = new UsuarioUseCase(db.UnitOfWork);
        await useCase.UnirsePartidaAsync(visitante.Id, partida.Id);
        Usuario tercero = await db.CrearUsuarioAsync("tercero");
        await db.CrearMazoAsync(tercero);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => useCase.UnirsePartidaAsync(tercero.Id, partida.Id));
        Assert.Equal(visitante.Id, db.Verify(s => s.Get<Partida>(partida.Id).Usuario2!.Id));
    }

    // ==================================================================
    // Usuario.abandonarPartida
    // ==================================================================

    [Fact]
    public async Task AbandonarPartida_LocalConPartidaEnEspera_BorraLaPartida()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var (local, _, _, partida) = await CrearPartidaEnEsperaYVisitanteAsync(db);
        var useCase = new UsuarioUseCase(db.UnitOfWork);

        // Act
        await useCase.AbandonarPartidaAsync(local.Id);

        // Assert
        Assert.Null(db.Verify(s => s.Get<Partida>(partida.Id)));
    }

    [Fact]
    public async Task AbandonarPartida_VisitanteConPartidaEnCurso_FinalizaYDaLaVictoriaAlLocal()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var (local, visitante, _, partida) = await CrearPartidaEnEsperaYVisitanteAsync(db);
        var useCase = new UsuarioUseCase(db.UnitOfWork);
        await useCase.UnirsePartidaAsync(visitante.Id, partida.Id);

        // Act
        await useCase.AbandonarPartidaAsync(visitante.Id);

        // Assert
        var guardada = db.Verify(s =>
        {
            Partida p = s.Get<Partida>(partida.Id);
            return (p.Estado, p.Ganador, TieneVisitante: p.Usuario2 != null);
        });
        Assert.Equal(EstadoPartida.Finalizada, guardada.Estado);
        Assert.Equal(local.Id, guardada.Ganador);
        Assert.False(guardada.TieneVisitante);
    }

    [Fact]
    public async Task AbandonarPartida_UsuarioSinPartidaActiva_LanzaModelException()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario usuario = await db.CrearUsuarioAsync();
        var useCase = new UsuarioUseCase(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => useCase.AbandonarPartidaAsync(usuario.Id));
    }

    // ==================================================================
    // Partida.finalizar
    // ==================================================================

    [Fact]
    public async Task Finalizar_Jugador2SinVida_FinalizaYGanaElLocal()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync();
        juego.Jugador2.Vida = 0;
        await db.UnitOfWork.JugadorPartidaRepository.SaveAsync(juego.Jugador2);
        var useCase = new PartidaUseCase(db.UnitOfWork);

        // Act
        await useCase.FinalizarAsync(juego.Partida.Id);

        // Assert
        var guardada = db.Verify(s =>
        {
            Partida p = s.Get<Partida>(juego.Partida.Id);
            return (p.Estado, p.Ganador);
        });
        Assert.Equal(EstadoPartida.Finalizada, guardada.Estado);
        Assert.Equal(juego.Local.Id, guardada.Ganador);
    }

    [Fact]
    public async Task Finalizar_Jugador1SinCartasEnElMazo_FinalizaYGanaElVisitante()
    {
        // Arrange: el jugador 1 se queda sin mazo, el jugador 2 conserva una carta
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync(cartasEnMazo: 0);
        await db.PonerCartaAsync(juego.Jugador2, TipoZona.Mazo, juego.Criatura);
        var useCase = new PartidaUseCase(db.UnitOfWork);

        // Act
        await useCase.FinalizarAsync(juego.Partida.Id);

        // Assert
        Assert.Equal(juego.Visitante.Id, db.Verify(s => s.Get<Partida>(juego.Partida.Id).Ganador));
    }

    [Fact]
    public async Task Finalizar_PartidaYaFinalizada_LanzaModelExceptionYConservaElGanador()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync();
        juego.Partida.Estado = EstadoPartida.Finalizada;
        juego.Partida.Ganador = juego.Visitante.Id;
        await db.UnitOfWork.PartidaRepository.SaveAsync(juego.Partida);
        var useCase = new PartidaUseCase(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => useCase.FinalizarAsync(juego.Partida.Id));
        Assert.Equal(juego.Visitante.Id, db.Verify(s => s.Get<Partida>(juego.Partida.Id).Ganador));
    }

    // ==================================================================
    // Partida.pasarTurno
    // ==================================================================

    [Fact]
    public async Task PasarTurno_PrimerTurno_ActivaAlJugador1ConUnoDeManaYRobaUnaCarta()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync(cartasEnMazo: 3);
        var useCase = new PartidaUseCase(db.UnitOfWork);

        // Act
        await useCase.PasarTurnoAsync(juego.Partida.Id);

        // Assert
        var j1 = db.Verify(s => s.Get<JugadorPartida>(juego.Jugador1.Id));
        var turnos = db.Verify(s => s.Query<Turno>()
            .Where(t => t.Partida.Id == juego.Partida.Id)
            .Select(t => new { t.NumTurno, t.ManaTurno, JugadorId = t.Jugador.Id })
            .ToList());
        Assert.True(j1.Activo);
        Assert.Equal(1, j1.Mana);
        Assert.Equal(1, db.ContarCartas(juego.Jugador1.Id, TipoZona.Mano));
        Assert.Equal(2, db.ContarCartas(juego.Jugador1.Id, TipoZona.Mazo));
        var turno = Assert.Single(turnos);
        Assert.Equal(1, turno.NumTurno);
        Assert.Equal(1, turno.ManaTurno);
        Assert.Equal(juego.Jugador1.Id, turno.JugadorId);
    }

    [Fact]
    public async Task PasarTurno_TresVeces_AlternaJugadoresYAumentaElMana()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync(cartasEnMazo: 5);
        var useCase = new PartidaUseCase(db.UnitOfWork);

        // Act
        await useCase.PasarTurnoAsync(juego.Partida.Id); // turno 1: jugador 1
        await useCase.PasarTurnoAsync(juego.Partida.Id); // turno 2: jugador 2
        await useCase.PasarTurnoAsync(juego.Partida.Id); // turno 3: jugador 1

        // Assert
        var j1 = db.Verify(s => s.Get<JugadorPartida>(juego.Jugador1.Id));
        var j2 = db.Verify(s => s.Get<JugadorPartida>(juego.Jugador2.Id));
        var ordenTurnos = db.Verify(s => s.Query<Turno>()
            .Where(t => t.Partida.Id == juego.Partida.Id)
            .OrderBy(t => t.NumTurno)
            .Select(t => t.Jugador.Id)
            .ToList());

        Assert.Equal(new[] { juego.Jugador1.Id, juego.Jugador2.Id, juego.Jugador1.Id }, ordenTurnos);
        Assert.True(j1.Activo);
        Assert.False(j2.Activo);
        Assert.Equal(2, j1.Mana);
        Assert.Equal(1, j2.Mana);
        Assert.Equal(2, db.ContarCartas(juego.Jugador1.Id, TipoZona.Mano));
        Assert.Equal(1, db.ContarCartas(juego.Jugador2.Id, TipoZona.Mano));
    }

    [Fact]
    public async Task PasarTurno_CriaturaEnCampo_SoloPuedeAtacarEnElTurnoDeSuDuenyo()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync(cartasEnMazo: 5);
        CartaPartida criatura = await db.PonerCartaAsync(juego.Jugador1, TipoZona.Campo, juego.Criatura);
        var useCase = new PartidaUseCase(db.UnitOfWork);

        // Act + Assert
        await useCase.PasarTurnoAsync(juego.Partida.Id); // empieza el jugador 1
        Assert.True(db.Verify(s => s.Get<CartaPartida>(criatura.Id).PuedeAtacar));

        await useCase.PasarTurnoAsync(juego.Partida.Id); // turno del jugador 2
        Assert.False(db.Verify(s => s.Get<CartaPartida>(criatura.Id).PuedeAtacar));
    }

    [Fact]
    public async Task PasarTurno_PartidaEnEspera_LanzaModelExceptionYNoCreaTurnos()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync();
        juego.Partida.Estado = EstadoPartida.Espera;
        await db.UnitOfWork.PartidaRepository.SaveAsync(juego.Partida);
        var useCase = new PartidaUseCase(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => useCase.PasarTurnoAsync(juego.Partida.Id));
        Assert.Equal(0, db.Contar<Turno>());
        Assert.False(db.Verify(s => s.Get<JugadorPartida>(juego.Jugador1.Id).Activo));
    }

    [Fact]
    public async Task PasarTurno_PartidaInexistente_LanzaModelException()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var useCase = new PartidaUseCase(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => useCase.PasarTurnoAsync(IdInexistente));
    }

    // ==================================================================
    // JugadorPartida.lanzarCarta
    // ==================================================================

    [Fact]
    public async Task LanzarCarta_CriaturaConManaSuficiente_PasaAlCampoConsumeManaYRegistraLaAccion()
    {
        // Arrange: jugador 1 activo con 3 de mana y un Golem (coste 2) en la mano
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync(jugador1Activo: true, manaJugador1: 3);
        await db.CrearTurnoAsync(juego.Partida, juego.Jugador1, 1, 3);
        CartaPartida carta = await db.PonerCartaAsync(juego.Jugador1, TipoZona.Mano, juego.Criatura);
        var useCase = new JugadorPartidaUseCase(db.UnitOfWork);

        // Act
        await useCase.LanzarCartaAsync(juego.Jugador1.Id, carta.Id);

        // Assert
        var guardada = db.Verify(s =>
        {
            CartaPartida c = s.Get<CartaPartida>(carta.Id);
            return (c.Zona.TipoZona, c.PuedeAtacar, c.VidaActual, c.AtaqueActual);
        });
        int acciones = db.Verify(s => s.Query<Accion>()
            .Count(a => a.Origen!.Id == carta.Id && a.Name == TipoAccion.Lanzar));

        Assert.Equal(TipoZona.Campo, guardada.TipoZona);
        Assert.False(guardada.PuedeAtacar);
        Assert.Equal(3, guardada.VidaActual);
        Assert.Equal(2, guardada.AtaqueActual);
        Assert.Equal(1, db.Verify(s => s.Get<JugadorPartida>(juego.Jugador1.Id).Mana));
        Assert.Equal(1, acciones);
    }

    [Fact]
    public async Task LanzarCarta_Hechizo_VaAlDescarte()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync(jugador1Activo: true, manaJugador1: 1);
        CartaPartida carta = await db.PonerCartaAsync(juego.Jugador1, TipoZona.Mano, juego.Hechizo);
        var useCase = new JugadorPartidaUseCase(db.UnitOfWork);

        // Act
        await useCase.LanzarCartaAsync(juego.Jugador1.Id, carta.Id);

        // Assert
        Assert.Equal(TipoZona.Descarte, db.ZonaDe(carta.Id));
        Assert.Equal(0, db.Verify(s => s.Get<JugadorPartida>(juego.Jugador1.Id).Mana));
    }

    [Fact]
    public async Task LanzarCarta_ManaInsuficiente_LanzaModelExceptionYNoModificaNada()
    {
        // Arrange: 1 de mana para un Golem de coste 2
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync(jugador1Activo: true, manaJugador1: 1);
        await db.CrearTurnoAsync(juego.Partida, juego.Jugador1, 1, 1);
        CartaPartida carta = await db.PonerCartaAsync(juego.Jugador1, TipoZona.Mano, juego.Criatura);
        var useCase = new JugadorPartidaUseCase(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => useCase.LanzarCartaAsync(juego.Jugador1.Id, carta.Id));
        Assert.Equal(TipoZona.Mano, db.ZonaDe(carta.Id));
        Assert.Equal(1, db.Verify(s => s.Get<JugadorPartida>(juego.Jugador1.Id).Mana));
        Assert.Equal(0, db.Contar<Accion>());
    }

    [Fact]
    public async Task LanzarCarta_JugadorNoActivo_LanzaModelException()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync(jugador1Activo: false, manaJugador1: 5);
        CartaPartida carta = await db.PonerCartaAsync(juego.Jugador1, TipoZona.Mano, juego.Criatura);
        var useCase = new JugadorPartidaUseCase(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => useCase.LanzarCartaAsync(juego.Jugador1.Id, carta.Id));
        Assert.Equal(TipoZona.Mano, db.ZonaDe(carta.Id));
    }

    [Fact]
    public async Task LanzarCarta_CartaQueNoEstaEnLaMano_LanzaModelException()
    {
        // Arrange: la carta esta en el mazo, no en la mano
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync(jugador1Activo: true, manaJugador1: 5, cartasEnMazo: 1);
        CartaPartida cartaDelMazo = CraftCasterTestDb.Zona(juego.Jugador1, TipoZona.Mazo).Cartas.Single();
        var useCase = new JugadorPartidaUseCase(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => useCase.LanzarCartaAsync(juego.Jugador1.Id, cartaDelMazo.Id));
        Assert.Equal(TipoZona.Mazo, db.ZonaDe(cartaDelMazo.Id));
        Assert.Equal(5, db.Verify(s => s.Get<JugadorPartida>(juego.Jugador1.Id).Mana));
    }

    [Fact]
    public async Task LanzarCarta_CartaDelRival_LanzaModelException()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync(jugador1Activo: true, manaJugador1: 5);
        CartaPartida cartaRival = await db.PonerCartaAsync(juego.Jugador2, TipoZona.Mano, juego.Criatura);
        var useCase = new JugadorPartidaUseCase(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => useCase.LanzarCartaAsync(juego.Jugador1.Id, cartaRival.Id));
        Assert.Equal(0, db.ContarCartas(juego.Jugador1.Id, TipoZona.Campo));
    }

    // ==================================================================
    // JugadorPartida.robarCarta
    // ==================================================================

    [Fact]
    public async Task RobarCarta_MazoConCartas_PasaLaPrimeraCartaALaMano()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync(cartasEnMazo: 2);
        CartaPartida primera = CraftCasterTestDb.Zona(juego.Jugador1, TipoZona.Mazo).Cartas[0];
        var useCase = new JugadorPartidaUseCase(db.UnitOfWork);

        // Act
        await useCase.RobarCartaAsync(juego.Jugador1.Id);

        // Assert
        Assert.Equal(TipoZona.Mano, db.ZonaDe(primera.Id));
        Assert.Equal(1, db.ContarCartas(juego.Jugador1.Id, TipoZona.Mano));
        Assert.Equal(1, db.ContarCartas(juego.Jugador1.Id, TipoZona.Mazo));
    }

    [Fact]
    public async Task RobarCarta_ManoLlena_EnviaLaCartaAlDescarte()
    {
        // Arrange: 10 cartas en la mano (maximo)
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync(cartasEnMazo: 1);
        for (int i = 0; i < 10; i++)
        {
            await db.PonerCartaAsync(juego.Jugador1, TipoZona.Mano, juego.Criatura);
        }
        var useCase = new JugadorPartidaUseCase(db.UnitOfWork);

        // Act
        await useCase.RobarCartaAsync(juego.Jugador1.Id);

        // Assert
        Assert.Equal(10, db.ContarCartas(juego.Jugador1.Id, TipoZona.Mano));
        Assert.Equal(1, db.ContarCartas(juego.Jugador1.Id, TipoZona.Descarte));
        Assert.Equal(0, db.ContarCartas(juego.Jugador1.Id, TipoZona.Mazo));
    }

    [Fact]
    public async Task RobarCarta_MazoVacio_LanzaModelException()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync(cartasEnMazo: 0);
        var useCase = new JugadorPartidaUseCase(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => useCase.RobarCartaAsync(juego.Jugador1.Id));
        Assert.Equal(0, db.ContarCartas(juego.Jugador1.Id, TipoZona.Mano));
    }

    // ==================================================================
    // JugadorPartida.inicioTurno / finalTurno
    // ==================================================================

    [Fact]
    public async Task InicioTurno_JugadorInactivo_LoActivaRegeneraManaHabilitaCriaturasYRoba()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync(jugador1Activo: false, manaJugador1: 0, cartasEnMazo: 1);
        CartaPartida criatura = await db.PonerCartaAsync(juego.Jugador1, TipoZona.Campo, juego.Criatura, puedeAtacar: false);
        var useCase = new JugadorPartidaUseCase(db.UnitOfWork);

        // Act
        await useCase.InicioTurnoAsync(juego.Jugador1.Id);

        // Assert
        var j1 = db.Verify(s => s.Get<JugadorPartida>(juego.Jugador1.Id));
        Assert.True(j1.Activo);
        Assert.Equal(1, j1.Mana);
        Assert.True(db.Verify(s => s.Get<CartaPartida>(criatura.Id).PuedeAtacar));
        Assert.Equal(1, db.ContarCartas(juego.Jugador1.Id, TipoZona.Mano));
        Assert.Equal(0, db.ContarCartas(juego.Jugador1.Id, TipoZona.Mazo));
    }

    [Fact]
    public async Task InicioTurno_MazoVacio_LanzaModelExceptionYNoActivaAlJugador()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync(jugador1Activo: false, cartasEnMazo: 0);
        CartaPartida criatura = await db.PonerCartaAsync(juego.Jugador1, TipoZona.Campo, juego.Criatura, puedeAtacar: false);
        var useCase = new JugadorPartidaUseCase(db.UnitOfWork);

        // Act + Assert: el rollback debe deshacer tambien los cambios previos al robo
        await Assert.ThrowsAsync<ModelException>(() => useCase.InicioTurnoAsync(juego.Jugador1.Id));
        Assert.False(db.Verify(s => s.Get<JugadorPartida>(juego.Jugador1.Id).Activo));
        Assert.False(db.Verify(s => s.Get<CartaPartida>(criatura.Id).PuedeAtacar));
    }

    [Fact]
    public async Task FinalTurno_JugadorActivo_LoDesactivaYBloqueaElAtaqueDeSusCriaturas()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync(jugador1Activo: true);
        CartaPartida criatura = await db.PonerCartaAsync(juego.Jugador1, TipoZona.Campo, juego.Criatura, puedeAtacar: true);
        var useCase = new JugadorPartidaUseCase(db.UnitOfWork);

        // Act
        await useCase.FinalTurnoAsync(juego.Jugador1.Id);

        // Assert
        Assert.False(db.Verify(s => s.Get<JugadorPartida>(juego.Jugador1.Id).Activo));
        Assert.False(db.Verify(s => s.Get<CartaPartida>(criatura.Id).PuedeAtacar));
    }

    [Fact]
    public async Task FinalTurno_JugadorInexistente_LanzaModelException()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var useCase = new JugadorPartidaUseCase(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => useCase.FinalTurnoAsync(IdInexistente));
    }

    // ==================================================================
    // ZonaPartida.operationTransaction
    // ==================================================================

    [Fact]
    public async Task OperationTransaction_IndiceValido_MueveLaCartaIndicadaAlDestino()
    {
        // Arrange: dos cartas en la mano, se mueve la segunda (indice 1)
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync(cartasEnMazo: 0);
        CartaPartida primera = await db.PonerCartaAsync(juego.Jugador1, TipoZona.Mano, juego.Criatura);
        CartaPartida segunda = await db.PonerCartaAsync(juego.Jugador1, TipoZona.Mano, juego.Criatura);
        ZonaPartida mano = CraftCasterTestDb.Zona(juego.Jugador1, TipoZona.Mano);
        ZonaPartida campo = CraftCasterTestDb.Zona(juego.Jugador1, TipoZona.Campo);
        var useCase = new ZonaPartidaUseCase(db.UnitOfWork);

        // Act
        await useCase.OperationTransactionAsync(mano.Id, campo.Id, 1);

        // Assert
        Assert.Equal(TipoZona.Mano, db.ZonaDe(primera.Id));
        Assert.Equal(TipoZona.Campo, db.ZonaDe(segunda.Id));
    }

    [Fact]
    public async Task OperationTransaction_IndiceFueraDeRango_LanzaModelExceptionYNoMueveNada()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync(cartasEnMazo: 0);
        CartaPartida carta = await db.PonerCartaAsync(juego.Jugador1, TipoZona.Mano, juego.Criatura);
        ZonaPartida mano = CraftCasterTestDb.Zona(juego.Jugador1, TipoZona.Mano);
        ZonaPartida campo = CraftCasterTestDb.Zona(juego.Jugador1, TipoZona.Campo);
        var useCase = new ZonaPartidaUseCase(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => useCase.OperationTransactionAsync(mano.Id, campo.Id, 5));
        Assert.Equal(TipoZona.Mano, db.ZonaDe(carta.Id));
    }

    [Fact]
    public async Task OperationTransaction_ZonaOrigenVacia_LanzaModelException()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync(cartasEnMazo: 0);
        ZonaPartida mano = CraftCasterTestDb.Zona(juego.Jugador1, TipoZona.Mano);
        ZonaPartida campo = CraftCasterTestDb.Zona(juego.Jugador1, TipoZona.Campo);
        var useCase = new ZonaPartidaUseCase(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => useCase.OperationTransactionAsync(mano.Id, campo.Id, 0));
    }

    // ==================================================================
    // Habilidad.activar
    // (en todas: el jugador 1 esta activo, el jugador 2 es el rival)
    // ==================================================================

    private static async Task<(PartidaEnJuego Juego, Habilidad Habilidad)> CrearJuegoConHabilidadAsync(
        CraftCasterTestDb db, TipoHabilidad tipo, int cantidad, TipoObjetivo tipoObjetivo, int objetivo)
    {
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync(jugador1Activo: true, cartasEnMazo: 3);
        Carta lanzador = await db.CrearCartaAsync(juego.Local, "Hechicero");
        Habilidad habilidad = await db.CrearHabilidadAsync(lanzador, tipo, cantidad, tipoObjetivo, objetivo);
        return (juego, habilidad);
    }

    [Fact]
    public async Task HabilidadActivar_CuraSelf_SumaVidaAlJugadorActivo()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var (juego, habilidad) = await CrearJuegoConHabilidadAsync(db, TipoHabilidad.Cura, 5, TipoObjetivo.Self, 0);
        juego.Jugador1.Vida = 20;
        await db.UnitOfWork.JugadorPartidaRepository.SaveAsync(juego.Jugador1);
        var useCase = new HabilidadUseCase(db.UnitOfWork);

        // Act
        await useCase.ActivarAsync(habilidad.Id, juego.Partida.Id);

        // Assert
        Assert.Equal(25, db.Verify(s => s.Get<JugadorPartida>(juego.Jugador1.Id).Vida));
        Assert.Equal(30, db.Verify(s => s.Get<JugadorPartida>(juego.Jugador2.Id).Vida));
    }

    [Fact]
    public async Task HabilidadActivar_DanyoAlJugador_RestaVidaSoloAlRival()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var (juego, habilidad) = await CrearJuegoConHabilidadAsync(db, TipoHabilidad.Danyo, 4, TipoObjetivo.Jugador, 0);
        var useCase = new HabilidadUseCase(db.UnitOfWork);

        // Act
        await useCase.ActivarAsync(habilidad.Id, juego.Partida.Id);

        // Assert
        Assert.Equal(26, db.Verify(s => s.Get<JugadorPartida>(juego.Jugador2.Id).Vida));
        Assert.Equal(30, db.Verify(s => s.Get<JugadorPartida>(juego.Jugador1.Id).Vida));
    }

    [Fact]
    public async Task HabilidadActivar_AtaqueConObjetivoCero_RestaVidaAlRival()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var (juego, habilidad) = await CrearJuegoConHabilidadAsync(db, TipoHabilidad.Ataque, 3, TipoObjetivo.Libre, 0);
        var useCase = new HabilidadUseCase(db.UnitOfWork);

        // Act
        await useCase.ActivarAsync(habilidad.Id, juego.Partida.Id);

        // Assert
        Assert.Equal(27, db.Verify(s => s.Get<JugadorPartida>(juego.Jugador2.Id).Vida));
    }

    [Fact]
    public async Task HabilidadActivar_DanyoATodosLosEnemigos_DanyaCadaCriaturaDelCampoRival()
    {
        // Arrange: criatura debil (vida 2) y fuerte (vida 5) en el campo rival; danyo 3
        await using var db = await CraftCasterTestDb.CreateAsync();
        var (juego, habilidad) = await CrearJuegoConHabilidadAsync(db, TipoHabilidad.Danyo, 3, TipoObjetivo.TodosEnemigos, 0);
        Carta debil = await db.CrearCartaAsync(juego.Visitante, "Debil", vida: 2);
        Carta fuerte = await db.CrearCartaAsync(juego.Visitante, "Fuerte", vida: 5);
        CartaPartida cartaDebil = await db.PonerCartaAsync(juego.Jugador2, TipoZona.Campo, debil);
        CartaPartida cartaFuerte = await db.PonerCartaAsync(juego.Jugador2, TipoZona.Campo, fuerte);
        CartaPartida propia = await db.PonerCartaAsync(juego.Jugador1, TipoZona.Campo, juego.Criatura);
        var useCase = new HabilidadUseCase(db.UnitOfWork);

        // Act
        await useCase.ActivarAsync(habilidad.Id, juego.Partida.Id);

        // Assert
        Assert.Equal(TipoZona.Descarte, db.ZonaDe(cartaDebil.Id));
        Assert.Equal(2, db.Verify(s => s.Get<CartaPartida>(cartaFuerte.Id).VidaActual));
        Assert.Equal(TipoZona.Campo, db.ZonaDe(cartaFuerte.Id));
        Assert.Equal(3, db.Verify(s => s.Get<CartaPartida>(propia.Id).VidaActual)); // la propia no se toca
    }

    [Fact]
    public async Task HabilidadActivar_Descartar_EnviaLaCartaDeLaManoRivalAlDescarte()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var (juego, habilidad) = await CrearJuegoConHabilidadAsync(db, TipoHabilidad.Descartar, 1, TipoObjetivo.Enemigo, 0);
        CartaPartida enMano = await db.PonerCartaAsync(juego.Jugador2, TipoZona.Mano, juego.Criatura);
        var useCase = new HabilidadUseCase(db.UnitOfWork);

        // Act
        await useCase.ActivarAsync(habilidad.Id, juego.Partida.Id);

        // Assert
        Assert.Equal(TipoZona.Descarte, db.ZonaDe(enMano.Id));
        Assert.Equal(0, db.ContarCartas(juego.Jugador2.Id, TipoZona.Mano));
    }

    [Fact]
    public async Task HabilidadActivar_Destruir_EnviaLaCriaturaRivalAlDescarte()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var (juego, habilidad) = await CrearJuegoConHabilidadAsync(db, TipoHabilidad.Destruir, 1, TipoObjetivo.Enemigo, 0);
        CartaPartida enCampo = await db.PonerCartaAsync(juego.Jugador2, TipoZona.Campo, juego.Criatura);
        var useCase = new HabilidadUseCase(db.UnitOfWork);

        // Act
        await useCase.ActivarAsync(habilidad.Id, juego.Partida.Id);

        // Assert
        Assert.Equal(TipoZona.Descarte, db.ZonaDe(enCampo.Id));
    }

    [Fact]
    public async Task HabilidadActivar_DestruirSinCriaturas_LanzaModelException()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var (juego, habilidad) = await CrearJuegoConHabilidadAsync(db, TipoHabilidad.Destruir, 1, TipoObjetivo.Enemigo, 0);
        var useCase = new HabilidadUseCase(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => useCase.ActivarAsync(habilidad.Id, juego.Partida.Id));
    }

    /// <summary>
    /// FALLA con la implementacion actual: HabilidadUseCase_Activar llama a GetCartaAsync
    /// con zona = 1 ("Zona 1 es el campo"), pero en el enum TipoZona Mano = 1 y Campo = 2,
    /// asi que el danyo va a una carta de la MANO del rival.
    /// </summary>
    [Fact]
    public async Task HabilidadActivar_DanyoAEnemigo_DanyaLaCriaturaIndicadaDelCampoRival()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var (juego, habilidad) = await CrearJuegoConHabilidadAsync(db, TipoHabilidad.Danyo, 2, TipoObjetivo.Enemigo, 0);
        CartaPartida enCampo = await db.PonerCartaAsync(juego.Jugador2, TipoZona.Campo, juego.Criatura);
        CartaPartida enMano = await db.PonerCartaAsync(juego.Jugador2, TipoZona.Mano, juego.Criatura);
        var useCase = new HabilidadUseCase(db.UnitOfWork);

        // Act
        await useCase.ActivarAsync(habilidad.Id, juego.Partida.Id);

        // Assert
        Assert.Equal(1, db.Verify(s => s.Get<CartaPartida>(enCampo.Id).VidaActual));
        Assert.Equal(3, db.Verify(s => s.Get<CartaPartida>(enMano.Id).VidaActual));
    }

    /// <summary>
    /// FALLA con la implementacion actual: el caso Robar llama a RobarCartaAsync(self),
    /// pasando el indice (0/1) del jugador en lugar de su Id (idSelf).
    /// </summary>
    [Fact]
    public async Task HabilidadActivar_Robar_ElJugadorActivoRobaLasCartasIndicadas()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var (juego, habilidad) = await CrearJuegoConHabilidadAsync(db, TipoHabilidad.Robar, 2, TipoObjetivo.Self, 0);
        var useCase = new HabilidadUseCase(db.UnitOfWork);

        // Act
        await useCase.ActivarAsync(habilidad.Id, juego.Partida.Id);

        // Assert
        Assert.Equal(2, db.ContarCartas(juego.Jugador1.Id, TipoZona.Mano));
        Assert.Equal(1, db.ContarCartas(juego.Jugador1.Id, TipoZona.Mazo));
        Assert.Equal(0, db.ContarCartas(juego.Jugador2.Id, TipoZona.Mano));
    }

    [Fact]
    public async Task HabilidadActivar_HabilidadInexistente_LanzaModelException()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync(jugador1Activo: true);
        var useCase = new HabilidadUseCase(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => useCase.ActivarAsync(IdInexistente, juego.Partida.Id));
    }
}
