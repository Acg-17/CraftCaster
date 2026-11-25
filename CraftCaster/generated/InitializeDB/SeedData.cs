using CraftCaster.Application.Services;
using CraftCaster.Application.UseCases.Habilidad;
using CraftCaster.Application.UseCases.JugadorPartida;
using CraftCaster.Application.UseCases.Partida;
using CraftCaster.Application.UseCases.Usuario;
using CraftCaster.Application.UseCases.ZonaPartida;
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
            var sampleUsuario = await usuarioService.NewAsync("Sample Usuario NombreUsuario", "sample1@example.com", "Password123!", 1, EstadoCuenta.Activa, TipoUsuario.Cliente);
            var sampleUsuarioLoginToken = await usuarioService.LoginAsync(sampleUsuario.Id, "Password123!");
            Ensure(!string.IsNullOrWhiteSpace(sampleUsuarioLoginToken), "Usuario.login returns a login token for sampleUsuario.");
            var sampleUsuarioReadBack = await usuarioService.ReadOIDAsync(sampleUsuario.Id);
            Ensure(sampleUsuarioReadBack is not null, "Usuario read by OID returns the generated entity.");
            var sampleUsuarioReadAll = await usuarioService.ReadAllAsync();
            Ensure(sampleUsuarioReadAll.Count > 0, "Usuario read all returns generated data.");
            var sampleUsuarioObtenerUsuarioLocalResult = await usuarioService.ObtenerUsuarioLocalAsync(default!);
            Ensure(sampleUsuarioObtenerUsuarioLocalResult is not null, "Usuario.obtenerUsuarioLocal filter executes.");
            var sampleUsuarioObtenerUsuarioPorEmailONombreResult = await usuarioService.ObtenerUsuarioPorEmailONombreAsync("Sample Usuario Identificador");
            Ensure(sampleUsuarioObtenerUsuarioPorEmailONombreResult is not null, "Usuario.obtenerUsuarioPorEmailONombre filter executes.");
            await usuarioService.ModifyAsync(sampleUsuario.Id, "Sample Usuario NombreUsuario", "sample1@example.com", "Password123!", 1, EstadoCuenta.Activa, TipoUsuario.Cliente);
            Ensure(await usuarioService.ReadOIDAsync(sampleUsuario.Id) is not null, "Usuario modify keeps the entity readable.");

            var mazoService = new MazoService(unitOfWorkFactory);
            var sampleMazo = await mazoService.NewAsync("Sample Mazo Nombre", 2.5f, 2, EstadoMazo.Incompleto, true, 2.5f, 1, sampleUsuario.Id);
            Ensure(sampleMazo.Usuario?.Equals(sampleUsuario) == true, "Mazo.new relates sampleMazo and sampleUsuario.");
            var sampleMazoReadBack = await mazoService.ReadOIDAsync(sampleMazo.Id);
            Ensure(sampleMazoReadBack is not null, "Mazo read by OID returns the generated entity.");
            var sampleMazoReadAll = await mazoService.ReadAllAsync();
            Ensure(sampleMazoReadAll.Count > 0, "Mazo read all returns generated data.");
            var sampleMazoObtenerMazosPublicosResult = await mazoService.ObtenerMazosPublicosAsync();
            Ensure(sampleMazoObtenerMazosPublicosResult is not null, "Mazo.obtenerMazosPublicos filter executes.");
            var sampleMazoObtenerMazosUsuarioResult = await mazoService.ObtenerMazosUsuarioAsync(sampleUsuario.Id);
            Ensure(sampleMazoObtenerMazosUsuarioResult is not null, "Mazo.obtenerMazosUsuario filter executes.");
            var sampleMazoObtenerMazosPorNombreResult = await mazoService.ObtenerMazosPorNombreAsync("Sample Mazo Nombre");
            Ensure(sampleMazoObtenerMazosPorNombreResult is not null, "Mazo.obtenerMazosPorNombre filter executes.");
            var sampleMazoObtenerMazoPorCosteMedioResult = await mazoService.ObtenerMazoPorCosteMedioAsync(2, 2);
            Ensure(sampleMazoObtenerMazoPorCosteMedioResult is not null, "Mazo.obtenerMazoPorCosteMedio filter executes.");
            var sampleMazoObtenerMazoPorValoracionResult = await mazoService.ObtenerMazoPorValoracionAsync(2, 2);
            Ensure(sampleMazoObtenerMazoPorValoracionResult is not null, "Mazo.obtenerMazoPorValoracion filter executes.");
            var sampleMazoObtenerMazoJugador1Result = await mazoService.ObtenerMazoJugador1Async(default!);
            Ensure(sampleMazoObtenerMazoJugador1Result is not null, "Mazo.obtenerMazoJugador1 filter executes.");
            var sampleMazoObtenerMazoJugador2Result = await mazoService.ObtenerMazoJugador2Async(default!);
            Ensure(sampleMazoObtenerMazoJugador2Result is not null, "Mazo.obtenerMazoJugador2 filter executes.");
            await mazoService.ModifyAsync(sampleMazo.Id, "Sample Mazo Nombre", 2.5f, 2, EstadoMazo.Incompleto, true, 2.5f, 1, sampleUsuario.Id);
            Ensure(await mazoService.ReadOIDAsync(sampleMazo.Id) is not null, "Mazo modify keeps the entity readable.");

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

            var lineaMazoService = new LineaMazoService(unitOfWorkFactory);
            var sampleLineaMazo = await lineaMazoService.NewAsync(4, sampleMazo.Id, sampleCarta.Id);
            Ensure(sampleLineaMazo.Mazo?.Equals(sampleMazo) == true, "LineaMazo.new relates sampleLineaMazo and sampleMazo.");
            Ensure(sampleLineaMazo.Carta?.Equals(sampleCarta) == true, "LineaMazo.new relates sampleLineaMazo and sampleCarta.");
            var sampleLineaMazoReadBack = await lineaMazoService.ReadOIDAsync(sampleLineaMazo.Id);
            Ensure(sampleLineaMazoReadBack is not null, "LineaMazo read by OID returns the generated entity.");
            var sampleLineaMazoReadAll = await lineaMazoService.ReadAllAsync();
            Ensure(sampleLineaMazoReadAll.Count > 0, "LineaMazo read all returns generated data.");
            await lineaMazoService.ModifyAsync(sampleLineaMazo.Id, 4, sampleMazo.Id, sampleCarta.Id);
            Ensure(await lineaMazoService.ReadOIDAsync(sampleLineaMazo.Id) is not null, "LineaMazo modify keeps the entity readable.");

            var estadoService = new EstadoService(unitOfWorkFactory);
            var sampleEstado = await estadoService.NewAsync(TipoEstado.Entrar, sampleCarta.Id);
            Ensure(sampleEstado.Carta?.Equals(sampleCarta) == true, "Estado.new relates sampleEstado and sampleCarta.");
            var sampleEstadoReadBack = await estadoService.ReadOIDAsync(sampleEstado.Id);
            Ensure(sampleEstadoReadBack is not null, "Estado read by OID returns the generated entity.");
            var sampleEstadoReadAll = await estadoService.ReadAllAsync();
            Ensure(sampleEstadoReadAll.Count > 0, "Estado read all returns generated data.");
            await estadoService.ModifyAsync(sampleEstado.Id, TipoEstado.Entrar, sampleCarta.Id);
            Ensure(await estadoService.ReadOIDAsync(sampleEstado.Id) is not null, "Estado modify keeps the entity readable.");

            var habilidadService = new HabilidadService(unitOfWorkFactory);
            var sampleHabilidad = await habilidadService.NewAsync(TipoHabilidad.Ataque, 6, TipoObjetivo.Jugador, 6, sampleEstado.Id);
            Ensure(sampleHabilidad.Estado?.Equals(sampleEstado) == true, "Habilidad.new relates sampleHabilidad and sampleEstado.");
            var sampleHabilidadReadBack = await habilidadService.ReadOIDAsync(sampleHabilidad.Id);
            Ensure(sampleHabilidadReadBack is not null, "Habilidad read by OID returns the generated entity.");
            var sampleHabilidadReadAll = await habilidadService.ReadAllAsync();
            Ensure(sampleHabilidadReadAll.Count > 0, "Habilidad read all returns generated data.");
            await habilidadService.ModifyAsync(sampleHabilidad.Id, TipoHabilidad.Ataque, 6, TipoObjetivo.Jugador, 6, sampleEstado.Id);
            Ensure(await habilidadService.ReadOIDAsync(sampleHabilidad.Id) is not null, "Habilidad modify keeps the entity readable.");

            var partidaService = new PartidaService(unitOfWorkFactory);
            var samplePartida = await partidaService.NewAsync(EstadoPartida.Espera, new DateTime(2026, 1, 7), sampleUsuario.Id, sampleMazo.Id);
            Ensure(samplePartida.Usuario1?.Equals(sampleUsuario) == true, "Partida.new relates samplePartida and sampleUsuario.");
            Ensure(samplePartida.Mazo1?.Equals(sampleMazo) == true, "Partida.new relates samplePartida and sampleMazo.");
            var samplePartidaReadBack = await partidaService.ReadOIDAsync(samplePartida.Id);
            Ensure(samplePartidaReadBack is not null, "Partida read by OID returns the generated entity.");
            var samplePartidaReadAll = await partidaService.ReadAllAsync();
            Ensure(samplePartidaReadAll.Count > 0, "Partida read all returns generated data.");
            var samplePartidaObtenerPartidasUsuarioResult = await partidaService.ObtenerPartidasUsuarioAsync(sampleUsuario.Id);
            Ensure(samplePartidaObtenerPartidasUsuarioResult is not null, "Partida.obtenerPartidasUsuario filter executes.");
            await partidaService.ModifyAsync(samplePartida.Id, EstadoPartida.Espera, new DateTime(2026, 1, 7), sampleUsuario.Id, sampleMazo.Id);
            Ensure(await partidaService.ReadOIDAsync(samplePartida.Id) is not null, "Partida modify keeps the entity readable.");

            var jugadorPartidaService = new JugadorPartidaService(unitOfWorkFactory);
            var sampleJugadorPartida = await jugadorPartidaService.NewAsync(8, 8, 8, true, samplePartida.Id);
            Ensure(sampleJugadorPartida.Partida?.Equals(samplePartida) == true, "JugadorPartida.new relates sampleJugadorPartida and samplePartida.");
            var sampleJugadorPartidaReadBack = await jugadorPartidaService.ReadOIDAsync(sampleJugadorPartida.Id);
            Ensure(sampleJugadorPartidaReadBack is not null, "JugadorPartida read by OID returns the generated entity.");
            var sampleJugadorPartidaReadAll = await jugadorPartidaService.ReadAllAsync();
            Ensure(sampleJugadorPartidaReadAll.Count > 0, "JugadorPartida read all returns generated data.");
            await jugadorPartidaService.ModifyAsync(sampleJugadorPartida.Id, 8, 8, 8, true, samplePartida.Id);
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
            var sampleCartaPartida = await cartaPartidaService.NewAsync(10, 10, true, sampleZonaPartida.Id, sampleCarta.Id);
            Ensure(sampleCartaPartida.Zona?.Equals(sampleZonaPartida) == true, "CartaPartida.new relates sampleCartaPartida and sampleZonaPartida.");
            Ensure(sampleCartaPartida.Original?.Equals(sampleCarta) == true, "CartaPartida.new relates sampleCartaPartida and sampleCarta.");
            var sampleCartaPartidaReadBack = await cartaPartidaService.ReadOIDAsync(sampleCartaPartida.Id);
            Ensure(sampleCartaPartidaReadBack is not null, "CartaPartida read by OID returns the generated entity.");
            var sampleCartaPartidaReadAll = await cartaPartidaService.ReadAllAsync();
            Ensure(sampleCartaPartidaReadAll.Count > 0, "CartaPartida read all returns generated data.");
            await cartaPartidaService.ModifyAsync(sampleCartaPartida.Id, 10, 10, true, sampleZonaPartida.Id, sampleCarta.Id);
            Ensure(await cartaPartidaService.ReadOIDAsync(sampleCartaPartida.Id) is not null, "CartaPartida modify keeps the entity readable.");

            var turnoService = new TurnoService(unitOfWorkFactory);
            var sampleTurno = await turnoService.NewAsync(11, 11, samplePartida.Id, sampleJugadorPartida.Id);
            Ensure(sampleTurno.Partida?.Equals(samplePartida) == true, "Turno.new relates sampleTurno and samplePartida.");
            Ensure(sampleTurno.Jugador?.Equals(sampleJugadorPartida) == true, "Turno.new relates sampleTurno and sampleJugadorPartida.");
            var sampleTurnoReadBack = await turnoService.ReadOIDAsync(sampleTurno.Id);
            Ensure(sampleTurnoReadBack is not null, "Turno read by OID returns the generated entity.");
            var sampleTurnoReadAll = await turnoService.ReadAllAsync();
            Ensure(sampleTurnoReadAll.Count > 0, "Turno read all returns generated data.");
            await turnoService.ModifyAsync(sampleTurno.Id, 11, 11, samplePartida.Id, sampleJugadorPartida.Id);
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
            await usuarioService.AddPartidasVisitanteAsync(sampleUsuario.Id, new[] { samplePartida.Id });
            Ensure(sampleUsuario.PartidasVisitante.Contains(samplePartida), "Usuario.addPartidasVisitante relates Usuario and Partida.");
            await mazoService.AddLineaAsync(sampleMazo.Id, new[] { sampleLineaMazo.Id });
            Ensure(sampleMazo.Lineas.Contains(sampleLineaMazo), "Mazo.addLinea relates Mazo and LineaMazo.");
            await mazoService.AddPartidasLocalAsync(sampleMazo.Id, new[] { samplePartida.Id });
            Ensure(sampleMazo.PartidasLocal.Contains(samplePartida), "Mazo.addPartidasLocal relates Mazo and Partida.");
            await mazoService.AddPartidasVisitanteAsync(sampleMazo.Id, new[] { samplePartida.Id });
            Ensure(sampleMazo.PartidasVisitante.Contains(samplePartida), "Mazo.addPartidasVisitante relates Mazo and Partida.");
            await cartaService.AddEstadoAsync(sampleCarta.Id, new[] { sampleEstado.Id });
            Ensure(sampleCarta.Estados.Contains(sampleEstado), "Carta.addEstado relates Carta and Estado.");
            await estadoService.AddHabilidadeAsync(sampleEstado.Id, new[] { sampleHabilidad.Id });
            Ensure(sampleEstado.Habilidades.Contains(sampleHabilidad), "Estado.addHabilidade relates Estado and Habilidad.");
            await partidaService.AddTurnoAsync(samplePartida.Id, new[] { sampleTurno.Id });
            Ensure(samplePartida.Turnos.Contains(sampleTurno), "Partida.addTurno relates Partida and Turno.");
            await partidaService.AddJugadoresAsync(samplePartida.Id, new[] { sampleJugadorPartida.Id });
            Ensure(samplePartida.Jugadores.Contains(sampleJugadorPartida), "Partida.addJugadores relates Partida and JugadorPartida.");
            await partidaService.AddUsuario2Async(samplePartida.Id, sampleUsuario.Id);
            Ensure(samplePartida.Usuario2?.Equals(sampleUsuario) == true, "Partida.addUsuario2 relates Partida and Usuario.");
            await partidaService.AddMazo2Async(samplePartida.Id, sampleMazo.Id);
            Ensure(samplePartida.Mazo2?.Equals(sampleMazo) == true, "Partida.addMazo2 relates Partida and Mazo.");
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

            await usuarioService.RemovePartidasVisitanteAsync(sampleUsuario.Id, new[] { samplePartida.Id });
            Ensure(!sampleUsuario.PartidasVisitante.Contains(samplePartida), "Usuario.removePartidasVisitante removes the relation between Usuario and Partida.");
            await partidaService.RemoveUsuario2Async(samplePartida.Id, sampleUsuario.Id);
            Ensure(samplePartida.Usuario2 is null, "Partida.removeUsuario2 removes the relation between Partida and Usuario.");
            await partidaService.RemoveMazo2Async(samplePartida.Id, sampleMazo.Id);
            Ensure(samplePartida.Mazo2 is null, "Partida.removeMazo2 removes the relation between Partida and Mazo.");
            await accionService.RemoveOrigenAsync(sampleAccion.Id, sampleCartaPartida.Id);
            Ensure(sampleAccion.Origen is null, "Accion.removeOrigen removes the relation between Accion and CartaPartida.");
            await accionService.RemoveObjetivoAsync(sampleAccion.Id, new[] { sampleCartaPartida.Id });
            Ensure(!sampleAccion.Objetivo.Contains(sampleCartaPartida), "Accion.removeObjetivo removes the relation between Accion and CartaPartida.");

            // Detach optional references and shared collections before deleting seed data.
            sampleUsuario.Cartas.Clear();
            sampleUsuario.Mazos.Clear();
            sampleUsuario.PartidasLocal.Clear();
            sampleUsuario.PartidasVisitante.Clear();
            await usuarioService.ModifierUsuarioDefaultAsync(sampleUsuario);
            // Detach optional references and shared collections before deleting seed data.
            sampleMazo.PartidasLocal.Clear();
            sampleMazo.PartidasVisitante.Clear();
            await mazoService.ModifierMazoDefaultAsync(sampleMazo);
            // Detach optional references and shared collections before deleting seed data.
            samplePartida.Usuario2 = null;
            samplePartida.Mazo2 = null;
            await partidaService.ModifierPartidaDefaultAsync(samplePartida);
            // Detach optional references and shared collections before deleting seed data.
            sampleAccion.Origen = null;
            sampleAccion.Objetivo.Clear();
            await accionService.ModifierAccionDefaultAsync(sampleAccion);
            await lineaMazoService.DestroyAsync(sampleLineaMazo.Id);
            await habilidadService.DestroyAsync(sampleHabilidad.Id);
            await estadoService.DestroyAsync(sampleEstado.Id);
            await accionService.DestroyAsync(sampleAccion.Id);
            await cartaPartidaService.DestroyAsync(sampleCartaPartida.Id);
            await cartaService.DestroyAsync(sampleCarta.Id);
            await zonaPartidaService.DestroyAsync(sampleZonaPartida.Id);
            await turnoService.DestroyAsync(sampleTurno.Id);
            await jugadorPartidaService.DestroyAsync(sampleJugadorPartida.Id);
            await partidaService.DestroyAsync(samplePartida.Id);
            await mazoService.DestroyAsync(sampleMazo.Id);
            await usuarioService.DestroyAsync(sampleUsuario.Id);

            Ensure((await usuarioService.ReadAllAsync()).Count == 0, "Usuario seed cleanup completed.");
            Ensure((await mazoService.ReadAllAsync()).Count == 0, "Mazo seed cleanup completed.");
            Ensure((await cartaService.ReadAllAsync()).Count == 0, "Carta seed cleanup completed.");
            Ensure((await lineaMazoService.ReadAllAsync()).Count == 0, "LineaMazo seed cleanup completed.");
            Ensure((await estadoService.ReadAllAsync()).Count == 0, "Estado seed cleanup completed.");
            Ensure((await habilidadService.ReadAllAsync()).Count == 0, "Habilidad seed cleanup completed.");
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
            var usuarioService = new UsuarioService(unitOfWorkFactory);
            var cartaService = new CartaService(unitOfWorkFactory);
            var estadoService = new EstadoService(unitOfWorkFactory);
            var habilidadService = new HabilidadService(unitOfWorkFactory);
            var mazoService = new MazoService(unitOfWorkFactory);
            var lineaMazoService = new LineaMazoService(unitOfWorkFactory);
            var partidaService = new PartidaService(unitOfWorkFactory);
            var jugadorPartidaService = new JugadorPartidaService(unitOfWorkFactory);
            var zonaPartidaService = new ZonaPartidaService(unitOfWorkFactory);
            var cartaPartidaService = new CartaPartidaService(unitOfWorkFactory);
            var turnoService = new TurnoService(unitOfWorkFactory);

            // ---------- Usuarios (contrasenya de todos: Password123!) ----------
            var admin = await usuarioService.NewAsync("admin", "admin@craftcaster.com", "Password123!", 1, EstadoCuenta.Activa, TipoUsuario.Admin);
            var ana = await usuarioService.NewAsync("ana", "ana@craftcaster.com", "Password123!", 1, EstadoCuenta.Activa, TipoUsuario.Cliente);
            var beto = await usuarioService.NewAsync("beto", "beto@craftcaster.com", "Password123!", 1, EstadoCuenta.Activa, TipoUsuario.Cliente);

            // ---------- Cartas (ataque y vida solo cuentan en las criaturas) ----------
            var dragon = await cartaService.NewAsync("Dragon Rojo", Rareza.Legendario, 6, TipoCarta.Criatura, "Al entrar inflige 2 de danyo a un enemigo.", 1, true, 6, 6, ana.Id);
            var caballero = await cartaService.NewAsync("Caballero Novato", Rareza.Comun, 2, TipoCarta.Criatura, "Un soldado fiel.", 1, true, 2, 3, ana.Id);
            var arquera = await cartaService.NewAsync("Arquera Elfica", Rareza.Especial, 3, TipoCarta.Criatura, "Al atacar inflige 1 de danyo extra.", 1, true, 3, 2, ana.Id);
            var bolaFuego = await cartaService.NewAsync("Bola de Fuego", Rareza.Epico, 4, TipoCarta.Hechizo, "Inflige 5 de danyo al objetivo elegido.", 1, true, 0, 0, ana.Id);
            var pocion = await cartaService.NewAsync("Pocion de Curacion", Rareza.Comun, 1, TipoCarta.Hechizo, "Cura 3 puntos de vida al jugador.", 1, true, 0, 0, ana.Id);
            var golem = await cartaService.NewAsync("Golem de Piedra", Rareza.Especial, 5, TipoCarta.Criatura, "Al entrar gana 2 de defensa.", 1, true, 4, 8, ana.Id);
            var lobo = await cartaService.NewAsync("Lobo Sombrio", Rareza.Comun, 2, TipoCarta.Criatura, "Rapido y agresivo.", 1, true, 3, 1, beto.Id);
            var rayo = await cartaService.NewAsync("Rayo Arcano", Rareza.Especial, 2, TipoCarta.Hechizo, "Inflige 3 de danyo a un enemigo.", 1, true, 0, 0, beto.Id);
            await cartaService.NewAsync("Carta Secreta", Rareza.Epico, 7, TipoCarta.Criatura, "Carta privada de beto, aun sin compartir.", 1, false, 7, 7, beto.Id);

            // ---------- Estados y habilidades de las cartas ----------
            var estadoDragon = await estadoService.NewAsync(TipoEstado.Entrar, dragon.Id);
            await habilidadService.NewAsync(TipoHabilidad.Danyo, 2, TipoObjetivo.Enemigo, 0, estadoDragon.Id);
            var estadoArquera = await estadoService.NewAsync(TipoEstado.Atacar, arquera.Id);
            await habilidadService.NewAsync(TipoHabilidad.Danyo, 1, TipoObjetivo.Enemigo, 0, estadoArquera.Id);
            var estadoBola = await estadoService.NewAsync(TipoEstado.Entrar, bolaFuego.Id);
            await habilidadService.NewAsync(TipoHabilidad.Danyo, 5, TipoObjetivo.Libre, 0, estadoBola.Id);
            var estadoPocion = await estadoService.NewAsync(TipoEstado.Entrar, pocion.Id);
            await habilidadService.NewAsync(TipoHabilidad.Cura, 3, TipoObjetivo.Jugador, 0, estadoPocion.Id);
            var estadoGolem = await estadoService.NewAsync(TipoEstado.Entrar, golem.Id);
            await habilidadService.NewAsync(TipoHabilidad.Defensa, 2, TipoObjetivo.Self, 0, estadoGolem.Id);
            var estadoRayo = await estadoService.NewAsync(TipoEstado.Entrar, rayo.Id);
            await habilidadService.NewAsync(TipoHabilidad.Danyo, 3, TipoObjetivo.Enemigo, 0, estadoRayo.Id);

            // ---------- Mazos (numCartas y costeMedio cuadran con sus lineas) ----------
            // Fuego y Acero: 10x2 + 8x3 + 6x4 + 2x6 + 4x1 = 84 de mana en 30 cartas -> coste medio 2,8
            var mazoAna = await mazoService.NewAsync("Fuego y Acero", 2.8f, 30, EstadoMazo.Jugable, true, 4.5f, 1, ana.Id);
            await lineaMazoService.NewAsync(10, mazoAna.Id, caballero.Id);
            await lineaMazoService.NewAsync(8, mazoAna.Id, arquera.Id);
            await lineaMazoService.NewAsync(6, mazoAna.Id, bolaFuego.Id);
            await lineaMazoService.NewAsync(2, mazoAna.Id, dragon.Id);
            await lineaMazoService.NewAsync(4, mazoAna.Id, pocion.Id);

            // Manada Nocturna: 16x2 + 14x2 = 60 de mana en 30 cartas -> coste medio 2
            var mazoBeto = await mazoService.NewAsync("Manada Nocturna", 2f, 30, EstadoMazo.Jugable, true, 3.5f, 1, beto.Id);
            await lineaMazoService.NewAsync(16, mazoBeto.Id, lobo.Id);
            await lineaMazoService.NewAsync(14, mazoBeto.Id, rayo.Id);

            // Pruebas: privado y sin terminar (5 cartas de coste 1)
            var mazoPruebas = await mazoService.NewAsync("Pruebas", 1f, 5, EstadoMazo.Incompleto, false, 0f, 1, ana.Id);
            await lineaMazoService.NewAsync(5, mazoPruebas.Id, pocion.Id);

            // ---------- Partida en espera: beto busca rival ----------
            // Se crea con la CustomTransaction iniciarPartida (como desde la web) para que tenga su
            // JugadorPartida 1, sus 4 zonas y las cartas del mazo. Con partidaService.NewAsync la partida
            // quedaba sin jugadores y no se podía abrir el tablero ni empezarla.
            await using (var unitOfWorkEspera = unitOfWorkFactory.Create())
            {
                await new UsuarioUseCase(unitOfWorkEspera).IniciarPartidaAsync(beto.Id);
            }

            // ---------- Partida en curso: ana (jugador 1) contra beto (jugador 2), turno 1 ----------
            var partida = await partidaService.NewAsync(EstadoPartida.EnCurso, DateTime.Today, ana.Id, mazoAna.Id);
            await partidaService.AddUsuario2Async(partida.Id, beto.Id);
            await partidaService.AddMazo2Async(partida.Id, mazoBeto.Id);

            var jugador1 = await jugadorPartidaService.NewAsync(20, 1, 1, true, partida.Id);
            var jugador2 = await jugadorPartidaService.NewAsync(20, 2, 0, false, partida.Id);

            // Zonas de cada jugador
            var mano1 = await zonaPartidaService.NewAsync(TipoZona.Mano, jugador1.Id);
            var campo1 = await zonaPartidaService.NewAsync(TipoZona.Campo, jugador1.Id);
            await zonaPartidaService.NewAsync(TipoZona.Descarte, jugador1.Id);
            var mazo1 = await zonaPartidaService.NewAsync(TipoZona.Mazo, jugador1.Id);
            var mano2 = await zonaPartidaService.NewAsync(TipoZona.Mano, jugador2.Id);
            await zonaPartidaService.NewAsync(TipoZona.Campo, jugador2.Id);
            await zonaPartidaService.NewAsync(TipoZona.Descarte, jugador2.Id);
            var mazo2 = await zonaPartidaService.NewAsync(TipoZona.Mazo, jugador2.Id);

            // Cartas en juego (vidaActual, ataqueActual, puedeAtacar, zona, carta original)
            await cartaPartidaService.NewAsync(3, 2, false, campo1.Id, caballero.Id);
            await cartaPartidaService.NewAsync(3, 2, false, mano1.Id, caballero.Id);
            await cartaPartidaService.NewAsync(0, 0, false, mano1.Id, pocion.Id);
            await cartaPartidaService.NewAsync(2, 3, false, mazo1.Id, arquera.Id);
            await cartaPartidaService.NewAsync(0, 0, false, mazo1.Id, bolaFuego.Id);
            await cartaPartidaService.NewAsync(6, 6, false, mazo1.Id, dragon.Id);
            await cartaPartidaService.NewAsync(1, 3, false, mano2.Id, lobo.Id);
            await cartaPartidaService.NewAsync(0, 0, false, mano2.Id, rayo.Id);
            await cartaPartidaService.NewAsync(1, 3, false, mazo2.Id, lobo.Id);
            await cartaPartidaService.NewAsync(1, 3, false, mazo2.Id, lobo.Id);
            await cartaPartidaService.NewAsync(0, 0, false, mazo2.Id, rayo.Id);

            // Turno actual: turno 1 de ana con 1 de mana
            await turnoService.NewAsync(1, 1, partida.Id, jugador1.Id);

            Console.WriteLine("Datos de ejemplo creados: 3 usuarios, 9 cartas, 3 mazos y 2 partidas.");
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