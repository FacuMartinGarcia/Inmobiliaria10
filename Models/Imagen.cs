using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Inmobiliaria10.Models
{
    public class Imagen
    {
        [Key]
        public int IdImagen { get; set; }

        public int? IdInmueble { get; set; }
        public int? IdUsuario { get; set; }

        [ForeignKey(nameof(IdInmueble))]
        public Inmueble? Inmueble { get; set; }

        [Required(ErrorMessage = "Debe indicar la URL de la imagen")]
        [StringLength(500)]
        public string Url { get; set; } = string.Empty;

        [NotMapped]
        public IFormFile? Archivo { get; set; }
    }
}
