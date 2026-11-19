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

    public async Task ApplyEstadoRelationsAsync(Estado entity, int? p_carta_id, IReadOnlyList<Habilidad> p_habilidades)
    {
        if (p_carta_id is null) throw new ModelException("Estado.Carta is required.");
        entity.Carta = p_carta_id is int selectedCarta
            ? await _unitOfWork.CartaRepository.ReadAsync(selectedCarta) ?? throw new ModelException("Carta not found for Estado.Carta.") : null!;
        if (p_habilidades.Count < 0 || p_habilidades.Count > 500) throw new ModelException("Invalid number of Habilidades rows.");
        foreach (var removed in entity.Habilidades.Where(child => !p_habilidades.Contains(child)).ToList()) entity.Habilidades.Remove(removed);
        foreach (var child in p_habilidades)
        {
            child.Estado = entity;
            if (!entity.Habilidades.Contains(child)) entity.Habilidades.Add(child);
        }
    }

    public async Task<List<Habilidad>> PrepareEstadoHabilidadesAsync(Estado? parent, IReadOnlyList<(int Id, Habilidad Values)> rows)
    {
        if (rows.Count < 0 || rows.Count > 500) throw new ModelException("Invalid number of Habilidades rows.");
        var seen = new HashSet<int>();
        var result = new List<Habilidad>();
        foreach (var row in rows)
        {
            var existing = !EqualityComparer<int>.Default.Equals(row.Id, default!);
            if (existing && (parent is null || !seen.Add(row.Id) || !parent.Habilidades.Any(child => child.Id.Equals(row.Id))))
                throw new ModelException("The row does not belong to this record or is duplicated.");
            var child = existing ? parent!.Habilidades.Single(item => item.Id.Equals(row.Id)) : new Habilidad();
            child.Tipo = row.Values.Tipo;
            child.Cantidad = row.Values.Cantidad;
            child.TipoObjetivo = row.Values.TipoObjetivo;
            child.Objetivo = row.Values.Objetivo;
            result.Add(child);
        }
        return result;
    }

    public async Task<Estado> NewAsync(TipoEstado p_tipo, int p_carta_oid)
    {
        Carta cartaEntity = await _unitOfWork.CartaRepository.ReadAsync(p_carta_oid)
            ?? throw new ModelException("Carta not found for Estado.Carta.");

        Estado entity = new Estado
        {
            Tipo = p_tipo,
            Carta = cartaEntity,
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

    public async Task ModifyAsync(int p_oid, TipoEstado p_tipo, int p_carta_oid)
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

}
