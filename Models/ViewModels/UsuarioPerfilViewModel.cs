using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria10.Models.ViewModels
{
    public class UsuarioPerfilViewModel
    {
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "Debe ingresar el apellido y nombres de usuario")]
        [StringLength(100)]
        public string ApellidoNombres { get; set; } = "";

        [Required(ErrorMessage = "Debe ingresar un alias de identificación del usuario")]
        [StringLength(50)]
        public string Alias { get; set; } = "";

        [Required(ErrorMessage = "Debe ingresar un correo electrónico")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
        [StringLength(100)]
        public string Email { get; set; } = "";
    }
}
