using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using CraftCaster.Infrastructure.Logging;
using CraftCaster.Infrastructure.Repositories;

namespace InitializeDB;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("-----------------------------------------------------------------------------");
        Console.WriteLine("A database called: CraftCasterNHibernate will be created if it does not exist.");
        Console.WriteLine("-----------------------------------------------------------------------------");

        try
        {
            var configuration = BuildConfiguration();
            using var loggerFactory = CreateLoggerFactory(configuration);
            var cfg = new Configuration();

            /*PROTECTED REGION ID(initializeDatabase) START*/
            var cs = CreateDB.Create();

            cfg.Configure(Path.Combine(AppContext.BaseDirectory, "hibernate.cfg.xml"));
            cfg.SetProperty(NHibernate.Cfg.Environment.ConnectionString, cs);
            cfg.AddAssembly(typeof(UsuarioRepository).Assembly);
            cfg.EnableSqlStatementCapture();

            new SchemaExport(cfg).Execute(true, true, false);
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("Database schema created successfully");
            Console.WriteLine("-------------------------------------");
            /*PROTECTED REGION END*/

            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine("Let's initialize the data of your database!");
            Console.WriteLine("-------------------------------------------------------");

            /*PROTECTED REGION ID(seedData) START*/
            await SeedData.InitializeDataAsync(cfg, loggerFactory);
            await SeedData.InitializeDataManualAsync(cfg, loggerFactory);
            /*PROTECTED REGION END*/
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("The data has been inserted successfully!!");
            Console.WriteLine("-----------------------------------------");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static IConfiguration BuildConfiguration() =>
        new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{System.Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

    private static ILoggerFactory CreateLoggerFactory(IConfiguration configuration)
    {
        var minimumLevel = Enum.TryParse<LogLevel>(configuration["Logging:File:MinimumLevel"], ignoreCase: true, out var parsedLevel)
            ? parsedLevel
            : LogLevel.Information;

        var configuredPath = configuration["Logging:File:Path"];
        var filePath = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(GetLogBaseDirectory(), "logs", "application.log")
            : ResolveLogPath(configuredPath);

        return LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(minimumLevel);
            builder.AddProvider(new FileLoggerProvider(new FileLoggerOptions
            {
                FilePath = filePath,
                MinimumLevel = minimumLevel
            }));
        });
    }

    private static string ResolveLogPath(string configuredPath) =>
        Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(GetLogBaseDirectory(), configuredPath);

    private static string GetLogBaseDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "InitializeDB.csproj")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return AppContext.BaseDirectory;
    }
}