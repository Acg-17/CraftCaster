using NHibernate;
using Microsoft.Extensions.Logging;
using CraftCaster.Application.Abstractions;
using CraftCaster.Infrastructure.Exceptions;
using CraftCaster.Infrastructure.Logging;
using CraftCaster.Infrastructure.Repositories;
using CraftCaster.Infrastructure.Security;

namespace CraftCaster.Infrastructure;

public sealed class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly ISession _session;
    private readonly ILogger<UnitOfWork>? _logger;
    private readonly Microsoft.Extensions.Logging.ILoggerFactory? _loggerFactory;
    private ITransaction? _transaction;
    private SqlStatementCaptureScope? _sqlCaptureScope;
    private bool _disposed;

    public UnitOfWork(UnitOfWorkFactory unitOfWorkFactory)
        : this(
            unitOfWorkFactory.OpenSession(),
            unitOfWorkFactory.LoggerFactory,
            unitOfWorkFactory.PasswordHasher,
            unitOfWorkFactory.TokenGenerator)
    {
    }

    public UnitOfWork(ISession session, Microsoft.Extensions.Logging.ILoggerFactory? loggerFactory = null, IHariaPasswordHasher? passwordHasher = null, IHariaTokenGenerator? tokenGenerator = null)
    {
        _session = session;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory?.CreateLogger<UnitOfWork>();
        PasswordHasher = passwordHasher ?? new HariaPasswordHasher();
        TokenGenerator = tokenGenerator ?? new HariaTokenGenerator();
        UsuarioRepository = new UsuarioRepository(session, _loggerFactory?.CreateLogger<UsuarioRepository>());
        MazoRepository = new MazoRepository(session, _loggerFactory?.CreateLogger<MazoRepository>());
        CartaRepository = new CartaRepository(session, _loggerFactory?.CreateLogger<CartaRepository>());
        EstadoRepository = new EstadoRepository(session, _loggerFactory?.CreateLogger<EstadoRepository>());
        HabilidadRepository = new HabilidadRepository(session, _loggerFactory?.CreateLogger<HabilidadRepository>());
        PartidaRepository = new PartidaRepository(session, _loggerFactory?.CreateLogger<PartidaRepository>());
        JugadorPartidaRepository = new JugadorPartidaRepository(session, _loggerFactory?.CreateLogger<JugadorPartidaRepository>());
        ZonaPartidaRepository = new ZonaPartidaRepository(session, _loggerFactory?.CreateLogger<ZonaPartidaRepository>());
        CartaPartidaRepository = new CartaPartidaRepository(session, _loggerFactory?.CreateLogger<CartaPartidaRepository>());
        TurnoRepository = new TurnoRepository(session, _loggerFactory?.CreateLogger<TurnoRepository>());
        AccionRepository = new AccionRepository(session, _loggerFactory?.CreateLogger<AccionRepository>());
    }

    public IUsuarioRepository UsuarioRepository { get; }
    public IMazoRepository MazoRepository { get; }
    public ICartaRepository CartaRepository { get; }
    public IEstadoRepository EstadoRepository { get; }
    public IHabilidadRepository HabilidadRepository { get; }
    public IPartidaRepository PartidaRepository { get; }
    public IJugadorPartidaRepository JugadorPartidaRepository { get; }
    public IZonaPartidaRepository ZonaPartidaRepository { get; }
    public ICartaPartidaRepository CartaPartidaRepository { get; }
    public ITurnoRepository TurnoRepository { get; }
    public IAccionRepository AccionRepository { get; }

    public IHariaPasswordHasher PasswordHasher { get; }

    public IHariaTokenGenerator TokenGenerator { get; }

    public Task BeginTransactionAsync(string? businessOperation = null)
    {
        try
        {
            if (_transaction is null)
            {
                var transactionId = Guid.NewGuid();
                var effectiveBusinessOperation = ResolveBusinessOperation(businessOperation);
                _transaction = _session.BeginTransaction();
                _sqlCaptureScope = SqlStatementCapture.Begin(transactionId, effectiveBusinessOperation);

                _logger?.LogDebug(
                    "NHibernate transaction started. BusinessOperation={BusinessOperation} TransactionId={TransactionId}",
                    effectiveBusinessOperation,
                    transactionId);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error beginning NHibernate transaction.");
            DisposeSqlCaptureScope();
            throw new DataException("Error beginning NHibernate transaction.", ex);
        }
    }

    public async Task CommitAsync()
    {
        try
        {
            if (_transaction is not null)
            {
                await _transaction.CommitAsync();

                var sqlStatements = GetCapturedSqlStatements();
                var businessOperations = GetCapturedBusinessOperations();
                _logger?.LogInformation(
                    "NHibernate transaction committed successfully. BusinessOperation={BusinessOperation} BusinessOperations={BusinessOperations} TransactionId={TransactionId} SqlStatementCount={SqlStatementCount}",
                    _sqlCaptureScope?.BusinessOperation,
                    businessOperations,
                    _sqlCaptureScope?.TransactionId,
                    sqlStatements.Count);

                if (sqlStatements.Count > 0)
                {
                    _logger?.LogDebug(
                        "NHibernate SQL statements committed. BusinessOperation={BusinessOperation} BusinessOperations={BusinessOperations} TransactionId={TransactionId} SqlStatements={SqlStatements}",
                        _sqlCaptureScope?.BusinessOperation,
                        businessOperations,
                        _sqlCaptureScope?.TransactionId,
                        sqlStatements);
                }

                _transaction.Dispose();
                _transaction = null;
                DisposeSqlCaptureScope();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                ex,
                "Error committing NHibernate transaction. TransactionId={TransactionId}",
                _sqlCaptureScope?.TransactionId);
            throw new DataException("Error committing NHibernate transaction.", ex);
        }
    }

    public async Task RollbackAsync()
    {
        try
        {
            if (_transaction is not null)
            {
                await _transaction.RollbackAsync();

                var sqlStatements = GetCapturedSqlStatements();
                var businessOperations = GetCapturedBusinessOperations();
                _logger?.LogInformation(
                    "NHibernate transaction rolled back. BusinessOperation={BusinessOperation} BusinessOperations={BusinessOperations} TransactionId={TransactionId} SqlStatementCount={SqlStatementCount}",
                    _sqlCaptureScope?.BusinessOperation,
                    businessOperations,
                    _sqlCaptureScope?.TransactionId,
                    sqlStatements.Count);

                if (sqlStatements.Count > 0)
                {
                    _logger?.LogDebug(
                        "NHibernate SQL statements rolled back. BusinessOperation={BusinessOperation} BusinessOperations={BusinessOperations} TransactionId={TransactionId} SqlStatements={SqlStatements}",
                        _sqlCaptureScope?.BusinessOperation,
                        businessOperations,
                        _sqlCaptureScope?.TransactionId,
                        sqlStatements);
                }

                _transaction.Dispose();
                _transaction = null;
                DisposeSqlCaptureScope();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                ex,
                "Error rolling back NHibernate transaction. TransactionId={TransactionId}",
                _sqlCaptureScope?.TransactionId);
            throw new DataException("Error rolling back NHibernate transaction.", ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        if (_transaction is not null)
        {
            await RollbackAsync();
        }

        _session.Dispose();
        _disposed = true;
    }

    private IReadOnlyList<string> GetCapturedSqlStatements() =>
        _sqlCaptureScope?.GetStatements() ?? Array.Empty<string>();

    private IReadOnlyList<string> GetCapturedBusinessOperations() =>
        _sqlCaptureScope?.GetBusinessOperations() ?? Array.Empty<string>();

    private static string ResolveBusinessOperation(string? businessOperation)
    {
        if (!string.IsNullOrWhiteSpace(businessOperation))
        {
            return businessOperation;
        }

        return "UnitOfWork transaction";
    }

    private void DisposeSqlCaptureScope()
    {
        _sqlCaptureScope?.Dispose();
        _sqlCaptureScope = null;
    }
}
