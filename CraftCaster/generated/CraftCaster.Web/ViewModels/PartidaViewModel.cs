using System.ComponentModel.DataAnnotations;

namespace CraftCaster.Web.ViewModels;

public sealed class PartidaViewModel : IValidatableObject
{
    /*PROTECTED REGION ID(PartidaViewModel_generated) START*/
    [Key]
    public int Id { get; set; }

    [EnumDataType(typeof(CraftCaster.Domain.Enums.EstadoPartida), ErrorMessage = "El valor de {0} no es válido.")]
    public CraftCaster.Domain.Enums.EstadoPartida Estado { get; set; }

    [UIHint("HariaReadOnly:Create,Edit")]
    public int? Ganador { get; set; }

    [DataType(DataType.Date)]
    public DateTime FechaInicio { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [UIHint("HariaAssociation")]
    [Display(Name = "Usuario1")]
    public int? Usuario1Id { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Usuario1Options { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "Usuario1")]
    public string? Usuario1Label { get; set; }

    [UIHint("HariaAssociation")]
    [Display(Name = "Usuario2")]
    public int? Usuario2Id { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Usuario2Options { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "Usuario2")]
    public string? Usuario2Label { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [UIHint("HariaAssociation")]
    [Display(Name = "Mazo1")]
    public int? Mazo1Id { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Mazo1Options { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "Mazo1")]
    public string? Mazo1Label { get; set; }

    [UIHint("HariaAssociation")]
    [Display(Name = "Mazo2")]
    public int? Mazo2Id { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Mazo2Options { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "Mazo2")]
    public string? Mazo2Label { get; set; }

    // HARIA-WEB DEDICATED COLLECTION {"property":"Turnos","controller":"Turno","parent":"Partida"}

    // HARIA-WEB DEDICATED COLLECTION {"property":"Jugadores","controller":"JugadorPartida","parent":"Partida"}

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in ValidateAdditional(validationContext)) yield return result;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(PartidaViewModel_manual) ENABLED START*/
    // Add custom properties and validation here.
    private IEnumerable<ValidationResult> ValidateAdditional(ValidationContext validationContext)
    {
        yield break;
    }

    /*PROTECTED REGION END*/
}
