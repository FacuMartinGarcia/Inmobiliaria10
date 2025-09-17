using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria10.Models.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; } = "";

        [Required(ErrorMessage = "Debe ingresar una nueva contraseña")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string NuevaPassword { get; set; } = "";

        [Required(ErrorMessage = "Debe confirmar la contraseña")]
        [Compare("NuevaPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarPassword { get; set; } = "";
    }
}
