// HARIA-WEB MVC managed assembler
using CraftCaster.Domain.Entities;

namespace CraftCaster.Web.Assemblers;

public static class EstadoAssembler
{
    /*PROTECTED REGION ID(EstadoAssembler_method_ToViewModel) START*/
    public static CraftCaster.Web.ViewModels.EstadoViewModel ToViewModel(Estado entity) =>
        new()
        {
            Id = entity.Id,
            Tipo = entity.Tipo,
            CartaId = entity.Carta?.Id,
            CartaLabel = entity.Carta is null ? null : Convert.ToString(entity.Carta.Nombre, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            Habilidades = entity.Habilidades.Select(HabilidadAssembler.ToViewModel).ToList(),
        };
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(EstadoAssembler_method_ApplyToEntity) START*/
    public static void ApplyToEntity(CraftCaster.Web.ViewModels.EstadoViewModel model, Estado entity)
    {
        // Use validated input and a server-loaded entity. Persist through Application.
        // Identifiers, secrets and stored file paths are never copied from the client.
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entity);
        entity.Tipo = model.Tipo;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(EstadoAssembler_custom) ENABLED START*/
    // Add custom properties, validation helpers or actions here.
    /*PROTECTED REGION END*/
}
