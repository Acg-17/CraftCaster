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

    public async Task ApplyMazoRelationsAsync(Mazo entity, int? p_usuario_id, IReadOnlyList<LineaMazo> p_lineas)
    {
        if (p_usuario_id is null) throw new ModelException("Mazo.Usuario is required.");
        entity.Usuario = p_usuario_id is int selectedUsuario
            ? await _unitOfWork.UsuarioRepository.ReadAsync(selectedUsuario) ?? throw new ModelException("Usuario not found for Mazo.Usuario.") : null!;
        if (p_lineas.Count < 0 || p_lineas.Count > 500) throw new ModelException("Invalid number of Lineas rows.");
        foreach (var removed in entity.Lineas.Where(child => !p_lineas.Contains(child)).ToList()) entity.Lineas.Remove(removed);
        foreach (var child in p_lineas)
        {
            child.Mazo = entity;
            if (!entity.Lineas.Contains(child)) entity.Lineas.Add(child);
        }
    }

    public async Task<List<LineaMazo>> PrepareMazoLineasAsync(Mazo? parent, IReadOnlyList<(int Id, LineaMazo Values, int? CartaId)> rows)
    {
        if (rows.Count < 0 || rows.Count > 500) throw new ModelException("Invalid number of Lineas rows.");
        var seen = new HashSet<int>();
        var result = new List<LineaMazo>();
        foreach (var row in rows)
        {
            var existing = !EqualityComparer<int>.Default.Equals(row.Id, default!);
            if (existing && (parent is null || !seen.Add(row.Id) || !parent.Lineas.Any(child => child.Id.Equals(row.Id))))
                throw new ModelException("The row does not belong to this record or is duplicated.");
            var child = existing ? parent!.Lineas.Single(item => item.Id.Equals(row.Id)) : new LineaMazo();
            child.Cantidad = row.Values.Cantidad;
            if (row.CartaId is null) throw new ModelException("LineaMazo.Carta is required.");
            child.Carta = row.CartaId is int selectedCarta
                ? await _unitOfWork.CartaRepository.ReadAsync(selectedCarta) ?? throw new ModelException("Carta not found for LineaMazo.Carta.") : null!;
            result.Add(child);
        }
        return result;
    }

    public async Task<Mazo> NewAsync(string p_nombre, float p_costeMedio, int p_numCartas, EstadoMazo p_estadoMazo, bool p_esPublico, float p_valoracion, byte p_imagen, int p_usuario_oid)
    {
        Usuario usuarioEntity = await _unitOfWork.UsuarioRepository.ReadAsync(p_usuario_oid)
            ?? throw new ModelException("Usuario not found for Mazo.Usuario.");

        Mazo entity = new Mazo
        {
            Nombre = p_nombre,
            CosteMedio = p_costeMedio,
            NumCartas = p_numCartas,
            EstadoMazo = p_estadoMazo,
            EsPublico = p_esPublico,
            Valoracion = p_valoracion,
            Imagen = p_imagen,
            Usuario = usuarioEntity,
        };

        await _repository.SaveAsync(entity);
        return entity;
    }

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

    public async Task AddPartidasLocalAsync(int p_oid, IReadOnlyList<int> p_partida_oids)
    {
        Mazo entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Mazo not found.");
        entity.PartidasLocal ??= new List<Partida>();
        foreach (var relatedId in p_partida_oids)
        {
            Partida relatedEntity = await _unitOfWork.PartidaRepository.ReadAsync(relatedId)
                ?? throw new ModelException("Partida not found for Mazo.PartidasLocal.");
            if (!entity.PartidasLocal.Contains(relatedEntity))
            {
                entity.PartidasLocal.Add(relatedEntity);
            }
            relatedEntity.Mazo1 = entity;
            await _unitOfWork.PartidaRepository.SaveAsync(relatedEntity);
        }
        await _repository.SaveAsync(entity);
    }

    public async Task AddPartidasVisitanteAsync(int p_oid, IReadOnlyList<int> p_partida_oids)
    {
        Mazo entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Mazo not found.");
        entity.PartidasVisitante ??= new List<Partida>();
        foreach (var relatedId in p_partida_oids)
        {
            Partida relatedEntity = await _unitOfWork.PartidaRepository.ReadAsync(relatedId)
                ?? throw new ModelException("Partida not found for Mazo.PartidasVisitante.");
            if (!entity.PartidasVisitante.Contains(relatedEntity))
            {
                entity.PartidasVisitante.Add(relatedEntity);
            }
            relatedEntity.Mazo2 = entity;
            await _unitOfWork.PartidaRepository.SaveAsync(relatedEntity);
        }
        await _repository.SaveAsync(entity);
    }

    public async Task<IReadOnlyList<Mazo>> ObtenerMazosPublicosAsync()
    {
        return await _repository.ObtenerMazosPublicosAsync();
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
