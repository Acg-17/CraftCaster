using Microsoft.Extensions.DependencyInjection;
using CraftCaster.Application.Abstractions;

namespace CraftCaster.Infrastructure.Utils.Generated;

public sealed class UnitOfWorkUtils : IUnitOfWorkUtils
{
    private readonly IServiceProvider _serviceProvider;

    public UnitOfWorkUtils(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IServiceProvider Services => _serviceProvider;

    public T GetRequiredService<T>() where T : notnull =>
        _serviceProvider.GetRequiredService<T>();

    public T? GetService<T>() =>
        _serviceProvider.GetService<T>();
}