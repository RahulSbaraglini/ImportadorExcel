using Microsoft.AspNetCore.Mvc;

namespace DesafioConcilig1.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string email, string senha)
        {
            if (email == "admin@email.com" && senha == "123")
            {
                HttpContext.Session.SetString("usuarioLogado", email);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.MensagemErro = "Credenciais inválidas.";

            return RedirectToAction("Index", "Importador");
        }

        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
