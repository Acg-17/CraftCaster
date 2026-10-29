namespace CraftCaster.Infrastructure.Logging;

internal static class SqlStatementCapture
{
    private static readonly AsyncLocal<SqlStatementCaptureScope?> CurrentScope = new();

    public static SqlStatementCaptureScope Begin(Guid transactionId, string? businessOperation = null)
    {
        var scope = new SqlStatementCaptureScope(transactionId, businessOperation, CurrentScope.Value);
        CurrentScope.Value = scope;
        return scope;
    }

    public static void Record(string sql)
    {
        CurrentScope.Value?.Add(sql);
    }

    internal static void End(SqlStatementCaptureScope scope)
    {
        if (CurrentScope.Value == scope)
        {
            CurrentScope.Value = scope.Parent;
        }
    }
}

internal sealed class SqlStatementCaptureScope : IDisposable
{
    private readonly object _gate = new();
    private readonly List<string> _statements = new();
    private bool _disposed;

    internal SqlStatementCaptureScope(Guid transactionId, string? businessOperation, SqlStatementCaptureScope? parent)
    {
        TransactionId = transactionId;
        BusinessOperation = string.IsNullOrWhiteSpace(businessOperation)
            ? parent?.BusinessOperation
            : businessOperation;
        Parent = parent;
    }

    public Guid TransactionId { get; }

    public string? BusinessOperation { get; }

    internal SqlStatementCaptureScope? Parent { get; }

    public IReadOnlyList<string> GetStatements()
    {
        lock (_gate)
        {
            return _statements.ToArray();
        }
    }

    public IReadOnlyList<string> GetBusinessOperations() =>
        BusinessOperation is null ? Array.Empty<string>() : new[] { BusinessOperation };

    internal void Add(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            return;
        }

        lock (_gate)
        {
            _statements.Add(sql);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        SqlStatementCapture.End(this);
    }
}