using CraftCaster.Application.Abstractions;

namespace CraftCaster.Application.UseCases.ZonaPartida;

public sealed partial class ZonaPartidaUseCase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUnitOfWorkUtils? _utils;

    public ZonaPartidaUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public ZonaPartidaUseCase(IUnitOfWork unitOfWork, IUnitOfWorkUtils utils)
        : this(unitOfWork)
    {
        _utils = utils;
    }
}
