using System.ComponentModel.DataAnnotations;

namespace CraftCaster.Web.ViewModels;

public sealed class HabilidadViewModel : IValidatableObject
{
    /*PROTECTED REGION ID(HabilidadViewModel_generated) START*/
    [Key]
    public int Id { get; set; }

    [EnumDataType(typeof(CraftCaster.Domain.Enums.TipoHabilidad), ErrorMessage = "El valor de {0} no es válido.")]
    public CraftCaster.Domain.Enums.TipoHabilidad Tipo { get; set; }

    public int Cantidad { get; set; }

    [EnumDataType(typeof(CraftCaster.Domain.Enums.TipoObjetivo), ErrorMessage = "El valor de {0} no es válido.")]
    public CraftCaster.Domain.Enums.TipoObjetivo TipoObjetivo { get; set; }

    public int Objetivo { get; set; }

    [UIHint("HariaAssociation")]
    [Display(Name = "Estado")]
    public int? EstadoId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> EstadoOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "Estado")]
    public string? EstadoLabel { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in ValidateAdditional(validationContext)) yield return result;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(HabilidadViewModel_manual) ENABLED START*/
    // Add custom properties and validation here.
    private IEnumerable<ValidationResult> ValidateAdditional(ValidationContext validationContext)
    {
        yield break;
    }

    /*PROTECTED REGION END*/
}
