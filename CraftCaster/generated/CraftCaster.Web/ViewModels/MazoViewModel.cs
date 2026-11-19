using System.ComponentModel.DataAnnotations;

namespace CraftCaster.Web.ViewModels;

public sealed class MazoViewModel : IValidatableObject
{
    /*PROTECTED REGION ID(MazoViewModel_generated) START*/
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    public string Nombre { get; set; } = string.Empty;

    [UIHint("HariaReadOnly:Create")]
    public float CosteMedio { get; set; }

    [UIHint("HariaReadOnly:Create")]
    public int NumCartas { get; set; }

    [UIHint("HariaReadOnly:Create")]
    [EnumDataType(typeof(CraftCaster.Domain.Enums.EstadoMazo), ErrorMessage = "El valor de {0} no es válido.")]
    public CraftCaster.Domain.Enums.EstadoMazo EstadoMazo { get; set; }

    public bool EsPublico { get; set; }

    [UIHint("HariaReadOnly:Create")]
    public float Valoracion { get; set; }

    public byte Imagen { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [UIHint("HariaAssociation")]
    [Display(Name = "Usuario")]
    public int? UsuarioId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> UsuarioOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "Usuario")]
    public string? UsuarioLabel { get; set; }

    // HARIA-WEB COMPOSITION {"property":"Lineas","fullName":"CraftCaster.Web.ViewModels.LineaMazoViewModel","fields":[{"name":"Id","type":"int","key":true},{"name":"Cantidad","type":"int","key":false},{"name":"CartaId","type":"int?","association":true},{"name":"CartaLabel","type":"string?","readOnly":true}]}
    [UIHint("HariaComposition")]
    [MinLength(0)]
    public List<LineaMazoViewModel> Lineas { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public LineaMazoViewModel LineasTemplate { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in ValidateAdditional(validationContext)) yield return result;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(MazoViewModel_manual) ENABLED START*/
    // Add custom properties and validation here.
    private IEnumerable<ValidationResult> ValidateAdditional(ValidationContext validationContext)
    {
        yield break;
    }

    /*PROTECTED REGION END*/
}
