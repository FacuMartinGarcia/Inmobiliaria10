using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria10.Models.ViewModels
{
    public class RecuperarPasswordViewModel
    {
        [Required(ErrorMessage = "Debe ingresar un correo electrónico")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
        public string Email { get; set; } = string.Empty;
    }
}
