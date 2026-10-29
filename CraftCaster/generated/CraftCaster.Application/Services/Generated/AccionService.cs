using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Application.Services;

public sealed partial class AccionService : ServiceEntity<Accion>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccionRepository _repository;

    public AccionService(IUnitOfWork unitOfWork)
        : this(unitOfWork, ownsUnitOfWork: false)
    {
    }

    public AccionService(IUnitOfWorkFactory unitOfWorkFactory)
        : this(unitOfWorkFactory.Create(), ownsUnitOfWork: true)
    {
    }

    private AccionService(IUnitOfWork unitOfWork, bool ownsUnitOfWork) : base(unitOfWork, unitOfWork.AccionRepository, ownsUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.AccionRepository;
    }

    public Task<Accion> AddAccionDefaultAsync(Accion entity) =>
        AddDefaultAsync(entity);

    public Task DeleteAccionDefaultAsync(int id) =>
        DeleteDefaultAsync(id);

    public Task<Accion?> ReadOIDAccionDefaultAsync(int id) =>
        ReadOIDDefaultAsync(id);

    public Task<IReadOnlyList<Accion>> ReadAllAccionDefaultAsync() =>
        ReadAllDefaultAsync();

    public Task<Accion> ModifierAccionDefaultAsync(Accion entity) =>
        ModifierDefaultAsync(entity);

    public async Task<Accion> NewAsync(TipoAccion p_name, int p_turno_oid)
    {
        Turno turnoEntity = await _unitOfWork.TurnoRepository.ReadAsync(p_turno_oid)
            ?? throw new ModelException("Turno not found for Accion.Turno.");

        Accion entity = new Accion
        {
            Name = p_name,
            Turno = turnoEntity,
        };

        await _repository.SaveAsync(entity);
        return entity;
    }

    public async Task DestroyAsync(int p_oid)
    {
        Accion entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Accion not found.");
        await _repository.DeleteAsync(entity);
    }

    public async Task ModifyAsync(int p_oid, TipoAccion p_name, int p_turno_oid)
    {
        Accion entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Accion not found.");
        entity.Name = p_name;
        await _repository.SaveAsync(entity);
    }

    public async Task<Accion?> ReadOIDAsync(int p_oid)
    {
        return await _repository.ReadAsync(p_oid);
    }

    public async Task<IReadOnlyList<Accion>> ReadAllAsync()
    {
        return await _repository.ReadAllAsync();
    }

    public async Task AddOrigenAsync(int p_oid, int p_cartaPartida_oid)
    {
        Accion entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Accion not found.");
        CartaPartida relatedEntity = await _unitOfWork.CartaPartidaRepository.ReadAsync(p_cartaPartida_oid)
            ?? throw new ModelException("CartaPartida not found for Accion.Origen.");
        entity.Origen = relatedEntity;
        await _repository.SaveAsync(entity);
    }

    public async Task RemoveOrigenAsync(int p_oid, int p_cartaPartida_oid)
    {
        Accion entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Accion not found.");
        CartaPartida relatedEntity = await _unitOfWork.CartaPartidaRepository.ReadAsync(p_cartaPartida_oid)
            ?? throw new ModelException("CartaPartida not found for Accion.Origen.");
        if (entity.Origen is null || !entity.Origen.Equals(relatedEntity))
        {
            throw new ModelException("CartaPartida is not related to Accion.Origen.");
        }
        entity.Origen = null!;
        await _repository.SaveAsync(entity);
    }

    public async Task AddObjetivoAsync(int p_oid, IReadOnlyList<int> p_cartaPartida_oids)
    {
        Accion entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Accion not found.");
        entity.Objetivo ??= new List<CartaPartida>();
        foreach (var relatedId in p_cartaPartida_oids)
        {
            CartaPartida relatedEntity = await _unitOfWork.CartaPartidaRepository.ReadAsync(relatedId)
                ?? throw new ModelException("CartaPartida not found for Accion.Objetivo.");
            if (!entity.Objetivo.Contains(relatedEntity))
            {
                entity.Objetivo.Add(relatedEntity);
            }
        }
        await _repository.SaveAsync(entity);
    }

    public async Task RemoveObjetivoAsync(int p_oid, IReadOnlyList<int> p_cartaPartida_oids)
    {
        Accion entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Accion not found.");
        entity.Objetivo ??= new List<CartaPartida>();
        foreach (var relatedId in p_cartaPartida_oids)
        {
            CartaPartida relatedEntity = await _unitOfWork.CartaPartidaRepository.ReadAsync(relatedId)
                ?? throw new ModelException("CartaPartida not found for Accion.Objetivo.");
            if (!entity.Objetivo.Remove(relatedEntity))
            {
                throw new ModelException("CartaPartida is not related to Accion.Objetivo.");
            }
        }
        await _repository.SaveAsync(entity);
    }

}
