using CraftCaster.Application.Services;
using CraftCaster.Application.UseCases.Usuario;
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

            // TODO: No seed generated for Usuario; define a non-customized New operation in the service to initialize it.

            // TODO: Set required relationship Mazo.Usuario manually; it could not be ordered automatically.
            // TODO: No seed generated for Mazo; define a non-customized New operation in the service to initialize it.

            var lineaMazoService = new LineaMazoService(unitOfWorkFactory);
            var sampleLineaMazo = await lineaMazoService.NewAsync(3);
            var sampleLineaMazoReadBack = await lineaMazoService.ReadOIDAsync(sampleLineaMazo.Id);
            Ensure(sampleLineaMazoReadBack is not null, "LineaMazo read by OID returns the generated entity.");
            var sampleLineaMazoReadAll = await lineaMazoService.ReadAllAsync();
            Ensure(sampleLineaMazoReadAll.Count > 0, "LineaMazo read all returns generated data.");
            await lineaMazoService.ModifyAsync(sampleLineaMazo.Id, 3);
            Ensure(await lineaMazoService.ReadOIDAsync(sampleLineaMazo.Id) is not null, "LineaMazo modify keeps the entity readable.");

            var estadoService = new EstadoService(unitOfWorkFactory);
            var sampleEstado = await estadoService.NewAsync(TipoEstado.Entrar);
            var sampleEstadoReadBack = await estadoService.ReadOIDAsync(sampleEstado.Id);
            Ensure(sampleEstadoReadBack is not null, "Estado read by OID returns the generated entity.");
            var sampleEstadoReadAll = await estadoService.ReadAllAsync();
            Ensure(sampleEstadoReadAll.Count > 0, "Estado read all returns generated data.");
            await estadoService.ModifyAsync(sampleEstado.Id, TipoEstado.Entrar);
            Ensure(await estadoService.ReadOIDAsync(sampleEstado.Id) is not null, "Estado modify keeps the entity readable.");

            var habilidadService = new HabilidadService(unitOfWorkFactory);
            var sampleHabilidad = await habilidadService.NewAsync(TipoHabilidad.Ataque, 6, Objetivo.Jugador, sampleEstado.Id);
            Ensure(sampleHabilidad.Estado?.Equals(sampleEstado) == true, "Habilidad.new relates sampleHabilidad and sampleEstado.");
            var sampleHabilidadReadBack = await habilidadService.ReadOIDAsync(sampleHabilidad.Id);
            Ensure(sampleHabilidadReadBack is not null, "Habilidad read by OID returns the generated entity.");
            var sampleHabilidadReadAll = await habilidadService.ReadAllAsync();
            Ensure(sampleHabilidadReadAll.Count > 0, "Habilidad read all returns generated data.");
            await habilidadService.ModifyAsync(sampleHabilidad.Id, TipoHabilidad.Ataque, 6, Objetivo.Jugador, sampleEstado.Id);
            Ensure(await habilidadService.ReadOIDAsync(sampleHabilidad.Id) is not null, "Habilidad modify keeps the entity readable.");

            // TODO: Set required relationship Carta.Usuario manually; it could not be ordered automatically.
            // TODO: No seed generated for Carta; one or more New arguments reference classes that were not seeded yet.

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

            await estadoService.AddHabilidadeAsync(sampleEstado.Id, new[] { sampleHabilidad.Id });
            Ensure(sampleEstado.Habilidades.Contains(sampleHabilidad), "Estado.addHabilidade relates Estado and Habilidad.");

            await estadoService.RemoveHabilidadeAsync(sampleEstado.Id, new[] { sampleHabilidad.Id });
            Ensure(!sampleEstado.Habilidades.Contains(sampleHabilidad), "Estado.removeHabilidade removes the relation between Estado and Habilidad.");

            await lineaMazoService.DestroyAsync(sampleLineaMazo.Id);
            await habilidadService.DestroyAsync(sampleHabilidad.Id);
            await estadoService.DestroyAsync(sampleEstado.Id);

            Ensure((await lineaMazoService.ReadAllAsync()).Count == 0, "LineaMazo seed cleanup completed.");
            Ensure((await estadoService.ReadAllAsync()).Count == 0, "Estado seed cleanup completed.");
            Ensure((await habilidadService.ReadAllAsync()).Count == 0, "Habilidad seed cleanup completed.");
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