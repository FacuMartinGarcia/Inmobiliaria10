using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inmobiliaria10.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }  

        private string _apellidoNombres = "";
        [Required, StringLength(200)]
        public string ApellidoNombres
        {
            get => _apellidoNombres;
            set => _apellidoNombres = value?.ToUpper() ?? "";
        }

        private string _alias = "";
        [Required, StringLength(100)]
        public string Alias
        {
            get => _alias;
            set => _alias = value?.ToUpper() ?? "";
        }

        [Required, StringLength(255)]
        public string Password { get; set; } = ""; 

        private string _email = "";
        [Required, EmailAddress, StringLength(150)]
        public string Email
        {
            get => _email;
            set => _email = value ?? "";
        }

        [Required]
        public int IdRol { get; set; } 

        [ForeignKey(nameof(IdRol))]
        public Rol? Rol { get; set; }  

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
