using System.ComponentModel.DataAnnotations;
using CraftCaster.Domain.Entities;
using CraftCaster.Domain.Enums;

namespace CraftCaster.Web.ViewModels;

// Bloque simple de evento (Estado) + efecto (Habilidad) para el Creador de Cartas
public sealed class BloqueEventoInput
{
    public TipoEstado TipoEstado { get; set; } = TipoEstado.Entrar;
    public TipoHabilidad TipoHabilidad { get; set; } = TipoHabilidad.Danyo;
    [Range(1, 99, ErrorMessage = "La cantidad debe estar entre 1 y 99.")]
    public int Cantidad { get; set; } = 1;
    public TipoObjetivo TipoObjetivo { get; set; } = TipoObjetivo.Enemigo;
    public int Objetivo { get; set; } = 1;
}

// ViewModel del Creador y Editor de Cartas
public sealed class CreadorCartaViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(60)]
    public string Nombre { get; set; } = string.Empty;

    public Rareza Rareza { get; set; } = Rareza.Comun;

    [Range(0, 20, ErrorMessage = "El coste de maná debe estar entre 0 y 20.")]
    [Display(Name = "Coste de maná")]
    public int CosteMana { get; set; } = 1;

    public TipoCarta Tipo { get; set; } = TipoCarta.Criatura;

    [Range(0, 99)]
    public int Ataque { get; set; } = 1;

    [Range(0, 99)]
    public int Vida { get; set; } = 1;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(250)]
    [Display(Name = "Descripción")]
    public string Descripcion { get; set; } = string.Empty;

    [Display(Name = "Compartir con la comunidad")]
    public bool EsPublica { get; set; }

    // Subida de fichero (requisito: atributo type="file")
    [Display(Name = "Imagen de la carta")]
    public IFormFile? ImagenArchivo { get; set; }

    public string? ImagenUrlActual { get; set; }

    public List<BloqueEventoInput> Bloques { get; set; } = new();
}

// ViewModel de Detalle de Carta
public sealed class DetalleCartaViewModel
{
    public required Carta Carta { get; init; }
    public string? ImagenUrl { get; init; }
    public bool EsPropia { get; init; }
}

// ViewModel para crear un mazo nuevo con Mazo.newCustomized
public sealed class CrearMazoViewModel
{
    [Required(ErrorMessage = "El nombre del mazo es obligatorio.")]
    [StringLength(60)]
    public string Nombre { get; set; } = string.Empty;

    [Display(Name = "Compartir con la comunidad")]
    public bool EsPublico { get; set; }

    [Display(Name = "Portada del mazo")]
    public IFormFile? ImagenArchivo { get; set; }
}

// ViewModel del Editor de Mazos (colección, cartas del mazo, recuento 30/30, cuotas de rareza y curva de maná)
public sealed class EditorMazoViewModel
{
    public required Mazo Mazo { get; init; }
    public string? ImagenUrl { get; init; }
    public List<Carta> Coleccion { get; init; } = new();
    public Dictionary<int, string?> ImagenesCartas { get; init; } = new();
    public string? Buscar { get; init; }
    public Dictionary<int, int> CurvaMana { get; init; } = new();
    public int TotalCartas { get; init; }
    public int MaxCurva { get; init; }
    public int NumComunes { get; init; }
    public int NumEspeciales { get; init; }
    public int NumEpicas { get; init; }
    public int NumLegendarias { get; init; }
}
