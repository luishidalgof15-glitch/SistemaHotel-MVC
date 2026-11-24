using Microsoft.AspNetCore.Mvc;
using SistemaMVC.Filters;

namespace SistemaMVC.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>Interfaz del menú principal.</summary>
        [SesionRequerida]
        public IActionResult Index()
        {
            ViewBag.Rol = HttpContext.Session.GetString("Rol");
            return View();
        }

        public IActionResult Error() => View();
    }
}
