using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Tests.Manual;

/// <summary>
/// Pruebas de la semantica real de cada ReadFilter del .tdm:
/// devuelve solo lo que cumple el filtro, no da falsos positivos
/// y devuelve una coleccion vacia cuando no hay coincidencias.
/// </summary>
public sealed class FilterOperationTests
{
    private static void AssertIds<T>(IEnumerable<T> resultado, Func<T, int> id, params int[] esperados)
    {
        Assert.Equal(esperados.OrderBy(x => x), resultado.Select(id).OrderBy(x => x));
    }

    // ==================================================================
    // Usuario
    // ==================================================================

    [Fact]
    public async Task ObtenerUsuarioLocal_PartidaConLocalYVisitante_DevuelveSoloElLocal()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync();

        // Act
        IReadOnlyList<Usuario> resultado = await db.UnitOfWork.UsuarioRepository.ObtenerUsuarioLocalAsync(juego.Partida.Id);

        // Assert
        AssertIds(resultado, u => u.Id, juego.Local.Id);
    }

    [Fact]
    public async Task ObtenerUsuarioLocal_PartidaInexistente_DevuelveVacio()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        await db.CrearPartidaEnJuegoAsync();

        // Act
        IReadOnlyList<Usuario> resultado = await db.UnitOfWork.UsuarioRepository.ObtenerUsuarioLocalAsync(9999);

        // Assert
        Assert.Empty(resultado);
    }

    // ==================================================================
    // Mazo
    // ==================================================================

    [Fact]
    public async Task ObtenerMazosUsuario_VariosUsuarios_DevuelveSoloLosMazosDelUsuario()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario ana = await db.CrearUsuarioAsync("ana");
        Usuario luis = await db.CrearUsuarioAsync("luis");
        Mazo publico = await db.CrearMazoAsync(ana, "A1", publico: true);
        Mazo privado = await db.CrearMazoAsync(ana, "A2", publico: false);
        await db.CrearMazoAsync(luis, "L1");

        // Act
        IReadOnlyList<Mazo> resultado = await db.UnitOfWork.MazoRepository.ObtenerMazosUsuarioAsync(ana.Id);

        // Assert: incluye tambien los privados del propio usuario
        AssertIds(resultado, m => m.Id, publico.Id, privado.Id);
        Assert.All(resultado, m => Assert.Equal(ana.Id, m.Usuario.Id));
    }

    [Fact]
    public async Task ObtenerMazosUsuario_UsuarioSinMazos_DevuelveVacio()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario ana = await db.CrearUsuarioAsync("ana");
        Usuario luis = await db.CrearUsuarioAsync("luis");
        await db.CrearMazoAsync(luis);

        // Act
        IReadOnlyList<Mazo> resultado = await db.UnitOfWork.MazoRepository.ObtenerMazosUsuarioAsync(ana.Id);

        // Assert
        Assert.Empty(resultado);
    }

    [Fact]
    public async Task ObtenerMazosPorNombre_NombreExacto_DevuelveSoloLosPublicosConEseNombre()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario ana = await db.CrearUsuarioAsync("ana");
        Usuario luis = await db.CrearUsuarioAsync("luis");
        Mazo aggroAna = await db.CrearMazoAsync(ana, "Aggro", publico: true);
        Mazo aggroLuis = await db.CrearMazoAsync(luis, "Aggro", publico: true);
        await db.CrearMazoAsync(luis, "Aggro", publico: false);   // privado: no debe salir
        await db.CrearMazoAsync(ana, "Control", publico: true);   // otro nombre

        // Act
        IReadOnlyList<Mazo> resultado = await db.UnitOfWork.MazoRepository.ObtenerMazosPorNombreAsync("Aggro");

        // Assert
        AssertIds(resultado, m => m.Id, aggroAna.Id, aggroLuis.Id);
    }

    [Fact]
    public async Task ObtenerMazosPorNombre_SinCoincidencias_DevuelveVacio()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario ana = await db.CrearUsuarioAsync("ana");
        await db.CrearMazoAsync(ana, "Aggro");

        // Act
        IReadOnlyList<Mazo> resultado = await db.UnitOfWork.MazoRepository.ObtenerMazosPorNombreAsync("Midrange");

        // Assert
        Assert.Empty(resultado);
    }

    [Fact]
    public async Task ObtenerMazoPorCosteMedio_Rango_IncluyeLimitesYExcluyePrivadosYFueraDeRango()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario ana = await db.CrearUsuarioAsync("ana");
        Mazo limiteInferior = await db.CrearMazoAsync(ana, "C2", costeMedio: 2f);
        Mazo dentro = await db.CrearMazoAsync(ana, "C3", costeMedio: 3.2f);
        Mazo limiteSuperior = await db.CrearMazoAsync(ana, "C4", costeMedio: 4f);
        await db.CrearMazoAsync(ana, "C1", costeMedio: 1.9f);                  // por debajo
        await db.CrearMazoAsync(ana, "C5", costeMedio: 4.5f);                  // por encima
        await db.CrearMazoAsync(ana, "Privado", publico: false, costeMedio: 3f); // privado

        // Act
        IReadOnlyList<Mazo> resultado = await db.UnitOfWork.MazoRepository.ObtenerMazoPorCosteMedioAsync(2, 4);

        // Assert
        AssertIds(resultado, m => m.Id, limiteInferior.Id, dentro.Id, limiteSuperior.Id);
    }

    [Fact]
    public async Task ObtenerMazoPorValoracion_Rango_IncluyeLimitesYExcluyePrivadosYFueraDeRango()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario ana = await db.CrearUsuarioAsync("ana");
        Mazo tres = await db.CrearMazoAsync(ana, "V3", valoracion: 3f);
        Mazo cinco = await db.CrearMazoAsync(ana, "V5", valoracion: 5f);
        await db.CrearMazoAsync(ana, "V2", valoracion: 2.9f);
        await db.CrearMazoAsync(ana, "Privado", publico: false, valoracion: 4f);

        // Act
        IReadOnlyList<Mazo> resultado = await db.UnitOfWork.MazoRepository.ObtenerMazoPorValoracionAsync(3, 5);

        // Assert
        AssertIds(resultado, m => m.Id, tres.Id, cinco.Id);
    }

    [Fact]
    public async Task ObtenerMazoJugador1_PartidaConDosMazos_DevuelveSoloElMazoDelLocal()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync();
        PartidaEnJuego otra = await CrearSegundaPartidaAsync(db);

        // Act
        IReadOnlyList<Mazo> resultado = await db.UnitOfWork.MazoRepository.ObtenerMazoJugador1Async(juego.Partida.Id);

        // Assert
        AssertIds(resultado, m => m.Id, juego.MazoLocal.Id);
        Assert.DoesNotContain(resultado, m => m.Id == otra.MazoLocal.Id);
    }

    [Fact]
    public async Task ObtenerMazoJugador2_PartidaConDosMazos_DevuelveSoloElMazoDelVisitante()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync();

        // Act
        IReadOnlyList<Mazo> resultado = await db.UnitOfWork.MazoRepository.ObtenerMazoJugador2Async(juego.Partida.Id);

        // Assert
        AssertIds(resultado, m => m.Id, juego.MazoVisitante.Id);
    }

    [Fact]
    public async Task ObtenerMazoJugador2_PartidaEnEsperaSinVisitante_DevuelveVacio()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario ana = await db.CrearUsuarioAsync("ana");
        Mazo mazo = await db.CrearMazoAsync(ana);
        Partida partida = await db.CrearPartidaAsync(ana, mazo, EstadoPartida.Espera);

        // Act
        IReadOnlyList<Mazo> resultado = await db.UnitOfWork.MazoRepository.ObtenerMazoJugador2Async(partida.Id);

        // Assert
        Assert.Empty(resultado);
    }

    // ==================================================================
    // Carta
    // ==================================================================

    [Fact]
    public async Task ObtenerCartasUsuario_VariosUsuarios_DevuelveTodasLasCartasDelUsuario()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario ana = await db.CrearUsuarioAsync("ana");
        Usuario luis = await db.CrearUsuarioAsync("luis");
        Carta publica = await db.CrearCartaAsync(ana, "Publica", publica: true);
        Carta privada = await db.CrearCartaAsync(ana, "Privada", publica: false);
        await db.CrearCartaAsync(luis, "DeLuis");

        // Act
        IReadOnlyList<Carta> resultado = await db.UnitOfWork.CartaRepository.ObtenerCartasUsuarioAsync(ana.Id);

        // Assert
        AssertIds(resultado, c => c.Id, publica.Id, privada.Id);
    }

    [Fact]
    public async Task ObtenerCartaPorNombre_NombreExacto_DevuelveSoloLasPublicas()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario ana = await db.CrearUsuarioAsync("ana");
        Carta dragonPublico = await db.CrearCartaAsync(ana, "Dragon", publica: true);
        await db.CrearCartaAsync(ana, "Dragon", publica: false);
        await db.CrearCartaAsync(ana, "Dragon rojo", publica: true);

        // Act
        IReadOnlyList<Carta> resultado = await db.UnitOfWork.CartaRepository.ObtenerCartaPorNombreAsync("Dragon");

        // Assert
        AssertIds(resultado, c => c.Id, dragonPublico.Id);
    }

    [Fact]
    public async Task ObtenerCartaPorCosteMana_CosteConcreto_DevuelveSoloLasPublicasConEseCoste()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario ana = await db.CrearUsuarioAsync("ana");
        Carta coste3 = await db.CrearCartaAsync(ana, "A", coste: 3);
        Carta otraCoste3 = await db.CrearCartaAsync(ana, "B", TipoCarta.Hechizo, coste: 3);
        await db.CrearCartaAsync(ana, "C", coste: 4);
        await db.CrearCartaAsync(ana, "D", coste: 3, publica: false);

        // Act
        IReadOnlyList<Carta> resultado = await db.UnitOfWork.CartaRepository.ObtenerCartaPorCosteManaAsync(3);

        // Assert
        AssertIds(resultado, c => c.Id, coste3.Id, otraCoste3.Id);
        Assert.All(resultado, c => Assert.Equal(3, c.CosteMana));
    }

    [Fact]
    public async Task ObtenerCartaPorCosteMana_SinCoincidencias_DevuelveVacio()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario ana = await db.CrearUsuarioAsync("ana");
        await db.CrearCartaAsync(ana, "A", coste: 3);

        // Act
        IReadOnlyList<Carta> resultado = await db.UnitOfWork.CartaRepository.ObtenerCartaPorCosteManaAsync(10);

        // Assert
        Assert.Empty(resultado);
    }

    [Fact]
    public async Task ObtenerCartaPorRareza_Legendario_DevuelveSoloLasLegendariasPublicas()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario ana = await db.CrearUsuarioAsync("ana");
        Carta legendaria = await db.CrearCartaAsync(ana, "A", rareza: Rareza.Legendario);
        await db.CrearCartaAsync(ana, "B", rareza: Rareza.Legendario, publica: false);
        await db.CrearCartaAsync(ana, "C", rareza: Rareza.Epico);

        // Act
        IReadOnlyList<Carta> resultado = await db.UnitOfWork.CartaRepository.ObtenerCartaPorRarezaAsync(Rareza.Legendario);

        // Assert
        AssertIds(resultado, c => c.Id, legendaria.Id);
    }

    [Fact]
    public async Task ObtenerCartaPorRareza_RarezaSinCartas_DevuelveVacio()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario ana = await db.CrearUsuarioAsync("ana");
        await db.CrearCartaAsync(ana, "A", rareza: Rareza.Comun);

        // Act
        IReadOnlyList<Carta> resultado = await db.UnitOfWork.CartaRepository.ObtenerCartaPorRarezaAsync(Rareza.Especial);

        // Assert
        Assert.Empty(resultado);
    }

    [Theory]
    [InlineData(TipoCarta.Criatura)]
    [InlineData(TipoCarta.Hechizo)]
    public async Task ObtenerCartaPorTipo_Tipo_DevuelveSoloLasPublicasDeEseTipo(TipoCarta tipo)
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        Usuario ana = await db.CrearUsuarioAsync("ana");
        Carta criatura = await db.CrearCartaAsync(ana, "Golem", TipoCarta.Criatura);
        Carta hechizo = await db.CrearCartaAsync(ana, "Rayo", TipoCarta.Hechizo);
        await db.CrearCartaAsync(ana, "Privada", tipo, publica: false);
        int esperado = tipo == TipoCarta.Criatura ? criatura.Id : hechizo.Id;

        // Act
        IReadOnlyList<Carta> resultado = await db.UnitOfWork.CartaRepository.ObtenerCartaPorTipoAsync(tipo);

        // Assert
        AssertIds(resultado, c => c.Id, esperado);
    }

    // ==================================================================
    // Partida
    // ==================================================================

    [Fact]
    public async Task ObtenerPartidasUsuario_UsuarioLocalYVisitante_DevuelveAmbasPartidas()
    {
        // Arrange: el visitante de la primera partida crea una segunda como local (sin rival aun)
        await using var db = await CraftCasterTestDb.CreateAsync();
        PartidaEnJuego juego = await db.CrearPartidaEnJuegoAsync();
        Partida comoLocal = await db.CrearPartidaAsync(juego.Visitante, juego.MazoVisitante, EstadoPartida.Espera);
        PartidaEnJuego ajena = await CrearSegundaPartidaAsync(db);

        // Act
        IReadOnlyList<Partida> resultado = await db.UnitOfWork.PartidaRepository.ObtenerPartidasUsuarioAsync(juego.Visitante.Id);

        // Assert
        AssertIds(resultado, p => p.Id, juego.Partida.Id, comoLocal.Id);
        Assert.DoesNotContain(resultado, p => p.Id == ajena.Partida.Id);
    }

    [Fact]
    public async Task ObtenerPartidasUsuario_UsuarioSinPartidas_DevuelveVacio()
    {
        // Arrange
        await using var db = await CraftCasterTestDb.CreateAsync();
        await db.CrearPartidaEnJuegoAsync();
        Usuario sinPartidas = await db.CrearUsuarioAsync("solitario");

        // Act
        IReadOnlyList<Partida> resultado = await db.UnitOfWork.PartidaRepository.ObtenerPartidasUsuarioAsync(sinPartidas.Id);

        // Assert
        Assert.Empty(resultado);
    }

    // ------------------------------------------------------------------

    /// <summary>Segunda partida con usuarios y mazos distintos, para comprobar que no hay falsos positivos.</summary>
    private static async Task<PartidaEnJuego> CrearSegundaPartidaAsync(CraftCasterTestDb db)
    {
        Usuario local = await db.CrearUsuarioAsync("local2");
        Usuario visitante = await db.CrearUsuarioAsync("visitante2");
        Mazo mazoLocal = await db.CrearMazoAsync(local, "Mazo local 2");
        Mazo mazoVisitante = await db.CrearMazoAsync(visitante, "Mazo visitante 2");
        Partida partida = await db.CrearPartidaAsync(local, mazoLocal, EstadoPartida.EnCurso, visitante, mazoVisitante);
        JugadorPartida j1 = await db.CrearJugadorAsync(partida, 1);
        JugadorPartida j2 = await db.CrearJugadorAsync(partida, 2);
        Carta carta = await db.CrearCartaAsync(local, "Carta 2");
        Carta hechizo = await db.CrearCartaAsync(local, "Hechizo 2", TipoCarta.Hechizo);
        return new PartidaEnJuego(local, visitante, mazoLocal, mazoVisitante, partida, j1, j2, carta, hechizo);
    }
}
