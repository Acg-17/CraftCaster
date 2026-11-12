using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Application.Services;

public sealed partial class UsuarioService : ServiceEntity<Usuario>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioRepository _repository;
    private readonly IHariaPasswordHasher _passwordHasher;
    private readonly IHariaTokenGenerator _tokenGenerator;

    public UsuarioService(IUnitOfWork unitOfWork)
        : this(unitOfWork, ownsUnitOfWork: false)
    {
    }

    public UsuarioService(IUnitOfWorkFactory unitOfWorkFactory)
        : this(unitOfWorkFactory.Create(), ownsUnitOfWork: true)
    {
    }

    private UsuarioService(IUnitOfWork unitOfWork, bool ownsUnitOfWork) : base(unitOfWork, unitOfWork.UsuarioRepository, ownsUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.UsuarioRepository;
        _passwordHasher = unitOfWork.PasswordHasher;
        _tokenGenerator = unitOfWork.TokenGenerator;
    }

    public Task<Usuario> AddUsuarioDefaultAsync(Usuario entity)
    {
        HashPasswordProperties(entity);
        return AddDefaultAsync(entity);
    }

    public Task DeleteUsuarioDefaultAsync(int id) =>
        DeleteDefaultAsync(id);

    public Task<Usuario?> ReadOIDUsuarioDefaultAsync(int id) =>
        ReadOIDDefaultAsync(id);

    public Task<IReadOnlyList<Usuario>> ReadAllUsuarioDefaultAsync() =>
        ReadAllDefaultAsync();

    public Task<Usuario> ModifierUsuarioDefaultAsync(Usuario entity)
    {
        HashPasswordProperties(entity);
        return ModifierDefaultAsync(entity);
    }

    private void HashPasswordProperties(Usuario entity)
    {
        if (entity.Contrasenia is not null)
        {
            entity.Contrasenia = _passwordHasher.HashPasswordIfNeeded(entity.Contrasenia);
        }
    }

    public async Task<Usuario> NewAsync(string p_nombreUsuario, string p_email, string p_contrasenia, byte p_fotoPerfil, EstadoCuenta p_estadoCuenta, TipoUsuario p_tipoUsuario)
    {
        Usuario entity = new Usuario
        {
            NombreUsuario = p_nombreUsuario,
            Email = p_email,
            Contrasenia = _passwordHasher.HashPasswordIfNeeded(p_contrasenia),
            FotoPerfil = p_fotoPerfil,
            EstadoCuenta = p_estadoCuenta,
            TipoUsuario = p_tipoUsuario,
        };

        await _repository.SaveAsync(entity);
        return entity;
    }

    public async Task DestroyAsync(int p_oid)
    {
        Usuario entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Usuario not found.");
        await _repository.DeleteAsync(entity);
    }

    public async Task ModifyAsync(int p_oid, string p_nombreUsuario, string p_email, string p_contrasenia, byte p_fotoPerfil, EstadoCuenta p_estadoCuenta, TipoUsuario p_tipoUsuario)
    {
        Usuario entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Usuario not found.");
        entity.NombreUsuario = p_nombreUsuario;
        entity.Email = p_email;
        entity.Contrasenia = _passwordHasher.HashPasswordIfNeeded(p_contrasenia);
        entity.FotoPerfil = p_fotoPerfil;
        entity.EstadoCuenta = p_estadoCuenta;
        entity.TipoUsuario = p_tipoUsuario;
        await _repository.SaveAsync(entity);
    }

    public async Task<Usuario?> ReadOIDAsync(int p_oid)
    {
        return await _repository.ReadAsync(p_oid);
    }

    public async Task<IReadOnlyList<Usuario>> ReadAllAsync()
    {
        return await _repository.ReadAllAsync();
    }

    public async Task<string?> LoginAsync(int p_oid, string p_password)
    {
        try
        {
            Usuario? entity = await _repository.ReadAsync(p_oid);
            if (entity is null)
            {
                return null;
            }

            if (!_passwordHasher.VerifyPassword(entity.Contrasenia, p_password))
            {
                return null;
            }

            var tokenSubject = Convert.ToString(entity.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            var tokenClaims = new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["id"] = Convert.ToString(entity.Id, System.Globalization.CultureInfo.InvariantCulture),
            };
            return _tokenGenerator.CreateToken(tokenSubject, tokenClaims);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task AddCartaAsync(int p_oid, IReadOnlyList<int> p_carta_oids)
    {
        Usuario entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Usuario not found.");
        entity.Cartas ??= new List<Carta>();
        foreach (var relatedId in p_carta_oids)
        {
            Carta relatedEntity = await _unitOfWork.CartaRepository.ReadAsync(relatedId)
                ?? throw new ModelException("Carta not found for Usuario.Cartas.");
            if (!entity.Cartas.Contains(relatedEntity))
            {
                entity.Cartas.Add(relatedEntity);
            }
            relatedEntity.Usuario = entity;
            await _unitOfWork.CartaRepository.SaveAsync(relatedEntity);
        }
        await _repository.SaveAsync(entity);
    }

    public async Task AddMazoAsync(int p_oid, IReadOnlyList<int> p_mazo_oids)
    {
        Usuario entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Usuario not found.");
        entity.Mazos ??= new List<Mazo>();
        foreach (var relatedId in p_mazo_oids)
        {
            Mazo relatedEntity = await _unitOfWork.MazoRepository.ReadAsync(relatedId)
                ?? throw new ModelException("Mazo not found for Usuario.Mazos.");
            if (!entity.Mazos.Contains(relatedEntity))
            {
                entity.Mazos.Add(relatedEntity);
            }
            relatedEntity.Usuario = entity;
            await _unitOfWork.MazoRepository.SaveAsync(relatedEntity);
        }
        await _repository.SaveAsync(entity);
    }

    public async Task AddPartidasLocalAsync(int p_oid, IReadOnlyList<int> p_partida_oids)
    {
        Usuario entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Usuario not found.");
        entity.PartidasLocal ??= new List<Partida>();
        foreach (var relatedId in p_partida_oids)
        {
            Partida relatedEntity = await _unitOfWork.PartidaRepository.ReadAsync(relatedId)
                ?? throw new ModelException("Partida not found for Usuario.PartidasLocal.");
            if (!entity.PartidasLocal.Contains(relatedEntity))
            {
                entity.PartidasLocal.Add(relatedEntity);
            }
            relatedEntity.Usuario1 = entity;
            await _unitOfWork.PartidaRepository.SaveAsync(relatedEntity);
        }
        await _repository.SaveAsync(entity);
    }

    public async Task AddPartidasVisitanteAsync(int p_oid, IReadOnlyList<int> p_partida_oids)
    {
        Usuario entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Usuario not found.");
        entity.PartidasVisitante ??= new List<Partida>();
        foreach (var relatedId in p_partida_oids)
        {
            Partida relatedEntity = await _unitOfWork.PartidaRepository.ReadAsync(relatedId)
                ?? throw new ModelException("Partida not found for Usuario.PartidasVisitante.");
            if (!entity.PartidasVisitante.Contains(relatedEntity))
            {
                entity.PartidasVisitante.Add(relatedEntity);
            }
            relatedEntity.Usuario2 = entity;
            await _unitOfWork.PartidaRepository.SaveAsync(relatedEntity);
        }
        await _repository.SaveAsync(entity);
    }

    public async Task RemovePartidasVisitanteAsync(int p_oid, IReadOnlyList<int> p_partida_oids)
    {
        Usuario entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Usuario not found.");
        entity.PartidasVisitante ??= new List<Partida>();
        foreach (var relatedId in p_partida_oids)
        {
            Partida relatedEntity = await _unitOfWork.PartidaRepository.ReadAsync(relatedId)
                ?? throw new ModelException("Partida not found for Usuario.PartidasVisitante.");
            if (!entity.PartidasVisitante.Remove(relatedEntity))
            {
                throw new ModelException("Partida is not related to Usuario.PartidasVisitante.");
            }
            if (relatedEntity.Usuario2 is not null && relatedEntity.Usuario2.Equals(entity))
            {
                relatedEntity.Usuario2 = null!;
            }
            await _unitOfWork.PartidaRepository.SaveAsync(relatedEntity);
        }
        await _repository.SaveAsync(entity);
    }

    public async Task<IReadOnlyList<Usuario>> ObtenerUsuarioLocalAsync(int p_oid)
    {
        return await _repository.ObtenerUsuarioLocalAsync(p_oid);
    }

}
