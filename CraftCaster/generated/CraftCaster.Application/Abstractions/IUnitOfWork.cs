namespace CraftCaster.Application.Abstractions;

public interface IUnitOfWork : IAsyncDisposable
{
    IUsuarioRepository UsuarioRepository { get; }
    IMazoRepository MazoRepository { get; }
    ICartaRepository CartaRepository { get; }
    IEstadoRepository EstadoRepository { get; }
    IHabilidadRepository HabilidadRepository { get; }
    IPartidaRepository PartidaRepository { get; }
    IJugadorPartidaRepository JugadorPartidaRepository { get; }
    IZonaPartidaRepository ZonaPartidaRepository { get; }
    ICartaPartidaRepository CartaPartidaRepository { get; }
    ITurnoRepository TurnoRepository { get; }
    IAccionRepository AccionRepository { get; }

    IHariaPasswordHasher PasswordHasher { get; }
    IHariaTokenGenerator TokenGenerator { get; }

    Task BeginTransactionAsync(string? businessOperation = null);
    Task CommitAsync();
    Task RollbackAsync();
}
