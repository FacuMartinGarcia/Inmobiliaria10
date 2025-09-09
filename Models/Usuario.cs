using System.ComponentModel.DataAnnotations; 
using System.ComponentModel.DataAnnotations.Schema;

namespace Inmobiliaria10.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }  

        private string _apellidoNombres = "";
        [Required(ErrorMessage = "Debe ingresar el apellido y nombres de usuario"), StringLength(200)]
        public string ApellidoNombres
        {
            get => _apellidoNombres;
            set => _apellidoNombres = value?.ToUpper() ?? "";
        }

        private string _alias = "";
        [Required(ErrorMessage = "Debe ingresar un alias de identificación del usuario"), StringLength(100)]
        public string Alias
        {
            get => _alias;
            set => _alias = value?.ToUpper() ?? "";
        }

        [Required(ErrorMessage = "Debe ingresar una contraseña")]
        [StringLength(255)]
        public string Password { get; set; } = ""; 

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

        [ForeignKey(nameof(IdRol))]
        public Rol? Rol { get; set; }  

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
