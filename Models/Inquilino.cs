using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria10.Models;

/*
CREATE TABLE inquilinos (
  id_inquilino INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  documento VARCHAR(20) NOT NULL,
  apellido_nombres VARCHAR(100) NOT NULL,
  domicilio VARCHAR(255) DEFAULT NULL,
  telefono VARCHAR(50) DEFAULT NULL,
  email VARCHAR(100) DEFAULT NULL,
  PRIMARY KEY (id_inquilino),
  UNIQUE KEY uq_inq_documento (documento)
);

*/
public class Inquilino
{
    private string _apellidoNombres = string.Empty;
    private string _domicilio = string.Empty;

    [Key]
    [Display(Name = "Cód Inquilino")]
    public int IdInquilino { get; set; }

    [Required(ErrorMessage = "El documento es obligatorio")]
    [StringLength(9, ErrorMessage = "El documento no debe exceder los 9 dígitos")]
    [RegularExpression("^[1-9][0-9]*$", ErrorMessage = "El documento debe contener solo números y no puede ser 0")]
    [Display(Name = "DNI")]
    public string Documento { get; set; } = null!;

    [Required, StringLength(100)]
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

    [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
    [StringLength(100)]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [StringLength(50)]
    [RegularExpression("^[0-9]*$", ErrorMessage = "El teléfono solo puede contener números")]
    public string? Telefono { get; set; }
    
}