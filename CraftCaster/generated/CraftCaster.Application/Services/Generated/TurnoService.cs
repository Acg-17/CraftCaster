using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Application.Services;

public sealed partial class TurnoService : ServiceEntity<Turno>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITurnoRepository _repository;

    public TurnoService(IUnitOfWork unitOfWork)
        : this(unitOfWork, ownsUnitOfWork: false)
    {
    }

    public TurnoService(IUnitOfWorkFactory unitOfWorkFactory)
        : this(unitOfWorkFactory.Create(), ownsUnitOfWork: true)
    {
    }

    private TurnoService(IUnitOfWork unitOfWork, bool ownsUnitOfWork) : base(unitOfWork, unitOfWork.TurnoRepository, ownsUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.TurnoRepository;
    }

    public Task<Turno> AddTurnoDefaultAsync(Turno entity) =>
        AddDefaultAsync(entity);

    public Task DeleteTurnoDefaultAsync(int id) =>
        DeleteDefaultAsync(id);

    public Task<Turno?> ReadOIDTurnoDefaultAsync(int id) =>
        ReadOIDDefaultAsync(id);

    public Task<IReadOnlyList<Turno>> ReadAllTurnoDefaultAsync() =>
        ReadAllDefaultAsync();

    public Task<Turno> ModifierTurnoDefaultAsync(Turno entity) =>
        ModifierDefaultAsync(entity);

    public async Task ApplyTurnoRelationsAsync(Turno entity, int? p_partida_id, int? p_jugador_id, IReadOnlyList<Accion> p_acciones)
    {
        if (p_partida_id is null) throw new ModelException("Turno.Partida is required.");
        entity.Partida = p_partida_id is int selectedPartida
            ? await _unitOfWork.PartidaRepository.ReadAsync(selectedPartida) ?? throw new ModelException("Partida not found for Turno.Partida.") : null!;
        if (p_jugador_id is null) throw new ModelException("Turno.Jugador is required.");
        entity.Jugador = p_jugador_id is int selectedJugador
            ? await _unitOfWork.JugadorPartidaRepository.ReadAsync(selectedJugador) ?? throw new ModelException("JugadorPartida not found for Turno.Jugador.") : null!;
        if (p_acciones.Count < 0 || p_acciones.Count > 500) throw new ModelException("Invalid number of Acciones rows.");
        foreach (var removed in entity.Acciones.Where(child => !p_acciones.Contains(child)).ToList()) entity.Acciones.Remove(removed);
        foreach (var child in p_acciones)
        {
            child.Turno = entity;
            if (!entity.Acciones.Contains(child)) entity.Acciones.Add(child);
        }
    }

    public async Task<List<Accion>> PrepareTurnoAccionesAsync(Turno? parent, IReadOnlyList<(int Id, Accion Values, int? OrigenId)> rows)
    {
        if (rows.Count < 0 || rows.Count > 500) throw new ModelException("Invalid number of Acciones rows.");
        var seen = new HashSet<int>();
        var result = new List<Accion>();
        foreach (var row in rows)
        {
            var existing = !EqualityComparer<int>.Default.Equals(row.Id, default!);
            if (existing && (parent is null || !seen.Add(row.Id) || !parent.Acciones.Any(child => child.Id.Equals(row.Id))))
                throw new ModelException("The row does not belong to this record or is duplicated.");
            var child = existing ? parent!.Acciones.Single(item => item.Id.Equals(row.Id)) : new Accion();
            child.Name = row.Values.Name;
            child.Origen = row.OrigenId is int selectedOrigen
                ? await _unitOfWork.CartaPartidaRepository.ReadAsync(selectedOrigen) ?? throw new ModelException("CartaPartida not found for Accion.Origen.") : null!;
            result.Add(child);
        }
        return result;
    }

    public async Task<Turno> NewAsync(int p_numTurno, int p_manaTurno, int p_partida_oid, int p_jugador_oid)
    {
        Partida partidaEntity = await _unitOfWork.PartidaRepository.ReadAsync(p_partida_oid)
            ?? throw new ModelException("Partida not found for Turno.Partida.");

        JugadorPartida jugadorEntity = await _unitOfWork.JugadorPartidaRepository.ReadAsync(p_jugador_oid)
            ?? throw new ModelException("JugadorPartida not found for Turno.Jugador.");

        Turno entity = new Turno
        {
            NumTurno = p_numTurno,
            ManaTurno = p_manaTurno,
            Partida = partidaEntity,
            Jugador = jugadorEntity,
        };

        await _repository.SaveAsync(entity);
        return entity;
    }

    public async Task DestroyAsync(int p_oid)
    {
        Turno entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Turno not found.");
        await _repository.DeleteAsync(entity);
    }

    public async Task ModifyAsync(int p_oid, int p_numTurno, int p_manaTurno, int p_partida_oid, int p_jugador_oid)
    {
        Turno entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Turno not found.");
        entity.NumTurno = p_numTurno;
        entity.ManaTurno = p_manaTurno;
        await _repository.SaveAsync(entity);
    }

    public async Task<Turno?> ReadOIDAsync(int p_oid)
    {
        return await _repository.ReadAsync(p_oid);
    }

    public async Task<IReadOnlyList<Turno>> ReadAllAsync()
    {
        return await _repository.ReadAllAsync();
    }

    public async Task AddAccioneAsync(int p_oid, IReadOnlyList<int> p_accion_oids)
    {
        Turno entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Turno not found.");
        entity.Acciones ??= new List<Accion>();
        foreach (var relatedId in p_accion_oids)
        {
            Accion relatedEntity = await _unitOfWork.AccionRepository.ReadAsync(relatedId)
                ?? throw new ModelException("Accion not found for Turno.Acciones.");
            if (!entity.Acciones.Contains(relatedEntity))
            {
                entity.Acciones.Add(relatedEntity);
            }
            relatedEntity.Turno = entity;
            await _unitOfWork.AccionRepository.SaveAsync(relatedEntity);
        }
        await _repository.SaveAsync(entity);
    }

}
