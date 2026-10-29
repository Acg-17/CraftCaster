using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Application.Services;

public sealed partial class ZonaPartidaService : ServiceEntity<ZonaPartida>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IZonaPartidaRepository _repository;

    public ZonaPartidaService(IUnitOfWork unitOfWork)
        : this(unitOfWork, ownsUnitOfWork: false)
    {
    }

    public ZonaPartidaService(IUnitOfWorkFactory unitOfWorkFactory)
        : this(unitOfWorkFactory.Create(), ownsUnitOfWork: true)
    {
    }

    private ZonaPartidaService(IUnitOfWork unitOfWork, bool ownsUnitOfWork) : base(unitOfWork, unitOfWork.ZonaPartidaRepository, ownsUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.ZonaPartidaRepository;
    }

    public Task<ZonaPartida> AddZonaPartidaDefaultAsync(ZonaPartida entity) =>
        AddDefaultAsync(entity);

    public Task DeleteZonaPartidaDefaultAsync(int id) =>
        DeleteDefaultAsync(id);

    public Task<ZonaPartida?> ReadOIDZonaPartidaDefaultAsync(int id) =>
        ReadOIDDefaultAsync(id);

    public Task<IReadOnlyList<ZonaPartida>> ReadAllZonaPartidaDefaultAsync() =>
        ReadAllDefaultAsync();

    public Task<ZonaPartida> ModifierZonaPartidaDefaultAsync(ZonaPartida entity) =>
        ModifierDefaultAsync(entity);

    public async Task<ZonaPartida> NewAsync(TipoZona p_tipoZona, int p_jugador_oid)
    {
        JugadorPartida jugadorEntity = await _unitOfWork.JugadorPartidaRepository.ReadAsync(p_jugador_oid)
            ?? throw new ModelException("JugadorPartida not found for ZonaPartida.Jugador.");

        ZonaPartida entity = new ZonaPartida
        {
            TipoZona = p_tipoZona,
            Jugador = jugadorEntity,
        };

        await _repository.SaveAsync(entity);
        return entity;
    }

    public async Task DestroyAsync(int p_oid)
    {
        ZonaPartida entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("ZonaPartida not found.");
        await _repository.DeleteAsync(entity);
    }

    public async Task ModifyAsync(int p_oid, TipoZona p_tipoZona, int p_jugador_oid)
    {
        ZonaPartida entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("ZonaPartida not found.");
        entity.TipoZona = p_tipoZona;
        await _repository.SaveAsync(entity);
    }

    public async Task<ZonaPartida?> ReadOIDAsync(int p_oid)
    {
        return await _repository.ReadAsync(p_oid);
    }

    public async Task<IReadOnlyList<ZonaPartida>> ReadAllAsync()
    {
        return await _repository.ReadAllAsync();
    }

    public async Task AddCartaAsync(int p_oid, IReadOnlyList<int> p_cartaPartida_oids)
    {
        ZonaPartida entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("ZonaPartida not found.");
        entity.Cartas ??= new List<CartaPartida>();
        foreach (var relatedId in p_cartaPartida_oids)
        {
            CartaPartida relatedEntity = await _unitOfWork.CartaPartidaRepository.ReadAsync(relatedId)
                ?? throw new ModelException("CartaPartida not found for ZonaPartida.Cartas.");
            if (!entity.Cartas.Contains(relatedEntity))
            {
                entity.Cartas.Add(relatedEntity);
            }
            relatedEntity.Zona = entity;
            await _unitOfWork.CartaPartidaRepository.SaveAsync(relatedEntity);
        }
        await _repository.SaveAsync(entity);
    }

    public async Task RemoveCartaAsync(int p_oid, IReadOnlyList<int> p_cartaPartida_oids)
    {
        ZonaPartida entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("ZonaPartida not found.");
        entity.Cartas ??= new List<CartaPartida>();
        foreach (var relatedId in p_cartaPartida_oids)
        {
            CartaPartida relatedEntity = await _unitOfWork.CartaPartidaRepository.ReadAsync(relatedId)
                ?? throw new ModelException("CartaPartida not found for ZonaPartida.Cartas.");
            if (!entity.Cartas.Remove(relatedEntity))
            {
                throw new ModelException("CartaPartida is not related to ZonaPartida.Cartas.");
            }
            if (relatedEntity.Zona is not null && relatedEntity.Zona.Equals(entity))
            {
                relatedEntity.Zona = null!;
            }
            await _unitOfWork.CartaPartidaRepository.SaveAsync(relatedEntity);
        }
        await _repository.SaveAsync(entity);
    }

}
