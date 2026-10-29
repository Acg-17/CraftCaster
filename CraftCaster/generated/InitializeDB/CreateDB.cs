using Microsoft.Extensions.Configuration;

namespace InitializeDB;

public static class CreateDB
{
    public static string Create()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var cs = config.GetConnectionString("DbConnection")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__DbConnection")
            ?? Environment.GetEnvironmentVariable("DB_CONNECTION")
            ?? throw new InvalidOperationException("No application connection string provided.");

        return cs;
    }
}