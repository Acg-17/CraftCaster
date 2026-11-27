using CraftCaster.Domain.Enums;
using CraftCaster.Web.Services;

namespace CraftCaster.Web.ViewModels;

public sealed class AdminPanelViewModel
{
    public string? Busqueda { get; set; }
    public EstadoCuenta? FiltroEstado { get; set; }
    public TipoUsuario? FiltroTipo { get; set; }

    public int TotalUsuarios { get; set; }
    public int UsuariosActivos { get; set; }
    public int UsuariosReportados { get; set; }
    public int UsuariosSuspendidos { get; set; }
    public int TotalAdmins { get; set; }
    public int TotalMazos { get; set; }
    public int TotalCartas { get; set; }
    public int TotalPartidas { get; set; }

    public int? UsuarioActualId { get; set; }

    public List<UsuarioViewModel> Usuarios { get; set; } = new();
    public List<UsuarioViewModel> UsuariosSuspendidosLista { get; set; } = new();
    public List<UsuarioViewModel> UsuariosReportadosLista { get; set; } = new();
    public List<ReporteMazoItem> ReportesMazos { get; set; } = new();
    public Dictionary<int, int> ReportesPorUsuario { get; set; } = new();
}
