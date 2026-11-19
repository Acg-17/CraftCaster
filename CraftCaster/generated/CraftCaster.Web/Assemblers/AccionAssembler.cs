// HARIA-WEB MVC managed assembler
using CraftCaster.Domain.Entities;

namespace CraftCaster.Web.Assemblers;

public static class AccionAssembler
{
    /*PROTECTED REGION ID(AccionAssembler_method_ToViewModel) START*/
    public static CraftCaster.Web.ViewModels.AccionViewModel ToViewModel(Accion entity) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            TurnoId = entity.Turno?.Id,
            TurnoLabel = entity.Turno is null ? null : Convert.ToString(entity.Turno.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            OrigenId = entity.Origen?.Id,
            OrigenLabel = entity.Origen is null ? null : Convert.ToString(entity.Origen.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
        };
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(AccionAssembler_method_ApplyToEntity) START*/
    public static void ApplyToEntity(CraftCaster.Web.ViewModels.AccionViewModel model, Accion entity)
    {
        // Use validated input and a server-loaded entity. Persist through Application.
        // Identifiers, secrets and stored file paths are never copied from the client.
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entity);
        entity.Name = model.Name;
    }
    /*PROTECTED REGION END*/
    /*PROTECTED REGION ID(AccionAssembler_method_Create) START*/
    public static Accion Create(CraftCaster.Web.ViewModels.AccionViewModel model)
    {
        var entity = new Accion();
        ApplyToEntity(model, entity);
        return entity;
    }
    /*PROTECTED REGION END*/
    /*PROTECTED REGION ID(AccionAssembler_custom) ENABLED START*/
    // Add custom properties, validation helpers or actions here.
    /*PROTECTED REGION END*/
}
