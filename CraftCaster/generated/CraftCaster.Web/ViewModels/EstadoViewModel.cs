using System.ComponentModel.DataAnnotations;

namespace CraftCaster.Web.ViewModels;

public sealed class EstadoViewModel : IValidatableObject
{
    /*PROTECTED REGION ID(EstadoViewModel_generated) START*/
    [Key]
    public int Id { get; set; }

    [EnumDataType(typeof(CraftCaster.Domain.Enums.TipoEstado), ErrorMessage = "El valor de {0} no es válido.")]
    public CraftCaster.Domain.Enums.TipoEstado Tipo { get; set; }

    [UIHint("HariaAssociation")]
    [Display(Name = "Carta")]
    public int? CartaId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> CartaOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "Carta")]
    public string? CartaLabel { get; set; }

    // HARIA-WEB COMPOSITION {"property":"Habilidades","fullName":"CraftCaster.Web.ViewModels.HabilidadViewModel","fields":[{"name":"Id","type":"int","key":true},{"name":"Tipo","type":"CraftCaster.Domain.Enums.TipoHabilidad","key":false,"enumType":"CraftCaster.Domain.Enums.TipoHabilidad"},{"name":"Cantidad","type":"int","key":false},{"name":"TipoObjetivo","type":"CraftCaster.Domain.Enums.TipoObjetivo","key":false,"enumType":"CraftCaster.Domain.Enums.TipoObjetivo"},{"name":"Objetivo","type":"int","key":false}]}
    [UIHint("HariaComposition")]
    [MinLength(0)]
    public List<HabilidadViewModel> Habilidades { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public HabilidadViewModel HabilidadesTemplate { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in ValidateAdditional(validationContext)) yield return result;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(EstadoViewModel_manual) ENABLED START*/
    // Add custom properties and validation here.
    private IEnumerable<ValidationResult> ValidateAdditional(ValidationContext validationContext)
    {
        yield break;
    }

    /*PROTECTED REGION END*/
}
