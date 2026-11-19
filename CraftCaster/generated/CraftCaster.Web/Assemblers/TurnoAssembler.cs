// HARIA-WEB MVC managed assembler
using CraftCaster.Domain.Entities;

namespace CraftCaster.Web.Assemblers;

public static class TurnoAssembler
{
    /*PROTECTED REGION ID(TurnoAssembler_method_ToViewModel) START*/
    public static CraftCaster.Web.ViewModels.TurnoViewModel ToViewModel(Turno entity) =>
        new()
        {
            Id = entity.Id,
            NumTurno = entity.NumTurno,
            ManaTurno = entity.ManaTurno,
            PartidaId = entity.Partida?.Id,
            PartidaLabel = entity.Partida is null ? null : Convert.ToString(entity.Partida.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            JugadorId = entity.Jugador?.Id,
            JugadorLabel = entity.Jugador is null ? null : Convert.ToString(entity.Jugador.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            Acciones = entity.Acciones.Select(AccionAssembler.ToViewModel).ToList(),
        };
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(TurnoAssembler_method_ApplyToEntity) START*/
    public static void ApplyToEntity(CraftCaster.Web.ViewModels.TurnoViewModel model, Turno entity)
    {
        // Use validated input and a server-loaded entity. Persist through Application.
        // Identifiers, secrets and stored file paths are never copied from the client.
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entity);
        entity.NumTurno = model.NumTurno;
        entity.ManaTurno = model.ManaTurno;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(TurnoAssembler_custom) ENABLED START*/
    // Add custom properties, validation helpers or actions here.
    /*PROTECTED REGION END*/
}
