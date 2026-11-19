// HARIA-WEB MVC managed assembler
using CraftCaster.Domain.Entities;

namespace CraftCaster.Web.Assemblers;

public static class ZonaPartidaAssembler
{
    /*PROTECTED REGION ID(ZonaPartidaAssembler_method_ToViewModel) START*/
    public static CraftCaster.Web.ViewModels.ZonaPartidaViewModel ToViewModel(ZonaPartida entity) =>
        new()
        {
            Id = entity.Id,
            TipoZona = entity.TipoZona,
            JugadorId = entity.Jugador?.Id,
            JugadorLabel = entity.Jugador is null ? null : Convert.ToString(entity.Jugador.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            Cartas = entity.Cartas.Select(CartaPartidaAssembler.ToViewModel).ToList(),
        };
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(ZonaPartidaAssembler_method_ApplyToEntity) START*/
    public static void ApplyToEntity(CraftCaster.Web.ViewModels.ZonaPartidaViewModel model, ZonaPartida entity)
    {
        // Use validated input and a server-loaded entity. Persist through Application.
        // Identifiers, secrets and stored file paths are never copied from the client.
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entity);
        entity.TipoZona = model.TipoZona;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(ZonaPartidaAssembler_custom) ENABLED START*/
    // Add custom properties, validation helpers or actions here.
    /*PROTECTED REGION END*/
}
