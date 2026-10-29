using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Cfg;
using CraftCaster.Application.Abstractions;

namespace CraftCaster.Infrastructure;

public sealed class UnitOfWorkFactory : IUnitOfWorkFactory, IDisposable
{
    private readonly ISessionFactory _sessionFactory;
    private readonly Microsoft.Extensions.Logging.ILoggerFactory? _loggerFactory;
    private readonly IHariaPasswordHasher? _passwordHasher;
    private readonly IHariaTokenGenerator? _tokenGenerator;

    public UnitOfWorkFactory(Configuration configuration, Microsoft.Extensions.Logging.ILoggerFactory? loggerFactory = null, IHariaPasswordHasher? passwordHasher = null, IHariaTokenGenerator? tokenGenerator = null)
        : this(configuration.BuildSessionFactory(), loggerFactory, passwordHasher, tokenGenerator)
    {
    }

    public UnitOfWorkFactory(ISessionFactory sessionFactory, Microsoft.Extensions.Logging.ILoggerFactory? loggerFactory = null, IHariaPasswordHasher? passwordHasher = null, IHariaTokenGenerator? tokenGenerator = null)
    {
        _sessionFactory = sessionFactory;
        _loggerFactory = loggerFactory;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    internal Microsoft.Extensions.Logging.ILoggerFactory? LoggerFactory => _loggerFactory;

    internal IHariaPasswordHasher? PasswordHasher => _passwordHasher;

    internal IHariaTokenGenerator? TokenGenerator => _tokenGenerator;

    internal ISession OpenSession() =>
        _sessionFactory.OpenSession();

    public IUnitOfWork Create() =>
        new UnitOfWork(this);

    public void Dispose() =>
        _sessionFactory.Dispose();
}