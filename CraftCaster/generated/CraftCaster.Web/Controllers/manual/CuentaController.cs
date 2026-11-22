using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

// Acceso (login por usuario o correo y registro), Mi cuenta y cerrar sesión.
// Sustituye a /Account/Login, que generado por HARIA Web pide el id numérico del usuario.
[Authorize]
public sealed class CuentaController : Controller
{
    private readonly UsuarioService _usuarioService;
    private readonly CartaService _cartaService;
    private readonly MazoService _mazoService;
    private readonly PartidaService _partidaService;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly ILogger<CuentaController> _logger;

    public CuentaController(
        UsuarioService usuarioService,
        CartaService cartaService,
        MazoService mazoService,
        PartidaService partidaService,
        IUnitOfWorkFactory unitOfWorkFactory,
        ILogger<CuentaController> logger)
    {
        _usuarioService = usuarioService;
        _cartaService = cartaService;
        _mazoService = mazoService;
        _partidaService = partidaService;
        _unitOfWorkFactory = unitOfWorkFactory;
        _logger = logger;
    }

    // GET /Cuenta/Acceso
    // Tambien responde en /Account/Login: esa es la ruta a la que manda la cookie cuando hace falta
    // sesion (LoginPath de Program.cs) y la que usan el layout y el Logout generados. Asi la pantalla
    // generada que pide el id numerico ya no se muestra nunca y no hay que tocar codigo generado.
    [AllowAnonymous]
    [HttpGet("/Cuenta/Acceso")]
    [HttpGet("/Account/Login")]
    public IActionResult Acceso(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(new AccesoViewModel { Login = new LoginFormViewModel { ReturnUrl = returnUrl } });
    }

    // GET y POST /Usuario/Register
    // El registro generado deja elegir FotoPerfil (un byte), EstadoCuenta y TipoUsuario, asi que
    // cualquiera podria crearse una cuenta Admin. Se redirige al registro propio (UseCase registrarUsuario).
    [AllowAnonymous]
    [HttpGet("/Usuario/Register")]
    [HttpPost("/Usuario/Register")]
    [IgnoreAntiforgeryToken]
    public IActionResult RegistroGenerado() => Redirect(Url.Action(nameof(Acceso)) + "#registro");

    // POST /Cuenta/Entrar
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Entrar([Bind(Prefix = nameof(AccesoViewModel.Login))] LoginFormViewModel login)
    {
        if (!ModelState.IsValid)
        {
            return View(nameof(Acceso), new AccesoViewModel { Login = login });
        }

        // 1. Buscar al usuario por correo o nombre (filtro obtenerUsuarioPorEmailONombre)
        var encontrados = await _usuarioService.ObtenerUsuarioPorEmailONombreAsync(login.Identificador.Trim());
        var usuario = encontrados.FirstOrDefault();

        // 2. La contraseña la comprueba la lógica (Usuario.login), no el controlador
        var token = usuario is null ? null : await _usuarioService.LoginAsync(usuario.Id, login.Contrasenia);
        if (usuario is null || string.IsNullOrWhiteSpace(token))
        {
            ModelState.AddModelError(nameof(AccesoViewModel.Login), "Usuario o contraseña incorrectos.");
            return View(nameof(Acceso), new AccesoViewModel { Login = login });
        }

        if (usuario.EstadoCuenta == EstadoCuenta.Suspendida)
        {
            ModelState.AddModelError(nameof(AccesoViewModel.Login), "Tu cuenta está suspendida. Contacta con un administrador.");
            return View(nameof(Acceso), new AccesoViewModel { Login = login });
        }

        await IniciarSesionAsync(usuario);

        return Url.IsLocalUrl(login.ReturnUrl)
            ? LocalRedirect(login.ReturnUrl!)
            : RedirectToAction("Index", "Home");
    }

    // POST /Cuenta/Registrarse
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrarse([Bind(Prefix = nameof(AccesoViewModel.Registro))] RegistroFormViewModel registro)
    {
        if (!ModelState.IsValid)
        {
            return View(nameof(Acceso), new AccesoViewModel { Registro = registro });
        }

        CraftCaster.Domain.Entities.Usuario nuevo;
        try
        {
            // CustomTransaction registrarUsuario: comprueba que el correo y el nombre no existan
            await using var unitOfWork = _unitOfWorkFactory.Create();
            var useCase = new CraftCaster.Application.UseCases.Usuario.UsuarioUseCase(unitOfWork);
            nuevo = await useCase.RegistrarUsuarioAsync(registro.NombreUsuario.Trim(), registro.Email.Trim(), registro.Contrasenia, 1);
        }
        catch (ModelException error)
        {
            ModelState.AddModelError(nameof(AccesoViewModel.Registro), MensajeDe(error));
            return View(nameof(Acceso), new AccesoViewModel { Registro = registro });
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            _logger.LogError(error, "No se ha podido registrar el usuario.");
            ModelState.AddModelError(nameof(AccesoViewModel.Registro), "No se ha podido crear la cuenta. Inténtalo de nuevo.");
            return View(nameof(Acceso), new AccesoViewModel { Registro = registro });
        }

        await IniciarSesionAsync(nuevo);
        return RedirectToAction(nameof(Index));
    }

    // GET /Cuenta (Mi cuenta)
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var usuarioId = UsuarioActualId();
        var usuario = usuarioId is null ? null : await _usuarioService.ReadOIDAsync(usuarioId.Value);
        if (usuario is null)
        {
            await CerrarSesionAsync();
            return RedirectToAction(nameof(Acceso));
        }

        var cartas = await _cartaService.ObtenerCartasUsuarioAsync(usuario.Id);
        var mazos = await _mazoService.ObtenerMazosUsuarioAsync(usuario.Id);
        var partidas = await _partidaService.ObtenerPartidasUsuarioAsync(usuario.Id);

        return View(new MiCuentaViewModel
        {
            Usuario = UsuarioAssembler.ToViewModel(usuario),
            Cartas = cartas.Select(CartaAssembler.ToViewModel).OrderBy(c => c.Nombre).ToList(),
            Mazos = mazos.Select(MazoAssembler.ToViewModel).OrderBy(m => m.Nombre).ToList(),
            Partidas = partidas.Select(PartidaAssembler.ToViewModel).OrderByDescending(p => p.FechaInicio).ToList(),
        });
    }

    // POST /Cuenta/Salir
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Salir()
    {
        await CerrarSesionAsync();
        return RedirectToAction("Index", "Home");
    }

    // Id del usuario de la cookie (HARIA Web guarda el id en ClaimTypes.Name)
    private int? UsuarioActualId() =>
        User.FindFirstValue("HariaUserClass") == "Usuario"
        && int.TryParse(User.FindFirstValue(ClaimTypes.Name), out var id)
            ? id
            : null;

    // Mismo inicio de sesión que el AccountController generado por HARIA Web
    private async Task IniciarSesionAsync(CraftCaster.Domain.Entities.Usuario usuario)
    {
        await HariaWebAuthentication.SignInAsync(HttpContext, "Usuario", usuario.Id, usuario);
        HttpContext.Session.SetUser(UsuarioAssembler.ToViewModel(usuario));
    }

    private async Task CerrarSesionAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.ClearClasses();
    }

    // Los casos de uso envuelven el error: mostramos el mensaje original si lo hay
    private static string MensajeDe(ModelException error) =>
        error.InnerException is ModelException interna ? interna.Message : error.Message;
}
