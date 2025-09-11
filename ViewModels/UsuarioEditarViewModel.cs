using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria10.Models
{
    public class UsuarioEditViewModel
    {
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "Debe ingresar el apellido y nombres de usuario")]
        [StringLength(200)]
        public string ApellidoNombres { get; set; } = "";

        [Required(ErrorMessage = "Debe ingresar un alias de identificación del usuario")]
        [StringLength(100)]
        public string Alias { get; set; } = "";

        [StringLength(255)]
        public string? Password { get; set; }   

        private string _email = "";
        [Required(ErrorMessage = "Debe ingresar un correo electrónico")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
        [StringLength(150)]
        public string Email
        {
            get => _email;
            set => _email = value?.ToLower() ?? "";
        }


        [Required(ErrorMessage = "Debe seleccionar un rol")]
        public int IdRol { get; set; }
    }
}
