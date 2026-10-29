using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Application.Services;

public sealed partial class JugadorPartidaService : ServiceEntity<JugadorPartida>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJugadorPartidaRepository _repository;

    public JugadorPartidaService(IUnitOfWork unitOfWork)
        : this(unitOfWork, ownsUnitOfWork: false)
    {
    }

    public JugadorPartidaService(IUnitOfWorkFactory unitOfWorkFactory)
        : this(unitOfWorkFactory.Create(), ownsUnitOfWork: true)
    {
    }

    private JugadorPartidaService(IUnitOfWork unitOfWork, bool ownsUnitOfWork) : base(unitOfWork, unitOfWork.JugadorPartidaRepository, ownsUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.JugadorPartidaRepository;
    }

    public Task<JugadorPartida> AddJugadorPartidaDefaultAsync(JugadorPartida entity) =>
        AddDefaultAsync(entity);

    public Task DeleteJugadorPartidaDefaultAsync(int id) =>
        DeleteDefaultAsync(id);

    public Task<JugadorPartida?> ReadOIDJugadorPartidaDefaultAsync(int id) =>
        ReadOIDDefaultAsync(id);

    public Task<IReadOnlyList<JugadorPartida>> ReadAllJugadorPartidaDefaultAsync() =>
        ReadAllDefaultAsync();

    public Task<JugadorPartida> ModifierJugadorPartidaDefaultAsync(JugadorPartida entity) =>
        ModifierDefaultAsync(entity);

    public async Task<JugadorPartida> NewAsync(int p_vida, int p_numJugador, int p_partida_oid)
    {
        Partida partidaEntity = await _unitOfWork.PartidaRepository.ReadAsync(p_partida_oid)
            ?? throw new ModelException("Partida not found for JugadorPartida.Partida.");

        JugadorPartida entity = new JugadorPartida
        {
            Vida = p_vida,
            NumJugador = p_numJugador,
            Partida = partidaEntity,
        };

        await _repository.SaveAsync(entity);
        return entity;
    }

    public async Task DestroyAsync(int p_oid)
    {
        JugadorPartida entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("JugadorPartida not found.");
        await _repository.DeleteAsync(entity);
    }

    public async Task ModifyAsync(int p_oid, int p_vida, int p_numJugador, int p_partida_oid)
    {
        JugadorPartida entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("JugadorPartida not found.");
        entity.Vida = p_vida;
        entity.NumJugador = p_numJugador;
        await _repository.SaveAsync(entity);
    }

    public async Task<JugadorPartida?> ReadOIDAsync(int p_oid)
    {
        return await _repository.ReadAsync(p_oid);
    }

    public async Task<IReadOnlyList<JugadorPartida>> ReadAllAsync()
    {
        return await _repository.ReadAllAsync();
    }

    public async Task AddZonaAsync(int p_oid, IReadOnlyList<int> p_zonaPartida_oids)
    {
        JugadorPartida entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("JugadorPartida not found.");
        entity.Zonas ??= new List<ZonaPartida>();
        foreach (var relatedId in p_zonaPartida_oids)
        {
            ZonaPartida relatedEntity = await _unitOfWork.ZonaPartidaRepository.ReadAsync(relatedId)
                ?? throw new ModelException("ZonaPartida not found for JugadorPartida.Zonas.");
            if (!entity.Zonas.Contains(relatedEntity))
            {
                entity.Zonas.Add(relatedEntity);
            }
            relatedEntity.Jugador = entity;
            await _unitOfWork.ZonaPartidaRepository.SaveAsync(relatedEntity);
        }
        await _repository.SaveAsync(entity);
    }

    public async Task RemoveZonaAsync(int p_oid, IReadOnlyList<int> p_zonaPartida_oids)
    {
        JugadorPartida entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("JugadorPartida not found.");
        entity.Zonas ??= new List<ZonaPartida>();
        foreach (var relatedId in p_zonaPartida_oids)
        {
            ZonaPartida relatedEntity = await _unitOfWork.ZonaPartidaRepository.ReadAsync(relatedId)
                ?? throw new ModelException("ZonaPartida not found for JugadorPartida.Zonas.");
            if (!entity.Zonas.Remove(relatedEntity))
            {
                throw new ModelException("ZonaPartida is not related to JugadorPartida.Zonas.");
            }
            if (relatedEntity.Jugador is not null && relatedEntity.Jugador.Equals(entity))
            {
                relatedEntity.Jugador = null!;
            }
            await _unitOfWork.ZonaPartidaRepository.SaveAsync(relatedEntity);
        }
        await _repository.SaveAsync(entity);
    }

}
