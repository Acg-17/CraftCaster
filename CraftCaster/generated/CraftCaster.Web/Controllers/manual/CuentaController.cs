using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
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
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CuentaController> _logger;
    private readonly ITimeLimitedDataProtector _protectorRecuperacion;

    // Enlace de recuperación de contraseña: caduca y deja de valer en cuanto se cambia la contraseña
    private static readonly TimeSpan CaducidadEnlaceRecuperacion = TimeSpan.FromMinutes(30);

    public CuentaController(
        UsuarioService usuarioService,
        CartaService cartaService,
        MazoService mazoService,
        PartidaService partidaService,
        IUnitOfWorkFactory unitOfWorkFactory,
        IWebHostEnvironment env,
        IConfiguration configuration,
        ILogger<CuentaController> logger,
        IDataProtectionProvider dataProtectionProvider)
    {
        _protectorRecuperacion = dataProtectionProvider
            .CreateProtector("CraftCaster.Cuenta.RecuperarContrasenia")
            .ToTimeLimitedDataProtector();
        _usuarioService = usuarioService;
        _cartaService = cartaService;
        _mazoService = mazoService;
        _partidaService = partidaService;
        _unitOfWorkFactory = unitOfWorkFactory;
        _env = env;
        _configuration = configuration;
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

    // GET /Cuenta/AccesoGoogle -> Inicia el flujo OAuth 2.0 con Google
    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccesoGoogle(string? returnUrl = null)
    {
        var clientId = _configuration["Authentication:Google:ClientId"];
        var clientSecret = _configuration["Authentication:Google:ClientSecret"];
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            TempData["Error"] = "Google OAuth no está configurado todavía. Añade ClientId y ClientSecret en appsettings.Development.json (sección Authentication:Google).";
            return RedirectToAction(nameof(Acceso), new { returnUrl });
        }

        var redirectUrl = Url.Action(nameof(GoogleCallback), "Cuenta", new { returnUrl });
        var props = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(props, Microsoft.AspNetCore.Authentication.Google.GoogleDefaults.AuthenticationScheme);
    }

    // GET /Cuenta/GoogleCallback -> Recibe los claims de Google, busca o registra el Usuario en BD e inicia sesión
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GoogleCallback(string? returnUrl = null)
    {
        var result = await HttpContext.AuthenticateAsync("GoogleExternal");
        if (!result.Succeeded || result.Principal is null)
        {
            TempData["Error"] = "No se pudo completar el inicio de sesión con Google.";
            return RedirectToAction(nameof(Acceso), new { returnUrl });
        }

        var email = result.Principal.FindFirstValue(ClaimTypes.Email)?.Trim();
        var nombreGoogle = result.Principal.FindFirstValue(ClaimTypes.Name)?.Trim();
        await HttpContext.SignOutAsync("GoogleExternal");

        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["Error"] = "Google no devolvió un correo electrónico válido.";
            return RedirectToAction(nameof(Acceso), new { returnUrl });
        }

        var existentes = await _usuarioService.ObtenerUsuarioPorEmailONombreAsync(email);
        var usuario = existentes.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));

        if (usuario is null)
        {
            // Crear cuenta automáticamente con CustomTransaction registrarUsuario
            var baseNombre = !string.IsNullOrWhiteSpace(nombreGoogle)
                ? nombreGoogle.Replace(" ", "")
                : email.Split('@')[0];
            if (baseNombre.Length > 24) baseNombre = baseNombre[..24];
            if (baseNombre.Length < 3) baseNombre = $"User{Random.Shared.Next(100, 999)}";

            var nombreFinal = baseNombre;
            var duplicados = await _usuarioService.ObtenerUsuarioPorEmailONombreAsync(nombreFinal);
            if (duplicados.Any(u => string.Equals(u.NombreUsuario, nombreFinal, StringComparison.OrdinalIgnoreCase)))
            {
                nombreFinal = $"{baseNombre[..Math.Min(baseNombre.Length, 20)]}{Random.Shared.Next(100, 9999)}";
            }

            try
            {
                await using var unitOfWork = _unitOfWorkFactory.Create();
                var useCase = new CraftCaster.Application.UseCases.Usuario.UsuarioUseCase(unitOfWork);
                var passAleatoria = $"Ggl!{Guid.NewGuid():N}";
                usuario = await useCase.RegistrarUsuarioAsync(nombreFinal, email, passAleatoria, 1);
            }
            catch (ModelException error)
            {
                TempData["Error"] = MensajeDe(error);
                return RedirectToAction(nameof(Acceso), new { returnUrl });
            }
        }

        if (usuario.EstadoCuenta == EstadoCuenta.Suspendida)
        {
            TempData["Error"] = "Tu cuenta está suspendida. Contacta con un administrador.";
            return RedirectToAction(nameof(Acceso), new { returnUrl });
        }

        await IniciarSesionAsync(usuario);
        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction("Index", "Home");
    }

    // =====================================================================
    // Recuperar contraseña
    // 1. El usuario escribe su correo y se genera un enlace firmado y con caducidad (30 min) para esa cuenta.
    // 2. El enlace se "envía" al correo. La aplicación no tiene servidor de correo configurado, así que
    //    se escribe en el log del servidor (consola de dotnet run). Para producción: sustituir
    //    EnviarEnlaceRecuperacion por el envío real del correo.
    // 3. Con el enlace se elige la contraseña nueva. Lleva una huella de la contraseña actual,
    //    así que deja de valer en cuanto se usa (un solo uso).
    // La respuesta es siempre la misma, exista o no la cuenta, para no revelar qué correos están registrados.
    // =====================================================================

    // GET /Cuenta/RecuperarContrasenia
    [AllowAnonymous]
    [HttpGet]
    public IActionResult RecuperarContrasenia()
    {
        return View(new RecuperarContraseniaViewModel());
    }

    // POST /Cuenta/RecuperarContrasenia
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecuperarContrasenia(RecuperarContraseniaViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var email = model.Email.Trim();
        var usuario = (await _usuarioService.ObtenerUsuarioPorEmailONombreAsync(email))
            .FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));

        if (usuario is not null && usuario.EstadoCuenta != EstadoCuenta.Suspendida)
        {
            var token = _protectorRecuperacion.Protect(
                $"{usuario.Id}|{HuellaContrasenia(usuario)}", CaducidadEnlaceRecuperacion);
            var enlace = Url.Action(nameof(RestablecerContrasenia), "Cuenta", new { token }, Request.Scheme)!;
            EnviarEnlaceRecuperacion(usuario, enlace);
        }

        TempData["Exito"] = "Si el correo pertenece a una cuenta activa, te hemos enviado un enlace para cambiar la contraseña. Caduca en 30 minutos.";
        return RedirectToAction(nameof(Acceso));
    }

    // GET /Cuenta/RestablecerContrasenia?token=...
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> RestablecerContrasenia(string? token)
    {
        if (await UsuarioDelTokenAsync(token) is null)
        {
            TempData["Error"] = "El enlace no es válido o ha caducado. Solicita uno nuevo.";
            return RedirectToAction(nameof(RecuperarContrasenia));
        }

        return View(new RestablecerContraseniaViewModel { Token = token! });
    }

    // POST /Cuenta/RestablecerContrasenia
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestablecerContrasenia(RestablecerContraseniaViewModel model)
    {
        var usuarioToken = await UsuarioDelTokenAsync(model.Token);
        if (usuarioToken is null)
        {
            TempData["Error"] = "El enlace no es válido o ha caducado. Solicita uno nuevo.";
            return RedirectToAction(nameof(RecuperarContrasenia));
        }

        if (!ModelState.IsValid) return View(model);

        await using var unitOfWork = _unitOfWorkFactory.Create();
        var service = new UsuarioService(unitOfWork);
        var usuario = await service.ReadOIDAsync(usuarioToken.Id);
        if (usuario is null)
        {
            TempData["Error"] = "El enlace no es válido o ha caducado. Solicita uno nuevo.";
            return RedirectToAction(nameof(RecuperarContrasenia));
        }

        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.Cuenta.RestablecerContrasenia");
            await service.ModifyAsync(
                usuario.Id,
                usuario.NombreUsuario,
                usuario.Email,
                model.NuevaContrasenia,
                usuario.FotoPerfil,
                usuario.EstadoCuenta,
                usuario.TipoUsuario);
            await unitOfWork.CommitAsync();
        }
        catch (ModelException error)
        {
            await unitOfWork.RollbackAsync();
            ModelState.AddModelError(string.Empty, MensajeDe(error));
            return View(model);
        }

        TempData["Exito"] = "Contraseña actualizada correctamente. Ya puedes iniciar sesión.";
        return RedirectToAction(nameof(Acceso));
    }

    // Devuelve el usuario del enlace si la firma es válida, no ha caducado, la contraseña no ha cambiado
    // desde que se pidió y la cuenta no está suspendida. Si no, null.
    private async Task<CraftCaster.Domain.Entities.Usuario?> UsuarioDelTokenAsync(string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;

        string contenido;
        try
        {
            contenido = _protectorRecuperacion.Unprotect(token);
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            return null; // firma no válida o enlace caducado
        }

        var partes = contenido.Split('|', 2);
        if (partes.Length != 2 || !int.TryParse(partes[0], out var usuarioId)) return null;

        var usuario = await _usuarioService.ReadOIDAsync(usuarioId);
        if (usuario is null || usuario.EstadoCuenta == EstadoCuenta.Suspendida) return null;

        return string.Equals(partes[1], HuellaContrasenia(usuario), StringComparison.Ordinal) ? usuario : null;
    }

    // Huella de la contraseña guardada (ya cifrada) y del correo: cambia al cambiar cualquiera de los dos
    private static string HuellaContrasenia(CraftCaster.Domain.Entities.Usuario usuario)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes($"{usuario.Id}|{usuario.Email}|{usuario.Contrasenia}"));
        return WebEncoders.Base64UrlEncode(bytes);
    }

    // Sustituir por el envío real del correo cuando haya un servidor SMTP configurado
    private void EnviarEnlaceRecuperacion(CraftCaster.Domain.Entities.Usuario usuario, string enlace)
    {
        _logger.LogWarning(
            "[Recuperar contraseña] Enlace para {Usuario} <{Email}> (caduca en 30 min): {Enlace}",
            usuario.NombreUsuario, usuario.Email, enlace);
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

        ViewBag.FotoPerfilUrl = BuscarFotoPerfil(usuario.Id);

        return View(new MiCuentaViewModel
        {
            Usuario = UsuarioAssembler.ToViewModel(usuario),
            Cartas = cartas.Select(CartaAssembler.ToViewModel).OrderBy(c => c.Nombre).ToList(),
            Mazos = mazos.Select(MazoAssembler.ToViewModel).OrderBy(m => m.Nombre).ToList(),
            Partidas = partidas.Select(PartidaAssembler.ToViewModel).OrderByDescending(p => p.FechaInicio).ToList(),
        });
    }

    // POST /Cuenta/SubirFotoPerfil -> Actualiza Usuario.FotoPerfil en BD y guarda la imagen
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubirFotoPerfil(IFormFile? fotoArchivo)
    {
        var usuarioId = UsuarioActualId();
        if (usuarioId is null) return RedirectToAction(nameof(Acceso));

        if (fotoArchivo is not { Length: > 0 })
        {
            TempData["Error"] = "Selecciona una imagen para tu foto de perfil.";
            return RedirectToAction(nameof(Index));
        }

        await using var unitOfWork = _unitOfWorkFactory.Create();
        var usuario = await unitOfWork.UsuarioRepository.ReadAsync(usuarioId.Value);
        if (usuario is null) return RedirectToAction(nameof(Acceso));

        await unitOfWork.BeginTransactionAsync("MVC.Cuenta.SubirFotoPerfil");
        usuario.FotoPerfil = 1;
        await unitOfWork.UsuarioRepository.SaveAsync(usuario);
        await unitOfWork.CommitAsync();

        var ext = Path.GetExtension(fotoArchivo.FileName).ToLowerInvariant();
        if (ext is not (".png" or ".jpg" or ".jpeg" or ".webp" or ".gif")) ext = ".png";
        var dir = Path.Combine(_env.WebRootPath, "uploads", "perfiles");
        Directory.CreateDirectory(dir);
        foreach (var antiguo in Directory.GetFiles(dir, $"usuario_{usuario.Id}.*"))
        {
            System.IO.File.Delete(antiguo);
        }
        var destino = Path.Combine(dir, $"usuario_{usuario.Id}{ext}");
        await using (var stream = new FileStream(destino, FileMode.Create))
        {
            await fotoArchivo.CopyToAsync(stream);
        }

        TempData["Exito"] = "Foto de perfil actualizada.";
        return RedirectToAction(nameof(Index));
    }

    private string? BuscarFotoPerfil(int usuarioId)
    {
        var dir = Path.Combine(_env.WebRootPath, "uploads", "perfiles");
        if (!Directory.Exists(dir)) return null;
        var file = Directory.GetFiles(dir, $"usuario_{usuarioId}.*").FirstOrDefault();
        return file is null ? null : $"/uploads/perfiles/{Path.GetFileName(file)}";
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
