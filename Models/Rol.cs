using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inmobiliaria10.Models
{
    public class Rol
    {
        [Key]
        public int IdRol { get; set; }  

        [Required, StringLength(50)]
        public string DenominacionRol { get; set; } = ""; 

        [ForeignKey(nameof(Usuario.IdRol))]
        public ICollection<Usuario>? Usuarios { get; set; }
    }
}
