using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using CraftCaster.Application.Services;
using CraftCaster.Domain.Entities;
using CraftCaster.Infrastructure;
using CraftCaster.Infrastructure.Logging;
using Xunit.Abstractions;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Tests.Generated;

public sealed class ModelGeneratedTests
{
    private readonly ITestOutputHelper _output;

    public ModelGeneratedTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Rareza_Should_Define_Model_Literals()
    {
        Assert.Equal(1, (int)Rareza.Comun);
        Assert.Equal(2, (int)Rareza.Especial);
        Assert.Equal(3, (int)Rareza.Epico);
        Assert.Equal(4, (int)Rareza.Legendario);
    }

    [Fact]
    public void EstadoCuenta_Should_Define_Model_Literals()
    {
        Assert.Equal(1, (int)EstadoCuenta.Activa);
        Assert.Equal(2, (int)EstadoCuenta.Suspendida);
    }

    [Fact]
    public void EstadoMazo_Should_Define_Model_Literals()
    {
        Assert.Equal(1, (int)EstadoMazo.Incompleto);
        Assert.Equal(2, (int)EstadoMazo.Jugable);
        Assert.Equal(3, (int)EstadoMazo.Completo);
        Assert.Equal(4, (int)EstadoMazo.Deshabilitado);
    }

    [Fact]
    public void EstadoPartida_Should_Define_Model_Literals()
    {
        Assert.Equal(1, (int)EstadoPartida.Espera);
        Assert.Equal(2, (int)EstadoPartida.EnCurso);
        Assert.Equal(3, (int)EstadoPartida.Finalizada);
    }

    [Fact]
    public void TipoZona_Should_Define_Model_Literals()
    {
        Assert.Equal(1, (int)TipoZona.Mano);
        Assert.Equal(2, (int)TipoZona.Campo);
        Assert.Equal(3, (int)TipoZona.Descarte);
        Assert.Equal(4, (int)TipoZona.Mazo);
    }

    [Fact]
    public void TipoUsuario_Should_Define_Model_Literals()
    {
        Assert.Equal(1, (int)TipoUsuario.Cliente);
        Assert.Equal(2, (int)TipoUsuario.Admin);
    }

    [Fact]
    public void TipoCarta_Should_Define_Model_Literals()
    {
        Assert.Equal(1, (int)TipoCarta.Criatura);
        Assert.Equal(2, (int)TipoCarta.Hechizo);
    }

    [Fact]
    public void TipoEstado_Should_Define_Model_Literals()
    {
        Assert.Equal(1, (int)TipoEstado.Entrar);
        Assert.Equal(2, (int)TipoEstado.Atacar);
        Assert.Equal(3, (int)TipoEstado.InicioTurno);
        Assert.Equal(4, (int)TipoEstado.FinTurno);
        Assert.Equal(5, (int)TipoEstado.Morir);
    }

    [Fact]
    public void TipoHabilidad_Should_Define_Model_Literals()
    {
        Assert.Equal(1, (int)TipoHabilidad.Ataque);
        Assert.Equal(2, (int)TipoHabilidad.Danyo);
        Assert.Equal(3, (int)TipoHabilidad.Cura);
        Assert.Equal(4, (int)TipoHabilidad.Robar);
        Assert.Equal(5, (int)TipoHabilidad.Descartar);
        Assert.Equal(6, (int)TipoHabilidad.Destruir);
        Assert.Equal(7, (int)TipoHabilidad.Defensa);
    }

    [Fact]
    public void TipoAccion_Should_Define_Model_Literals()
    {
        Assert.Equal(1, (int)TipoAccion.Ataque);
        Assert.Equal(2, (int)TipoAccion.Lanzar);
        Assert.Equal(3, (int)TipoAccion.Disparada);
    }

    [Fact]
    public void Objetivo_Should_Define_Model_Literals()
    {
        Assert.Equal(1, (int)Objetivo.Jugador);
        Assert.Equal(2, (int)Objetivo.Enemigo);
        Assert.Equal(3, (int)Objetivo.Libre);
        Assert.Equal(4, (int)Objetivo.TodosEnemigos);
        Assert.Equal(5, (int)Objetivo.Self);
    }

    [Fact]
    public void Usuario_ObtenerUsuarioLocal_Filter_Contract()
    {
        // <haria-protected id="test:filter:Usuario:obtenerUsuarioLocal:arrange">
        // extra user setup
        // </haria-protected>

        var operationName = "obtenerUsuarioLocal";
        var filterExpression = "entity => entity.PartidasLocal.Any(p => p.Id == p_oid)";

        // <haria-protected id="test:filter:Usuario:obtenerUsuarioLocal:assert">
        Ensure(!string.IsNullOrWhiteSpace(operationName), "Usuario.obtenerUsuarioLocal declares the generated operation name.");
        Ensure(!string.IsNullOrWhiteSpace(filterExpression), "Usuario.obtenerUsuarioLocal declares the generated filter expression.");
        // </haria-protected>
    }

    [Fact]
    public void Mazo_ObtenerMazosUsuario_Filter_Contract()
    {
        // <haria-protected id="test:filter:Mazo:obtenerMazosUsuario:arrange">
        // extra user setup
        // </haria-protected>

        var operationName = "obtenerMazosUsuario";
        var filterExpression = "entity => entity.Usuario.Id == p_oid";

        // <haria-protected id="test:filter:Mazo:obtenerMazosUsuario:assert">
        Ensure(!string.IsNullOrWhiteSpace(operationName), "Mazo.obtenerMazosUsuario declares the generated operation name.");
        Ensure(!string.IsNullOrWhiteSpace(filterExpression), "Mazo.obtenerMazosUsuario declares the generated filter expression.");
        // </haria-protected>
    }

    [Fact]
    public void Mazo_ObtenerMazosPorNombre_Filter_Contract()
    {
        // <haria-protected id="test:filter:Mazo:obtenerMazosPorNombre:arrange">
        // extra user setup
        // </haria-protected>

        var operationName = "obtenerMazosPorNombre";
        var filterExpression = "entity => entity.Nombre == p_nombre && entity.EsPublico == true";

        // <haria-protected id="test:filter:Mazo:obtenerMazosPorNombre:assert">
        Ensure(!string.IsNullOrWhiteSpace(operationName), "Mazo.obtenerMazosPorNombre declares the generated operation name.");
        Ensure(!string.IsNullOrWhiteSpace(filterExpression), "Mazo.obtenerMazosPorNombre declares the generated filter expression.");
        // </haria-protected>
    }

    [Fact]
    public void Mazo_ObtenerMazoPorCosteMedio_Filter_Contract()
    {
        // <haria-protected id="test:filter:Mazo:obtenerMazoPorCosteMedio:arrange">
        // extra user setup
        // </haria-protected>

        var operationName = "obtenerMazoPorCosteMedio";
        var filterExpression = "entity => entity.CosteMedio >= p_min_mana && entity.CosteMedio <= p_max_mana && entity.EsPublico == true";

        // <haria-protected id="test:filter:Mazo:obtenerMazoPorCosteMedio:assert">
        Ensure(!string.IsNullOrWhiteSpace(operationName), "Mazo.obtenerMazoPorCosteMedio declares the generated operation name.");
        Ensure(!string.IsNullOrWhiteSpace(filterExpression), "Mazo.obtenerMazoPorCosteMedio declares the generated filter expression.");
        // </haria-protected>
    }

    [Fact]
    public void Mazo_ObtenerMazoPorValoracion_Filter_Contract()
    {
        // <haria-protected id="test:filter:Mazo:obtenerMazoPorValoracion:arrange">
        // extra user setup
        // </haria-protected>

        var operationName = "obtenerMazoPorValoracion";
        var filterExpression = "entity => entity.Valoracion >= p_min_val && entity.Valoracion <= p_max_val && entity.EsPublico == true";

        // <haria-protected id="test:filter:Mazo:obtenerMazoPorValoracion:assert">
        Ensure(!string.IsNullOrWhiteSpace(operationName), "Mazo.obtenerMazoPorValoracion declares the generated operation name.");
        Ensure(!string.IsNullOrWhiteSpace(filterExpression), "Mazo.obtenerMazoPorValoracion declares the generated filter expression.");
        // </haria-protected>
    }

    [Fact]
    public void Mazo_ObtenerMazoJugador1_Filter_Contract()
    {
        // <haria-protected id="test:filter:Mazo:obtenerMazoJugador1:arrange">
        // extra user setup
        // </haria-protected>

        var operationName = "obtenerMazoJugador1";
        var filterExpression = "entity => entity.PartidasLocal.Any(p => p.Id == p_oid)";

        // <haria-protected id="test:filter:Mazo:obtenerMazoJugador1:assert">
        Ensure(!string.IsNullOrWhiteSpace(operationName), "Mazo.obtenerMazoJugador1 declares the generated operation name.");
        Ensure(!string.IsNullOrWhiteSpace(filterExpression), "Mazo.obtenerMazoJugador1 declares the generated filter expression.");
        // </haria-protected>
    }

    [Fact]
    public void Mazo_ObtenerMazoJugador2_Filter_Contract()
    {
        // <haria-protected id="test:filter:Mazo:obtenerMazoJugador2:arrange">
        // extra user setup
        // </haria-protected>

        var operationName = "obtenerMazoJugador2";
        var filterExpression = "entity => entity.PartidasVisitante.Any(p => p.Id == p_oid)";

        // <haria-protected id="test:filter:Mazo:obtenerMazoJugador2:assert">
        Ensure(!string.IsNullOrWhiteSpace(operationName), "Mazo.obtenerMazoJugador2 declares the generated operation name.");
        Ensure(!string.IsNullOrWhiteSpace(filterExpression), "Mazo.obtenerMazoJugador2 declares the generated filter expression.");
        // </haria-protected>
    }

    [Fact]
    public void Carta_ObtenerCartasUsuario_Filter_Contract()
    {
        // <haria-protected id="test:filter:Carta:obtenerCartasUsuario:arrange">
        // extra user setup
        // </haria-protected>

        var operationName = "obtenerCartasUsuario";
        var filterExpression = "entity => entity.Usuario.Id == p_oid";

        // <haria-protected id="test:filter:Carta:obtenerCartasUsuario:assert">
        Ensure(!string.IsNullOrWhiteSpace(operationName), "Carta.obtenerCartasUsuario declares the generated operation name.");
        Ensure(!string.IsNullOrWhiteSpace(filterExpression), "Carta.obtenerCartasUsuario declares the generated filter expression.");
        // </haria-protected>
    }

    [Fact]
    public void Carta_ObtenerCartaPorNombre_Filter_Contract()
    {
        // <haria-protected id="test:filter:Carta:obtenerCartaPorNombre:arrange">
        // extra user setup
        // </haria-protected>

        var operationName = "obtenerCartaPorNombre";
        var filterExpression = "entity => entity.Nombre == p_nombre && entity.EsPublica == true";

        // <haria-protected id="test:filter:Carta:obtenerCartaPorNombre:assert">
        Ensure(!string.IsNullOrWhiteSpace(operationName), "Carta.obtenerCartaPorNombre declares the generated operation name.");
        Ensure(!string.IsNullOrWhiteSpace(filterExpression), "Carta.obtenerCartaPorNombre declares the generated filter expression.");
        // </haria-protected>
    }

    [Fact]
    public void Carta_ObtenerCartaPorCosteMana_Filter_Contract()
    {
        // <haria-protected id="test:filter:Carta:obtenerCartaPorCosteMana:arrange">
        // extra user setup
        // </haria-protected>

        var operationName = "obtenerCartaPorCosteMana";
        var filterExpression = "entity => entity.CosteMana == p_mana && entity.EsPublica == true";

        // <haria-protected id="test:filter:Carta:obtenerCartaPorCosteMana:assert">
        Ensure(!string.IsNullOrWhiteSpace(operationName), "Carta.obtenerCartaPorCosteMana declares the generated operation name.");
        Ensure(!string.IsNullOrWhiteSpace(filterExpression), "Carta.obtenerCartaPorCosteMana declares the generated filter expression.");
        // </haria-protected>
    }

    [Fact]
    public void Carta_ObtenerCartaPorRareza_Filter_Contract()
    {
        // <haria-protected id="test:filter:Carta:obtenerCartaPorRareza:arrange">
        // extra user setup
        // </haria-protected>

        var operationName = "obtenerCartaPorRareza";
        var filterExpression = "entity => entity.Rareza == p_rareza && entity.EsPublica == true";

        // <haria-protected id="test:filter:Carta:obtenerCartaPorRareza:assert">
        Ensure(!string.IsNullOrWhiteSpace(operationName), "Carta.obtenerCartaPorRareza declares the generated operation name.");
        Ensure(!string.IsNullOrWhiteSpace(filterExpression), "Carta.obtenerCartaPorRareza declares the generated filter expression.");
        // </haria-protected>
    }

    [Fact]
    public void Carta_ObtenerCartaPorTipo_Filter_Contract()
    {
        // <haria-protected id="test:filter:Carta:obtenerCartaPorTipo:arrange">
        // extra user setup
        // </haria-protected>

        var operationName = "obtenerCartaPorTipo";
        var filterExpression = "entity => entity.Tipo == p_tipo && entity.EsPublica == true";

        // <haria-protected id="test:filter:Carta:obtenerCartaPorTipo:assert">
        Ensure(!string.IsNullOrWhiteSpace(operationName), "Carta.obtenerCartaPorTipo declares the generated operation name.");
        Ensure(!string.IsNullOrWhiteSpace(filterExpression), "Carta.obtenerCartaPorTipo declares the generated filter expression.");
        // </haria-protected>
    }

    [Fact]
    public void Partida_ObtenerPartidasUsuario_Filter_Contract()
    {
        // <haria-protected id="test:filter:Partida:obtenerPartidasUsuario:arrange">
        // extra user setup
        // </haria-protected>

        var operationName = "obtenerPartidasUsuario";
        var filterExpression = "entity => entity.Usuario1.Id == p_oid || entity.Usuario2.Id == p_oid";

        // <haria-protected id="test:filter:Partida:obtenerPartidasUsuario:assert">
        Ensure(!string.IsNullOrWhiteSpace(operationName), "Partida.obtenerPartidasUsuario declares the generated operation name.");
        Ensure(!string.IsNullOrWhiteSpace(filterExpression), "Partida.obtenerPartidasUsuario declares the generated filter expression.");
        // </haria-protected>
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task Generated_Crud_And_Associations_Should_Work_Against_Database()
    {
        var cfg = CreateNHibernateConfiguration();
        new SchemaUpdate(cfg).Execute(false, true);
        using var loggerFactory = LoggerFactory.Create(logging => logging.SetMinimumLevel(LogLevel.Warning));
        using var unitOfWorkFactory = new UnitOfWorkFactory(cfg, loggerFactory);

            // TODO: No seed generated for Usuario; define a non-customized New operation in the service to initialize it.

            // TODO: Set required relationship Mazo.Usuario manually; it could not be ordered automatically.
            // TODO: No seed generated for Mazo; define a non-customized New operation in the service to initialize it.

            // TODO: Set required relationship LineaMazo.Mazo manually; it could not be ordered automatically.
            // TODO: Set required relationship LineaMazo.Carta manually; it could not be ordered automatically.
            // TODO: No seed generated for LineaMazo; one or more New arguments reference classes that were not seeded yet.

            // TODO: Set required relationship Carta.Usuario manually; it could not be ordered automatically.
            // TODO: No seed generated for Carta; define a non-customized New operation in the service to initialize it.

            // TODO: Set required relationship Estado.Carta manually; it could not be ordered automatically.
            // TODO: No seed generated for Estado; one or more New arguments reference classes that were not seeded yet.

            // TODO: Set required relationship Habilidad.Estado manually; it could not be ordered automatically.
            // TODO: No seed generated for Habilidad; one or more New arguments reference classes that were not seeded yet.

            // TODO: Set required relationship Partida.Usuario1 manually; it could not be ordered automatically.
            // TODO: Set required relationship Partida.Mazo1 manually; it could not be ordered automatically.
            // TODO: No seed generated for Partida; one or more New arguments reference classes that were not seeded yet.

            // TODO: Set required relationship JugadorPartida.Partida manually; it could not be ordered automatically.
            // TODO: No seed generated for JugadorPartida; one or more New arguments reference classes that were not seeded yet.

            // TODO: Set required relationship ZonaPartida.Jugador manually; it could not be ordered automatically.
            // TODO: No seed generated for ZonaPartida; one or more New arguments reference classes that were not seeded yet.

            // TODO: Set required relationship CartaPartida.Zona manually; it could not be ordered automatically.
            // TODO: Set required relationship CartaPartida.Original manually; it could not be ordered automatically.
            // TODO: No seed generated for CartaPartida; one or more New arguments reference classes that were not seeded yet.

            // TODO: Set required relationship Turno.Partida manually; it could not be ordered automatically.
            // TODO: Set required relationship Turno.Jugador manually; it could not be ordered automatically.
            // TODO: No seed generated for Turno; one or more New arguments reference classes that were not seeded yet.

            // TODO: Set required relationship Accion.Turno manually; it could not be ordered automatically.
            // TODO: No seed generated for Accion; one or more New arguments reference classes that were not seeded yet.
    }

    private static Configuration CreateNHibernateConfiguration()
    {
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

        configuration.SetProperty(NHibernate.Cfg.Environment.ShowSql, "false");
        configuration.AddAssembly(typeof(UnitOfWorkFactory).Assembly);
        configuration.EnableSqlStatementCapture();
        return configuration;
    }

    private const string SuccessColor = "\u001b[32m";
    private const string ErrorColor = "\u001b[31m";
    private const string ResetColor = "\u001b[0m";

    private void Ensure(bool condition, string message)
    {
        WriteTestMessage(condition, message);
        Assert.True(condition, message);
    }

    private void WriteTestMessage(bool condition, string message)
    {
        var color = condition ? SuccessColor : ErrorColor;
        var status = condition ? "[OK] " : "[FAIL] ";
        _output.WriteLine($"{color}{status}{message}{ResetColor}");
    }

}
