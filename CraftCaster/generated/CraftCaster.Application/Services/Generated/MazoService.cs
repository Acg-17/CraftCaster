using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Application.Services;

public sealed partial class MazoService : ServiceEntity<Mazo>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMazoRepository _repository;

    public MazoService(IUnitOfWork unitOfWork)
        : this(unitOfWork, ownsUnitOfWork: false)
    {
    }

    public MazoService(IUnitOfWorkFactory unitOfWorkFactory)
        : this(unitOfWorkFactory.Create(), ownsUnitOfWork: true)
    {
    }

    private MazoService(IUnitOfWork unitOfWork, bool ownsUnitOfWork) : base(unitOfWork, unitOfWork.MazoRepository, ownsUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.MazoRepository;
    }

    public Task<Mazo> AddMazoDefaultAsync(Mazo entity) =>
        AddDefaultAsync(entity);

    public Task DeleteMazoDefaultAsync(int id) =>
        DeleteDefaultAsync(id);

    public Task<Mazo?> ReadOIDMazoDefaultAsync(int id) =>
        ReadOIDDefaultAsync(id);

    public Task<IReadOnlyList<Mazo>> ReadAllMazoDefaultAsync() =>
        ReadAllDefaultAsync();

    public Task<Mazo> ModifierMazoDefaultAsync(Mazo entity) =>
        ModifierDefaultAsync(entity);

    public async Task DestroyAsync(int p_oid)
    {
        Mazo entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Mazo not found.");
        await _repository.DeleteAsync(entity);
    }

    public async Task ModifyAsync(int p_oid, string p_nombre, float p_costeMedio, int p_numCartas, EstadoMazo p_estadoMazo, bool p_esPublico, float p_valoracion, byte p_imagen, int p_usuario_oid)
    {
        Mazo entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Mazo not found.");
        entity.Nombre = p_nombre;
        entity.CosteMedio = p_costeMedio;
        entity.NumCartas = p_numCartas;
        entity.EstadoMazo = p_estadoMazo;
        entity.EsPublico = p_esPublico;
        entity.Valoracion = p_valoracion;
        entity.Imagen = p_imagen;
        await _repository.SaveAsync(entity);
    }

    public async Task<Mazo?> ReadOIDAsync(int p_oid)
    {
        return await _repository.ReadAsync(p_oid);
    }

    public async Task<IReadOnlyList<Mazo>> ReadAllAsync()
    {
        return await _repository.ReadAllAsync();
    }

    public async Task AddLineaAsync(int p_oid, IReadOnlyList<int> p_lineaMazo_oids)
    {
        Mazo entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Mazo not found.");
        entity.Lineas ??= new List<LineaMazo>();
        foreach (var relatedId in p_lineaMazo_oids)
        {
            LineaMazo relatedEntity = await _unitOfWork.LineaMazoRepository.ReadAsync(relatedId)
                ?? throw new ModelException("LineaMazo not found for Mazo.Lineas.");
            if (!entity.Lineas.Contains(relatedEntity))
            {
                entity.Lineas.Add(relatedEntity);
            }
            relatedEntity.Mazo = entity;
            await _unitOfWork.LineaMazoRepository.SaveAsync(relatedEntity);
        }
        await _repository.SaveAsync(entity);
    }

    public async Task RemoveLineaAsync(int p_oid, IReadOnlyList<int> p_lineaMazo_oids)
    {
        Mazo entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Mazo not found.");
        entity.Lineas ??= new List<LineaMazo>();
        foreach (var relatedId in p_lineaMazo_oids)
        {
            LineaMazo relatedEntity = await _unitOfWork.LineaMazoRepository.ReadAsync(relatedId)
                ?? throw new ModelException("LineaMazo not found for Mazo.Lineas.");
            if (!entity.Lineas.Remove(relatedEntity))
            {
                throw new ModelException("LineaMazo is not related to Mazo.Lineas.");
            }
            if (relatedEntity.Mazo is not null && relatedEntity.Mazo.Equals(entity))
            {
                relatedEntity.Mazo = null!;
            }
            await _unitOfWork.LineaMazoRepository.SaveAsync(relatedEntity);
        }
        await _repository.SaveAsync(entity);
    }

    public async Task<IReadOnlyList<Mazo>> ObtenerMazosUsuarioAsync(int p_oid)
    {
        return await _repository.ObtenerMazosUsuarioAsync(p_oid);
    }

    public async Task<IReadOnlyList<Mazo>> ObtenerMazosPorNombreAsync(string p_nombre)
    {
        return await _repository.ObtenerMazosPorNombreAsync(p_nombre);
    }

    public async Task<IReadOnlyList<Mazo>> ObtenerMazoPorCosteMedioAsync(int p_min_mana, int p_max_mana)
    {
        return await _repository.ObtenerMazoPorCosteMedioAsync(p_min_mana, p_max_mana);
    }

    public async Task<IReadOnlyList<Mazo>> ObtenerMazoPorValoracionAsync(int p_min_val, int p_max_val)
    {
        return await _repository.ObtenerMazoPorValoracionAsync(p_min_val, p_max_val);
    }

    public async Task<IReadOnlyList<Mazo>> ObtenerMazoJugador1Async(int p_oid)
    {
        return await _repository.ObtenerMazoJugador1Async(p_oid);
    }

    public async Task<IReadOnlyList<Mazo>> ObtenerMazoJugador2Async(int p_oid)
    {
        return await _repository.ObtenerMazoJugador2Async(p_oid);
    }

}
