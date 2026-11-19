// HARIA-WEB MVC managed assembler
using CraftCaster.Domain.Entities;

namespace CraftCaster.Web.Assemblers;

public static class PartidaAssembler
{
    /*PROTECTED REGION ID(PartidaAssembler_method_ToViewModel) START*/
    public static CraftCaster.Web.ViewModels.PartidaViewModel ToViewModel(Partida entity) =>
        new()
        {
            Id = entity.Id,
            Estado = entity.Estado,
            Ganador = entity.Ganador,
            FechaInicio = entity.FechaInicio,
            Usuario1Id = entity.Usuario1?.Id,
            Usuario1Label = entity.Usuario1 is null ? null : Convert.ToString(entity.Usuario1.NombreUsuario, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            Usuario2Id = entity.Usuario2?.Id,
            Usuario2Label = entity.Usuario2 is null ? null : Convert.ToString(entity.Usuario2.NombreUsuario, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            Mazo1Id = entity.Mazo1?.Id,
            Mazo1Label = entity.Mazo1 is null ? null : Convert.ToString(entity.Mazo1.Nombre, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            Mazo2Id = entity.Mazo2?.Id,
            Mazo2Label = entity.Mazo2 is null ? null : Convert.ToString(entity.Mazo2.Nombre, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
        };
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(PartidaAssembler_method_ApplyToEntity) START*/
    public static void ApplyToEntity(CraftCaster.Web.ViewModels.PartidaViewModel model, Partida entity)
    {
        // Use validated input and a server-loaded entity. Persist through Application.
        // Identifiers, secrets and stored file paths are never copied from the client.
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entity);
        entity.Estado = model.Estado;
        entity.Ganador = model.Ganador;
        entity.FechaInicio = model.FechaInicio;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(PartidaAssembler_custom) ENABLED START*/
    // Add custom properties, validation helpers or actions here.
    /*PROTECTED REGION END*/
}
