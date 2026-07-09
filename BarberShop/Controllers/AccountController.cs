using BarberShop.Models;
using BarberShop.Services.Auth;
using BarberShop.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.Controllers;

[Route("Account")]
public class AccountController(
    IAuthenticationService authenticationService,
    IPasswordResetService passwordResetService,
    UserManager<ApplicationUser> userManager,
    IEmailService emailService) : Controller
{
    [HttpGet("Register")]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost("Register")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, message) = await authenticationService.RegisterAsync(model);
        if (!success)
        {
            ModelState.AddModelError("", message);
            return View(model);
        }

        TempData["SuccessMessage"] = message;
        return RedirectToAction("Login");
    }

    [HttpGet("Login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost("Login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, message) = await authenticationService.LoginAsync(model);
        if (!success)
        {
            // If login failed, check if the reason is unconfirmed email and offer resend
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user != null && !user.EmailConfirmed)
            {
                ViewBag.UnverifiedEmail = model.Email;
                ViewBag.Message = "Please confirm your email address before logging in. Check your email for the confirmation link.";
                return View(model);
            }

            ModelState.AddModelError("", message);
            return View(model);
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost("Logout")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await authenticationService.LogoutAsync();
        TempData["SuccessMessage"] = "You have been logged out successfully";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("ConfirmEmail")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            ViewBag.Error = "Invalid confirmation link";
            return View();
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            ViewBag.Error = "User not found";
            return View();
        }

        var result = await userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            ViewBag.Success = true;
            ViewBag.Message = "Email confirmed successfully! You can now log in.";
        }
        else
        {
            ViewBag.Error = "Email confirmation failed. The link may be expired or invalid.";
        }

        return View();
    }

    [HttpPost("ResendConfirmation")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendConfirmation(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            TempData["ErrorMessage"] = "Email address is required.";
            return RedirectToAction("Login");
        }

        var user = await userManager.FindByEmailAsync(email);
        // Don't reveal whether the user exists
        if (user == null)
        {
            TempData["InfoMessage"] = "If an account exists with that email, a confirmation link has been sent.";
            return RedirectToAction("Login");
        }

        if (user.EmailConfirmed)
        {
            TempData["InfoMessage"] = "Your email is already confirmed. You can log in.";
            return RedirectToAction("Login");
        }

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token }, Request.Scheme)
                              ?? $"https://{Request.Host}/Account/ConfirmEmail?userId={user.Id}&token={Uri.EscapeDataString(token)}";

        try
        {
            var emailResult = await emailService.SendWelcomeEmailAsync(user.Email, user.FirstName, confirmationLink);
            if (emailResult.Success)
            {
                TempData["SuccessMessage"] = "Confirmation email resent. Please check your inbox.";
            }
            else
            {
                // include diagnostic message in logs but not in UI
                TempData["ErrorMessage"] = "Failed to send confirmation email. Please try again later.";
                _ = emailResult; // keep for debugging in logs if needed
            }
        }
        catch (Exception ex)
        {
            // log exception if logger available via service provider? For now set friendly message
            TempData["ErrorMessage"] = "Failed to send confirmation email. Please try again later.";
        }

        return RedirectToAction("Login");
    }

    [HttpGet("ForgotPassword")]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost("ForgotPassword")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            ModelState.AddModelError("", "Please enter your email address");
            return View();
        }

        var (success, message) = await passwordResetService.RequestPasswordResetAsync(email);

        // Always show success message for security (don't reveal if email exists)
        ViewBag.Message = "If an account exists with this email, a password reset link has been sent.";
        ViewBag.Email = email;

        return View("ForgotPasswordConfirmation");
    }

    [HttpGet("ResetPassword")]
    [AllowAnonymous]
    public IActionResult ResetPassword(string? userId, string? token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            ViewBag.Error = "Invalid password reset link";
            return View();
        }

        ViewData["UserId"] = userId;
        ViewData["Token"] = token;
        return View();
    }

    [HttpPost("ResetPassword")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string userId, string token, string password, string confirmPassword)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            ModelState.AddModelError("", "Invalid password reset link");
            return View();
        }

        var (success, message) = await passwordResetService.ResetPasswordAsync(userId, token, password, confirmPassword);
        if (!success)
        {
            ModelState.AddModelError("", message);
            ViewData["UserId"] = userId;
            ViewData["Token"] = token;
            return View();
        }

        TempData["SuccessMessage"] = message;
        return RedirectToAction("Login");
    }
}
