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
    public void Mazo_ObtenerMazoPorCreador_Filter_Contract()
    {
        // <haria-protected id="test:filter:Mazo:obtenerMazoPorCreador:arrange">
        // extra user setup
        // </haria-protected>

        var operationName = "obtenerMazoPorCreador";
        var filterExpression = "entity => entity.Creador == p_creador && entity.EsPublico == true";

        // <haria-protected id="test:filter:Mazo:obtenerMazoPorCreador:assert">
        Ensure(!string.IsNullOrWhiteSpace(operationName), "Mazo.obtenerMazoPorCreador declares the generated operation name.");
        Ensure(!string.IsNullOrWhiteSpace(filterExpression), "Mazo.obtenerMazoPorCreador declares the generated filter expression.");
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

            var usuarioService = new UsuarioService(unitOfWorkFactory);
            var sampleUsuario = await usuarioService.NewAsync("Sample Usuario NombreUsuario", "sample1@example.com", "Password123!", 1, 1, 1);
            var sampleUsuarioLoginToken = await usuarioService.LoginAsync(sampleUsuario.Id, "Password123!");
            Ensure(!string.IsNullOrWhiteSpace(sampleUsuarioLoginToken), "Usuario.login returns a login token for sampleUsuario.");
            var sampleUsuarioReadBack = await usuarioService.ReadOIDAsync(sampleUsuario.Id);
            Ensure(sampleUsuarioReadBack is not null, "Usuario read by OID returns the generated entity.");
            var sampleUsuarioReadAll = await usuarioService.ReadAllAsync();
            Ensure(sampleUsuarioReadAll.Count > 0, "Usuario read all returns generated data.");
            var sampleUsuarioObtenerUsuarioLocalResult = await usuarioService.ObtenerUsuarioLocalAsync(default!);
            Ensure(sampleUsuarioObtenerUsuarioLocalResult is not null, "Usuario.obtenerUsuarioLocal filter executes.");
            await usuarioService.ModifyAsync(sampleUsuario.Id, "Sample Usuario NombreUsuario", "sample1@example.com", "Password123!", 1, 1, 1);
            Ensure(await usuarioService.ReadOIDAsync(sampleUsuario.Id) is not null, "Usuario modify keeps the entity readable.");

            var mazoService = new MazoService(unitOfWorkFactory);
            var sampleMazo = await mazoService.NewAsync("Sample Mazo Nombre", 2.5f, 2, EstadoMazo.Incompleto, true, 2.5f, 1, "Sample Mazo Creador", sampleUsuario.Id);
            Ensure(sampleMazo.Usuario?.Equals(sampleUsuario) == true, "Mazo.new relates sampleMazo and sampleUsuario.");
            var sampleMazoReadBack = await mazoService.ReadOIDAsync(sampleMazo.Id);
            Ensure(sampleMazoReadBack is not null, "Mazo read by OID returns the generated entity.");
            var sampleMazoReadAll = await mazoService.ReadAllAsync();
            Ensure(sampleMazoReadAll.Count > 0, "Mazo read all returns generated data.");
            var sampleMazoObtenerMazosUsuarioResult = await mazoService.ObtenerMazosUsuarioAsync(sampleUsuario.Id);
            Ensure(sampleMazoObtenerMazosUsuarioResult is not null, "Mazo.obtenerMazosUsuario filter executes.");
            var sampleMazoObtenerMazosPorNombreResult = await mazoService.ObtenerMazosPorNombreAsync("Sample Mazo Nombre");
            Ensure(sampleMazoObtenerMazosPorNombreResult is not null, "Mazo.obtenerMazosPorNombre filter executes.");
            var sampleMazoObtenerMazoPorCosteMedioResult = await mazoService.ObtenerMazoPorCosteMedioAsync(2, 2);
            Ensure(sampleMazoObtenerMazoPorCosteMedioResult is not null, "Mazo.obtenerMazoPorCosteMedio filter executes.");
            var sampleMazoObtenerMazoPorValoracionResult = await mazoService.ObtenerMazoPorValoracionAsync(2, 2);
            Ensure(sampleMazoObtenerMazoPorValoracionResult is not null, "Mazo.obtenerMazoPorValoracion filter executes.");
            var sampleMazoObtenerMazoPorCreadorResult = await mazoService.ObtenerMazoPorCreadorAsync("Sample Mazo Creador");
            Ensure(sampleMazoObtenerMazoPorCreadorResult is not null, "Mazo.obtenerMazoPorCreador filter executes.");
            var sampleMazoObtenerMazoJugador1Result = await mazoService.ObtenerMazoJugador1Async(default!);
            Ensure(sampleMazoObtenerMazoJugador1Result is not null, "Mazo.obtenerMazoJugador1 filter executes.");
            var sampleMazoObtenerMazoJugador2Result = await mazoService.ObtenerMazoJugador2Async(default!);
            Ensure(sampleMazoObtenerMazoJugador2Result is not null, "Mazo.obtenerMazoJugador2 filter executes.");
            await mazoService.ModifyAsync(sampleMazo.Id, "Sample Mazo Nombre", 2.5f, 2, EstadoMazo.Incompleto, true, 2.5f, 1, "Sample Mazo Creador", sampleUsuario.Id);
            Ensure(await mazoService.ReadOIDAsync(sampleMazo.Id) is not null, "Mazo modify keeps the entity readable.");

            var estadoService = new EstadoService(unitOfWorkFactory);
            var sampleEstado = await estadoService.NewAsync(TipoEstado.Entrar);
            var sampleEstadoReadBack = await estadoService.ReadOIDAsync(sampleEstado.Id);
            Ensure(sampleEstadoReadBack is not null, "Estado read by OID returns the generated entity.");
            var sampleEstadoReadAll = await estadoService.ReadAllAsync();
            Ensure(sampleEstadoReadAll.Count > 0, "Estado read all returns generated data.");
            await estadoService.ModifyAsync(sampleEstado.Id, TipoEstado.Entrar);
            Ensure(await estadoService.ReadOIDAsync(sampleEstado.Id) is not null, "Estado modify keeps the entity readable.");

            var habilidadService = new HabilidadService(unitOfWorkFactory);
            var sampleHabilidad = await habilidadService.NewAsync(TipoHabilidad.Ataque, 5, Objetivo.Jugador, sampleEstado.Id);
            Ensure(sampleHabilidad.Estado?.Equals(sampleEstado) == true, "Habilidad.new relates sampleHabilidad and sampleEstado.");
            var sampleHabilidadReadBack = await habilidadService.ReadOIDAsync(sampleHabilidad.Id);
            Ensure(sampleHabilidadReadBack is not null, "Habilidad read by OID returns the generated entity.");
            var sampleHabilidadReadAll = await habilidadService.ReadAllAsync();
            Ensure(sampleHabilidadReadAll.Count > 0, "Habilidad read all returns generated data.");
            await habilidadService.ModifyAsync(sampleHabilidad.Id, TipoHabilidad.Ataque, 5, Objetivo.Jugador, sampleEstado.Id);
            Ensure(await habilidadService.ReadOIDAsync(sampleHabilidad.Id) is not null, "Habilidad modify keeps the entity readable.");

            var cartaService = new CartaService(unitOfWorkFactory);
            var sampleCarta = await cartaService.NewAsync("Sample Carta Nombre", Rareza.Comun, 3, TipoCarta.Criatura, "Sample Carta Descripcion", 1, true, sampleUsuario.Id);
            Ensure(sampleCarta.Usuario?.Equals(sampleUsuario) == true, "Carta.new relates sampleCarta and sampleUsuario.");
            var sampleCartaReadBack = await cartaService.ReadOIDAsync(sampleCarta.Id);
            Ensure(sampleCartaReadBack is not null, "Carta read by OID returns the generated entity.");
            var sampleCartaReadAll = await cartaService.ReadAllAsync();
            Ensure(sampleCartaReadAll.Count > 0, "Carta read all returns generated data.");
            var sampleCartaObtenerCartasUsuarioResult = await cartaService.ObtenerCartasUsuarioAsync(sampleUsuario.Id);
            Ensure(sampleCartaObtenerCartasUsuarioResult is not null, "Carta.obtenerCartasUsuario filter executes.");
            var sampleCartaObtenerCartaPorNombreResult = await cartaService.ObtenerCartaPorNombreAsync("Sample Carta Nombre");
            Ensure(sampleCartaObtenerCartaPorNombreResult is not null, "Carta.obtenerCartaPorNombre filter executes.");
            var sampleCartaObtenerCartaPorCosteManaResult = await cartaService.ObtenerCartaPorCosteManaAsync(3);
            Ensure(sampleCartaObtenerCartaPorCosteManaResult is not null, "Carta.obtenerCartaPorCosteMana filter executes.");
            var sampleCartaObtenerCartaPorRarezaResult = await cartaService.ObtenerCartaPorRarezaAsync(Rareza.Comun);
            Ensure(sampleCartaObtenerCartaPorRarezaResult is not null, "Carta.obtenerCartaPorRareza filter executes.");
            var sampleCartaObtenerCartaPorTipoResult = await cartaService.ObtenerCartaPorTipoAsync(TipoCarta.Criatura);
            Ensure(sampleCartaObtenerCartaPorTipoResult is not null, "Carta.obtenerCartaPorTipo filter executes.");
            await cartaService.ModifyAsync(sampleCarta.Id, "Sample Carta Nombre", Rareza.Comun, 3, TipoCarta.Criatura, "Sample Carta Descripcion", 1, true, sampleUsuario.Id);
            Ensure(await cartaService.ReadOIDAsync(sampleCarta.Id) is not null, "Carta modify keeps the entity readable.");

            var partidaService = new PartidaService(unitOfWorkFactory);
            var samplePartida = await partidaService.NewAsync(EstadoPartida.Espera, sampleUsuario.Id, sampleMazo.Id);
            Ensure(samplePartida.Usuario1?.Equals(sampleUsuario) == true, "Partida.new relates samplePartida and sampleUsuario.");
            Ensure(samplePartida.Mazo1?.Equals(sampleMazo) == true, "Partida.new relates samplePartida and sampleMazo.");
            var samplePartidaReadBack = await partidaService.ReadOIDAsync(samplePartida.Id);
            Ensure(samplePartidaReadBack is not null, "Partida read by OID returns the generated entity.");
            var samplePartidaReadAll = await partidaService.ReadAllAsync();
            Ensure(samplePartidaReadAll.Count > 0, "Partida read all returns generated data.");
            var samplePartidaObtenerPartidasUsuarioResult = await partidaService.ObtenerPartidasUsuarioAsync(sampleUsuario.Id);
            Ensure(samplePartidaObtenerPartidasUsuarioResult is not null, "Partida.obtenerPartidasUsuario filter executes.");
            await partidaService.ModifyAsync(samplePartida.Id, EstadoPartida.Espera, sampleUsuario.Id, sampleMazo.Id);
            Ensure(await partidaService.ReadOIDAsync(samplePartida.Id) is not null, "Partida modify keeps the entity readable.");

            var jugadorPartidaService = new JugadorPartidaService(unitOfWorkFactory);
            var sampleJugadorPartida = await jugadorPartidaService.NewAsync(7, 7, samplePartida.Id);
            Ensure(sampleJugadorPartida.Partida?.Equals(samplePartida) == true, "JugadorPartida.new relates sampleJugadorPartida and samplePartida.");
            var sampleJugadorPartidaReadBack = await jugadorPartidaService.ReadOIDAsync(sampleJugadorPartida.Id);
            Ensure(sampleJugadorPartidaReadBack is not null, "JugadorPartida read by OID returns the generated entity.");
            var sampleJugadorPartidaReadAll = await jugadorPartidaService.ReadAllAsync();
            Ensure(sampleJugadorPartidaReadAll.Count > 0, "JugadorPartida read all returns generated data.");
            await jugadorPartidaService.ModifyAsync(sampleJugadorPartida.Id, 7, 7, samplePartida.Id);
            Ensure(await jugadorPartidaService.ReadOIDAsync(sampleJugadorPartida.Id) is not null, "JugadorPartida modify keeps the entity readable.");

            var zonaPartidaService = new ZonaPartidaService(unitOfWorkFactory);
            var sampleZonaPartida = await zonaPartidaService.NewAsync(TipoZona.Mano, sampleJugadorPartida.Id);
            Ensure(sampleZonaPartida.Jugador?.Equals(sampleJugadorPartida) == true, "ZonaPartida.new relates sampleZonaPartida and sampleJugadorPartida.");
            var sampleZonaPartidaReadBack = await zonaPartidaService.ReadOIDAsync(sampleZonaPartida.Id);
            Ensure(sampleZonaPartidaReadBack is not null, "ZonaPartida read by OID returns the generated entity.");
            var sampleZonaPartidaReadAll = await zonaPartidaService.ReadAllAsync();
            Ensure(sampleZonaPartidaReadAll.Count > 0, "ZonaPartida read all returns generated data.");
            await zonaPartidaService.ModifyAsync(sampleZonaPartida.Id, TipoZona.Mano, sampleJugadorPartida.Id);
            Ensure(await zonaPartidaService.ReadOIDAsync(sampleZonaPartida.Id) is not null, "ZonaPartida modify keeps the entity readable.");

            var cartaPartidaService = new CartaPartidaService(unitOfWorkFactory);
            var sampleCartaPartida = await cartaPartidaService.NewAsync(sampleZonaPartida.Id, sampleCarta.Id);
            Ensure(sampleCartaPartida.Zona?.Equals(sampleZonaPartida) == true, "CartaPartida.new relates sampleCartaPartida and sampleZonaPartida.");
            Ensure(sampleCartaPartida.Original?.Equals(sampleCarta) == true, "CartaPartida.new relates sampleCartaPartida and sampleCarta.");
            var sampleCartaPartidaReadBack = await cartaPartidaService.ReadOIDAsync(sampleCartaPartida.Id);
            Ensure(sampleCartaPartidaReadBack is not null, "CartaPartida read by OID returns the generated entity.");
            var sampleCartaPartidaReadAll = await cartaPartidaService.ReadAllAsync();
            Ensure(sampleCartaPartidaReadAll.Count > 0, "CartaPartida read all returns generated data.");
            await cartaPartidaService.ModifyAsync(sampleCartaPartida.Id, sampleZonaPartida.Id, sampleCarta.Id);
            Ensure(await cartaPartidaService.ReadOIDAsync(sampleCartaPartida.Id) is not null, "CartaPartida modify keeps the entity readable.");

            var turnoService = new TurnoService(unitOfWorkFactory);
            var sampleTurno = await turnoService.NewAsync(10, samplePartida.Id, sampleJugadorPartida.Id);
            Ensure(sampleTurno.Partida?.Equals(samplePartida) == true, "Turno.new relates sampleTurno and samplePartida.");
            Ensure(sampleTurno.Jugador?.Equals(sampleJugadorPartida) == true, "Turno.new relates sampleTurno and sampleJugadorPartida.");
            var sampleTurnoReadBack = await turnoService.ReadOIDAsync(sampleTurno.Id);
            Ensure(sampleTurnoReadBack is not null, "Turno read by OID returns the generated entity.");
            var sampleTurnoReadAll = await turnoService.ReadAllAsync();
            Ensure(sampleTurnoReadAll.Count > 0, "Turno read all returns generated data.");
            await turnoService.ModifyAsync(sampleTurno.Id, 10, samplePartida.Id, sampleJugadorPartida.Id);
            Ensure(await turnoService.ReadOIDAsync(sampleTurno.Id) is not null, "Turno modify keeps the entity readable.");

            var accionService = new AccionService(unitOfWorkFactory);
            var sampleAccion = await accionService.NewAsync(TipoAccion.Ataque, sampleTurno.Id);
            Ensure(sampleAccion.Turno?.Equals(sampleTurno) == true, "Accion.new relates sampleAccion and sampleTurno.");
            var sampleAccionReadBack = await accionService.ReadOIDAsync(sampleAccion.Id);
            Ensure(sampleAccionReadBack is not null, "Accion read by OID returns the generated entity.");
            var sampleAccionReadAll = await accionService.ReadAllAsync();
            Ensure(sampleAccionReadAll.Count > 0, "Accion read all returns generated data.");
            await accionService.ModifyAsync(sampleAccion.Id, TipoAccion.Ataque, sampleTurno.Id);
            Ensure(await accionService.ReadOIDAsync(sampleAccion.Id) is not null, "Accion modify keeps the entity readable.");

            await usuarioService.AddCartaAsync(sampleUsuario.Id, new[] { sampleCarta.Id });
            Ensure(sampleUsuario.Cartas.Contains(sampleCarta), "Usuario.addCarta relates Usuario and Carta.");
            await usuarioService.AddMazoAsync(sampleUsuario.Id, new[] { sampleMazo.Id });
            Ensure(sampleUsuario.Mazos.Contains(sampleMazo), "Usuario.addMazo relates Usuario and Mazo.");
            await usuarioService.AddPartidasLocalAsync(sampleUsuario.Id, new[] { samplePartida.Id });
            Ensure(sampleUsuario.PartidasLocal.Contains(samplePartida), "Usuario.addPartidasLocal relates Usuario and Partida.");
            await mazoService.AddCartaAsync(sampleMazo.Id, new[] { sampleCarta.Id });
            Ensure(sampleMazo.Cartas.Contains(sampleCarta), "Mazo.addCarta relates Mazo and Carta.");
            await estadoService.AddHabilidadeAsync(sampleEstado.Id, new[] { sampleHabilidad.Id });
            Ensure(sampleEstado.Habilidades.Contains(sampleHabilidad), "Estado.addHabilidade relates Estado and Habilidad.");
            await partidaService.AddTurnoAsync(samplePartida.Id, new[] { sampleTurno.Id });
            Ensure(samplePartida.Turnos.Contains(sampleTurno), "Partida.addTurno relates Partida and Turno.");
            await partidaService.AddJugadoreAsync(samplePartida.Id, new[] { sampleJugadorPartida.Id });
            Ensure(samplePartida.Jugadores.Contains(sampleJugadorPartida), "Partida.addJugadore relates Partida and JugadorPartida.");
            await jugadorPartidaService.AddZonaAsync(sampleJugadorPartida.Id, new[] { sampleZonaPartida.Id });
            Ensure(sampleJugadorPartida.Zonas.Contains(sampleZonaPartida), "JugadorPartida.addZona relates JugadorPartida and ZonaPartida.");
            await zonaPartidaService.AddCartaAsync(sampleZonaPartida.Id, new[] { sampleCartaPartida.Id });
            Ensure(sampleZonaPartida.Cartas.Contains(sampleCartaPartida), "ZonaPartida.addCarta relates ZonaPartida and CartaPartida.");
            await turnoService.AddAccioneAsync(sampleTurno.Id, new[] { sampleAccion.Id });
            Ensure(sampleTurno.Acciones.Contains(sampleAccion), "Turno.addAccione relates Turno and Accion.");
            await accionService.AddOrigenAsync(sampleAccion.Id, sampleCartaPartida.Id);
            Ensure(sampleAccion.Origen?.Equals(sampleCartaPartida) == true, "Accion.addOrigen relates Accion and CartaPartida.");
            await accionService.AddObjetivoAsync(sampleAccion.Id, new[] { sampleCartaPartida.Id });
            Ensure(sampleAccion.Objetivo.Contains(sampleCartaPartida), "Accion.addObjetivo relates Accion and CartaPartida.");

            await usuarioService.RemoveCartaAsync(sampleUsuario.Id, new[] { sampleCarta.Id });
            Ensure(!sampleUsuario.Cartas.Contains(sampleCarta), "Usuario.removeCarta removes the relation between Usuario and Carta.");
            await usuarioService.RemoveMazoAsync(sampleUsuario.Id, new[] { sampleMazo.Id });
            Ensure(!sampleUsuario.Mazos.Contains(sampleMazo), "Usuario.removeMazo removes the relation between Usuario and Mazo.");
            await usuarioService.RemovePartidasLocalAsync(sampleUsuario.Id, new[] { samplePartida.Id });
            Ensure(!sampleUsuario.PartidasLocal.Contains(samplePartida), "Usuario.removePartidasLocal removes the relation between Usuario and Partida.");
            await mazoService.RemoveCartaAsync(sampleMazo.Id, new[] { sampleCarta.Id });
            Ensure(!sampleMazo.Cartas.Contains(sampleCarta), "Mazo.removeCarta removes the relation between Mazo and Carta.");
            await estadoService.RemoveHabilidadeAsync(sampleEstado.Id, new[] { sampleHabilidad.Id });
            Ensure(!sampleEstado.Habilidades.Contains(sampleHabilidad), "Estado.removeHabilidade removes the relation between Estado and Habilidad.");
            await partidaService.RemoveTurnoAsync(samplePartida.Id, new[] { sampleTurno.Id });
            Ensure(!samplePartida.Turnos.Contains(sampleTurno), "Partida.removeTurno removes the relation between Partida and Turno.");
            await partidaService.RemoveJugadoreAsync(samplePartida.Id, new[] { sampleJugadorPartida.Id });
            Ensure(!samplePartida.Jugadores.Contains(sampleJugadorPartida), "Partida.removeJugadore removes the relation between Partida and JugadorPartida.");
            await jugadorPartidaService.RemoveZonaAsync(sampleJugadorPartida.Id, new[] { sampleZonaPartida.Id });
            Ensure(!sampleJugadorPartida.Zonas.Contains(sampleZonaPartida), "JugadorPartida.removeZona removes the relation between JugadorPartida and ZonaPartida.");
            await zonaPartidaService.RemoveCartaAsync(sampleZonaPartida.Id, new[] { sampleCartaPartida.Id });
            Ensure(!sampleZonaPartida.Cartas.Contains(sampleCartaPartida), "ZonaPartida.removeCarta removes the relation between ZonaPartida and CartaPartida.");
            await turnoService.RemoveAccioneAsync(sampleTurno.Id, new[] { sampleAccion.Id });
            Ensure(!sampleTurno.Acciones.Contains(sampleAccion), "Turno.removeAccione removes the relation between Turno and Accion.");
            await accionService.RemoveOrigenAsync(sampleAccion.Id, sampleCartaPartida.Id);
            Ensure(sampleAccion.Origen is null, "Accion.removeOrigen removes the relation between Accion and CartaPartida.");
            await accionService.RemoveObjetivoAsync(sampleAccion.Id, new[] { sampleCartaPartida.Id });
            Ensure(!sampleAccion.Objetivo.Contains(sampleCartaPartida), "Accion.removeObjetivo removes the relation between Accion and CartaPartida.");

            await habilidadService.DestroyAsync(sampleHabilidad.Id);
            await partidaService.DestroyAsync(samplePartida.Id);
            await accionService.DestroyAsync(sampleAccion.Id);
            await cartaPartidaService.DestroyAsync(sampleCartaPartida.Id);
            await cartaService.DestroyAsync(sampleCarta.Id);
            await mazoService.DestroyAsync(sampleMazo.Id);
            await usuarioService.DestroyAsync(sampleUsuario.Id);
            await estadoService.DestroyAsync(sampleEstado.Id);
            await zonaPartidaService.DestroyAsync(sampleZonaPartida.Id);
            await turnoService.DestroyAsync(sampleTurno.Id);
            await jugadorPartidaService.DestroyAsync(sampleJugadorPartida.Id);

            Ensure((await usuarioService.ReadAllAsync()).Count == 0, "Usuario seed cleanup completed.");
            Ensure((await mazoService.ReadAllAsync()).Count == 0, "Mazo seed cleanup completed.");
            Ensure((await estadoService.ReadAllAsync()).Count == 0, "Estado seed cleanup completed.");
            Ensure((await habilidadService.ReadAllAsync()).Count == 0, "Habilidad seed cleanup completed.");
            Ensure((await cartaService.ReadAllAsync()).Count == 0, "Carta seed cleanup completed.");
            Ensure((await partidaService.ReadAllAsync()).Count == 0, "Partida seed cleanup completed.");
            Ensure((await jugadorPartidaService.ReadAllAsync()).Count == 0, "JugadorPartida seed cleanup completed.");
            Ensure((await zonaPartidaService.ReadAllAsync()).Count == 0, "ZonaPartida seed cleanup completed.");
            Ensure((await cartaPartidaService.ReadAllAsync()).Count == 0, "CartaPartida seed cleanup completed.");
            Ensure((await turnoService.ReadAllAsync()).Count == 0, "Turno seed cleanup completed.");
            Ensure((await accionService.ReadAllAsync()).Count == 0, "Accion seed cleanup completed.");
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
