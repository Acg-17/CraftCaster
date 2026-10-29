using CraftCaster.Application.Services;

using CraftCaster.Domain.Entities;

using CraftCaster.Domain.Enums;
using CraftCaster.Infrastructure;
using CraftCaster.Infrastructure.Repositories;
using CraftCaster.Infrastructure.Security;
using Microsoft.Extensions.Logging;

/*PROTECTED REGION ID(seedDataUsings) ENABLED START*/
// Add custom usings here.
/*PROTECTED REGION END*/

namespace InitializeDB;

public static class SeedData
{
    public static async Task InitializeDataAsync(NHibernate.Cfg.Configuration cfg, ILoggerFactory? loggerFactory = null)
    {
        try
        {
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
        catch (Exception ex)
        {
            Console.WriteLine(ex.InnerException ?? ex);
            throw;
        }
    }

    public static async Task InitializeDataManualAsync(NHibernate.Cfg.Configuration cfg, ILoggerFactory? loggerFactory = null)
    {
        try
        {
            using var unitOfWorkFactory = new UnitOfWorkFactory(cfg, loggerFactory);

            /*PROTECTED REGION ID(initializeDataManual) ENABLED START*/
            /*PROTECTED REGION END*/
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.InnerException ?? ex);
            throw;
        }
    }

    private static void Ensure(bool condition, string message)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = condition ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine((condition ? "[OK] " : "[FAIL] ") + message);
        Console.ForegroundColor = previousColor;

        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}