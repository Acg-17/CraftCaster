using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Application.Services;

public sealed partial class CartaPartidaService : ServiceEntity<CartaPartida>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICartaPartidaRepository _repository;

    public CartaPartidaService(IUnitOfWork unitOfWork)
        : this(unitOfWork, ownsUnitOfWork: false)
    {
    }

    public CartaPartidaService(IUnitOfWorkFactory unitOfWorkFactory)
        : this(unitOfWorkFactory.Create(), ownsUnitOfWork: true)
    {
    }

    private CartaPartidaService(IUnitOfWork unitOfWork, bool ownsUnitOfWork) : base(unitOfWork, unitOfWork.CartaPartidaRepository, ownsUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.CartaPartidaRepository;
    }

    public Task<CartaPartida> AddCartaPartidaDefaultAsync(CartaPartida entity) =>
        AddDefaultAsync(entity);

    public Task DeleteCartaPartidaDefaultAsync(int id) =>
        DeleteDefaultAsync(id);

    public Task<CartaPartida?> ReadOIDCartaPartidaDefaultAsync(int id) =>
        ReadOIDDefaultAsync(id);

    public Task<IReadOnlyList<CartaPartida>> ReadAllCartaPartidaDefaultAsync() =>
        ReadAllDefaultAsync();

    public Task<CartaPartida> ModifierCartaPartidaDefaultAsync(CartaPartida entity) =>
        ModifierDefaultAsync(entity);

    public async Task<CartaPartida> NewAsync(int p_zona_oid, int p_original_oid)
    {
        ZonaPartida zonaEntity = await _unitOfWork.ZonaPartidaRepository.ReadAsync(p_zona_oid)
            ?? throw new ModelException("ZonaPartida not found for CartaPartida.Zona.");

        Carta originalEntity = await _unitOfWork.CartaRepository.ReadAsync(p_original_oid)
            ?? throw new ModelException("Carta not found for CartaPartida.Original.");

        CartaPartida entity = new CartaPartida
        {
            Zona = zonaEntity,
            Original = originalEntity,
        };

        await _repository.SaveAsync(entity);
        return entity;
    }

    public async Task DestroyAsync(int p_oid)
    {
        CartaPartida entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("CartaPartida not found.");
        await _repository.DeleteAsync(entity);
    }

    public async Task ModifyAsync(int p_oid, int p_zona_oid, int p_original_oid)
    {
        CartaPartida entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("CartaPartida not found.");
        await _repository.SaveAsync(entity);
    }

    public async Task<CartaPartida?> ReadOIDAsync(int p_oid)
    {
        return await _repository.ReadAsync(p_oid);
    }

    public async Task<IReadOnlyList<CartaPartida>> ReadAllAsync()
    {
        return await _repository.ReadAllAsync();
    }

}
