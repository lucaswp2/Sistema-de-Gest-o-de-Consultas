using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UVVConsultas.Data;
using UVVConsultas.Models;

namespace UVVConsultas.Controllers
{
    // Controller responsável por Cadastro, Login e Logout de usuários
    public class ContaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<Usuario> _passwordHasher = new();

        public ContaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Conta/Cadastro
        [HttpGet]
        public IActionResult Cadastro()
        {
            return View(new Usuario());
        }

        // POST: /Conta/Cadastro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cadastro([Bind("Nome,Email,Senha")] Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                return View(usuario);
            }

            bool emailJaExiste = await _context.Usuarios
                .AnyAsync(u => u.Email == usuario.Email);

            if (emailJaExiste)
            {
                ModelState.AddModelError(nameof(usuario.Email), "Este e-mail já está cadastrado.");
                return View(usuario);
            }

            usuario.DataCadastro = DateTime.Now;
            // Nunca armazenar senha em texto puro: gera o hash antes de persistir
            usuario.Senha = _passwordHasher.HashPassword(usuario, usuario.Senha);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            TempData["Mensagem"] = "Cadastro realizado com sucesso! Faça login para continuar.";
            return RedirectToAction(nameof(Login));
        }

        // GET: /Conta/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        // POST: /Conta/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
                return View(model);
            }

            var resultadoVerificacao = _passwordHasher.VerifyHashedPassword(
                usuario, usuario.Senha, model.Senha);

            if (resultadoVerificacao == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Consultas");
        }

        // POST: /Conta/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
