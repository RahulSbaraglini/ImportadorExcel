using DesafioConcilig1.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DesafioConcilig1.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            // Se já estiver logado, redireciona para Home/Index
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            // Aponta para Views/Login/Index.cshtml
            return View("~/Views/Login/Index.cshtml");
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string nomeUsuario, string senha, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrEmpty(nomeUsuario) || string.IsNullOrEmpty(senha))
            {
                ViewBag.MensagemErro = "Usuário e senha são obrigatórios.";
                return View("~/Views/Login/Index.cshtml");
            }

            // Busca o usuário pelo NomeUsuario e Senha
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.NomeUsuario == nomeUsuario && u.Senha == senha);

            if (usuario == null)
            {
                ViewBag.MensagemErro = "Usuário ou senha inválidos.";
                return View("~/Views/Login/Index.cshtml");
            }

            // Cria os Claims e faz o SignIn para gerar o cookie
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.NomeUsuario)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Redireciona para returnUrl, se informado e válido, ou para Home/Index
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Após logout, volta para a View de login em Views/Login/Index.cshtml
            return RedirectToAction("Login", "Account");
        }
    }
}