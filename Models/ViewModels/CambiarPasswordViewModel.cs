using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria10.Models.ViewModels{
    public class CambiarPasswordViewModel
    {
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "Debe ingresar su contraseña actual")]
        public string PasswordActual { get; set; } = "";

        [Required(ErrorMessage = "Debe ingresar una nueva contraseña")]
        [MinLength(6, ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres")]
        public string NuevaPassword { get; set; } = "";

        [Required(ErrorMessage = "Debe confirmar la contraseña")]
        [Compare("NuevaPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarPassword { get; set; } = "";
    }
}
