using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Application.Services;

public sealed partial class PartidaService : ServiceEntity<Partida>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPartidaRepository _repository;

    public PartidaService(IUnitOfWork unitOfWork)
        : this(unitOfWork, ownsUnitOfWork: false)
    {
    }

    public PartidaService(IUnitOfWorkFactory unitOfWorkFactory)
        : this(unitOfWorkFactory.Create(), ownsUnitOfWork: true)
    {
    }

    private PartidaService(IUnitOfWork unitOfWork, bool ownsUnitOfWork) : base(unitOfWork, unitOfWork.PartidaRepository, ownsUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.PartidaRepository;
    }

    public Task<Partida> AddPartidaDefaultAsync(Partida entity) =>
        AddDefaultAsync(entity);

    public Task DeletePartidaDefaultAsync(int id) =>
        DeleteDefaultAsync(id);

    public Task<Partida?> ReadOIDPartidaDefaultAsync(int id) =>
        ReadOIDDefaultAsync(id);

    public Task<IReadOnlyList<Partida>> ReadAllPartidaDefaultAsync() =>
        ReadAllDefaultAsync();

    public Task<Partida> ModifierPartidaDefaultAsync(Partida entity) =>
        ModifierDefaultAsync(entity);

    public async Task<Partida> NewAsync(EstadoPartida p_estado, DateTime p_fechaInicio, int p_usuario1_oid, int p_mazo1_oid)
    {
        Usuario usuario1Entity = await _unitOfWork.UsuarioRepository.ReadAsync(p_usuario1_oid)
            ?? throw new ModelException("Usuario not found for Partida.Usuario1.");

        Mazo mazo1Entity = await _unitOfWork.MazoRepository.ReadAsync(p_mazo1_oid)
            ?? throw new ModelException("Mazo not found for Partida.Mazo1.");

        Partida entity = new Partida
        {
            Estado = p_estado,
            FechaInicio = p_fechaInicio,
            Usuario1 = usuario1Entity,
            Mazo1 = mazo1Entity,
        };

        await _repository.SaveAsync(entity);
        return entity;
    }

    public async Task DestroyAsync(int p_oid)
    {
        Partida entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Partida not found.");
        await _repository.DeleteAsync(entity);
    }

    public async Task ModifyAsync(int p_oid, EstadoPartida p_estado, DateTime p_fechaInicio, int p_usuario1_oid, int p_mazo1_oid)
    {
        Partida entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Partida not found.");
        entity.Estado = p_estado;
        entity.FechaInicio = p_fechaInicio;
        await _repository.SaveAsync(entity);
    }

    public async Task<Partida?> ReadOIDAsync(int p_oid)
    {
        return await _repository.ReadAsync(p_oid);
    }

    public async Task<IReadOnlyList<Partida>> ReadAllAsync()
    {
        return await _repository.ReadAllAsync();
    }

    public async Task AddTurnoAsync(int p_oid, IReadOnlyList<int> p_turno_oids)
    {
        Partida entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Partida not found.");
        entity.Turnos ??= new List<Turno>();
        foreach (var relatedId in p_turno_oids)
        {
            Turno relatedEntity = await _unitOfWork.TurnoRepository.ReadAsync(relatedId)
                ?? throw new ModelException("Turno not found for Partida.Turnos.");
            if (!entity.Turnos.Contains(relatedEntity))
            {
                entity.Turnos.Add(relatedEntity);
            }
            relatedEntity.Partida = entity;
            await _unitOfWork.TurnoRepository.SaveAsync(relatedEntity);
        }
        await _repository.SaveAsync(entity);
    }

    public async Task RemoveTurnoAsync(int p_oid, IReadOnlyList<int> p_turno_oids)
    {
        Partida entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Partida not found.");
        entity.Turnos ??= new List<Turno>();
        foreach (var relatedId in p_turno_oids)
        {
            Turno relatedEntity = await _unitOfWork.TurnoRepository.ReadAsync(relatedId)
                ?? throw new ModelException("Turno not found for Partida.Turnos.");
            if (!entity.Turnos.Remove(relatedEntity))
            {
                throw new ModelException("Turno is not related to Partida.Turnos.");
            }
            if (relatedEntity.Partida is not null && relatedEntity.Partida.Equals(entity))
            {
                relatedEntity.Partida = null!;
            }
            await _unitOfWork.TurnoRepository.SaveAsync(relatedEntity);
        }
        await _repository.SaveAsync(entity);
    }

    public async Task AddJugadoresAsync(int p_oid, IReadOnlyList<int> p_jugadorPartida_oids)
    {
        Partida entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Partida not found.");
        entity.Jugadores ??= new List<JugadorPartida>();
        foreach (var relatedId in p_jugadorPartida_oids)
        {
            JugadorPartida relatedEntity = await _unitOfWork.JugadorPartidaRepository.ReadAsync(relatedId)
                ?? throw new ModelException("JugadorPartida not found for Partida.Jugadores.");
            if (!entity.Jugadores.Contains(relatedEntity))
            {
                entity.Jugadores.Add(relatedEntity);
            }
            relatedEntity.Partida = entity;
            await _unitOfWork.JugadorPartidaRepository.SaveAsync(relatedEntity);
        }
        await _repository.SaveAsync(entity);
    }

    public async Task RemoveJugadoresAsync(int p_oid, IReadOnlyList<int> p_jugadorPartida_oids)
    {
        Partida entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Partida not found.");
        entity.Jugadores ??= new List<JugadorPartida>();
        foreach (var relatedId in p_jugadorPartida_oids)
        {
            JugadorPartida relatedEntity = await _unitOfWork.JugadorPartidaRepository.ReadAsync(relatedId)
                ?? throw new ModelException("JugadorPartida not found for Partida.Jugadores.");
            if (!entity.Jugadores.Remove(relatedEntity))
            {
                throw new ModelException("JugadorPartida is not related to Partida.Jugadores.");
            }
            if (relatedEntity.Partida is not null && relatedEntity.Partida.Equals(entity))
            {
                relatedEntity.Partida = null!;
            }
            await _unitOfWork.JugadorPartidaRepository.SaveAsync(relatedEntity);
        }
        await _repository.SaveAsync(entity);
    }

    public async Task AddUsuario2Async(int p_oid, int p_usuario_oid)
    {
        Partida entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Partida not found.");
        Usuario relatedEntity = await _unitOfWork.UsuarioRepository.ReadAsync(p_usuario_oid)
            ?? throw new ModelException("Usuario not found for Partida.Usuario2.");
        entity.Usuario2 = relatedEntity;
        relatedEntity.PartidasVisitante ??= new List<Partida>();
        if (!relatedEntity.PartidasVisitante.Contains(entity))
        {
            relatedEntity.PartidasVisitante.Add(entity);
        }
        await _unitOfWork.UsuarioRepository.SaveAsync(relatedEntity);
        await _repository.SaveAsync(entity);
    }

    public async Task RemoveUsuario2Async(int p_oid, int p_usuario_oid)
    {
        Partida entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Partida not found.");
        Usuario relatedEntity = await _unitOfWork.UsuarioRepository.ReadAsync(p_usuario_oid)
            ?? throw new ModelException("Usuario not found for Partida.Usuario2.");
        if (entity.Usuario2 is null || !entity.Usuario2.Equals(relatedEntity))
        {
            throw new ModelException("Usuario is not related to Partida.Usuario2.");
        }
        relatedEntity.PartidasVisitante ??= new List<Partida>();
        relatedEntity.PartidasVisitante.Remove(entity);
        entity.Usuario2 = null!;
        await _unitOfWork.UsuarioRepository.SaveAsync(relatedEntity);
        await _repository.SaveAsync(entity);
    }

    public async Task AddMazo2Async(int p_oid, int p_mazo_oid)
    {
        Partida entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Partida not found.");
        Mazo relatedEntity = await _unitOfWork.MazoRepository.ReadAsync(p_mazo_oid)
            ?? throw new ModelException("Mazo not found for Partida.Mazo2.");
        entity.Mazo2 = relatedEntity;
        relatedEntity.PartidasVisitante ??= new List<Partida>();
        if (!relatedEntity.PartidasVisitante.Contains(entity))
        {
            relatedEntity.PartidasVisitante.Add(entity);
        }
        await _unitOfWork.MazoRepository.SaveAsync(relatedEntity);
        await _repository.SaveAsync(entity);
    }

    public async Task RemoveMazo2Async(int p_oid, int p_mazo_oid)
    {
        Partida entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Partida not found.");
        Mazo relatedEntity = await _unitOfWork.MazoRepository.ReadAsync(p_mazo_oid)
            ?? throw new ModelException("Mazo not found for Partida.Mazo2.");
        if (entity.Mazo2 is null || !entity.Mazo2.Equals(relatedEntity))
        {
            throw new ModelException("Mazo is not related to Partida.Mazo2.");
        }
        relatedEntity.PartidasVisitante ??= new List<Partida>();
        relatedEntity.PartidasVisitante.Remove(entity);
        entity.Mazo2 = null!;
        await _unitOfWork.MazoRepository.SaveAsync(relatedEntity);
        await _repository.SaveAsync(entity);
    }

    public async Task<IReadOnlyList<Partida>> ObtenerPartidasUsuarioAsync(int p_oid)
    {
        return await _repository.ObtenerPartidasUsuarioAsync(p_oid);
    }

}
