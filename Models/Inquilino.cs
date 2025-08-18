using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria10.Models;

public class Inquilino
{
    public long Id { get; set; }

    [Required, StringLength(20)]
    public string Dni { get; set; } = null!;

    [Required, Display(Name="Nombre completo"), StringLength(150)]
    public string NombreCompleto { get; set; } = null!;

    [EmailAddress, StringLength(120)]
    public string? Email { get; set; }

    [StringLength(30)]
    public string? Telefono { get; set; }
}
