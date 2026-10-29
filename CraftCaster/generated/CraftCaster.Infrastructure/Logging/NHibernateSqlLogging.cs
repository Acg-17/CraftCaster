using NHibernate;
using NHibernate.Cfg;
using NHibernate.SqlCommand;

namespace CraftCaster.Infrastructure.Logging;

public static class NHibernateSqlLogging
{
    public static Configuration EnableSqlStatementCapture(this Configuration configuration)
    {
        configuration.SetInterceptor(new SqlStatementCaptureInterceptor());
        return configuration;
    }
}

public sealed class SqlStatementCaptureInterceptor : EmptyInterceptor
{
    public override SqlString OnPrepareStatement(SqlString sql)
    {
        SqlStatementCapture.Record(sql.ToString());
        return base.OnPrepareStatement(sql);
    }
}