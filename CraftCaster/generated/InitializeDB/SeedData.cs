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