using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Application.Services;

public sealed partial class HabilidadService : ServiceEntity<Habilidad>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHabilidadRepository _repository;

    public HabilidadService(IUnitOfWork unitOfWork)
        : this(unitOfWork, ownsUnitOfWork: false)
    {
    }

    public HabilidadService(IUnitOfWorkFactory unitOfWorkFactory)
        : this(unitOfWorkFactory.Create(), ownsUnitOfWork: true)
    {
    }

    private HabilidadService(IUnitOfWork unitOfWork, bool ownsUnitOfWork) : base(unitOfWork, unitOfWork.HabilidadRepository, ownsUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.HabilidadRepository;
    }

    public Task<Habilidad> AddHabilidadDefaultAsync(Habilidad entity) =>
        AddDefaultAsync(entity);

    public Task DeleteHabilidadDefaultAsync(int id) =>
        DeleteDefaultAsync(id);

    public Task<Habilidad?> ReadOIDHabilidadDefaultAsync(int id) =>
        ReadOIDDefaultAsync(id);

    public Task<IReadOnlyList<Habilidad>> ReadAllHabilidadDefaultAsync() =>
        ReadAllDefaultAsync();

    public Task<Habilidad> ModifierHabilidadDefaultAsync(Habilidad entity) =>
        ModifierDefaultAsync(entity);

    public async Task<Habilidad> NewAsync(TipoHabilidad p_tipo, int p_cantidad, Objetivo p_objetivo, int p_estado_oid)
    {
        Estado estadoEntity = await _unitOfWork.EstadoRepository.ReadAsync(p_estado_oid)
            ?? throw new ModelException("Estado not found for Habilidad.Estado.");

        Habilidad entity = new Habilidad
        {
            Tipo = p_tipo,
            Cantidad = p_cantidad,
            Objetivo = p_objetivo,
            Estado = estadoEntity,
        };

        await _repository.SaveAsync(entity);
        return entity;
    }

    public async Task DestroyAsync(int p_oid)
    {
        Habilidad entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Habilidad not found.");
        await _repository.DeleteAsync(entity);
    }

    public async Task ModifyAsync(int p_oid, TipoHabilidad p_tipo, int p_cantidad, Objetivo p_objetivo, int p_estado_oid)
    {
        Habilidad entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Habilidad not found.");
        entity.Tipo = p_tipo;
        entity.Cantidad = p_cantidad;
        entity.Objetivo = p_objetivo;
        await _repository.SaveAsync(entity);
    }

    public async Task<Habilidad?> ReadOIDAsync(int p_oid)
    {
        return await _repository.ReadAsync(p_oid);
    }

    public async Task<IReadOnlyList<Habilidad>> ReadAllAsync()
    {
        return await _repository.ReadAllAsync();
    }

}
