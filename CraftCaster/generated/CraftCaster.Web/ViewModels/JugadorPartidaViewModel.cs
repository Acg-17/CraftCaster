using System.ComponentModel.DataAnnotations;

namespace CraftCaster.Web.ViewModels;

public sealed class JugadorPartidaViewModel : IValidatableObject
{
    /*PROTECTED REGION ID(JugadorPartidaViewModel_generated) START*/
    [Key]
    public int Id { get; set; }

    public int Vida { get; set; }

    public int NumJugador { get; set; }

    public int Mana { get; set; }

    public bool Activo { get; set; }

    [UIHint("HariaAssociation")]
    [Display(Name = "Partida")]
    public int? PartidaId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> PartidaOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "Partida")]
    public string? PartidaLabel { get; set; }

    // HARIA-WEB DEDICATED COLLECTION {"property":"Zonas","controller":"ZonaPartida","parent":"Jugador"}

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in ValidateAdditional(validationContext)) yield return result;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(JugadorPartidaViewModel_manual) ENABLED START*/
    // Add custom properties and validation here.
    private IEnumerable<ValidationResult> ValidateAdditional(ValidationContext validationContext)
    {
        yield break;
    }

    /*PROTECTED REGION END*/
}
