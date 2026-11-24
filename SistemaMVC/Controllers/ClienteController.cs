using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SistemaMVC.Data;
using SistemaMVC.Filters;
using SistemaMVC.Models;

namespace SistemaMVC.Controllers
{
    /// <summary>CU Mantener Clientes – flujo "Agregar Cliente" (punto de extensión del CU Reservar Habitación).</summary>
    [SesionRequerida("Recepcionista,Administrador")]
    public class ClienteController : Controller
    {
        private readonly ClienteDAO _clienteDao;
        public ClienteController(ClienteDAO clienteDao) => _clienteDao = clienteDao;

        [HttpGet]
        public IActionResult Create() => View(new Cliente());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Cliente cliente)
        {
            ModelState.Remove(nameof(Cliente.CodCliente));
            if (!ModelState.IsValid) return View(cliente);
            try
            {
                var codigo = _clienteDao.Registrar(cliente);
                TempData["Mensaje"] = $"Cliente registrado con el código {codigo}.";
                return RedirectToAction("SeleccionarCliente", "Reserva", new { cod = codigo });
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError(string.Empty, "Error al registrar el cliente: " + ex.Message);
                return View(cliente);
            }
        }
    }
}
