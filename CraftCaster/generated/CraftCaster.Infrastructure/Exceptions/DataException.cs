namespace CraftCaster.Infrastructure.Exceptions;

public sealed class DataException : Exception
{
    public DataException(string message)
        : base(message)
    {
    }

    public DataException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}