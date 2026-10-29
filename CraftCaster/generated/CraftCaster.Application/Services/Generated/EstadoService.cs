using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Application.Services;

public sealed partial class EstadoService : ServiceEntity<Estado>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEstadoRepository _repository;

    public EstadoService(IUnitOfWork unitOfWork)
        : this(unitOfWork, ownsUnitOfWork: false)
    {
    }

    public EstadoService(IUnitOfWorkFactory unitOfWorkFactory)
        : this(unitOfWorkFactory.Create(), ownsUnitOfWork: true)
    {
    }

    private EstadoService(IUnitOfWork unitOfWork, bool ownsUnitOfWork) : base(unitOfWork, unitOfWork.EstadoRepository, ownsUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.EstadoRepository;
    }

    public Task<Estado> AddEstadoDefaultAsync(Estado entity) =>
        AddDefaultAsync(entity);

    public Task DeleteEstadoDefaultAsync(int id) =>
        DeleteDefaultAsync(id);

    public Task<Estado?> ReadOIDEstadoDefaultAsync(int id) =>
        ReadOIDDefaultAsync(id);

    public Task<IReadOnlyList<Estado>> ReadAllEstadoDefaultAsync() =>
        ReadAllDefaultAsync();

    public Task<Estado> ModifierEstadoDefaultAsync(Estado entity) =>
        ModifierDefaultAsync(entity);

    public async Task<Estado> NewAsync(TipoEstado p_tipo)
    {
        Estado entity = new Estado
        {
            Tipo = p_tipo,
        };

        await _repository.SaveAsync(entity);
        return entity;
    }

    public async Task DestroyAsync(int p_oid)
    {
        Estado entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Estado not found.");
        await _repository.DeleteAsync(entity);
    }

    public async Task ModifyAsync(int p_oid, TipoEstado p_tipo)
    {
        Estado entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Estado not found.");
        entity.Tipo = p_tipo;
        await _repository.SaveAsync(entity);
    }

    public async Task<Estado?> ReadOIDAsync(int p_oid)
    {
        return await _repository.ReadAsync(p_oid);
    }

    public async Task<IReadOnlyList<Estado>> ReadAllAsync()
    {
        return await _repository.ReadAllAsync();
    }

    public async Task AddHabilidadeAsync(int p_oid, IReadOnlyList<int> p_habilidad_oids)
    {
        Estado entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Estado not found.");
        entity.Habilidades ??= new List<Habilidad>();
        foreach (var relatedId in p_habilidad_oids)
        {
            Habilidad relatedEntity = await _unitOfWork.HabilidadRepository.ReadAsync(relatedId)
                ?? throw new ModelException("Habilidad not found for Estado.Habilidades.");
            if (!entity.Habilidades.Contains(relatedEntity))
            {
                entity.Habilidades.Add(relatedEntity);
            }
            relatedEntity.Estado = entity;
            await _unitOfWork.HabilidadRepository.SaveAsync(relatedEntity);
        }
        await _repository.SaveAsync(entity);
    }

    public async Task RemoveHabilidadeAsync(int p_oid, IReadOnlyList<int> p_habilidad_oids)
    {
        Estado entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Estado not found.");
        entity.Habilidades ??= new List<Habilidad>();
        foreach (var relatedId in p_habilidad_oids)
        {
            Habilidad relatedEntity = await _unitOfWork.HabilidadRepository.ReadAsync(relatedId)
                ?? throw new ModelException("Habilidad not found for Estado.Habilidades.");
            if (!entity.Habilidades.Remove(relatedEntity))
            {
                throw new ModelException("Habilidad is not related to Estado.Habilidades.");
            }
            if (relatedEntity.Estado is not null && relatedEntity.Estado.Equals(entity))
            {
                relatedEntity.Estado = null!;
            }
            await _unitOfWork.HabilidadRepository.SaveAsync(relatedEntity);
        }
        await _repository.SaveAsync(entity);
    }

}
