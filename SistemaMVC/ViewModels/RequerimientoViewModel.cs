using System.Text.Json.Serialization;
using SistemaMVC.Models;

namespace SistemaMVC.ViewModels
{
    /// <summary>Estado de la interfaz GENERAR REQUERIMIENTO (se guarda en sesión).</summary>
    public class RequerimientoViewModel
    {
        public string CodOperario { get; set; } = "";
        public List<DetalleRequerimiento> Detalles { get; set; } = new();

        [JsonIgnore] public int? NumRequerimientoGenerado { get; set; }
        [JsonIgnore] public string? Mensaje { get; set; }
        [JsonIgnore] public string? Error { get; set; }
        [JsonIgnore] public bool MostrarMateriales { get; set; }
        [JsonIgnore] public List<Material> ListaMateriales { get; set; } = new();
    }

    /// <summary>Una fila de la ventana "Consultar Materiales".</summary>
    public class SeleccionMaterial
    {
        public string? CodMaterial { get; set; }
        public bool Seleccionado { get; set; }
        public int Cantidad { get; set; }
    }
}
