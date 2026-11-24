using System.Text.Json.Serialization;
using SistemaMVC.Models;

namespace SistemaMVC.ViewModels
{
    /// <summary>Estado de la interfaz RESERVA (se guarda en sesión entre cada acción).</summary>
    public class ReservaViewModel
    {
        public const decimal TASA_IGV = 0.18m;

        public Cliente? Cliente { get; set; }
        public DateTime? FechaLlegada { get; set; }
        public DateTime? FechaSalida { get; set; }
        public int CantidadDias { get; set; }
        public Habitacion? HabitacionSeleccionada { get; set; }
        public string? NombreHuesped { get; set; }
        public List<DetalleReserva> Detalles { get; set; } = new();
        public decimal SubTotal { get; set; }
        public decimal IGV { get; set; }
        public decimal MontoTotal { get; set; }

        // ---- Solo para la vista (no se guardan en sesión) ----
        [JsonIgnore] public int? NumReservaGenerada { get; set; }
        [JsonIgnore] public string? Mensaje { get; set; }
        [JsonIgnore] public string? Error { get; set; }
        [JsonIgnore] public bool ClienteNoExiste { get; set; }
        [JsonIgnore] public bool MostrarClientes { get; set; }
        [JsonIgnore] public bool MostrarHabitaciones { get; set; }
        [JsonIgnore] public List<Cliente> ListaClientes { get; set; } = new();
        [JsonIgnore] public List<Habitacion> ListaHabitaciones { get; set; } = new();

        /// <summary>Paso 13: recalcula el pago de cada habitación, el subtotal, el IGV y el monto total.</summary>
        public void CalcularTotales()
        {
            foreach (var d in Detalles)
            {
                d.Dias = CantidadDias;
                d.CalcularPago();
            }
            SubTotal = Detalles.Sum(d => d.MontoPagar);
            IGV = Math.Round(SubTotal * TASA_IGV, 2);
            MontoTotal = SubTotal + IGV;
        }
    }

    /// <summary>Datos que llegan del formulario RESERVA en cada botón.</summary>
    public class ReservaForm
    {
        public string? CodCliente { get; set; }
        public DateTime? FechaLlegada { get; set; }
        public DateTime? FechaSalida { get; set; }
        public string? NumHabitacion { get; set; }
        public string? NombreHuesped { get; set; }
        public string? HabitacionEliminar { get; set; }
    }
}
