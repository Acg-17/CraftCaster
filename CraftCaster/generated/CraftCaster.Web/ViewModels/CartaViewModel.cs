using System.ComponentModel.DataAnnotations;

namespace CraftCaster.Web.ViewModels;

public sealed class CartaViewModel : IValidatableObject
{
    /*PROTECTED REGION ID(CartaViewModel_generated) START*/
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    public string Nombre { get; set; } = string.Empty;

    [EnumDataType(typeof(CraftCaster.Domain.Enums.Rareza), ErrorMessage = "El valor de {0} no es válido.")]
    public CraftCaster.Domain.Enums.Rareza Rareza { get; set; }

    public int CosteMana { get; set; }

    [EnumDataType(typeof(CraftCaster.Domain.Enums.TipoCarta), ErrorMessage = "El valor de {0} no es válido.")]
    public CraftCaster.Domain.Enums.TipoCarta Tipo { get; set; }

    [UIHint("HariaReadOnly:Create,Edit")]
    public int? Vida { get; set; }

    [UIHint("HariaReadOnly:Create,Edit")]
    public int? Ataque { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    public string Descripcion { get; set; } = string.Empty;

    public byte Imagen { get; set; }

    public bool EsPublica { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [UIHint("HariaAssociation")]
    [Display(Name = "Usuario")]
    public int? UsuarioId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> UsuarioOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "Usuario")]
    public string? UsuarioLabel { get; set; }

    // HARIA-WEB DEDICATED COLLECTION {"property":"Estados","controller":"Estado","parent":"Carta"}

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in ValidateAdditional(validationContext)) yield return result;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(CartaViewModel_manual) ENABLED START*/
    // Add custom properties and validation here.
    private IEnumerable<ValidationResult> ValidateAdditional(ValidationContext validationContext)
    {
        yield break;
    }

    /*PROTECTED REGION END*/
}
