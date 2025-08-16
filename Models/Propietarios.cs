using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria10.Models;

public class Propietario
{
    [Key]
    public int IdPropietario { get; set; }

    [Required, StringLength(20)]
    public string Documento { get; set; } = null!;

    [Required, StringLength(100)]
    [Display(Name = "Apellido y Nombres")]
    public string ApellidoNombres { get; set; } = null!;

    [StringLength(255)]
    public string? Domicilio { get; set; }

    [StringLength(50)]
    public string? Telefono { get; set; }

    [EmailAddress, StringLength(100)]
    public string? Email { get; set; }
}
