namespace SistemaMVC.Models
{
    public class Habitacion
    {
        public string NumHabitacion { get; set; } = "";
        public string Tipo { get; set; } = "";
        public decimal CostoDia { get; set; }
        public string Estado { get; set; } = "Disponible";   // Disponible | Reservado | Ocupado
    }
}
