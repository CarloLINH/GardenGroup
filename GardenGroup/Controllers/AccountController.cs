using System.Security.Claims;
using GardenGroup.Models;
using GardenGroup.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GardenGroup.Controllers;

[Authorize]
public sealed class AccountController : Controller
{
    private readonly IEmployeeService _employeeService;
    private readonly IForgotPasswordService _forgotPasswordService;

    public AccountController(IEmployeeService employeeService, IForgotPasswordService forgotPasswordService)
    {
        _employeeService = employeeService;
        _forgotPasswordService = forgotPasswordService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var employee = await _employeeService.AuthenticateAsync(model.Email, model.Password);
        if (employee is null)
        {
            ModelState.AddModelError(string.Empty, "The email address or password is incorrect, or the account is inactive.");
            return View(model);
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, employee.EmployeeId.ToString()),
            new Claim(ClaimTypes.Name, $"{employee.FirstName} {employee.LastName}".Trim()),
            new Claim(ClaimTypes.Email, employee.Email),
            new Claim(ClaimTypes.Role, employee.Role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var token = await _forgotPasswordService.CreateResetTokenAsync(model.Email);
        TempData["ResetLink"] = token is null ? null : Url.Action(nameof(ResetPassword), new { email = model.Email, token });
        return View("ForgotPasswordConfirmation");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string email, string token) =>
        View(new ResetPasswordViewModel { Email = email, Token = token });

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid || !await _forgotPasswordService.ResetPasswordAsync(model.Email, model.Token, model.Password))
        {
            ModelState.AddModelError(string.Empty, "The reset link is invalid or has expired.");
            return View(model);
        }

        return RedirectToAction(nameof(Login));
    }
}