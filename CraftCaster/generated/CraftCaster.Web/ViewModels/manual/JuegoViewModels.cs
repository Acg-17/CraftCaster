using System.ComponentModel.DataAnnotations;
using CraftCaster.Domain.Enums;
using CraftCaster.Web.Services;

namespace CraftCaster.Web.ViewModels;

// ===================== OPCIONES PARTIDA (sala) =====================

// Sala de partidas: la partida activa del usuario (si la tiene) y las partidas en espera a las que unirse.
public sealed class SalaViewModel
{
    public int UsuarioId { get; set; }

    // Partida en Espera o EnCurso en la que participa el usuario (como local o visitante)
    public PartidaViewModel? PartidaActiva { get; set; }
    public bool SoyLocal { get; set; }

    // Mazo jugable (30 cartas) con el que jugaría
    public string? MazoConElQueJuego { get; set; }
    public int? MazoNumCartas { get; set; }
    public bool TieneMazoJugable { get; set; }

    public List<PartidaEnEsperaViewModel> PartidasEnEspera { get; set; } = new();
}

public sealed class PartidaEnEsperaViewModel
{
    public int Id { get; set; }

    [Display(Name = "Anfitrión")]
    public string Anfitrion { get; set; } = string.Empty;

    [Display(Name = "Mazo del anfitrión")]
    public string Mazo { get; set; } = string.Empty;

    [Display(Name = "Creada")]
    [DataType(DataType.Date)]
    public DateTime FechaInicio { get; set; }
}

// Formulario POST de unirse (solo se envía el id de la partida; el usuario sale de la cookie)
public sealed class UnirsePartidaFormViewModel
{
    [Required(ErrorMessage = "Debes indicar la partida.")]
    [Range(1, int.MaxValue, ErrorMessage = "Partida no válida.")]
    public int? PartidaId { get; set; }
}

// Historial: resultado del ReadFilter Partida.obtenerPartidasUsuario
public sealed class HistorialViewModel
{
    public int UsuarioId { get; set; }
    public List<PartidaHistorialViewModel> Partidas { get; set; } = new();
}

public sealed class PartidaHistorialViewModel
{
    public int Id { get; set; }
    public DateTime FechaInicio { get; set; }
    public EstadoPartida Estado { get; set; }
    public string Rival { get; set; } = string.Empty;
    public string? MiMazo { get; set; }
    public string Resultado { get; set; } = string.Empty;
}

// ===================== TABLERO PARTIDA =====================

public sealed class TableroViewModel
{
    public int PartidaId { get; set; }
    public EstadoPartida Estado { get; set; }
    public int NumTurno { get; set; }
    public int RondaActual { get; set; }

    // Calculado con las Custom Partida.getActivo + Partida.getPlayer
    public bool PartidaSinEmpezar { get; set; }
    public bool EsMiTurno { get; set; }
    public string? NombreJugadorActivo { get; set; }

    // Firma ligera para el polling en segundo plano sin recargar si nada ha cambiado
    public string FirmaEstado { get; set; } = string.Empty;

    public JugadorTableroViewModel Yo { get; set; } = new();
    public JugadorTableroViewModel? Rival { get; set; }

    // Formularios del tablero (POST)
    public LanzarCartaFormViewModel Lanzar { get; set; } = new();
    public AtacarFormViewModel Atacar { get; set; } = new();
    public ActivarHabilidadFormViewModel Activar { get; set; } = new();
}

public sealed class JugadorTableroViewModel
{
    public int JugadorId { get; set; }
    public int NumJugador { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Mazo { get; set; }

    [Display(Name = "Vida")]
    public int Vida { get; set; }

    [Display(Name = "Maná")]
    public int Mana { get; set; }

    public int ManaMaximo { get; set; }
    public int TurnosJugados { get; set; }
    public bool Activo { get; set; }

    // Mazo: solo el número de cartas (Custom ZonaPartida.getAllCartas)
    [Display(Name = "Cartas en el mazo")]
    public int CartasEnMazo { get; set; }

    // Mano: visible para mí, solo el número para el rival (se muestran ocultas)
    [Display(Name = "Cartas en la mano")]
    public int CartasEnMano { get; set; }
    public List<CartaTableroViewModel> Mano { get; set; } = new();

    public List<CartaTableroViewModel> Campo { get; set; } = new();
    public List<CartaTableroViewModel> Descarte { get; set; } = new();
    public int CartasEnDescarte => Descarte.Count;
}

// CartaPartida aplanada con los datos de su Carta original (asociación CartaPartida -> Carta)
// y las habilidades de sus estados (Carta -> Estado -> Habilidad).
public sealed class CartaTableroViewModel
{
    public int CartaPartidaId { get; set; }
    public int CartaOriginalId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
    public TipoCarta Tipo { get; set; }
    public Rareza Rareza { get; set; }
    public int CosteMana { get; set; }
    public int VidaBase { get; set; }
    public int AtaqueBase { get; set; }
    public int VidaActual { get; set; }
    public int AtaqueActual { get; set; }
    public bool PuedeAtacar { get; set; }

    // Lo rellena el controlador según la zona y el turno
    public bool SePuedeLanzar { get; set; }
    public bool PuedeAtacarAhora { get; set; }
    public bool SePuedenActivarHabilidades { get; set; }
    public ModoObjetivoCarta ModoObjetivo { get; set; }

    public List<HabilidadTableroViewModel> Habilidades { get; set; } = new();
}

public sealed class HabilidadTableroViewModel
{
    public int Id { get; set; }

    [Display(Name = "Momento")]
    public TipoEstado Momento { get; set; }

    public TipoHabilidad Tipo { get; set; }
    public int Cantidad { get; set; }
    public TipoObjetivo TipoObjetivo { get; set; }
    public int Objetivo { get; set; }

    public string Texto => $"{Momento}: {Tipo} {Cantidad} → {TipoObjetivo}";
}

public sealed class LanzarCartaFormViewModel
{
    [Required(ErrorMessage = "Debes elegir una carta.")]
    [Range(1, int.MaxValue, ErrorMessage = "Carta no válida.")]
    public int? CartaPartidaId { get; set; }

    public int? ObjetivoCartaId { get; set; }
    public bool ObjetivoJugador { get; set; }
}

public sealed class AtacarFormViewModel
{
    [Required(ErrorMessage = "Debes elegir una criatura atacante.")]
    [Range(1, int.MaxValue, ErrorMessage = "Criatura atacante no válida.")]
    public int? AtacanteId { get; set; }

    public int? ObjetivoCartaId { get; set; }
    public bool ObjetivoJugador { get; set; }
}

public sealed class ActivarHabilidadFormViewModel
{
    [Required(ErrorMessage = "Debes elegir una habilidad.")]
    [Range(1, int.MaxValue, ErrorMessage = "Habilidad no válida.")]
    public int? HabilidadId { get; set; }
}

// ===================== FIN DE PARTIDA =====================

public sealed class FinPartidaViewModel
{
    public int PartidaId { get; set; }
    public DateTime FechaInicio { get; set; }
    public int NumTurnos { get; set; }

    public int? GanadorId { get; set; }

    [Display(Name = "Ganador")]
    public string? GanadorNombre { get; set; }

    public bool HeGanado { get; set; }
    public bool SoyParticipante { get; set; }

    public string NombreLocal { get; set; } = string.Empty;
    public int? VidaLocal { get; set; }
    public string? NombreVisitante { get; set; }
    public int? VidaVisitante { get; set; }
}
