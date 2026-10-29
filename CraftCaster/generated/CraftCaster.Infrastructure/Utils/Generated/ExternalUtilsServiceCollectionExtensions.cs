using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CraftCaster.Application.Abstractions;
using CraftCaster.Infrastructure.Utils.Manual;

namespace CraftCaster.Infrastructure.Utils.Generated;

public static class ExternalUtilsServiceCollectionExtensions
{
    public static IServiceCollection AddHariaExternalUtils(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUnitOfWorkUtils, UnitOfWorkUtils>();
        ExternalUtilRegistration.Register(services, configuration);
        return services;
    }
}