using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SistemaMVC.Data;
using SistemaMVC.Filters;
using SistemaMVC.Helpers;
using SistemaMVC.Models;
using SistemaMVC.ViewModels;

namespace SistemaMVC.Controllers
{
    /// <summary>
    /// Controlador del CU "Generar requerimiento". Actor: Operario.
    /// </summary>
    [SesionRequerida("Operario,Administrador")]
    public class RequerimientoController : Controller
    {
        private const string CLAVE_SESION = "RequerimientoActual";

        private readonly MaterialDAO _materialDao;
        private readonly RequerimientoDAO _requerimientoDao;

        public RequerimientoController(MaterialDAO materialDao, RequerimientoDAO requerimientoDao)
        {
            _materialDao = materialDao;
            _requerimientoDao = requerimientoDao;
        }

        private RequerimientoViewModel ObtenerVM()
        {
            var vm = HttpContext.Session.GetObjeto<RequerimientoViewModel>(CLAVE_SESION) ?? new RequerimientoViewModel();
            // Código de operario precargado por el inicio de sesión
            vm.CodOperario = HttpContext.Session.GetString("CodOperario") ?? "";
            return vm;
        }

        private IActionResult Mostrar(RequerimientoViewModel vm)
        {
            HttpContext.Session.SetObjeto(CLAVE_SESION, vm);
            ModelState.Clear();
            return View("Index", vm);
        }

        /// <summary>Opción "Generar requerimiento" del menú principal.</summary>
        public IActionResult Nuevo()
        {
            HttpContext.Session.Remove(CLAVE_SESION);
            return RedirectToAction(nameof(Index));
        }

        /// <summary>Paso 1: muestra la interfaz Generar Requerimiento.</summary>
        public IActionResult Index()
        {
            var vm = ObtenerVM();
            if (string.IsNullOrEmpty(vm.CodOperario))
                vm.Error = "Su usuario no tiene un código de operario asignado.";
            return Mostrar(vm);
        }

        /// <summary>Pasos 2 al 4: Consultar Materiales (incluye CU Consultar Materiales).</summary>
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult ConsultarMateriales()
        {
            var vm = ObtenerVM();
            try
            {
                vm.ListaMateriales = _materialDao.Listar();
                vm.MostrarMateriales = true;
                if (vm.ListaMateriales.Count == 0) vm.Error = "No hay materiales registrados.";
            }
            catch (SqlException ex) { vm.Error = "Error de base de datos: " + ex.Message; }
            return Mostrar(vm);
        }

        /// <summary>Paso 5: el operario selecciona los materiales que considere conveniente.</summary>
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult AgregarMateriales(List<SeleccionMaterial> seleccion)
        {
            var vm = ObtenerVM();
            var elegidos = seleccion.Where(s => s.Seleccionado && !string.IsNullOrEmpty(s.CodMaterial)).ToList();

            if (elegidos.Count == 0)
            {
                vm.Error = "No seleccionó ningún material.";
                return Mostrar(vm);
            }
            if (elegidos.Any(s => s.Cantidad <= 0))
            {
                vm.Error = "La cantidad de cada material seleccionado debe ser mayor a cero.";
                return Mostrar(vm);
            }

            try
            {
                foreach (var s in elegidos)
                {
                    var material = _materialDao.Buscar(s.CodMaterial!);
                    if (material == null) continue;

                    var existente = vm.Detalles.FirstOrDefault(d => d.CodMaterial == material.CodMaterial);
                    if (existente != null)
                        existente.Cantidad += s.Cantidad;
                    else
                        vm.Detalles.Add(new DetalleRequerimiento
                        {
                            CodOperario = vm.CodOperario,
                            CodMaterial = material.CodMaterial,
                            Material = material.Nombre,
                            Marca = material.Marca,
                            Cantidad = s.Cantidad
                        });
                }
                vm.Mensaje = $"{elegidos.Count} material(es) agregado(s) a la cuadrícula.";
            }
            catch (SqlException ex) { vm.Error = "Error de base de datos: " + ex.Message; }
            return Mostrar(vm);
        }

        /// <summary>Flujo alternativo: Eliminar material seleccionado antes de generar.</summary>
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Eliminar(string? materialEliminar)
        {
            var vm = ObtenerVM();
            if (string.IsNullOrEmpty(materialEliminar))
                vm.Error = "Seleccione un material de la cuadrícula para eliminar.";
            else
            {
                vm.Detalles.RemoveAll(d => d.CodMaterial == materialEliminar);
                vm.Mensaje = $"Material {materialEliminar} eliminado.";
            }
            return Mostrar(vm);
        }

        /// <summary>Opción "Limpiar": vacía la cuadrícula.</summary>
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Limpiar()
        {
            var vm = ObtenerVM();
            vm.Detalles.Clear();
            return Mostrar(vm);
        }

        /// <summary>Pasos 6 al 9: Generar Requerimiento (autogenera número y graba).</summary>
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Generar()
        {
            var vm = ObtenerVM();
            if (string.IsNullOrEmpty(vm.CodOperario))
            {
                vm.Error = "Su usuario no tiene un código de operario asignado.";
                return Mostrar(vm);
            }
            if (vm.Detalles.Count == 0)
            {
                vm.Error = "Seleccione al menos un material antes de generar el requerimiento.";
                return Mostrar(vm);
            }

            try
            {
                var requerimiento = new Requerimiento { CodOperario = vm.CodOperario, Detalles = vm.Detalles };
                int numero = _requerimientoDao.Grabar(requerimiento);
                HttpContext.Session.Remove(CLAVE_SESION);
                var nuevo = new RequerimientoViewModel
                {
                    CodOperario = vm.CodOperario,
                    NumRequerimientoGenerado = numero,
                    Mensaje = $"Requerimiento generado con el Nro. {numero}"
                };
                ModelState.Clear();
                return View("Index", nuevo);
            }
            catch (SqlException ex)
            {
                vm.Error = "Error al grabar el requerimiento: " + ex.Message;
                return Mostrar(vm);
            }
        }

        /// <summary>Pasos 10 y 11: Salir, regresa al menú principal.</summary>
        public IActionResult Salir()
        {
            HttpContext.Session.Remove(CLAVE_SESION);
            return RedirectToAction("Index", "Home");
        }
    }
}
