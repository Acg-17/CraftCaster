// HARIA-WEB MVC managed assembler
using CraftCaster.Domain.Entities;

namespace CraftCaster.Web.Assemblers;

public static class HabilidadAssembler
{
    /*PROTECTED REGION ID(HabilidadAssembler_method_ToViewModel) START*/
    public static CraftCaster.Web.ViewModels.HabilidadViewModel ToViewModel(Habilidad entity) =>
        new()
        {
            Id = entity.Id,
            Tipo = entity.Tipo,
            Cantidad = entity.Cantidad,
            TipoObjetivo = entity.TipoObjetivo,
            Objetivo = entity.Objetivo,
            EstadoId = entity.Estado?.Id,
            EstadoLabel = entity.Estado is null ? null : Convert.ToString(entity.Estado.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
        };
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(HabilidadAssembler_method_ApplyToEntity) START*/
    public static void ApplyToEntity(CraftCaster.Web.ViewModels.HabilidadViewModel model, Habilidad entity)
    {
        // Use validated input and a server-loaded entity. Persist through Application.
        // Identifiers, secrets and stored file paths are never copied from the client.
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entity);
        entity.Tipo = model.Tipo;
        entity.Cantidad = model.Cantidad;
        entity.TipoObjetivo = model.TipoObjetivo;
        entity.Objetivo = model.Objetivo;
    }
    /*PROTECTED REGION END*/
    /*PROTECTED REGION ID(HabilidadAssembler_method_Create) START*/
    public static Habilidad Create(CraftCaster.Web.ViewModels.HabilidadViewModel model)
    {
        var entity = new Habilidad();
        ApplyToEntity(model, entity);
        return entity;
    }
    /*PROTECTED REGION END*/
    /*PROTECTED REGION ID(HabilidadAssembler_custom) ENABLED START*/
    // Add custom properties, validation helpers or actions here.
    /*PROTECTED REGION END*/
}
