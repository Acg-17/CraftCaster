// HARIA-WEB MVC managed assembler
using CraftCaster.Domain.Entities;

namespace CraftCaster.Web.Assemblers;

public static class JugadorPartidaAssembler
{
    /*PROTECTED REGION ID(JugadorPartidaAssembler_method_ToViewModel) START*/
    public static CraftCaster.Web.ViewModels.JugadorPartidaViewModel ToViewModel(JugadorPartida entity) =>
        new()
        {
            Id = entity.Id,
            Vida = entity.Vida,
            NumJugador = entity.NumJugador,
            Mana = entity.Mana,
            Activo = entity.Activo,
            PartidaId = entity.Partida?.Id,
            PartidaLabel = entity.Partida is null ? null : Convert.ToString(entity.Partida.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
        };
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(JugadorPartidaAssembler_method_ApplyToEntity) START*/
    public static void ApplyToEntity(CraftCaster.Web.ViewModels.JugadorPartidaViewModel model, JugadorPartida entity)
    {
        // Use validated input and a server-loaded entity. Persist through Application.
        // Identifiers, secrets and stored file paths are never copied from the client.
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entity);
        entity.Vida = model.Vida;
        entity.NumJugador = model.NumJugador;
        entity.Mana = model.Mana;
        entity.Activo = model.Activo;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(JugadorPartidaAssembler_custom) ENABLED START*/
    // Add custom properties, validation helpers or actions here.
    /*PROTECTED REGION END*/
}
