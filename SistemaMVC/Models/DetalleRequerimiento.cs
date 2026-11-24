namespace SistemaMVC.Models
{
    public class DetalleRequerimiento
    {
        public string CodOperario { get; set; } = "";
        public string CodMaterial { get; set; } = "";
        public string Material { get; set; } = "";
        public string Marca { get; set; } = "";
        public int Cantidad { get; set; }
    }
}
