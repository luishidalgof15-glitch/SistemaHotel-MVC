namespace SistemaMVC.Models
{
    public class Reserva
    {
        public int NumReserva { get; set; }
        public string CodCliente { get; set; } = "";
        public DateTime FechaLlegada { get; set; }
        public DateTime FechaSalida { get; set; }
        public int CantidadDias { get; set; }
        public decimal SubTotal { get; set; }
        public decimal IGV { get; set; }
        public decimal MontoTotal { get; set; }
        public List<DetalleReserva> Detalles { get; set; } = new();
    }
}
