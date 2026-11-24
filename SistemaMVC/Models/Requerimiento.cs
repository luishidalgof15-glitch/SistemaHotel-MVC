namespace SistemaMVC.Models
{
    public class Requerimiento
    {
        public int NumRequerimiento { get; set; }
        public string CodOperario { get; set; } = "";
        public DateTime Fecha { get; set; } = DateTime.Now;
        public List<DetalleRequerimiento> Detalles { get; set; } = new();
    }
}
