// HARIA-WEB MVC managed assembler
using CraftCaster.Domain.Entities;

namespace CraftCaster.Web.Assemblers;

public static class MazoAssembler
{
    /*PROTECTED REGION ID(MazoAssembler_method_ToViewModel) START*/
    public static CraftCaster.Web.ViewModels.MazoViewModel ToViewModel(Mazo entity) =>
        new()
        {
            Id = entity.Id,
            Nombre = entity.Nombre,
            CosteMedio = entity.CosteMedio,
            NumCartas = entity.NumCartas,
            EstadoMazo = entity.EstadoMazo,
            EsPublico = entity.EsPublico,
            Valoracion = entity.Valoracion,
            Imagen = entity.Imagen,
            UsuarioId = entity.Usuario?.Id,
            UsuarioLabel = entity.Usuario is null ? null : Convert.ToString(entity.Usuario.NombreUsuario, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            Lineas = entity.Lineas.Select(LineaMazoAssembler.ToViewModel).ToList(),
        };
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(MazoAssembler_method_ApplyToEntity) START*/
    public static void ApplyToEntity(CraftCaster.Web.ViewModels.MazoViewModel model, Mazo entity)
    {
        // Use validated input and a server-loaded entity. Persist through Application.
        // Identifiers, secrets and stored file paths are never copied from the client.
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entity);
        entity.Nombre = model.Nombre;
        entity.CosteMedio = model.CosteMedio;
        entity.NumCartas = model.NumCartas;
        entity.EstadoMazo = model.EstadoMazo;
        entity.EsPublico = model.EsPublico;
        entity.Valoracion = model.Valoracion;
        entity.Imagen = model.Imagen;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(MazoAssembler_custom) ENABLED START*/
    // Add custom properties, validation helpers or actions here.
    /*PROTECTED REGION END*/
}
