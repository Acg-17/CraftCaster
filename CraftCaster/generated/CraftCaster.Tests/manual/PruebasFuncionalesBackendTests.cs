using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using Xunit;
using Xunit.Abstractions;
using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Application.UseCases.ZonaPartida;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;
using CraftCaster.Infrastructure;

namespace CraftCaster.Tests.Manual;

/// <summary>
/// Batería de pruebas funcionales para validar la lógica de negocio real
/// de operaciones Custom (Servicios), CustomTransaction (UseCases) y ReadFilters.
/// </summary>
public sealed class PruebasFuncionalesBackendTests
{
    private readonly ITestOutputHelper _output;

    public PruebasFuncionalesBackendTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private static (Configuration, IUnitOfWorkFactory) CreateTestFactory()
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
        new SchemaUpdate(configuration).Execute(false, true);

        var loggerFactory = LoggerFactory.Create(logging => logging.SetMinimumLevel(LogLevel.Warning));
        var factory = new UnitOfWorkFactory(configuration, loggerFactory);
        return (configuration, factory);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task Custom_JugadorPartida_ActualizarVida_Y_Mana_Debe_Modificar_Valores_Correctamente()
    {
        var (_, factory) = CreateTestFactory();
        await using var uow = factory.Create();

        // Arrange: Crear usuario, mazo, partida y jugador
        var usuario = new Usuario
        {
            NombreUsuario = "JugadorTest",
            Email = "jugador@test.com",
            Contrasenia = "hash_1234",
            FotoPerfil = 1,
            EstadoCuenta = EstadoCuenta.Activa,
            TipoUsuario = TipoUsuario.Cliente
        };
        await uow.UsuarioRepository.SaveAsync(usuario);

        var mazo = new Mazo
        {
            Nombre = "MazoTest",
            EsPublico = true,
            Imagen = 1,
            EstadoMazo = EstadoMazo.Completo,
            CosteMedio = 3.0f,
            NumCartas = 30,
            Valoracion = 4.5f,
            Usuario = usuario
        };
        await uow.MazoRepository.SaveAsync(mazo);

        var partida = new Partida
        {
            Estado = EstadoPartida.EnCurso,
            FechaInicio = DateTime.UtcNow,
            Usuario1 = usuario,
            Mazo1 = mazo
        };
        await uow.PartidaRepository.SaveAsync(partida);

        var jugador = new JugadorPartida
        {
            Vida = 30,
            Mana = 5,
            NumJugador = 1,
            Activo = true,
            Partida = partida
        };
        await uow.JugadorPartidaRepository.SaveAsync(jugador);

        var service = new JugadorPartidaService(uow);

        // Act 1: Aplicar daño (10) -> Vida baja a 20
        await service.ActualizarVidaAsync(jugador.Id, 10);
        var jugadorTrasDanyo = await uow.JugadorPartidaRepository.ReadAsync(jugador.Id);
        Assert.NotNull(jugadorTrasDanyo);
        Assert.Equal(20, jugadorTrasDanyo.Vida);

        // Act 2: Aplicar curación/aumento de vida (-5) -> Vida sube a 25
        await service.ActualizarVidaAsync(jugador.Id, -5);
        var jugadorTrasCura = await uow.JugadorPartidaRepository.ReadAsync(jugador.Id);
        Assert.NotNull(jugadorTrasCura);
        Assert.Equal(25, jugadorTrasCura.Vida);

        // Act 3: Actualizar mana a 9
        await service.ActualizarManaAsync(jugador.Id, 9);
        var jugadorTrasMana = await uow.JugadorPartidaRepository.ReadAsync(jugador.Id);
        Assert.NotNull(jugadorTrasMana);
        Assert.Equal(9, jugadorTrasMana.Mana);

        _output.WriteLine("[OK] ActualizarVida y ActualizarMana probados con exito.");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task Custom_CartaPartida_RecibirDanyo_Debe_Reducir_Vida_Y_Mover_A_Descarte_Al_Morir()
    {
        var (_, factory) = CreateTestFactory();
        await using var uow = factory.Create();

        // Arrange
        var usuario = new Usuario
        {
            NombreUsuario = "UsuarioCartas",
            Email = "cartas@test.com",
            Contrasenia = "hash_5678",
            FotoPerfil = 1,
            EstadoCuenta = EstadoCuenta.Activa,
            TipoUsuario = TipoUsuario.Cliente
        };
        await uow.UsuarioRepository.SaveAsync(usuario);

        var mazo = new Mazo
        {
            Nombre = "MazoCartas",
            EsPublico = true,
            Imagen = 1,
            EstadoMazo = EstadoMazo.Completo,
            CosteMedio = 2.0f,
            NumCartas = 20,
            Valoracion = 4.0f,
            Usuario = usuario
        };
        await uow.MazoRepository.SaveAsync(mazo);

        var partida = new Partida
        {
            Estado = EstadoPartida.EnCurso,
            FechaInicio = DateTime.UtcNow,
            Usuario1 = usuario,
            Mazo1 = mazo
        };
        await uow.PartidaRepository.SaveAsync(partida);

        var jugador = new JugadorPartida
        {
            Vida = 30,
            Mana = 10,
            NumJugador = 1,
            Activo = true,
            Partida = partida,
            Zonas = new List<ZonaPartida>()
        };
        await uow.JugadorPartidaRepository.SaveAsync(jugador);

        var zonaCampo = new ZonaPartida
        {
            TipoZona = TipoZona.Campo,
            Jugador = jugador,
            Cartas = new List<CartaPartida>()
        };
        await uow.ZonaPartidaRepository.SaveAsync(zonaCampo);

        var zonaDescarte = new ZonaPartida
        {
            TipoZona = TipoZona.Descarte,
            Jugador = jugador,
            Cartas = new List<CartaPartida>()
        };
        await uow.ZonaPartidaRepository.SaveAsync(zonaDescarte);

        jugador.Zonas.Add(zonaCampo);
        jugador.Zonas.Add(zonaDescarte);
        await uow.JugadorPartidaRepository.SaveAsync(jugador);

        var cartaOriginal = new Carta
        {
            Nombre = "Dragon Test",
            Rareza = Rareza.Legendario,
            CosteMana = 5,
            Tipo = TipoCarta.Criatura,
            Vida = 10,
            Ataque = 8,
            Descripcion = "Criatura poderosa de prueba",
            Imagen = 1,
            EsPublica = true,
            Usuario = usuario
        };
        await uow.CartaRepository.SaveAsync(cartaOriginal);

        var cartaPartida = new CartaPartida
        {
            VidaActual = 10,
            AtaqueActual = 8,
            PuedeAtacar = true,
            Original = cartaOriginal,
            Zona = zonaCampo
        };
        await uow.CartaPartidaRepository.SaveAsync(cartaPartida);
        zonaCampo.Cartas.Add(cartaPartida);
        await uow.ZonaPartidaRepository.SaveAsync(zonaCampo);

        var service = new CartaPartidaService(uow);

        // Act 1: Daño parcial (4 de daño a carta con 10 de vida)
        await service.RecibirDanyoAsync(cartaPartida.Id, 4);
        var cartaTrasDanyoParcial = await uow.CartaPartidaRepository.ReadAsync(cartaPartida.Id);
        Assert.NotNull(cartaTrasDanyoParcial);
        Assert.Equal(6, cartaTrasDanyoParcial.VidaActual);
        Assert.True(cartaTrasDanyoParcial.PuedeAtacar);

        // Act 2: Daño letal (6 de daño restante) -> Vida llega a 0 y pasa a Descarte
        await service.RecibirDanyoAsync(cartaPartida.Id, 6);
        var cartaMuerta = await uow.CartaPartidaRepository.ReadAsync(cartaPartida.Id);
        Assert.NotNull(cartaMuerta);
        Assert.Equal(0, cartaMuerta.VidaActual);
        Assert.False(cartaMuerta.PuedeAtacar);
        Assert.NotNull(cartaMuerta.Zona);
        Assert.Equal(TipoZona.Descarte, cartaMuerta.Zona.TipoZona);

        _output.WriteLine("[OK] RecibirDanyo probado con exito (daño parcial y muerte con traslado a Descarte).");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task CustomTransaction_ZonaPartida_OperationTransaction_Debe_Mover_Carta_Entre_Zonas()
    {
        var (_, factory) = CreateTestFactory();
        await using var uow = factory.Create();

        // Arrange
        var usuario = new Usuario
        {
            NombreUsuario = "UserTrans",
            Email = "trans@test.com",
            Contrasenia = "hash_9999",
            FotoPerfil = 1,
            EstadoCuenta = EstadoCuenta.Activa,
            TipoUsuario = TipoUsuario.Cliente
        };
        await uow.UsuarioRepository.SaveAsync(usuario);

        var mazo = new Mazo
        {
            Nombre = "MazoTrans",
            EsPublico = true,
            Imagen = 1,
            EstadoMazo = EstadoMazo.Completo,
            CosteMedio = 2.0f,
            NumCartas = 20,
            Valoracion = 3.5f,
            Usuario = usuario
        };
        await uow.MazoRepository.SaveAsync(mazo);

        var partida = new Partida
        {
            Estado = EstadoPartida.EnCurso,
            FechaInicio = DateTime.UtcNow,
            Usuario1 = usuario,
            Mazo1 = mazo
        };
        await uow.PartidaRepository.SaveAsync(partida);

        var jugador = new JugadorPartida
        {
            Vida = 30,
            Mana = 5,
            NumJugador = 1,
            Activo = true,
            Partida = partida
        };
        await uow.JugadorPartidaRepository.SaveAsync(jugador);

        var zonaMano = new ZonaPartida
        {
            TipoZona = TipoZona.Mano,
            Jugador = jugador,
            Cartas = new List<CartaPartida>()
        };
        await uow.ZonaPartidaRepository.SaveAsync(zonaMano);

        var zonaCampo = new ZonaPartida
        {
            TipoZona = TipoZona.Campo,
            Jugador = jugador,
            Cartas = new List<CartaPartida>()
        };
        await uow.ZonaPartidaRepository.SaveAsync(zonaCampo);

        var cartaOriginal = new Carta
        {
            Nombre = "Carta Movible",
            Rareza = Rareza.Comun,
            CosteMana = 1,
            Tipo = TipoCarta.Criatura,
            Vida = 2,
            Ataque = 1,
            Descripcion = "Carta para mover entre zonas",
            Imagen = 1,
            EsPublica = true,
            Usuario = usuario
        };
        await uow.CartaRepository.SaveAsync(cartaOriginal);

        var cartaPartida = new CartaPartida
        {
            VidaActual = 2,
            AtaqueActual = 1,
            PuedeAtacar = false,
            Original = cartaOriginal,
            Zona = zonaMano
        };
        await uow.CartaPartidaRepository.SaveAsync(cartaPartida);
        zonaMano.Cartas.Add(cartaPartida);
        await uow.ZonaPartidaRepository.SaveAsync(zonaMano);

        // Act: Ejecutar la transaccion personalizada para mover carta de Mano a Campo
        var useCase = new ZonaPartidaUseCase(uow);
        await useCase.OperationTransactionAsync(zonaMano.Id, zonaCampo.Id);

        // Assert
        var cartaMovida = await uow.CartaPartidaRepository.ReadAsync(cartaPartida.Id);
        Assert.NotNull(cartaMovida);
        Assert.NotNull(cartaMovida.Zona);
        Assert.Equal(zonaCampo.Id, cartaMovida.Zona.Id);
        Assert.Equal(TipoZona.Campo, cartaMovida.Zona.TipoZona);

        _output.WriteLine("[OK] ZonaPartida.OperationTransaction probado con exito (carta trasladada entre zonas en transaccion).");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task ReadFilter_Carta_ObtenerCartaPorRareza_Debe_Filtrar_Correctamente()
    {
        var (_, factory) = CreateTestFactory();
        await using var uow = factory.Create();

        // Arrange
        var usuario = new Usuario
        {
            NombreUsuario = "UserFiltros",
            Email = "filtros@test.com",
            Contrasenia = "hash_7777",
            FotoPerfil = 1,
            EstadoCuenta = EstadoCuenta.Activa,
            TipoUsuario = TipoUsuario.Cliente
        };
        await uow.UsuarioRepository.SaveAsync(usuario);

        var cartaEpica = new Carta
        {
            Nombre = "Carta Epica Test",
            Rareza = Rareza.Epico,
            CosteMana = 4,
            Tipo = TipoCarta.Criatura,
            Vida = 5,
            Ataque = 4,
            Descripcion = "Carta epica publica",
            Imagen = 1,
            EsPublica = true,
            Usuario = usuario
        };
        await uow.CartaRepository.SaveAsync(cartaEpica);

        var cartaComun = new Carta
        {
            Nombre = "Carta Comun Test",
            Rareza = Rareza.Comun,
            CosteMana = 1,
            Tipo = TipoCarta.Criatura,
            Vida = 1,
            Ataque = 1,
            Descripcion = "Carta comun publica",
            Imagen = 1,
            EsPublica = true,
            Usuario = usuario
        };
        await uow.CartaRepository.SaveAsync(cartaComun);

        // Act: Consultar filtro por rareza Epico
        var cartasEpicas = await uow.CartaRepository.ObtenerCartaPorRarezaAsync(Rareza.Epico);

        // Assert
        Assert.NotNull(cartasEpicas);
        Assert.Contains(cartasEpicas, c => c.Id == cartaEpica.Id);
        Assert.DoesNotContain(cartasEpicas, c => c.Id == cartaComun.Id);

        _output.WriteLine("[OK] ReadFilter Carta.obtenerCartaPorRareza probado con exito.");
    }
}
