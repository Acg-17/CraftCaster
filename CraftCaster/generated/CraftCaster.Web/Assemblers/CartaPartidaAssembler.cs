// HARIA-WEB MVC managed assembler
using CraftCaster.Domain.Entities;

namespace CraftCaster.Web.Assemblers;

public static class CartaPartidaAssembler
{
    /*PROTECTED REGION ID(CartaPartidaAssembler_method_ToViewModel) START*/
    public static CraftCaster.Web.ViewModels.CartaPartidaViewModel ToViewModel(CartaPartida entity) =>
        new()
        {
            Id = entity.Id,
            VidaActual = entity.VidaActual,
            AtaqueActual = entity.AtaqueActual,
            PuedeAtacar = entity.PuedeAtacar,
            ZonaId = entity.Zona?.Id,
            ZonaLabel = entity.Zona is null ? null : Convert.ToString(entity.Zona.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            OriginalId = entity.Original?.Id,
            OriginalLabel = entity.Original is null ? null : Convert.ToString(entity.Original.Nombre, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
        };
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(CartaPartidaAssembler_method_ApplyToEntity) START*/
    public static void ApplyToEntity(CraftCaster.Web.ViewModels.CartaPartidaViewModel model, CartaPartida entity)
    {
        // Use validated input and a server-loaded entity. Persist through Application.
        // Identifiers, secrets and stored file paths are never copied from the client.
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entity);
        entity.VidaActual = model.VidaActual;
        entity.AtaqueActual = model.AtaqueActual;
        entity.PuedeAtacar = model.PuedeAtacar;
    }
    /*PROTECTED REGION END*/
    /*PROTECTED REGION ID(CartaPartidaAssembler_method_Create) START*/
    public static CartaPartida Create(CraftCaster.Web.ViewModels.CartaPartidaViewModel model)
    {
        var entity = new CartaPartida();
        ApplyToEntity(model, entity);
        return entity;
    }
    /*PROTECTED REGION END*/
    /*PROTECTED REGION ID(CartaPartidaAssembler_custom) ENABLED START*/
    // Add custom properties, validation helpers or actions here.
    /*PROTECTED REGION END*/
}
