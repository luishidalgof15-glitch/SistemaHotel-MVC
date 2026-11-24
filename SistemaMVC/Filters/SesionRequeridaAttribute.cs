using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SistemaMVC.Filters
{
    /// <summary>
    /// Precondición de los CU: el usuario debe estar logueado (y opcionalmente tener un rol).
    /// </summary>
    public class SesionRequeridaAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;

        public SesionRequeridaAttribute(string roles = "")
        {
            _roles = roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var sesion = context.HttpContext.Session;
            var usuario = sesion.GetString("Usuario");
            if (string.IsNullOrEmpty(usuario))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            if (_roles.Length > 0)
            {
                var rol = sesion.GetString("Rol") ?? "";
                if (!_roles.Contains(rol))
                {
                    if (context.Controller is Controller c)
                        c.TempData["Error"] = "Su usuario no tiene permiso para usar esta opción.";
                    context.Result = new RedirectToActionResult("Index", "Home", null);
                }
            }
        }
    }
}
