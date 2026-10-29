using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Application.Services;

public sealed partial class CartaService : ServiceEntity<Carta>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICartaRepository _repository;

    public CartaService(IUnitOfWork unitOfWork)
        : this(unitOfWork, ownsUnitOfWork: false)
    {
    }

    public CartaService(IUnitOfWorkFactory unitOfWorkFactory)
        : this(unitOfWorkFactory.Create(), ownsUnitOfWork: true)
    {
    }

    private CartaService(IUnitOfWork unitOfWork, bool ownsUnitOfWork) : base(unitOfWork, unitOfWork.CartaRepository, ownsUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.CartaRepository;
    }

    public Task<Carta> AddCartaDefaultAsync(Carta entity) =>
        AddDefaultAsync(entity);

    public Task DeleteCartaDefaultAsync(int id) =>
        DeleteDefaultAsync(id);

    public Task<Carta?> ReadOIDCartaDefaultAsync(int id) =>
        ReadOIDDefaultAsync(id);

    public Task<IReadOnlyList<Carta>> ReadAllCartaDefaultAsync() =>
        ReadAllDefaultAsync();

    public Task<Carta> ModifierCartaDefaultAsync(Carta entity) =>
        ModifierDefaultAsync(entity);

    public async Task<Carta> NewAsync(string p_nombre, Rareza p_rareza, int p_costeMana, TipoCarta p_tipo, string p_descripcion, byte p_imagen, bool p_esPublica, int p_usuario_oid)
    {
        Usuario usuarioEntity = await _unitOfWork.UsuarioRepository.ReadAsync(p_usuario_oid)
            ?? throw new ModelException("Usuario not found for Carta.Usuario.");

        Carta entity = new Carta
        {
            Nombre = p_nombre,
            Rareza = p_rareza,
            CosteMana = p_costeMana,
            Tipo = p_tipo,
            Descripcion = p_descripcion,
            Imagen = p_imagen,
            EsPublica = p_esPublica,
            Usuario = usuarioEntity,
        };

        await _repository.SaveAsync(entity);
        return entity;
    }

    public async Task DestroyAsync(int p_oid)
    {
        Carta entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Carta not found.");
        await _repository.DeleteAsync(entity);
    }

    public async Task ModifyAsync(int p_oid, string p_nombre, Rareza p_rareza, int p_costeMana, TipoCarta p_tipo, string p_descripcion, byte p_imagen, bool p_esPublica, int p_usuario_oid)
    {
        Carta entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Carta not found.");
        entity.Nombre = p_nombre;
        entity.Rareza = p_rareza;
        entity.CosteMana = p_costeMana;
        entity.Tipo = p_tipo;
        entity.Descripcion = p_descripcion;
        entity.Imagen = p_imagen;
        entity.EsPublica = p_esPublica;
        await _repository.SaveAsync(entity);
    }

    public async Task<Carta?> ReadOIDAsync(int p_oid)
    {
        return await _repository.ReadAsync(p_oid);
    }

    public async Task<IReadOnlyList<Carta>> ReadAllAsync()
    {
        return await _repository.ReadAllAsync();
    }

    public async Task<IReadOnlyList<Carta>> ObtenerCartasUsuarioAsync(int p_oid)
    {
        return await _repository.ObtenerCartasUsuarioAsync(p_oid);
    }

    public async Task<IReadOnlyList<Carta>> ObtenerCartaPorNombreAsync(string p_nombre)
    {
        return await _repository.ObtenerCartaPorNombreAsync(p_nombre);
    }

    public async Task<IReadOnlyList<Carta>> ObtenerCartaPorCosteManaAsync(int p_mana)
    {
        return await _repository.ObtenerCartaPorCosteManaAsync(p_mana);
    }

    public async Task<IReadOnlyList<Carta>> ObtenerCartaPorRarezaAsync(Rareza p_rareza)
    {
        return await _repository.ObtenerCartaPorRarezaAsync(p_rareza);
    }

    public async Task<IReadOnlyList<Carta>> ObtenerCartaPorTipoAsync(TipoCarta p_tipo)
    {
        return await _repository.ObtenerCartaPorTipoAsync(p_tipo);
    }

}
