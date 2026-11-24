using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SistemaMVC.Data;
using SistemaMVC.ViewModels;

namespace SistemaMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly UsuarioDAO _usuarioDao;
        public AccountController(UsuarioDAO usuarioDao) => _usuarioDao = usuarioDao;

        [HttpGet]
        public IActionResult Login()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Usuario")))
                return RedirectToAction("Index", "Home");
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            try
            {
                var usuario = _usuarioDao.Validar(vm.Usuario.Trim(), vm.Clave);
                if (usuario == null)
                {
                    ModelState.AddModelError(string.Empty, "Usuario o clave incorrectos.");
                    return View(vm);
                }
                HttpContext.Session.Clear();
                HttpContext.Session.SetString("Usuario", usuario.NombreUsuario);
                HttpContext.Session.SetString("Nombre", usuario.NombreCompleto);
                HttpContext.Session.SetString("Rol", usuario.Rol);
                HttpContext.Session.SetString("CodOperario", usuario.CodOperario ?? "");
                return RedirectToAction("Index", "Home");
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError(string.Empty, "No se pudo conectar con la base de datos: " + ex.Message);
                return View(vm);
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }
    }
}
