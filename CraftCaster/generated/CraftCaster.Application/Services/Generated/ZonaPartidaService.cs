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

    public async Task ApplyZonaPartidaRelationsAsync(ZonaPartida entity, int? p_jugador_id, IReadOnlyList<CartaPartida> p_cartas)
    {
        if (p_jugador_id is null) throw new ModelException("ZonaPartida.Jugador is required.");
        entity.Jugador = p_jugador_id is int selectedJugador
            ? await _unitOfWork.JugadorPartidaRepository.ReadAsync(selectedJugador) ?? throw new ModelException("JugadorPartida not found for ZonaPartida.Jugador.") : null!;
        if (p_cartas.Count < 0 || p_cartas.Count > 500) throw new ModelException("Invalid number of Cartas rows.");
        foreach (var removed in entity.Cartas.Where(child => !p_cartas.Contains(child)).ToList()) entity.Cartas.Remove(removed);
        foreach (var child in p_cartas)
        {
            child.Zona = entity;
            if (!entity.Cartas.Contains(child)) entity.Cartas.Add(child);
        }
    }

    public async Task<List<CartaPartida>> PrepareZonaPartidaCartasAsync(ZonaPartida? parent, IReadOnlyList<(int Id, CartaPartida Values, int? OriginalId)> rows)
    {
        if (rows.Count < 0 || rows.Count > 500) throw new ModelException("Invalid number of Cartas rows.");
        var seen = new HashSet<int>();
        var result = new List<CartaPartida>();
        foreach (var row in rows)
        {
            var existing = !EqualityComparer<int>.Default.Equals(row.Id, default!);
            if (existing && (parent is null || !seen.Add(row.Id) || !parent.Cartas.Any(child => child.Id.Equals(row.Id))))
                throw new ModelException("The row does not belong to this record or is duplicated.");
            var child = existing ? parent!.Cartas.Single(item => item.Id.Equals(row.Id)) : new CartaPartida();
            child.VidaActual = row.Values.VidaActual;
            child.AtaqueActual = row.Values.AtaqueActual;
            child.PuedeAtacar = row.Values.PuedeAtacar;
            if (row.OriginalId is null) throw new ModelException("CartaPartida.Original is required.");
            child.Original = row.OriginalId is int selectedOriginal
                ? await _unitOfWork.CartaRepository.ReadAsync(selectedOriginal) ?? throw new ModelException("Carta not found for CartaPartida.Original.") : null!;
            result.Add(child);
        }
        return result;
    }

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

}
