// HARIA-WEB GENERATED sha256:0bf30fb1cb0cf2894e98832b5b87ed814dfbb6fcb603d06e8d2d0a178eab0dc3
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CraftCaster.Web.Configuration;

public static class HariaApplicationRegistration
{
    public static IServiceCollection AddHariaApplication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var connectionString = RequireConnectionString(configuration, environment);
        var nhibernate = new NHibernate.Cfg.Configuration();
        nhibernate.SetProperty("connection.provider", "NHibernate.Connection.DriverConnectionProvider");
        nhibernate.SetProperty("dialect", "NHibernate.Dialect.SQLiteDialect");
        nhibernate.SetProperty("connection.driver_class", "NHibernate.Driver.SQLite20Driver");
        nhibernate.SetProperty("adonet.batch_size", "100");
        nhibernate.SetProperty("show_sql", "true");
        nhibernate.SetProperty(NHibernate.Cfg.Environment.ConnectionString, connectionString);
        nhibernate.AddAssembly(typeof(CraftCaster.Infrastructure.UnitOfWorkFactory).Assembly);
        services.AddSingleton(nhibernate.BuildSessionFactory());
        services.AddSingleton<CraftCaster.Application.Abstractions.IHariaPasswordHasher, CraftCaster.Infrastructure.Security.HariaPasswordHasher>();
        services.AddSingleton<CraftCaster.Application.Abstractions.IHariaTokenGenerator, CraftCaster.Infrastructure.Security.HariaTokenGenerator>();
        services.AddScoped<CraftCaster.Application.Abstractions.IUsuarioRepository, CraftCaster.Infrastructure.Repositories.UsuarioRepository>();
        services.AddSingleton<CraftCaster.Application.Abstractions.IUnitOfWorkFactory, CraftCaster.Infrastructure.UnitOfWorkFactory>();
        services.AddScoped<CraftCaster.Application.Services.MazoService>();
        services.AddScoped<CraftCaster.Application.Services.CartaService>();
        services.AddScoped<CraftCaster.Application.Services.LineaMazoService>();
        services.AddScoped<CraftCaster.Application.Services.EstadoService>();
        services.AddScoped<CraftCaster.Application.Services.HabilidadService>();
        services.AddScoped<CraftCaster.Application.Services.PartidaService>();
        services.AddScoped<CraftCaster.Application.Services.JugadorPartidaService>();
        services.AddScoped<CraftCaster.Application.Services.ZonaPartidaService>();
        services.AddScoped<CraftCaster.Application.Services.CartaPartidaService>();
        services.AddScoped<CraftCaster.Application.Services.TurnoService>();
        services.AddScoped<CraftCaster.Application.Services.AccionService>();
        services.AddScoped<CraftCaster.Application.Services.UsuarioService>();
        return services;
    }

    private static string RequireConnectionString(IConfiguration configuration, IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("DbConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Configure ConnectionStrings:DbConnection in appsettings.Haria.json, user-secrets, or the ConnectionStrings__DbConnection environment variable.");
        }

        var sqlite = new System.Data.Common.DbConnectionStringBuilder { ConnectionString = connectionString };
        var dataSourceKey = sqlite.Keys.Cast<string>().FirstOrDefault(key =>
            string.Equals(key.Replace(" ", ""), "DataSource", StringComparison.OrdinalIgnoreCase)
            || string.Equals(key, "Filename", StringComparison.OrdinalIgnoreCase));
        if (dataSourceKey is null)
        {
            throw new InvalidOperationException("The SQLite connection string must specify Data Source or Filename.");
        }
        var dataSource = Convert.ToString(sqlite[dataSourceKey]) ?? string.Empty;
        if (dataSource.Length == 0 || dataSource.StartsWith("file:", StringComparison.OrdinalIgnoreCase) || Path.IsPathRooted(dataSource)
            || string.Equals(dataSource, ":memory:", StringComparison.OrdinalIgnoreCase)
            || (sqlite.TryGetValue("Mode", out var mode) && string.Equals(Convert.ToString(mode), "Memory", StringComparison.OrdinalIgnoreCase)))
        {
            return sqlite.ConnectionString;
        }

        sqlite[dataSourceKey] = Path.GetFullPath(Path.Combine(environment.ContentRootPath, dataSource));
        return sqlite.ConnectionString;
    }
}
