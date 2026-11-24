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
    /// Controlador del CU "Generar reserva de habitación". Actor: Recepcionista.
    /// Cada acción corresponde a una opción de la interfaz RESERVA.
    /// </summary>
    [SesionRequerida("Recepcionista,Administrador")]
    public class ReservaController : Controller
    {
        private const string CLAVE_SESION = "ReservaActual";

        private readonly ClienteDAO _clienteDao;
        private readonly HabitacionDAO _habitacionDao;
        private readonly ReservaDAO _reservaDao;

        public ReservaController(ClienteDAO clienteDao, HabitacionDAO habitacionDao, ReservaDAO reservaDao)
        {
            _clienteDao = clienteDao;
            _habitacionDao = habitacionDao;
            _reservaDao = reservaDao;
        }

        // ---------------- utilitarios ----------------
        private ReservaViewModel ObtenerVM() =>
            HttpContext.Session.GetObjeto<ReservaViewModel>(CLAVE_SESION) ?? new ReservaViewModel();

        private void GuardarVM(ReservaViewModel vm) => HttpContext.Session.SetObjeto(CLAVE_SESION, vm);

        /// <summary>Junta lo que la recepcionista escribió en pantalla con la reserva guardada en sesión.</summary>
        private ReservaViewModel Combinar(ReservaForm f)
        {
            var vm = ObtenerVM();
            vm.FechaLlegada = f.FechaLlegada;
            vm.FechaSalida = f.FechaSalida;
            vm.NombreHuesped = f.NombreHuesped?.Trim();
            return vm;
        }

        /// <summary>Pasos 6 y 7: calcula la cantidad de días. Devuelve el mensaje de error o null.</summary>
        private static string? CalcularDiasInterno(ReservaViewModel vm)
        {
            if (vm.FechaLlegada == null || vm.FechaSalida == null)
            {
                vm.CantidadDias = 0;
                vm.CalcularTotales();
                return "Ingrese la fecha de llegada y la fecha de salida.";
            }
            if (vm.FechaLlegada.Value.Date < DateTime.Today)
                return "La fecha de llegada no puede ser anterior a hoy.";
            if (vm.FechaSalida.Value.Date <= vm.FechaLlegada.Value.Date)
                return "La fecha de salida debe ser mayor a la fecha de llegada.";

            vm.CantidadDias = (vm.FechaSalida.Value.Date - vm.FechaLlegada.Value.Date).Days;
            vm.CalcularTotales();
            return null;
        }

        private IActionResult Mostrar(ReservaViewModel vm)
        {
            GuardarVM(vm);
            ModelState.Clear();
            vm.Mensaje ??= TempData["Mensaje"] as string;
            return View("Index", vm);
        }

        // ---------------- acciones ----------------

        /// <summary>Opción "Generar Reserva" del menú principal: abre la interfaz RESERVA vacía.</summary>
        public IActionResult Nueva()
        {
            HttpContext.Session.Remove(CLAVE_SESION);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Index() => Mostrar(ObtenerVM());

        /// <summary>Pasos 2 al 4: Buscar Cliente (incluye CU Buscar Cliente).</summary>
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult BuscarCliente(ReservaForm f)
        {
            var vm = Combinar(f);
            try
            {
                if (string.IsNullOrWhiteSpace(f.CodCliente))
                {
                    vm.ListaClientes = _clienteDao.Listar();
                    vm.MostrarClientes = true;
                }
                else
                {
                    var cliente = _clienteDao.Buscar(f.CodCliente.Trim().ToUpper());
                    if (cliente == null)
                    {
                        // Flujo alternativo: Cliente no existe
                        vm.Cliente = null;
                        vm.ClienteNoExiste = true;
                        vm.Error = "Cliente no existe";
                    }
                    else
                    {
                        vm.Cliente = cliente;
                    }
                }
            }
            catch (SqlException ex) { vm.Error = "Error de base de datos: " + ex.Message; }
            return Mostrar(vm);
        }

        /// <summary>Selección de un cliente desde la ventana de búsqueda.</summary>
        public IActionResult SeleccionarCliente(string cod)
        {
            var vm = ObtenerVM();
            try
            {
                vm.Cliente = _clienteDao.Buscar(cod);
                if (vm.Cliente == null) vm.Error = "Cliente no existe";
            }
            catch (SqlException ex) { vm.Error = "Error de base de datos: " + ex.Message; }
            return Mostrar(vm);
        }

        /// <summary>Punto de extensión: ir a "Agregar Cliente" sin perder lo escrito.</summary>
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult IrAgregarCliente(ReservaForm f)
        {
            GuardarVM(Combinar(f));
            return RedirectToAction("Create", "Cliente");
        }

        /// <summary>Pasos 5 al 7: ingresa fechas y el sistema calcula la cantidad de días.</summary>
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult CalcularDias(ReservaForm f)
        {
            var vm = Combinar(f);
            vm.Error = CalcularDiasInterno(vm);
            return Mostrar(vm);
        }

        /// <summary>Pasos 8 al 10: Buscar Habitación disponible (incluye CU Buscar Habitación).</summary>
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult BuscarHabitacion(ReservaForm f)
        {
            var vm = Combinar(f);
            CalcularDiasInterno(vm);
            try
            {
                if (!string.IsNullOrWhiteSpace(f.NumHabitacion))
                {
                    var num = f.NumHabitacion.Trim();
                    var hab = _habitacionDao.Buscar(num);
                    if (hab == null)
                        vm.Error = $"La habitación {num} no existe.";
                    else if (hab.Estado != "Disponible")
                        vm.Error = $"La habitación {num} se encuentra en estado {hab.Estado}.";
                    else if (vm.Detalles.Any(d => d.NumHabitacion == num))
                        vm.Error = $"La habitación {num} ya está en el detalle de la reserva.";
                    else
                        vm.HabitacionSeleccionada = hab;
                }
                else
                {
                    var disponibles = _habitacionDao.ListarDisponibles()
                        .Where(h => vm.Detalles.All(d => d.NumHabitacion != h.NumHabitacion))
                        .ToList();
                    if (disponibles.Count == 0)
                    {
                        // Flujo alternativo: Habitaciones no disponibles
                        vm.Error = "No hay habitaciones disponibles";
                    }
                    else
                    {
                        vm.ListaHabitaciones = disponibles;
                        vm.MostrarHabitaciones = true;
                    }
                }
            }
            catch (SqlException ex) { vm.Error = "Error de base de datos: " + ex.Message; }
            return Mostrar(vm);
        }

        /// <summary>Selección de una habitación desde la ventana de búsqueda.</summary>
        public IActionResult SeleccionarHabitacion(string num)
        {
            var vm = ObtenerVM();
            try
            {
                var hab = _habitacionDao.Buscar(num);
                if (hab == null || hab.Estado != "Disponible")
                    vm.Error = "No hay habitaciones disponibles";
                else
                    vm.HabitacionSeleccionada = hab;
            }
            catch (SqlException ex) { vm.Error = "Error de base de datos: " + ex.Message; }
            return Mostrar(vm);
        }

        /// <summary>Pasos 11 al 13: agrega la habitación a la cuadrícula y calcula pago, subtotal y total.</summary>
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult AgregarHabitacion(ReservaForm f)
        {
            var vm = Combinar(f);
            var errorDias = CalcularDiasInterno(vm);

            if (errorDias != null) vm.Error = errorDias;
            else if (vm.HabitacionSeleccionada == null) vm.Error = "Primero busque y seleccione una habitación.";
            else if (string.IsNullOrWhiteSpace(vm.NombreHuesped)) vm.Error = "Ingrese el nombre del huésped de la habitación.";
            else if (vm.Detalles.Any(d => d.NumHabitacion == vm.HabitacionSeleccionada.NumHabitacion))
                vm.Error = "Esa habitación ya fue agregada.";
            else
            {
                var detalle = new DetalleReserva
                {
                    NumHabitacion = vm.HabitacionSeleccionada.NumHabitacion,
                    Tipo = vm.HabitacionSeleccionada.Tipo,
                    CostoDia = vm.HabitacionSeleccionada.CostoDia,
                    NombreHuesped = vm.NombreHuesped,
                    Dias = vm.CantidadDias
                };
                detalle.CalcularPago();
                vm.Detalles.Add(detalle);
                vm.CalcularTotales();

                vm.HabitacionSeleccionada = null;
                vm.NombreHuesped = null;
                vm.Mensaje = $"Habitación {detalle.NumHabitacion} agregada al detalle.";
            }
            return Mostrar(vm);
        }

        /// <summary>Flujo alternativo: Eliminar habitación de la cuadrícula.</summary>
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult EliminarHabitacion(ReservaForm f)
        {
            var vm = Combinar(f);
            if (string.IsNullOrEmpty(f.HabitacionEliminar))
                vm.Error = "Seleccione una habitación de la cuadrícula para eliminar.";
            else
            {
                vm.Detalles.RemoveAll(d => d.NumHabitacion == f.HabitacionEliminar);
                vm.CalcularTotales();
                vm.Mensaje = $"Habitación {f.HabitacionEliminar} eliminada del detalle.";
            }
            return Mostrar(vm);
        }

        /// <summary>Pasos 15 al 18: Grabar Reserva.</summary>
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Grabar(ReservaForm f)
        {
            var vm = Combinar(f);
            var errorDias = CalcularDiasInterno(vm);

            if (vm.Cliente == null) vm.Error = "Busque y seleccione un cliente.";
            else if (errorDias != null) vm.Error = errorDias;
            else if (vm.Detalles.Count == 0) vm.Error = "Agregue al menos una habitación a la reserva.";
            if (vm.Error != null) return Mostrar(vm);

            var reserva = new Reserva
            {
                CodCliente = vm.Cliente!.CodCliente,
                FechaLlegada = vm.FechaLlegada!.Value,
                FechaSalida = vm.FechaSalida!.Value,
                CantidadDias = vm.CantidadDias,
                SubTotal = vm.SubTotal,
                IGV = vm.IGV,
                MontoTotal = vm.MontoTotal,
                Detalles = vm.Detalles
            };

            try
            {
                int numero = _reservaDao.Grabar(reserva);
                HttpContext.Session.Remove(CLAVE_SESION);
                var nueva = new ReservaViewModel
                {
                    NumReservaGenerada = numero,
                    Mensaje = $"Reserva generada con el Nro. {numero}"
                };
                ModelState.Clear();
                return View("Index", nueva);
            }
            catch (InvalidOperationException ex) { vm.Error = ex.Message; }
            catch (SqlException ex) { vm.Error = "Error al grabar la reserva: " + ex.Message; }
            return Mostrar(vm);
        }

        /// <summary>Paso 19: cerrar la interfaz RESERVA y volver al menú principal.</summary>
        public IActionResult Salir()
        {
            HttpContext.Session.Remove(CLAVE_SESION);
            return RedirectToAction("Index", "Home");
        }
    }
}
