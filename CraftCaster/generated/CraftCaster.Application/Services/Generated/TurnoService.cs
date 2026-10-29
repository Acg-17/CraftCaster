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

    public async Task<Turno> NewAsync(int p_mana, int p_partida_oid, int p_jugador_oid)
    {
        Partida partidaEntity = await _unitOfWork.PartidaRepository.ReadAsync(p_partida_oid)
            ?? throw new ModelException("Partida not found for Turno.Partida.");

        JugadorPartida jugadorEntity = await _unitOfWork.JugadorPartidaRepository.ReadAsync(p_jugador_oid)
            ?? throw new ModelException("JugadorPartida not found for Turno.Jugador.");

        Turno entity = new Turno
        {
            Mana = p_mana,
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

    public async Task ModifyAsync(int p_oid, int p_mana, int p_partida_oid, int p_jugador_oid)
    {
        Turno entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Turno not found.");
        entity.Mana = p_mana;
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

    public async Task RemoveAccioneAsync(int p_oid, IReadOnlyList<int> p_accion_oids)
    {
        Turno entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Turno not found.");
        entity.Acciones ??= new List<Accion>();
        foreach (var relatedId in p_accion_oids)
        {
            Accion relatedEntity = await _unitOfWork.AccionRepository.ReadAsync(relatedId)
                ?? throw new ModelException("Accion not found for Turno.Acciones.");
            if (!entity.Acciones.Remove(relatedEntity))
            {
                throw new ModelException("Accion is not related to Turno.Acciones.");
            }
            if (relatedEntity.Turno is not null && relatedEntity.Turno.Equals(entity))
            {
                relatedEntity.Turno = null!;
            }
            await _unitOfWork.AccionRepository.SaveAsync(relatedEntity);
        }
        await _repository.SaveAsync(entity);
    }

}
