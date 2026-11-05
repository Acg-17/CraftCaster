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

    public async Task<LineaMazo> NewAsync(int p_cantidad, int p_mazo_oid, int p_carta_oid)
    {
        Mazo mazoEntity = await _unitOfWork.MazoRepository.ReadAsync(p_mazo_oid)
            ?? throw new ModelException("Mazo not found for LineaMazo.Mazo.");

        Carta cartaEntity = await _unitOfWork.CartaRepository.ReadAsync(p_carta_oid)
            ?? throw new ModelException("Carta not found for LineaMazo.Carta.");

        LineaMazo entity = new LineaMazo
        {
            Cantidad = p_cantidad,
            Mazo = mazoEntity,
            Carta = cartaEntity,
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

    public async Task ModifyAsync(int p_oid, int p_cantidad, int p_mazo_oid, int p_carta_oid)
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

}
