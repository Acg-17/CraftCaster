// HARIA-WEB MVC managed assembler
using CraftCaster.Domain.Entities;

namespace CraftCaster.Web.Assemblers;

public static class UsuarioAssembler
{
    /*PROTECTED REGION ID(UsuarioAssembler_method_ToViewModel) START*/
    public static CraftCaster.Web.ViewModels.UsuarioViewModel ToViewModel(Usuario entity) =>
        new()
        {
            Id = entity.Id,
            NombreUsuario = entity.NombreUsuario,
            Email = entity.Email,
            FotoPerfil = entity.FotoPerfil,
            EstadoCuenta = entity.EstadoCuenta,
            TipoUsuario = entity.TipoUsuario,

        };
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(UsuarioAssembler_method_ApplyToEntity) START*/
    public static void ApplyToEntity(CraftCaster.Web.ViewModels.UsuarioViewModel model, Usuario entity)
    {
        // Use validated input and a server-loaded entity. Persist through Application.
        // Identifiers, secrets and stored file paths are never copied from the client.
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entity);
        entity.NombreUsuario = model.NombreUsuario;
        entity.Email = model.Email;
        entity.FotoPerfil = model.FotoPerfil;
        entity.EstadoCuenta = model.EstadoCuenta;
        entity.TipoUsuario = model.TipoUsuario;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(UsuarioAssembler_custom) ENABLED START*/
    // Add custom properties, validation helpers or actions here.
    /*PROTECTED REGION END*/
}
