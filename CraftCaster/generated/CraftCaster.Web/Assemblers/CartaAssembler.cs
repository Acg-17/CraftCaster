// HARIA-WEB MVC managed assembler
using CraftCaster.Domain.Entities;

namespace CraftCaster.Web.Assemblers;

public static class CartaAssembler
{
    /*PROTECTED REGION ID(CartaAssembler_method_ToViewModel) START*/
    public static CraftCaster.Web.ViewModels.CartaViewModel ToViewModel(Carta entity) =>
        new()
        {
            Id = entity.Id,
            Nombre = entity.Nombre,
            Rareza = entity.Rareza,
            CosteMana = entity.CosteMana,
            Tipo = entity.Tipo,
            Vida = entity.Vida,
            Ataque = entity.Ataque,
            Descripcion = entity.Descripcion,
            Imagen = entity.Imagen,
            EsPublica = entity.EsPublica,
            UsuarioId = entity.Usuario?.Id,
            UsuarioLabel = entity.Usuario is null ? null : Convert.ToString(entity.Usuario.NombreUsuario, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
        };
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(CartaAssembler_method_ApplyToEntity) START*/
    public static void ApplyToEntity(CraftCaster.Web.ViewModels.CartaViewModel model, Carta entity)
    {
        // Use validated input and a server-loaded entity. Persist through Application.
        // Identifiers, secrets and stored file paths are never copied from the client.
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entity);
        entity.Nombre = model.Nombre;
        entity.Rareza = model.Rareza;
        entity.CosteMana = model.CosteMana;
        entity.Tipo = model.Tipo;
        entity.Vida = model.Vida;
        entity.Ataque = model.Ataque;
        entity.Descripcion = model.Descripcion;
        entity.Imagen = model.Imagen;
        entity.EsPublica = model.EsPublica;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(CartaAssembler_custom) ENABLED START*/
    // Add custom properties, validation helpers or actions here.
    /*PROTECTED REGION END*/
}
