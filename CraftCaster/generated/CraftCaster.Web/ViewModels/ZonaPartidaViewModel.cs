using System.ComponentModel.DataAnnotations;

namespace CraftCaster.Web.ViewModels;

public sealed class ZonaPartidaViewModel : IValidatableObject
{
    /*PROTECTED REGION ID(ZonaPartidaViewModel_generated) START*/
    [Key]
    public int Id { get; set; }

    [EnumDataType(typeof(CraftCaster.Domain.Enums.TipoZona), ErrorMessage = "El valor de {0} no es válido.")]
    public CraftCaster.Domain.Enums.TipoZona TipoZona { get; set; }

    [UIHint("HariaAssociation")]
    [Display(Name = "Jugador")]
    public int? JugadorId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> JugadorOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "Jugador")]
    public string? JugadorLabel { get; set; }

    // HARIA-WEB COMPOSITION {"property":"Cartas","fullName":"CraftCaster.Web.ViewModels.CartaPartidaViewModel","fields":[{"name":"Id","type":"int","key":true},{"name":"VidaActual","type":"int","key":false},{"name":"AtaqueActual","type":"int","key":false},{"name":"PuedeAtacar","type":"bool","key":false},{"name":"OriginalId","type":"int?","association":true},{"name":"OriginalLabel","type":"string?","readOnly":true}]}
    [UIHint("HariaComposition")]
    [MinLength(0)]
    public List<CartaPartidaViewModel> Cartas { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public CartaPartidaViewModel CartasTemplate { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in ValidateAdditional(validationContext)) yield return result;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(ZonaPartidaViewModel_manual) ENABLED START*/
    // Add custom properties and validation here.
    private IEnumerable<ValidationResult> ValidateAdditional(ValidationContext validationContext)
    {
        yield break;
    }

    /*PROTECTED REGION END*/
}
