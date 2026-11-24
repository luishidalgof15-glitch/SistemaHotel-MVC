using System.ComponentModel.DataAnnotations;

namespace SistemaMVC.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Ingrese el usuario")]
        public string Usuario { get; set; } = "";

        [Required(ErrorMessage = "Ingrese la clave")]
        [DataType(DataType.Password)]
        public string Clave { get; set; } = "";
    }
}
