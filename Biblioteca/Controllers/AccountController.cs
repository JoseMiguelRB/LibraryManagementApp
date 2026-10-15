using Biblioteca.Data.Entities;
using Biblioteca.Helpers;
using Biblioteca.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Controllers;

public class AccountController : Controller
{
    private readonly IUserHelper _userHelper;
    private readonly IMailHelper _mailHelper;
    private readonly IWebHostEnvironment _env;

    public AccountController(IUserHelper userHelper, IMailHelper mailHelper, IWebHostEnvironment env)
    {
        _userHelper = userHelper;
        _mailHelper = mailHelper;
        _env = env;
    }

    // ===================== LOGIN / LOGOUT =====================

    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _userHelper.LoginAsync(model);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Conta bloqueada temporariamente devido a demasiadas tentativas. Tente novamente dentro de 5 minutos.");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Email ou palavra-passe incorretos.");
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _userHelper.LogoutAsync();
        return RedirectToAction("Index", "Home");
    }

    // ===================== REGISTO =====================

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterNewUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existente = await _userHelper.GetUserByEmailAsync(model.Username);
        if (existente != null)
        {
            ModelState.AddModelError(nameof(model.Username), "Já existe uma conta com este email.");
            return View(model);
        }

        var user = new User
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Username,
            UserName = model.Username
        };

        var result = await _userHelper.AddUserAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, TraduzirErro(error.Code, error.Description));
            }
            return View(model);
        }

        // Login automático após o registo
        await _userHelper.LoginAsync(new LoginViewModel
        {
            Username = model.Username,
            Password = model.Password
        });

        TempData["Sucesso"] = "Conta criada com sucesso. Bem-vindo(a)!";
        return RedirectToAction("Index", "Home");
    }

    // ===================== PERFIL =====================

    [Authorize]
    public async Task<IActionResult> ChangeUser()
    {
        var user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);
        if (user == null)
        {
            return View("RegistoNaoEncontrado");
        }

        var model = new ChangeUserViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> ChangeUser(ChangeUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);
        if (user == null)
        {
            return View("RegistoNaoEncontrado");
        }

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.PhoneNumber = model.PhoneNumber;

        var result = await _userHelper.UpdateUserAsync(user);
        if (result.Succeeded)
        {
            TempData["Sucesso"] = "Perfil atualizado com sucesso.";
            return RedirectToAction(nameof(ChangeUser));
        }

        ModelState.AddModelError(string.Empty, "Não foi possível atualizar o perfil. Tente novamente.");
        return View(model);
    }

    // ===================== ALTERAR PALAVRA-PASSE =====================

    [Authorize]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);
        if (user == null)
        {
            return View("RegistoNaoEncontrado");
        }

        var result = await _userHelper.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
        if (result.Succeeded)
        {
            TempData["Sucesso"] = "Palavra-passe alterada com sucesso.";
            return RedirectToAction(nameof(ChangeUser));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, TraduzirErro(error.Code, error.Description));
        }

        return View(model);
    }

    // ===================== RECUPERAR PALAVRA-PASSE =====================

    public IActionResult RecoverPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecoverPassword(RecoverPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Por segurança, a mensagem é sempre a mesma (não revela se o email existe)
        ViewBag.Mensagem = "Se o email estiver registado, receberá as instruções para definir uma nova palavra-passe.";

        var user = await _userHelper.GetUserByEmailAsync(model.Email);
        if (user == null)
        {
            return View();
        }

        var token = await _userHelper.GeneratePasswordResetTokenAsync(user);
        var link = Url.Action(nameof(ResetPassword), "Account",
            new { token, email = user.Email }, protocol: Request.Scheme);

        var enviado = await _mailHelper.SendEmailAsync(
            user.Email!,
            "Biblioteca - Recuperação da palavra-passe",
            $"<h2>Recuperação da palavra-passe</h2>" +
            $"<p>Para definir uma nova palavra-passe clique na ligação abaixo:</p>" +
            $"<p><a href=\"{link}\">Definir nova palavra-passe</a></p>");

        // Em desenvolvimento (sem SMTP configurado) mostra o link no ecrã para demonstração
        if (!enviado && _env.IsDevelopment())
        {
            ViewBag.LinkDemo = link;
        }

        return View();
    }

    public IActionResult ResetPassword(string? token, string? email)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
        {
            return RedirectToAction(nameof(Login));
        }

        return View(new ResetPasswordViewModel { Token = token, Username = email });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userHelper.GetUserByEmailAsync(model.Username);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Não foi possível redefinir a palavra-passe.");
            return View(model);
        }

        var result = await _userHelper.ResetPasswordAsync(user, model.Token, model.Password);
        if (result.Succeeded)
        {
            TempData["Sucesso"] = "Palavra-passe redefinida com sucesso. Já pode iniciar sessão.";
            return RedirectToAction(nameof(Login));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, TraduzirErro(error.Code, error.Description));
        }

        return View(model);
    }

    public IActionResult NotAuthorized()
    {
        return View();
    }

    // Traduz os erros mais comuns do Identity para português
    private static string TraduzirErro(string code, string descricao) => code switch
    {
        "PasswordMismatch" => "A palavra-passe atual está incorreta.",
        "PasswordTooShort" => "A palavra-passe é demasiado curta (mínimo 6 caracteres).",
        "PasswordRequiresDigit" => "A palavra-passe deve conter pelo menos um número.",
        "DuplicateUserName" or "DuplicateEmail" => "Já existe uma conta com este email.",
        "InvalidToken" => "A ligação de recuperação é inválida ou expirou.",
        _ => descricao
    };
}
