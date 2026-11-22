using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CraftCaster.Web.ViewModels;

// Filtros del buscador de Explorar (llegan por GET en la URL).
public sealed class ExplorarFiltroViewModel
{
    [Display(Name = "Buscar", Prompt = "Nombre de mazo...")]
    [StringLength(60)]
    public string? Nombre { get; set; }

    [Display(Name = "Coste medio desde")]
    [Range(0, 20, ErrorMessage = "{0} debe estar entre {1} y {2}.")]
    public int? CosteMin { get; set; }

    [Display(Name = "hasta")]
    [Range(0, 20, ErrorMessage = "El coste medio máximo debe estar entre {1} y {2}.")]
    public int? CosteMax { get; set; }

    [Display(Name = "Valoración mínima")]
    [Range(1, 5, ErrorMessage = "{0} debe estar entre {1} y {2}.")]
    public int? ValoracionMin { get; set; }

    [Display(Name = "Ordenar")]
    public string Orden { get; set; } = "valoracion";
}

public sealed class ExplorarViewModel
{
    public ExplorarFiltroViewModel Filtro { get; set; } = new();
    public List<MazoViewModel> Mazos { get; set; } = new();

    public static List<SelectListItem> OpcionesOrden { get; } = new()
    {
        new SelectListItem("Mejor valorados", "valoracion"),
        new SelectListItem("Coste medio: menor primero", "coste-asc"),
        new SelectListItem("Coste medio: mayor primero", "coste-desc"),
        new SelectListItem("Nombre (A-Z)", "nombre"),
    };

    public static List<SelectListItem> OpcionesValoracion { get; } = new()
    {
        new SelectListItem("Cualquiera", ""),
        new SelectListItem("1 estrella o más", "1"),
        new SelectListItem("2 estrellas o más", "2"),
        new SelectListItem("3 estrellas o más", "3"),
        new SelectListItem("4 estrellas o más", "4"),
        new SelectListItem("5 estrellas", "5"),
    };
}

// Detalle de un mazo: sus datos, sus cartas y el formulario para valorarlo.
public sealed class DetalleMazoViewModel
{
    public MazoViewModel Mazo { get; set; } = new();
    public List<CartaEnMazoViewModel> Cartas { get; set; } = new();
    public bool EsPropio { get; set; }
    public bool PuedeValorar { get; set; }
    public ValorarMazoFormViewModel Valorar { get; set; } = new();

    // Curva de maná: cuántas cartas hay de cada coste (6 agrupa 6 o más).
    public IReadOnlyList<(string Coste, int Cantidad)> CurvaMana =>
        Enumerable.Range(0, 7)
            .Select(coste => (coste == 6 ? "6+" : coste.ToString(),
                Cartas.Where(c => Math.Min(c.CosteMana, 6) == coste).Sum(c => c.Cantidad)))
            .ToList();
}

public sealed class CartaEnMazoViewModel
{
    public int CartaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int CosteMana { get; set; }
    public CraftCaster.Domain.Enums.TipoCarta Tipo { get; set; }
    public CraftCaster.Domain.Enums.Rareza Rareza { get; set; }
    public int Cantidad { get; set; }
}

public sealed class ValorarMazoFormViewModel
{
    [Required(ErrorMessage = "Elige una valoración.")]
    [Range(1, 5, ErrorMessage = "La valoración debe estar entre {1} y {2} estrellas.")]
    [Display(Name = "Tu valoración")]
    public int? Valoracion { get; set; }
}
