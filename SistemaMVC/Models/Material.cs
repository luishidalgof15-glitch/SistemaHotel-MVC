namespace SistemaMVC.Models
{
    public class Material
    {
        public string CodMaterial { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Marca { get; set; } = "";
        public string UnidadMedida { get; set; } = "";
        public int Stock { get; set; }
    }
}
