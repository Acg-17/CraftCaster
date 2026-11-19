using System.ComponentModel.DataAnnotations;

namespace CraftCaster.Web.ViewModels;

public sealed class TurnoViewModel : IValidatableObject
{
    /*PROTECTED REGION ID(TurnoViewModel_generated) START*/
    [Key]
    public int Id { get; set; }

    public int NumTurno { get; set; }

    public int ManaTurno { get; set; }

    [UIHint("HariaAssociation")]
    [Display(Name = "Partida")]
    public int? PartidaId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> PartidaOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "Partida")]
    public string? PartidaLabel { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [UIHint("HariaAssociation")]
    [Display(Name = "Jugador")]
    public int? JugadorId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> JugadorOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "Jugador")]
    public string? JugadorLabel { get; set; }

    // HARIA-WEB COMPOSITION {"property":"Acciones","fullName":"CraftCaster.Web.ViewModels.AccionViewModel","fields":[{"name":"Id","type":"int","key":true},{"name":"Name","type":"CraftCaster.Domain.Enums.TipoAccion","key":false,"enumType":"CraftCaster.Domain.Enums.TipoAccion"},{"name":"OrigenId","type":"int?","association":true},{"name":"OrigenLabel","type":"string?","readOnly":true}]}
    [UIHint("HariaComposition")]
    [MinLength(0)]
    public List<AccionViewModel> Acciones { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public AccionViewModel AccionesTemplate { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in ValidateAdditional(validationContext)) yield return result;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(TurnoViewModel_manual) ENABLED START*/
    // Add custom properties and validation here.
    private IEnumerable<ValidationResult> ValidateAdditional(ValidationContext validationContext)
    {
        yield break;
    }

    /*PROTECTED REGION END*/
}
