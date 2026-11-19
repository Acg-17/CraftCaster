// HARIA-WEB GENERATED sha256:16f5671a517e4c7f9fdbf9ff3d87061491940ef259325c04c4d25006b8d90993
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CraftCaster.Web.Assemblers;
using CraftCaster.Web.Utils.Security;
using CraftCaster.Web.ViewModels;

namespace CraftCaster.Web.Controllers;

public sealed class AccountController : Controller
{
    private readonly CraftCaster.Application.Services.UsuarioService _userService;
    private readonly CraftCaster.Application.Abstractions.IUnitOfWorkFactory _userRepository;

    public AccountController(CraftCaster.Application.Services.UsuarioService userService,
        CraftCaster.Application.Abstractions.IUnitOfWorkFactory userRepository)
    {
        _userService = userService;
        _userRepository = userRepository;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null) =>
        View(new LoginViewModel { ReturnUrl = returnUrl });

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var token = await _userService.LoginAsync(model.Id, model.Password);
        if (string.IsNullOrWhiteSpace(token))
        {
            ModelState.AddModelError(string.Empty, "Credenciales incorrectas.");
            return View(model);
        }

        await using var unitOfWork = _userRepository.Create();
        var entity = await unitOfWork.UsuarioRepository.ReadAsync(model.Id);
        if (entity is null)
        {
            ModelState.AddModelError(string.Empty, "Credenciales incorrectas.");
            return View(model);
        }
        var user = UsuarioAssembler.ToViewModel(entity);

        await HariaWebAuthentication.SignInAsync(HttpContext, "Usuario", entity.Id, entity);
        HttpContext.Session.SetUser(user);

        return Url.IsLocalUrl(model.ReturnUrl)
            ? LocalRedirect(model.ReturnUrl!)
            : RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.ClearClasses();
        return RedirectToAction("Login");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied() => View();
}
