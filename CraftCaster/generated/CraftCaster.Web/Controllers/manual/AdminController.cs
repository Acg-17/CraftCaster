using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CraftCaster.Application.Abstractions;
using CraftCaster.Application.Exceptions;
using CraftCaster.Application.Services;
using CraftCaster.Domain.Enums;
using CraftCaster.Web.Assemblers;
using CraftCaster.Web.Utils.Security;
using CraftCaster.Web.ViewModels;

namespace CraftCaster.Web.Controllers;

// Panel de administración: gestión de usuarios, suspensión/reactivación de cuentas,
// reportes y acceso a los CRUDs generados por HARIA Web.
[Authorize(Roles = HariaWebAuthentication.RolAdmin)]
public sealed class AdminController : Controller
{
    private readonly UsuarioService _usuarioService;
    private readonly MazoService _mazoService;
    private readonly CartaService _cartaService;
    private readonly PartidaService _partidaService;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        UsuarioService usuarioService,
        MazoService mazoService,
        CartaService cartaService,
        PartidaService partidaService,
        IUnitOfWorkFactory unitOfWorkFactory,
        ILogger<AdminController> logger)
    {
        _usuarioService = usuarioService;
        _mazoService = mazoService;
        _cartaService = cartaService;
        _partidaService = partidaService;
        _unitOfWorkFactory = unitOfWorkFactory;
        _logger = logger;
    }

    // GET /Admin
    [HttpGet]
    public async Task<IActionResult> Index(string? busqueda = null, EstadoCuenta? estado = null, TipoUsuario? tipo = null)
    {
        var todosUsuarios = await _usuarioService.ReadAllAsync();
        var mazos = await _mazoService.ReadAllAsync();
        var cartas = await _cartaService.ReadAllAsync();
        var partidas = await _partidaService.ReadAllAsync();

        var filtrados = todosUsuarios.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var termino = busqueda.Trim();
            filtrados = filtrados.Where(u =>
                u.NombreUsuario.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(termino, StringComparison.OrdinalIgnoreCase));
        }

        if (estado is EstadoCuenta estadoFiltro)
        {
            filtrados = filtrados.Where(u => u.EstadoCuenta == estadoFiltro);
        }

        if (tipo is TipoUsuario tipoFiltro)
        {
            filtrados = filtrados.Where(u => u.TipoUsuario == tipoFiltro);
        }

        var viewModel = new AdminPanelViewModel
        {
            Busqueda = busqueda,
            FiltroEstado = estado,
            FiltroTipo = tipo,
            TotalUsuarios = todosUsuarios.Count,
            UsuariosActivos = todosUsuarios.Count(u => u.EstadoCuenta == EstadoCuenta.Activa),
            UsuariosReportados = todosUsuarios.Count(u => u.EstadoCuenta == EstadoCuenta.Reportada),
            UsuariosSuspendidos = todosUsuarios.Count(u => u.EstadoCuenta == EstadoCuenta.Suspendida),
            TotalAdmins = todosUsuarios.Count(u => u.TipoUsuario == TipoUsuario.Admin),
            TotalMazos = mazos.Count,
            TotalCartas = cartas.Count,
            TotalPartidas = partidas.Count,
            UsuarioActualId = UsuarioActualId(),
            Usuarios = filtrados
                .OrderBy(u => u.Id)
                .Select(UsuarioAssembler.ToViewModel)
                .ToList(),
            UsuariosSuspendidosLista = todosUsuarios
                .Where(u => u.EstadoCuenta == EstadoCuenta.Suspendida)
                .OrderBy(u => u.NombreUsuario)
                .Select(UsuarioAssembler.ToViewModel)
                .ToList(),
            UsuariosReportadosLista = todosUsuarios
                .Where(u => u.EstadoCuenta == EstadoCuenta.Reportada)
                .OrderBy(u => u.NombreUsuario)
                .Select(UsuarioAssembler.ToViewModel)
                .ToList(),
        };

        return View(viewModel);
    }

    // POST /Admin/CambiarEstado
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, EstadoCuenta nuevoEstado)
    {
        if (id == UsuarioActualId() && nuevoEstado == EstadoCuenta.Suspendida)
        {
            TempData["Error"] = "No puedes suspender tu propia cuenta de administrador.";
            return RedirectToAction(nameof(Index));
        }

        await using var unitOfWork = _unitOfWorkFactory.Create();
        var service = new UsuarioService(unitOfWork);
        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.Admin.CambiarEstado");
            await service.CambiarEstadoCuentaAsync(id, nuevoEstado);
            await unitOfWork.CommitAsync();
            TempData["Mensaje"] = nuevoEstado == EstadoCuenta.Suspendida
                ? "Cuenta suspendida correctamente."
                : "Cuenta reactivada correctamente.";
        }
        catch (ModelException error)
        {
            await unitOfWork.RollbackAsync();
            TempData["Error"] = error.Message;
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            await unitOfWork.RollbackAsync();
            _logger.LogError(error, "No se pudo cambiar el estado de la cuenta {Id}.", id);
            TempData["Error"] = "No se pudo actualizar el estado de la cuenta.";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST /Admin/Reportar -> Invoca Usuario.reportarUsuario directo en BD
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reportar(int id)
    {
        if (id == UsuarioActualId())
        {
            TempData["Error"] = "No puedes reportar tu propia cuenta de administrador.";
            return RedirectToAction(nameof(Index));
        }

        await using var unitOfWork = _unitOfWorkFactory.Create();
        var service = new UsuarioService(unitOfWork);
        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.Admin.Reportar");
            await service.ReportarUsuarioAsync(id);
            await unitOfWork.CommitAsync();
            TempData["Mensaje"] = "Usuario marcado como reportado (a revisar).";
        }
        catch (ModelException error)
        {
            await unitOfWork.RollbackAsync();
            TempData["Error"] = error.Message;
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            await unitOfWork.RollbackAsync();
            _logger.LogError(error, "No se pudo reportar al usuario {Id}.", id);
            TempData["Error"] = "No se pudo procesar el reporte del usuario.";
        }

        return RedirectToAction(nameof(Index));
    }

    private int? UsuarioActualId() =>
        User.FindFirstValue("HariaUserClass") == "Usuario"
        && int.TryParse(User.FindFirstValue(ClaimTypes.Name), out var id)
            ? id
            : null;
}
