// HARIA-WEB MVC managed assembler
using CraftCaster.Domain.Entities;

namespace CraftCaster.Web.Assemblers;

public static class LineaMazoAssembler
{
    /*PROTECTED REGION ID(LineaMazoAssembler_method_ToViewModel) START*/
    public static CraftCaster.Web.ViewModels.LineaMazoViewModel ToViewModel(LineaMazo entity) =>
        new()
        {
            Id = entity.Id,
            Cantidad = entity.Cantidad,
            MazoId = entity.Mazo?.Id,
            MazoLabel = entity.Mazo is null ? null : Convert.ToString(entity.Mazo.Nombre, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            CartaId = entity.Carta?.Id,
            CartaLabel = entity.Carta is null ? null : Convert.ToString(entity.Carta.Nombre, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
        };
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(LineaMazoAssembler_method_ApplyToEntity) START*/
    public static void ApplyToEntity(CraftCaster.Web.ViewModels.LineaMazoViewModel model, LineaMazo entity)
    {
        // Use validated input and a server-loaded entity. Persist through Application.
        // Identifiers, secrets and stored file paths are never copied from the client.
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entity);
        entity.Cantidad = model.Cantidad;
    }
    /*PROTECTED REGION END*/
    /*PROTECTED REGION ID(LineaMazoAssembler_method_Create) START*/
    public static LineaMazo Create(CraftCaster.Web.ViewModels.LineaMazoViewModel model)
    {
        var entity = new LineaMazo();
        ApplyToEntity(model, entity);
        return entity;
    }
    /*PROTECTED REGION END*/
    /*PROTECTED REGION ID(LineaMazoAssembler_custom) ENABLED START*/
    // Add custom properties, validation helpers or actions here.
    /*PROTECTED REGION END*/
}
