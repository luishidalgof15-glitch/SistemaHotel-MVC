using System.ComponentModel.DataAnnotations;

namespace SistemaMVC.Models
{
    public class Cliente
    {
        [Display(Name = "Código")]
        public string CodCliente { get; set; } = "";

        [Required(ErrorMessage = "Ingrese los nombres")]
        [StringLength(60)]
        public string Nombres { get; set; } = "";

        [Required(ErrorMessage = "Ingrese los apellidos")]
        [StringLength(60)]
        public string Apellidos { get; set; } = "";

        [RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI debe tener 8 dígitos")]
        public string? DNI { get; set; }

        [StringLength(15)]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        public string NombreCompleto => $"{Nombres} {Apellidos}";
    }
}
