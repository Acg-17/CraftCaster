using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Application.Services;

public sealed partial class LineaMazoService : ServiceEntity<LineaMazo>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILineaMazoRepository _repository;

    public LineaMazoService(IUnitOfWork unitOfWork)
        : this(unitOfWork, ownsUnitOfWork: false)
    {
    }

    public LineaMazoService(IUnitOfWorkFactory unitOfWorkFactory)
        : this(unitOfWorkFactory.Create(), ownsUnitOfWork: true)
    {
    }

    private LineaMazoService(IUnitOfWork unitOfWork, bool ownsUnitOfWork) : base(unitOfWork, unitOfWork.LineaMazoRepository, ownsUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.LineaMazoRepository;
    }

    public Task<LineaMazo> AddLineaMazoDefaultAsync(LineaMazo entity) =>
        AddDefaultAsync(entity);

    public Task DeleteLineaMazoDefaultAsync(int id) =>
        DeleteDefaultAsync(id);

    public Task<LineaMazo?> ReadOIDLineaMazoDefaultAsync(int id) =>
        ReadOIDDefaultAsync(id);

    public Task<IReadOnlyList<LineaMazo>> ReadAllLineaMazoDefaultAsync() =>
        ReadAllDefaultAsync();

    public Task<LineaMazo> ModifierLineaMazoDefaultAsync(LineaMazo entity) =>
        ModifierDefaultAsync(entity);

    public async Task<LineaMazo> NewAsync(int p_cantidad)
    {
        LineaMazo entity = new LineaMazo
        {
            Cantidad = p_cantidad,
        };

        await _repository.SaveAsync(entity);
        return entity;
    }

    public async Task DestroyAsync(int p_oid)
    {
        LineaMazo entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("LineaMazo not found.");
        await _repository.DeleteAsync(entity);
    }

    public async Task ModifyAsync(int p_oid, int p_cantidad)
    {
        LineaMazo entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("LineaMazo not found.");
        entity.Cantidad = p_cantidad;
        await _repository.SaveAsync(entity);
    }

    public async Task<LineaMazo?> ReadOIDAsync(int p_oid)
    {
        return await _repository.ReadAsync(p_oid);
    }

    public async Task<IReadOnlyList<LineaMazo>> ReadAllAsync()
    {
        return await _repository.ReadAllAsync();
    }

    public async Task AddCartaAsync(int p_oid, IReadOnlyList<int> p_carta_oids)
    {
        LineaMazo entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("LineaMazo not found.");
        entity.Cartas ??= new List<Carta>();
        foreach (var relatedId in p_carta_oids)
        {
            Carta relatedEntity = await _unitOfWork.CartaRepository.ReadAsync(relatedId)
                ?? throw new ModelException("Carta not found for LineaMazo.Cartas.");
            if (!entity.Cartas.Contains(relatedEntity))
            {
                entity.Cartas.Add(relatedEntity);
            }
        }
        await _repository.SaveAsync(entity);
    }

    public async Task RemoveCartaAsync(int p_oid, IReadOnlyList<int> p_carta_oids)
    {
        LineaMazo entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("LineaMazo not found.");
        entity.Cartas ??= new List<Carta>();
        foreach (var relatedId in p_carta_oids)
        {
            Carta relatedEntity = await _unitOfWork.CartaRepository.ReadAsync(relatedId)
                ?? throw new ModelException("Carta not found for LineaMazo.Cartas.");
            if (!entity.Cartas.Remove(relatedEntity))
            {
                throw new ModelException("Carta is not related to LineaMazo.Cartas.");
            }
        }
        await _repository.SaveAsync(entity);
    }

}
