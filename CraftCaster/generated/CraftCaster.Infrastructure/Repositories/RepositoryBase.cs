using Microsoft.Extensions.Logging;
using NHibernate;
using CraftCaster.Infrastructure.Exceptions;
using CraftCaster.Infrastructure.Logging;

namespace CraftCaster.Infrastructure.Repositories;

public abstract class RepositoryBase<TEntity>
    where TEntity : class
{
    private readonly ISession? _session;
    private readonly ISessionFactory? _sessionFactory;

    protected RepositoryBase(ILogger? logger = null)
        : this(CreateSessionFactory(), logger)
    {
    }

    protected RepositoryBase(ISession session, ILogger? logger = null)
    {
        _session = session;
        Logger = logger;
    }

    protected RepositoryBase(ISessionFactory sessionFactory, ILogger? logger = null)
    {
        _sessionFactory = sessionFactory;
        Logger = logger;
    }

    protected ILogger? Logger { get; }

    protected string EntityName => typeof(TEntity).Name;

    protected RepositorySessionContext OpenSession(string? operation = null, string? businessOperation = null)
    {
        var session = _session ?? _sessionFactory?.OpenSession() ?? throw new InvalidOperationException("No NHibernate session or session factory was provided.");
        var ownsSession = _session is null;
        ITransaction? tx = null;
        SqlStatementCaptureScope? sqlCaptureScope = null;
        var hasActiveTransaction = session.GetCurrentTransaction()?.IsActive == true;

        if (operation is not null && !hasActiveTransaction)
        {
            tx = session.BeginTransaction();
            sqlCaptureScope = SqlStatementCapture.Begin(Guid.NewGuid(), businessOperation ?? $"{GetType().Name}.{operation}Async");
        }

        return new RepositorySessionContext(session, ownsSession, tx, sqlCaptureScope);
    }

    protected async Task ExecuteRepositoryOperationInTransactionAsync(string operation, Func<ISession, Task> action, string errorMessage)
    {
        using var context = OpenSession(operation);

        try
        {
            await action(context.Session);
            await CommitAsync(context, operation);
        }
        catch (Exception ex)
        {
            await RollbackAsync(context, operation);
            Logger?.LogError(ex, "{ErrorMessage}", errorMessage);
            throw new DataException(errorMessage, ex);
        }
    }

    private async Task CommitAsync(RepositorySessionContext context, string operation)
    {
        if (context.Transaction is null)
        {
            return;
        }

        await context.Transaction.CommitAsync();
        LogCommittedTransaction(operation, context.SqlCaptureScope);
    }

    private async Task RollbackAsync(RepositorySessionContext context, string operation)
    {
        if (context.Transaction is null)
        {
            return;
        }

        try
        {
            await context.Transaction.RollbackAsync();
            LogRolledBackTransaction(operation, context.SqlCaptureScope);
        }
        catch (Exception rollbackException)
        {
            Logger?.LogError(rollbackException, "Error rolling back {Entity} repository transaction.", EntityName);
        }
    }

    private static ISessionFactory CreateSessionFactory()
    {
        var cfg = new NHibernate.Cfg.Configuration();
        var configPath = System.IO.Path.Combine(AppContext.BaseDirectory, "hibernate.cfg.xml");
        if (System.IO.File.Exists(configPath))
        {
            cfg.Configure(configPath);
        }
        else
        {
            cfg.Configure();
        }

        cfg.AddAssembly(typeof(RepositoryBase<TEntity>).Assembly);
        cfg.EnableSqlStatementCapture();
        return cfg.BuildSessionFactory();
    }

    private void LogCommittedTransaction(string operation, SqlStatementCaptureScope? sqlCaptureScope)
    {
        if (sqlCaptureScope is null)
        {
            return;
        }

        var sqlStatements = sqlCaptureScope.GetStatements();
        Logger?.LogInformation(
            "NHibernate repository transaction committed successfully. BusinessOperation={BusinessOperation} Operation={Operation} Entity={Entity} TransactionId={TransactionId} SqlStatementCount={SqlStatementCount}",
            sqlCaptureScope.BusinessOperation,
            operation,
            EntityName,
            sqlCaptureScope.TransactionId,
            sqlStatements.Count);

        if (sqlStatements.Count > 0)
        {
            Logger?.LogDebug(
                "NHibernate repository SQL statements committed. BusinessOperation={BusinessOperation} Operation={Operation} Entity={Entity} TransactionId={TransactionId} SqlStatements={SqlStatements}",
                sqlCaptureScope.BusinessOperation,
                operation,
                EntityName,
                sqlCaptureScope.TransactionId,
                sqlStatements);
        }
    }

    private void LogRolledBackTransaction(string operation, SqlStatementCaptureScope? sqlCaptureScope)
    {
        if (sqlCaptureScope is null)
        {
            return;
        }

        var sqlStatements = sqlCaptureScope.GetStatements();
        Logger?.LogInformation(
            "NHibernate repository transaction rolled back. BusinessOperation={BusinessOperation} Operation={Operation} Entity={Entity} TransactionId={TransactionId} SqlStatementCount={SqlStatementCount}",
            sqlCaptureScope.BusinessOperation,
            operation,
            EntityName,
            sqlCaptureScope.TransactionId,
            sqlStatements.Count);

        if (sqlStatements.Count > 0)
        {
            Logger?.LogDebug(
                "NHibernate repository SQL statements rolled back. BusinessOperation={BusinessOperation} Operation={Operation} Entity={Entity} TransactionId={TransactionId} SqlStatements={SqlStatements}",
                sqlCaptureScope.BusinessOperation,
                operation,
                EntityName,
                sqlCaptureScope.TransactionId,
                sqlStatements);
        }
    }

    protected sealed class RepositorySessionContext : IDisposable
    {
        private readonly bool _ownsSession;

        internal RepositorySessionContext(
            ISession session,
            bool ownsSession,
            ITransaction? transaction,
            SqlStatementCaptureScope? sqlCaptureScope)
        {
            Session = session;
            _ownsSession = ownsSession;
            Transaction = transaction;
            SqlCaptureScope = sqlCaptureScope;
        }

        public ISession Session { get; }

        internal ITransaction? Transaction { get; }

        internal SqlStatementCaptureScope? SqlCaptureScope { get; }

        public void Dispose()
        {
            Transaction?.Dispose();
            SqlCaptureScope?.Dispose();

            if (_ownsSession)
            {
                Session.Dispose();
            }
        }
    }
}