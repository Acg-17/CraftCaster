using System.ComponentModel.DataAnnotations;

namespace CraftCaster.Web.ViewModels;

// Pantalla de acceso: dos formularios (iniciar sesión y crear cuenta) en la misma vista.
public sealed class AccesoViewModel
{
    public LoginFormViewModel Login { get; set; } = new();
    public RegistroFormViewModel Registro { get; set; } = new();
}

public sealed class LoginFormViewModel
{
    [Required(ErrorMessage = "Debes indicar {0}.")]
    [Display(Name = "Usuario o correo", Prompt = "alvaro@craftcaster.com")]
    public string Identificador { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contrasenia { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public sealed class RegistroFormViewModel
{
    [Required(ErrorMessage = "Debes indicar {0}.")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "{0} debe tener entre {2} y {1} caracteres.")]
    [Display(Name = "Usuario", Prompt = "Ej: PauCremades")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [EmailAddress(ErrorMessage = "Escribe un correo electrónico válido.")]
    [Display(Name = "Correo electrónico", Prompt = "tu@correo.com")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos {2} caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña", Prompt = "Mínimo 8 caracteres")]
    public string Contrasenia { get; set; } = string.Empty;
}

// Recuperar contraseña: paso 1, pedir el enlace por correo
public sealed class RecuperarContraseniaViewModel
{
    [Required(ErrorMessage = "Debes indicar {0}.")]
    [EmailAddress(ErrorMessage = "Escribe un correo electrónico válido.")]
    [Display(Name = "Correo electrónico", Prompt = "ana@craftcaster.com")]
    public string Email { get; set; } = string.Empty;
}

// Formulario del enlace de recuperación: el token firmado identifica la cuenta
public sealed class RestablecerContraseniaViewModel
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos {2} caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva contraseña", Prompt = "Mínimo 8 caracteres")]
    public string NuevaContrasenia { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes confirmar la nueva contraseña.")]
    [Compare(nameof(NuevaContrasenia), ErrorMessage = "Las contraseñas no coinciden.")]
    [DataType(DataType.Password)]
    [Display(Name = "Repetir nueva contraseña")]
    public string ConfirmarContrasenia { get; set; } = string.Empty;
}

// Mi cuenta: datos del usuario y sus cartas, mazos y partidas.
public sealed class MiCuentaViewModel
{
    public UsuarioViewModel Usuario { get; set; } = new();
    public List<CartaViewModel> Cartas { get; set; } = new();
    public List<MazoViewModel> Mazos { get; set; } = new();
    public List<PartidaViewModel> Partidas { get; set; } = new();
}
