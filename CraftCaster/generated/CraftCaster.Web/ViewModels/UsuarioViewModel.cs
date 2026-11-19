using System.ComponentModel.DataAnnotations;

namespace CraftCaster.Web.ViewModels;

public sealed class UsuarioViewModel : IValidatableObject
{
    /*PROTECTED REGION ID(UsuarioViewModel_generated) START*/
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes indicar {0}.")]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    [UIHint("HariaPassword")]
    [System.Text.Json.Serialization.JsonIgnore]
    public string? Contrasenia { get; set; }

    public byte FotoPerfil { get; set; }

    [EnumDataType(typeof(CraftCaster.Domain.Enums.EstadoCuenta), ErrorMessage = "El valor de {0} no es válido.")]
    public CraftCaster.Domain.Enums.EstadoCuenta EstadoCuenta { get; set; }

    [EnumDataType(typeof(CraftCaster.Domain.Enums.TipoUsuario), ErrorMessage = "El valor de {0} no es válido.")]
    public CraftCaster.Domain.Enums.TipoUsuario TipoUsuario { get; set; }



    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in ValidateAdditional(validationContext)) yield return result;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(UsuarioViewModel_manual) ENABLED START*/
    // Add custom properties and validation here.
    private IEnumerable<ValidationResult> ValidateAdditional(ValidationContext validationContext)
    {
        yield break;
    }

    /*PROTECTED REGION END*/
}
