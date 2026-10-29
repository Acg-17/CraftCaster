using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CraftCaster.Infrastructure.Utils.Manual;

public static class ExternalUtilRegistration
{
    public static void Register(IServiceCollection services, IConfiguration configuration)
    {
        _ = services;
        _ = configuration;
    }
}