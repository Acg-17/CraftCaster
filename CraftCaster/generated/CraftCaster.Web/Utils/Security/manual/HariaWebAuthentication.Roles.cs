using System.Security.Claims;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Web.Utils.Security;

// Punto manual de HariaWebAuthentication (AddTrustedClaims).
// CraftCaster solo tiene una clase <User> (Usuario), así que RolesByUserClass no sirve para
// distinguir jugadores y administradores: el rol sale del atributo TipoUsuario de la entidad
// cargada en el servidor tras validar la contraseña (nunca del formulario).
//   TipoUsuario.Cliente -> rol "Cliente"
//   TipoUsuario.Admin   -> rol "Admin"
// Los roles viajan en la cookie: si cambia el TipoUsuario, ValidacionCuentaActiva cierra la sesión
// en la siguiente petición y el usuario vuelve a entrar con el rol nuevo.
public static partial class HariaWebAuthentication
{
    public const string RolCliente = "Cliente";
    public const string RolAdmin = "Admin";

    static partial void AddTrustedClaims(List<Claim> claims, string domainClass, object identifier, object? serverEntity)
    {
        if (serverEntity is not CraftCaster.Domain.Entities.Usuario usuario)
        {
            return;
        }

        var rol = usuario.TipoUsuario == TipoUsuario.Admin ? RolAdmin : RolCliente;
        if (!claims.Any(c => c.Type == ClaimTypes.Role && c.Value == rol))
        {
            claims.Add(new Claim(ClaimTypes.Role, rol));
        }
    }
}
