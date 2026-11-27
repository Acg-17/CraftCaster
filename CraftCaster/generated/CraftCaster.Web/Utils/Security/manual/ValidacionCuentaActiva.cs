using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using CraftCaster.Application.Services;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Web.Utils.Security;

// Se ejecuta en cada petición con la cookie de sesión (OnValidatePrincipal, ver Program.cs).
// Cierra la sesión si la cuenta se ha borrado, está suspendida o ha cambiado de rol desde que inició sesión.
// Sin esto, una cuenta suspendida seguía usando la web hasta que caducaba la cookie, y un cambio de rol
// (por ejemplo quitar el Admin) no se aplicaba hasta volver a iniciar sesión.
public static class ValidacionCuentaActiva
{
    public static async Task ValidarAsync(CookieValidatePrincipalContext context)
    {
        // Ficheros estáticos (css, js, imágenes): no hace falta consultar la base de datos
        if (Path.HasExtension(context.HttpContext.Request.Path.Value)) return;

        var principal = context.Principal;
        if (principal is null || principal.FindFirstValue("HariaUserClass") != "Usuario") return;
        if (!int.TryParse(principal.FindFirstValue(ClaimTypes.Name), out var usuarioId)) return;

        var usuarioService = context.HttpContext.RequestServices.GetRequiredService<UsuarioService>();
        var usuario = await usuarioService.ReadOIDAsync(usuarioId);

        var rolActual = usuario?.TipoUsuario == TipoUsuario.Admin
            ? HariaWebAuthentication.RolAdmin
            : HariaWebAuthentication.RolCliente;

        var sesionValida = usuario is not null
            && usuario.EstadoCuenta != EstadoCuenta.Suspendida
            && principal.IsInRole(rolActual);
        if (sesionValida) return;

        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        try
        {
            context.HttpContext.Session.Clear();
        }
        catch (InvalidOperationException)
        {
            // Sin sesión configurada en esta petición: no hay nada que limpiar
        }
    }
}
