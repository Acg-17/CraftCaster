// HARIA-WEB GENERATED sha256:33e7d75ef612b237128f3e0793b9411d726a142527de6ea1ee09623fee19705f
using System.ComponentModel.DataAnnotations;

namespace CraftCaster.Web.ViewModels;

public sealed class LoginViewModel
{
    public string UserClass { get; set; } = "Usuario";
    public string? Identifier { get; set; }
    [Display(Name = "id")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
