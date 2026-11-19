// HARIA-WEB GENERATED sha256:e78aea367e811972eef1b17a1994fe209d72f2b68c7fe0ee9e9413be7a4e46c8
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CraftCaster.Web.Utils.Security;

public static partial class HariaWebAuthentication
{
    public static async Task SignInAsync(HttpContext context, string domainClass, object identifier, object? serverEntity = null)
    {
        var id = Convert.ToString(identifier, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, domainClass + ":" + id),
            new(ClaimTypes.Name, id),
            new("HariaUserClass", domainClass)
        };
        // Politica explicita del servidor, aplicada despues de validar credenciales
        // y cargar la entidad. Sin configuracion no se concede ningun rol.
        if (serverEntity is not null)
        {
            var mapping = context.RequestServices.GetRequiredService<IConfiguration>()
                .GetSection("HariaWeb:Authentication:RolesByUserClass").GetChildren()
                .FirstOrDefault(section => string.Equals(section.Key, domainClass, StringComparison.OrdinalIgnoreCase));
            var roles = mapping?.GetChildren().Select(section => section.Value?.Trim())
                .Where(role => !string.IsNullOrWhiteSpace(role)).Distinct(StringComparer.Ordinal);
            if (roles is not null)
                foreach (var role in roles) claims.Add(new Claim(ClaimTypes.Role, role!));
        }
        AddTrustedClaims(claims, domainClass, identifier, serverEntity);
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme,
                ClaimTypes.Name, ClaimTypes.Role)));
    }

    // Implementa este punto en un parcial manual con información del servidor.
    // HariaUserClass identifica el origen; los roles requieren configuracion explicita
    // o este punto manual. Los roles no proceden del formulario ni del snapshot.
    static partial void AddTrustedClaims(List<Claim> claims, string domainClass, object identifier, object? serverEntity);
}
