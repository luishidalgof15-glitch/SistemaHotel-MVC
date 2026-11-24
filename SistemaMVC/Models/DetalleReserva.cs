namespace SistemaMVC.Models
{
    public class DetalleReserva
    {
        public string NumHabitacion { get; set; } = "";
        public string Tipo { get; set; } = "";
        public string NombreHuesped { get; set; } = "";
        public decimal CostoDia { get; set; }
        public int Dias { get; set; }
        public decimal MontoPagar { get; set; }

        /// <summary>Paso 13 del CU: pago de la habitación = costo por día x días.</summary>
        public void CalcularPago()
        {
            MontoPagar = CostoDia * Dias;
        }
    }
}
