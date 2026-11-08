using System.Data.SQLite;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Linq;
using NHibernate.Tool.hbm2ddl;
using CraftCaster.Application.Abstractions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;
using CraftCaster.Infrastructure;
using HariaUnitOfWork = CraftCaster.Infrastructure.UnitOfWork;

namespace CraftCaster.Tests.Manual;

/// <summary>
/// Datos de una partida en curso lista para usar en las pruebas:
/// dos usuarios con su mazo, dos jugadores con sus 4 zonas y cartas en el mazo.
/// </summary>
public sealed record PartidaEnJuego(
    Usuario Local,
    Usuario Visitante,
    Mazo MazoLocal,
    Mazo MazoVisitante,
    Partida Partida,
    JugadorPartida Jugador1,
    JugadorPartida Jugador2,
    Carta Criatura,
    Carta Hechizo);

/// <summary>
/// Escenario de prueba aislado: base de datos SQLite en memoria (la conexion
/// permanece abierta durante toda la prueba), esquema creado desde los mapeos
/// de HARIA y un UnitOfWork real. Cada prueba parte de una BD vacia.
/// </summary>
public sealed class CraftCasterTestDb : IAsyncDisposable
{
    private const string ConnectionString = "Data Source=:memory:;Version=3;New=True;";

    private readonly SQLiteConnection _connection;
    private readonly ISessionFactory _sessionFactory;
    private readonly HariaUnitOfWork _unitOfWork;
    private int _secuencia;

    private CraftCasterTestDb(SQLiteConnection connection, ISessionFactory sessionFactory, HariaUnitOfWork unitOfWork)
    {
        _connection = connection;
        _sessionFactory = sessionFactory;
        _unitOfWork = unitOfWork;
    }

    public IUnitOfWork UnitOfWork => _unitOfWork;

    public static async Task<CraftCasterTestDb> CreateAsync()
    {
        // 1. Configuracion NHibernate (dialecto/driver SQLite de hibernate.cfg.xml)
        var configuration = new Configuration();
        var configPath = Path.Combine(AppContext.BaseDirectory, "hibernate.cfg.xml");
        if (File.Exists(configPath))
        {
            configuration.Configure(configPath);
        }
        else
        {
            configuration.Configure();
        }

        configuration.SetProperty(NHibernate.Cfg.Environment.ConnectionString, ConnectionString);
        configuration.SetProperty(NHibernate.Cfg.Environment.ShowSql, "false");
        configuration.SetProperty("hbm2ddl.keywords", "none");
        configuration.AddAssembly(typeof(UnitOfWorkFactory).Assembly);

        // 2. BD en memoria: la conexion debe seguir abierta o se pierden los datos
        var connection = new SQLiteConnection(ConnectionString);
        await connection.OpenAsync();
        new SchemaExport(configuration).Execute(false, true, false, connection, null);

        // 3. Infraestructura: sesion sobre esa conexion y UnitOfWork real
        ISessionFactory sessionFactory = configuration.BuildSessionFactory();
        ISession session = sessionFactory.WithOptions().Connection(connection).OpenSession();
        var unitOfWork = new HariaUnitOfWork(session);

        return new CraftCasterTestDb(connection, sessionFactory, unitOfWork);
    }

    /// <summary>
    /// Ejecuta una consulta en una sesion NUEVA (sin la cache de la sesion principal)
    /// para comprobar lo que realmente se ha persistido en la base de datos.
    /// </summary>
    public T Verify<T>(Func<ISession, T> consulta)
    {
        using ISession session = _sessionFactory.WithOptions().Connection(_connection).OpenSession();
        return consulta(session);
    }

    public int ContarCartas(int jugadorId, TipoZona tipoZona) =>
        Verify(s => s.Query<CartaPartida>().Count(c => c.Zona.Jugador.Id == jugadorId && c.Zona.TipoZona == tipoZona));

    public TipoZona ZonaDe(int cartaPartidaId) =>
        Verify(s => s.Get<CartaPartida>(cartaPartidaId).Zona.TipoZona);

    public int Contar<TEntity>() where TEntity : class =>
        Verify(s => s.Query<TEntity>().Count());

    // ------------------------------------------------------------------
    // Constructores de datos
    // ------------------------------------------------------------------

    public async Task<Usuario> CrearUsuarioAsync(string? nombre = null)
    {
        string nombreFinal = nombre ?? $"usuario{++_secuencia}";
        var usuario = new Usuario
        {
            NombreUsuario = nombreFinal,
            Email = $"{nombreFinal}@test.com",
            Contrasenia = "hash",
            FotoPerfil = 1,
            EstadoCuenta = EstadoCuenta.Activa,
            TipoUsuario = TipoUsuario.Cliente,
        };
        await UnitOfWork.UsuarioRepository.SaveAsync(usuario);
        return usuario;
    }

    public async Task<Carta> CrearCartaAsync(
        Usuario usuario,
        string nombre,
        TipoCarta tipo = TipoCarta.Criatura,
        int coste = 1,
        int ataque = 1,
        int vida = 1,
        Rareza rareza = Rareza.Comun,
        bool publica = true)
    {
        var carta = new Carta
        {
            Nombre = nombre,
            Rareza = rareza,
            CosteMana = coste,
            Tipo = tipo,
            Ataque = tipo == TipoCarta.Criatura ? ataque : null,
            Vida = tipo == TipoCarta.Criatura ? vida : null,
            Descripcion = "Carta de prueba",
            Imagen = 1,
            EsPublica = publica,
            Usuario = usuario,
        };
        await UnitOfWork.CartaRepository.SaveAsync(carta);
        usuario.Cartas.Add(carta);
        return carta;
    }

    public async Task<Mazo> CrearMazoAsync(
        Usuario usuario,
        string nombre = "Mazo",
        bool publico = true,
        float costeMedio = 0f,
        float valoracion = 0f,
        EstadoMazo estado = EstadoMazo.Jugable)
    {
        var mazo = new Mazo
        {
            Nombre = nombre,
            EsPublico = publico,
            Imagen = 1,
            CosteMedio = costeMedio,
            NumCartas = 0,
            Valoracion = valoracion,
            EstadoMazo = estado,
            Usuario = usuario,
        };
        await UnitOfWork.MazoRepository.SaveAsync(mazo);
        usuario.Mazos.Add(mazo);
        return mazo;
    }

    public async Task<LineaMazo> AnyadirLineaAsync(Mazo mazo, Carta carta, int cantidad)
    {
        var linea = new LineaMazo { Cantidad = cantidad, Mazo = mazo, Carta = carta };
        await UnitOfWork.LineaMazoRepository.SaveAsync(linea);
        mazo.Lineas.Add(linea);
        return linea;
    }

    public async Task<Partida> CrearPartidaAsync(
        Usuario local,
        Mazo mazoLocal,
        EstadoPartida estado = EstadoPartida.EnCurso,
        Usuario? visitante = null,
        Mazo? mazoVisitante = null)
    {
        var partida = new Partida
        {
            Estado = estado,
            FechaInicio = new DateTime(2026, 11, 1),
            Usuario1 = local,
            Mazo1 = mazoLocal,
            Usuario2 = visitante,
            Mazo2 = mazoVisitante,
        };
        await UnitOfWork.PartidaRepository.SaveAsync(partida);

        local.PartidasLocal.Add(partida);
        mazoLocal.PartidasLocal.Add(partida);
        visitante?.PartidasVisitante.Add(partida);
        mazoVisitante?.PartidasVisitante.Add(partida);
        return partida;
    }

    /// <summary>Crea el jugador con sus 4 zonas (Mano, Campo, Descarte y Mazo).</summary>
    public async Task<JugadorPartida> CrearJugadorAsync(
        Partida partida,
        int numJugador,
        int vida = 30,
        int mana = 0,
        bool activo = false)
    {
        var jugador = new JugadorPartida
        {
            NumJugador = numJugador,
            Vida = vida,
            Mana = mana,
            Activo = activo,
            Partida = partida,
        };
        await UnitOfWork.JugadorPartidaRepository.SaveAsync(jugador);
        partida.Jugadores.Add(jugador);

        foreach (TipoZona tipo in new[] { TipoZona.Mano, TipoZona.Campo, TipoZona.Descarte, TipoZona.Mazo })
        {
            var zona = new ZonaPartida { TipoZona = tipo, Jugador = jugador };
            await UnitOfWork.ZonaPartidaRepository.SaveAsync(zona);
            jugador.Zonas.Add(zona);
        }

        return jugador;
    }

    public static ZonaPartida Zona(JugadorPartida jugador, TipoZona tipoZona) =>
        jugador.Zonas.First(z => z.TipoZona == tipoZona);

    public async Task<CartaPartida> PonerCartaAsync(
        JugadorPartida jugador,
        TipoZona tipoZona,
        Carta original,
        bool puedeAtacar = false)
    {
        ZonaPartida zona = Zona(jugador, tipoZona);
        var carta = new CartaPartida
        {
            VidaActual = original.Vida ?? 0,
            AtaqueActual = original.Ataque ?? 0,
            PuedeAtacar = puedeAtacar,
            Original = original,
            Zona = zona,
        };
        await UnitOfWork.CartaPartidaRepository.SaveAsync(carta);
        zona.Cartas.Add(carta);
        return carta;
    }

    public async Task<Turno> CrearTurnoAsync(Partida partida, JugadorPartida jugador, int numTurno, int manaTurno)
    {
        var turno = new Turno { NumTurno = numTurno, ManaTurno = manaTurno, Partida = partida, Jugador = jugador };
        await UnitOfWork.TurnoRepository.SaveAsync(turno);
        partida.Turnos.Add(turno);
        return turno;
    }

    /// <summary>Crea un Estado sobre la carta y una Habilidad dentro de ese estado.</summary>
    public async Task<Habilidad> CrearHabilidadAsync(
        Carta carta,
        TipoHabilidad tipo,
        int cantidad,
        TipoObjetivo tipoObjetivo,
        int objetivo,
        TipoEstado tipoEstado = TipoEstado.Entrar)
    {
        var estado = new Estado { Tipo = tipoEstado, Carta = carta };
        await UnitOfWork.EstadoRepository.SaveAsync(estado);
        carta.Estados.Add(estado);

        var habilidad = new Habilidad
        {
            Tipo = tipo,
            Cantidad = cantidad,
            TipoObjetivo = tipoObjetivo,
            Objetivo = objetivo,
            Estado = estado,
        };
        await UnitOfWork.HabilidadRepository.SaveAsync(habilidad);
        estado.Habilidades.Add(habilidad);
        return habilidad;
    }

    /// <summary>
    /// Partida EnCurso con dos jugadores (Jugador1 = local, Jugador2 = visitante),
    /// cada uno con <paramref name="cartasEnMazo"/> criaturas en su zona Mazo.
    /// </summary>
    public async Task<PartidaEnJuego> CrearPartidaEnJuegoAsync(
        bool jugador1Activo = false,
        int manaJugador1 = 0,
        int cartasEnMazo = 3)
    {
        Usuario local = await CrearUsuarioAsync("local");
        Usuario visitante = await CrearUsuarioAsync("visitante");
        Carta criatura = await CrearCartaAsync(local, "Golem", TipoCarta.Criatura, coste: 2, ataque: 2, vida: 3);
        Carta hechizo = await CrearCartaAsync(local, "Bola de fuego", TipoCarta.Hechizo, coste: 1);
        Mazo mazoLocal = await CrearMazoAsync(local, "Mazo local");
        Mazo mazoVisitante = await CrearMazoAsync(visitante, "Mazo visitante");

        Partida partida = await CrearPartidaAsync(local, mazoLocal, EstadoPartida.EnCurso, visitante, mazoVisitante);
        JugadorPartida jugador1 = await CrearJugadorAsync(partida, 1, mana: manaJugador1, activo: jugador1Activo);
        JugadorPartida jugador2 = await CrearJugadorAsync(partida, 2);

        for (int i = 0; i < cartasEnMazo; i++)
        {
            await PonerCartaAsync(jugador1, TipoZona.Mazo, criatura);
            await PonerCartaAsync(jugador2, TipoZona.Mazo, criatura);
        }

        return new PartidaEnJuego(local, visitante, mazoLocal, mazoVisitante, partida, jugador1, jugador2, criatura, hechizo);
    }

    public async ValueTask DisposeAsync()
    {
        await _unitOfWork.DisposeAsync();
        _sessionFactory.Dispose();
        await _connection.DisposeAsync();
    }
}
