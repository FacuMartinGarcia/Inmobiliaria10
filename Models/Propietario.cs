using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria10.Models
{

    public class Propietario
    {
        private string _apellidoNombres = string.Empty;
        private string _domicilio = string.Empty;
        [Key]
        public int IdPropietario { get; set; }

        [Required(ErrorMessage = "El documento es obligatorio")]
        [StringLength(9, ErrorMessage = "El documento no debe exceder los 9 dígitos")]
        [RegularExpression("^[1-9][0-9]*$", ErrorMessage = "El documento debe contener solo números y no puede ser 0")]
        [Display(Name = "DNI")]
        public string Documento { get; set; } = null!;

        [Required(ErrorMessage = "El apellido y nombre es obligatorio"), StringLength(100)]
        [Display(Name = "Apellido y Nombres")]
        public string ApellidoNombres
        {
            get => _apellidoNombres;
            set => _apellidoNombres = (value ?? string.Empty).ToUpper();
        }

        [Required, StringLength(255)]
        [Display(Name = "Domicilio")]
        public string Domicilio
        {
            get => _domicilio;
            set => _domicilio = (value ?? string.Empty).ToUpper();
        }

        [StringLength(50)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "El teléfono solo puede contener números")]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        public override string ToString()
        {
            var res = $"{ApellidoNombres}";
            if (!String.IsNullOrEmpty(Documento))
            {
                res += $" ({Documento})";
            }
            return res;
        }

    }
    
    public class PropietarioIndexVm
    {
        public PagedResult<Propietario> Data { get; set; } = new PagedResult<Propietario>();
        public int[] PageSizeOptions { get; set; } = new[] { 5, 10, 20, 50 };
    }
}
