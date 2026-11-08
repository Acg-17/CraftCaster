using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Tests.Manual;

/// <summary>
/// Pruebas de las operaciones Custom (Services/manual) y de los New customized.
/// Cada prueba sigue el patron AAA y comprueba el resultado con una sesion nueva.
/// </summary>
public sealed class CustomOperationTests
{
    private const int IdInexistente = 9999;

    // ==================================================================
    // Usuario
    // ==================================================================

    [Fact]
    public async Task UsuarioNew_DatosValidos_CreaUsuarioActivoConContraseniaCifrada()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var service = new UsuarioService(db.UnitOfWork);

        // Act
        Usuario usuario = await service.NewAsync("ana", "ana@test.com", "secreto123", 1, TipoUsuario.Cliente);

        // Assert
        Usuario guardado = db.Verify(s => s.Get<Usuario>(usuario.Id));
        Assert.NotNull(guardado);
        Assert.Equal(EstadoCuenta.Activa, guardado.EstadoCuenta);
        Assert.Equal(TipoUsuario.Cliente, guardado.TipoUsuario);
        Assert.NotEqual("secreto123", guardado.Contrasenia);
        Assert.True(db.UnitOfWork.PasswordHasher.VerifyPassword(guardado.Contrasenia, "secreto123"));
    }

    [Fact]
    public async Task CambiarEstadoCuenta_UsuarioExistente_ActualizaElEstado()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario usuario = await db.CrearUsuarioAsync();
        var service = new UsuarioService(db.UnitOfWork);

        // Act
        await service.CambiarEstadoCuentaAsync(usuario.Id, EstadoCuenta.Suspendida);

        // Assert
        Assert.Equal(EstadoCuenta.Suspendida, db.Verify(s => s.Get<Usuario>(usuario.Id).EstadoCuenta));
    }

    [Fact]
    public async Task CambiarEstadoCuenta_UsuarioInexistente_LanzaModelException()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var service = new UsuarioService(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() =>
            service.CambiarEstadoCuentaAsync(IdInexistente, EstadoCuenta.Suspendida));
    }

    [Fact]
    public async Task ReportarUsuario_UsuarioExistente_SuspendeLaCuenta()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario usuario = await db.CrearUsuarioAsync();
        var service = new UsuarioService(db.UnitOfWork);

        // Act
        await service.ReportarUsuarioAsync(usuario.Id);

        // Assert
        Assert.Equal(EstadoCuenta.Suspendida, db.Verify(s => s.Get<Usuario>(usuario.Id).EstadoCuenta));
    }

    [Fact]
    public async Task ReportarUsuario_UsuarioInexistente_LanzaModelException()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var service = new UsuarioService(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => service.ReportarUsuarioAsync(IdInexistente));
    }

    // ==================================================================
    // Mazo
    // ==================================================================

    [Fact]
    public async Task MazoNewCustomized_UsuarioExistente_CreaMazoVacioEIncompleto()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario usuario = await db.CrearUsuarioAsync();
        var service = new MazoService(db.UnitOfWork);

        // Act
        Mazo mazo = await service.NewCustomizedAsync("Aggro", true, 1, usuario.Id);

        // Assert
        var guardado = db.Verify(s =>
        {
            Mazo m = s.Get<Mazo>(mazo.Id);
            return (m.Nombre, m.NumCartas, m.CosteMedio, m.EstadoMazo, UsuarioId: m.Usuario.Id);
        });
        Assert.Equal("Aggro", guardado.Nombre);
        Assert.Equal(0, guardado.NumCartas);
        Assert.Equal(0f, guardado.CosteMedio);
        Assert.Equal(EstadoMazo.Incompleto, guardado.EstadoMazo);
        Assert.Equal(usuario.Id, guardado.UsuarioId);
    }

    [Fact]
    public async Task MazoNewCustomized_UsuarioInexistente_LanzaModelExceptionYNoCreaMazo()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var service = new MazoService(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => service.NewCustomizedAsync("Aggro", true, 1, IdInexistente));
        Assert.Equal(0, db.Contar<Mazo>());
    }

    [Fact]
    public async Task MazoCompartir_MazoPrivado_LoHacePublico()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario usuario = await db.CrearUsuarioAsync();
        Mazo mazo = await db.CrearMazoAsync(usuario, publico: false);
        var service = new MazoService(db.UnitOfWork);

        // Act
        await service.CompartirAsync(mazo.Id);

        // Assert
        Assert.True(db.Verify(s => s.Get<Mazo>(mazo.Id).EsPublico));
    }

    [Fact]
    public async Task MazoCompartir_MazoInexistente_LanzaModelException()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var service = new MazoService(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => service.CompartirAsync(IdInexistente));
    }

    [Fact]
    public async Task CalcularEstadisticas_TreintaCartas_CalculaCosteMedioYMarcaJugable()
    {
        // Arrange: 20 cartas de coste 2 + 10 de coste 5 -> (40 + 50) / 30 = 3
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario usuario = await db.CrearUsuarioAsync();
        Carta barata = await db.CrearCartaAsync(usuario, "Barata", coste: 2);
        Carta cara = await db.CrearCartaAsync(usuario, "Cara", coste: 5);
        Mazo mazo = await db.CrearMazoAsync(usuario, estado: EstadoMazo.Incompleto);
        await db.AnyadirLineaAsync(mazo, barata, 20);
        await db.AnyadirLineaAsync(mazo, cara, 10);
        var service = new MazoService(db.UnitOfWork);

        // Act
        await service.CalcularEstadisticasAsync(mazo.Id);

        // Assert
        var guardado = db.Verify(s =>
        {
            Mazo m = s.Get<Mazo>(mazo.Id);
            return (m.NumCartas, m.CosteMedio, m.EstadoMazo);
        });
        Assert.Equal(30, guardado.NumCartas);
        Assert.Equal(3f, guardado.CosteMedio, 3);
        Assert.Equal(EstadoMazo.Jugable, guardado.EstadoMazo);
    }

    [Fact]
    public async Task CalcularEstadisticas_MenosDeTreintaCartas_MarcaIncompleto()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario usuario = await db.CrearUsuarioAsync();
        Carta carta = await db.CrearCartaAsync(usuario, "Carta", coste: 4);
        Mazo mazo = await db.CrearMazoAsync(usuario, estado: EstadoMazo.Jugable);
        await db.AnyadirLineaAsync(mazo, carta, 29);
        var service = new MazoService(db.UnitOfWork);

        // Act
        await service.CalcularEstadisticasAsync(mazo.Id);

        // Assert
        var guardado = db.Verify(s =>
        {
            Mazo m = s.Get<Mazo>(mazo.Id);
            return (m.NumCartas, m.CosteMedio, m.EstadoMazo);
        });
        Assert.Equal(29, guardado.NumCartas);
        Assert.Equal(4f, guardado.CosteMedio, 3);
        Assert.Equal(EstadoMazo.Incompleto, guardado.EstadoMazo);
    }

    [Fact]
    public async Task CalcularEstadisticas_MazoSinLineas_PoneCerosEIncompleto()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario usuario = await db.CrearUsuarioAsync();
        Mazo mazo = await db.CrearMazoAsync(usuario, costeMedio: 2.5f, estado: EstadoMazo.Jugable);
        var service = new MazoService(db.UnitOfWork);

        // Act
        await service.CalcularEstadisticasAsync(mazo.Id);

        // Assert
        var guardado = db.Verify(s =>
        {
            Mazo m = s.Get<Mazo>(mazo.Id);
            return (m.NumCartas, m.CosteMedio, m.EstadoMazo);
        });
        Assert.Equal(0, guardado.NumCartas);
        Assert.Equal(0f, guardado.CosteMedio);
        Assert.Equal(EstadoMazo.Incompleto, guardado.EstadoMazo);
    }

    [Fact]
    public async Task CalcularEstadisticas_MazoInexistente_LanzaModelException()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var service = new MazoService(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => service.CalcularEstadisticasAsync(IdInexistente));
    }

    [Theory]
    [InlineData(0.5f)]
    [InlineData(3.5f)]
    [InlineData(5.0f)]
    public async Task ValorarMazo_ValoracionDentroDelRango_GuardaLaValoracion(float valoracion)
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario usuario = await db.CrearUsuarioAsync();
        Mazo mazo = await db.CrearMazoAsync(usuario);
        var service = new MazoService(db.UnitOfWork);

        // Act
        await service.ValorarMazoAsync(mazo.Id, valoracion);

        // Assert
        Assert.Equal(valoracion, db.Verify(s => s.Get<Mazo>(mazo.Id).Valoracion), 3);
    }

    [Theory]
    [InlineData(0.4f)]
    [InlineData(5.1f)]
    [InlineData(-1f)]
    public async Task ValorarMazo_ValoracionFueraDeRango_LanzaModelExceptionYNoModifica(float valoracion)
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario usuario = await db.CrearUsuarioAsync();
        Mazo mazo = await db.CrearMazoAsync(usuario, valoracion: 4f);
        var service = new MazoService(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => service.ValorarMazoAsync(mazo.Id, valoracion));
        Assert.Equal(4f, db.Verify(s => s.Get<Mazo>(mazo.Id).Valoracion), 3);
    }

    // ==================================================================
    // Carta
    // ==================================================================

    [Fact]
    public async Task CartaNew_Criatura_GuardaAtaqueYVida()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario usuario = await db.CrearUsuarioAsync();
        var service = new CartaService(db.UnitOfWork);

        // Act
        Carta carta = await service.NewAsync("Dragon", Rareza.Legendario, 7, TipoCarta.Criatura,
            "Un dragon", 1, true, 6, 8, usuario.Id);

        // Assert
        var guardada = db.Verify(s =>
        {
            Carta c = s.Get<Carta>(carta.Id);
            return (c.Tipo, c.Ataque, c.Vida, c.CosteMana, UsuarioId: c.Usuario.Id);
        });
        Assert.Equal(TipoCarta.Criatura, guardada.Tipo);
        Assert.Equal(6, guardada.Ataque);
        Assert.Equal(8, guardada.Vida);
        Assert.Equal(7, guardada.CosteMana);
        Assert.Equal(usuario.Id, guardada.UsuarioId);
    }

    [Fact]
    public async Task CartaNew_Hechizo_NoGuardaAtaqueNiVida()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario usuario = await db.CrearUsuarioAsync();
        var service = new CartaService(db.UnitOfWork);

        // Act
        Carta carta = await service.NewAsync("Rayo", Rareza.Comun, 1, TipoCarta.Hechizo,
            "Un rayo", 1, true, 6, 8, usuario.Id);

        // Assert
        var guardada = db.Verify(s =>
        {
            Carta c = s.Get<Carta>(carta.Id);
            return (c.Ataque, c.Vida);
        });
        Assert.Null(guardada.Ataque);
        Assert.Null(guardada.Vida);
    }

    [Fact]
    public async Task CartaNew_UsuarioInexistente_LanzaModelExceptionYNoCreaCarta()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var service = new CartaService(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => service.NewAsync("Rayo", Rareza.Comun, 1,
            TipoCarta.Hechizo, "Un rayo", 1, true, 0, 0, IdInexistente));
        Assert.Equal(0, db.Contar<Carta>());
    }

    [Fact]
    public async Task CartaCompartir_CartaPrivada_LaHacePublica()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario usuario = await db.CrearUsuarioAsync();
        Carta carta = await db.CrearCartaAsync(usuario, "Secreta", publica: false);
        var service = new CartaService(db.UnitOfWork);

        // Act
        await service.CompartirAsync(carta.Id);

        // Assert
        Assert.True(db.Verify(s => s.Get<Carta>(carta.Id).EsPublica));
    }

    [Fact]
    public async Task CartaCompartir_CartaInexistente_LanzaModelException()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var service = new CartaService(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => service.CompartirAsync(IdInexistente));
    }

    // ==================================================================
    // Estado
    // ==================================================================

    [Fact]
    public async Task EstadoActivar_EstadoExistente_NoLanzaYConservaSusHabilidades()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario usuario = await db.CrearUsuarioAsync();
        Carta carta = await db.CrearCartaAsync(usuario, "Clerigo");
        Habilidad habilidad = await db.CrearHabilidadAsync(carta, TipoHabilidad.Cura, 3, TipoObjetivo.Self, 0);
        var service = new EstadoService(db.UnitOfWork);

        // Act
        await service.ActivarAsync(habilidad.Estado.Id);

        // Assert
        Assert.Equal(1, db.Verify(s => s.Get<Estado>(habilidad.Estado.Id).Habilidades.Count));
    }

    [Fact]
    public async Task EstadoActivar_EstadoInexistente_LanzaModelException()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var service = new EstadoService(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => service.ActivarAsync(IdInexistente));
    }

    // ==================================================================
    // Partida
    // ==================================================================

    [Fact]
    public async Task CambiarEstado_DeEsperaAEnCurso_ActualizaElEstado()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario usuario = await db.CrearUsuarioAsync();
        Mazo mazo = await db.CrearMazoAsync(usuario);
        Partida partida = await db.CrearPartidaAsync(usuario, mazo, EstadoPartida.Espera);
        var service = new PartidaService(db.UnitOfWork);

        // Act
        await service.CambiarEstadoAsync(partida.Id, EstadoPartida.EnCurso);

        // Assert
        Assert.Equal(EstadoPartida.EnCurso, db.Verify(s => s.Get<Partida>(partida.Id).Estado));
    }

    [Theory]
    [InlineData(EstadoPartida.EnCurso, EstadoPartida.Espera)]       // volver atras
    [InlineData(EstadoPartida.EnCurso, EstadoPartida.EnCurso)]      // mismo estado
    [InlineData(EstadoPartida.Finalizada, EstadoPartida.EnCurso)]   // ya finalizada
    public async Task CambiarEstado_TransicionNoPermitida_LanzaModelExceptionYNoModifica(
        EstadoPartida estadoInicial, EstadoPartida nuevoEstado)
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario usuario = await db.CrearUsuarioAsync();
        Mazo mazo = await db.CrearMazoAsync(usuario);
        Partida partida = await db.CrearPartidaAsync(usuario, mazo, estadoInicial);
        var service = new PartidaService(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => service.CambiarEstadoAsync(partida.Id, nuevoEstado));
        Assert.Equal(estadoInicial, db.Verify(s => s.Get<Partida>(partida.Id).Estado));
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public async Task GetActivo_SegunJugadorActivo_DevuelveSuIndice(bool jugador1Activo, int esperado)
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync(jugador1Activo: jugador1Activo);
        juego.Jugador2.Activo = !jugador1Activo;
        var service = new PartidaService(db.UnitOfWork);

        // Act
        int activo = await service.GetActivoAsync(juego.Partida.Id);

        // Assert
        Assert.Equal(esperado, activo);
    }

    [Fact]
    public async Task GetActivo_PartidaSinJugadores_LanzaModelException()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario usuario = await db.CrearUsuarioAsync();
        Mazo mazo = await db.CrearMazoAsync(usuario);
        Partida partida = await db.CrearPartidaAsync(usuario, mazo, EstadoPartida.Espera);
        var service = new PartidaService(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => service.GetActivoAsync(partida.Id));
    }

    [Fact]
    public async Task GetPlayer_IndicesValidos_DevuelveElIdDeCadaJugador()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync();
        var service = new PartidaService(db.UnitOfWork);

        // Act
        int idJugador1 = await service.GetPlayerAsync(juego.Partida.Id, 0);
        int idJugador2 = await service.GetPlayerAsync(juego.Partida.Id, 1);

        // Assert
        Assert.Equal(juego.Jugador1.Id, idJugador1);
        Assert.Equal(juego.Jugador2.Id, idJugador2);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(-1)]
    public async Task GetPlayer_IndiceFueraDeRango_LanzaModelException(int indice)
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync();
        var service = new PartidaService(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => service.GetPlayerAsync(juego.Partida.Id, indice));
    }

    [Fact]
    public async Task GetCarta_PosicionValidaDelCampo_DevuelveElIdDeLaCarta()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync();
        CartaPartida primera = await db.PonerCartaAsync(juego.Jugador1, TipoZona.Campo, juego.Criatura);
        CartaPartida segunda = await db.PonerCartaAsync(juego.Jugador1, TipoZona.Campo, juego.Criatura);
        var service = new PartidaService(db.UnitOfWork);

        // Act
        int idPrimera = await service.GetCartaAsync(juego.Partida.Id, 0, (int)TipoZona.Campo, 0);
        int idSegunda = await service.GetCartaAsync(juego.Partida.Id, 0, (int)TipoZona.Campo, 1);

        // Assert
        Assert.Equal(primera.Id, idPrimera);
        Assert.Equal(segunda.Id, idSegunda);
    }

    [Fact]
    public async Task GetCarta_PosicionFueraDeRango_LanzaModelException()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync();
        await db.PonerCartaAsync(juego.Jugador1, TipoZona.Campo, juego.Criatura);
        var service = new PartidaService(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() =>
            service.GetCartaAsync(juego.Partida.Id, 0, (int)TipoZona.Campo, 5));
    }

    // ==================================================================
    // ZonaPartida
    // ==================================================================

    [Fact]
    public async Task GetAllCartas_ZonaConCartas_DevuelveLosIdsEnOrden()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync(cartasEnMazo: 0);
        CartaPartida c1 = await db.PonerCartaAsync(juego.Jugador1, TipoZona.Mano, juego.Criatura);
        CartaPartida c2 = await db.PonerCartaAsync(juego.Jugador1, TipoZona.Mano, juego.Hechizo);
        var service = new ZonaPartidaService(db.UnitOfWork);

        // Act
        IReadOnlyList<int> ids = await service.GetAllCartasAsync(CraftCasterTestDb.Zona(juego.Jugador1, TipoZona.Mano).Id);

        // Assert
        Assert.Equal(new[] { c1.Id, c2.Id }, ids);
    }

    [Fact]
    public async Task GetAllCartas_ZonaVacia_DevuelveListaVacia()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync(cartasEnMazo: 0);
        var service = new ZonaPartidaService(db.UnitOfWork);

        // Act
        IReadOnlyList<int> ids = await service.GetAllCartasAsync(CraftCasterTestDb.Zona(juego.Jugador1, TipoZona.Campo).Id);

        // Assert
        Assert.Empty(ids);
    }

    [Fact]
    public async Task GetAllCartas_ZonaInexistente_LanzaModelException()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var service = new ZonaPartidaService(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => service.GetAllCartasAsync(IdInexistente));
    }

    // ==================================================================
    // JugadorPartida (casos que faltaban en PruebasFuncionalesBackendTests)
    // ==================================================================

    [Fact]
    public async Task ActualizarVida_JugadorInexistente_LanzaModelException()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var service = new JugadorPartidaService(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => service.ActualizarVidaAsync(IdInexistente, 5));
    }

    [Fact]
    public async Task ActualizarVida_DanyoMayorQueLaVida_DejaLaVidaEnCeroONegativa()
    {
        // Arrange (caso limite: 30 de vida y 35 de danyo)
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync();
        var service = new JugadorPartidaService(db.UnitOfWork);

        // Act
        await service.ActualizarVidaAsync(juego.Jugador1.Id, 35);

        // Assert
        Assert.True(db.Verify(s => s.Get<JugadorPartida>(juego.Jugador1.Id).Vida) <= 0);
    }

    [Fact]
    public async Task ActualizarMana_ManaNegativo_LanzaModelExceptionYNoModifica()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync(manaJugador1: 4);
        var service = new JugadorPartidaService(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => service.ActualizarManaAsync(juego.Jugador1.Id, -1));
        Assert.Equal(4, db.Verify(s => s.Get<JugadorPartida>(juego.Jugador1.Id).Mana));
    }

    [Fact]
    public async Task ActualizarMana_JugadorInexistente_LanzaModelException()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var service = new JugadorPartidaService(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => service.ActualizarManaAsync(IdInexistente, 3));
    }

    // ==================================================================
    // CartaPartida (casos que faltaban en PruebasFuncionalesBackendTests)
    // ==================================================================

    [Fact]
    public async Task RecibirDanyo_DanyoNegativo_LanzaModelExceptionYNoModifica()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync(cartasEnMazo: 0);
        CartaPartida carta = await db.PonerCartaAsync(juego.Jugador1, TipoZona.Campo, juego.Criatura);
        var service = new CartaPartidaService(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => service.RecibirDanyoAsync(carta.Id, -2));
        Assert.Equal(3, db.Verify(s => s.Get<CartaPartida>(carta.Id).VidaActual));
        Assert.Equal(TipoZona.Campo, db.ZonaDe(carta.Id));
    }

    [Fact]
    public async Task RecibirDanyo_DanyoCero_NoCambiaLaCarta()
    {
        // Arrange (caso limite)
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync(cartasEnMazo: 0);
        CartaPartida carta = await db.PonerCartaAsync(juego.Jugador1, TipoZona.Campo, juego.Criatura, puedeAtacar: true);
        var service = new CartaPartidaService(db.UnitOfWork);

        // Act
        await service.RecibirDanyoAsync(carta.Id, 0);

        // Assert
        var guardada = db.Verify(s =>
        {
            CartaPartida c = s.Get<CartaPartida>(carta.Id);
            return (c.VidaActual, c.PuedeAtacar, c.Zona.TipoZona);
        });
        Assert.Equal(3, guardada.VidaActual);
        Assert.True(guardada.PuedeAtacar);
        Assert.Equal(TipoZona.Campo, guardada.TipoZona);
    }

    [Fact]
    public async Task RecibirDanyo_CartaInexistente_LanzaModelException()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        var service = new CartaPartidaService(db.UnitOfWork);

        // Act + Assert
        await Assert.ThrowsAsync<ModelException>(() => service.RecibirDanyoAsync(IdInexistente, 1));
    }
}
