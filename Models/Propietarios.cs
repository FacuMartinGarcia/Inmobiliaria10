using System.ComponentModel.DataAnnotations;

namespace InmobiliariaMvc.Models;

public class Propietario
{
    public long Id { get; set; }

    [Required, StringLength(20)]
    public string Dni { get; set; } = null!;

    [Required, StringLength(100)]
    public string Apellido { get; set; } = null!;

    [Required, StringLength(100)]
    public string Nombre { get; set; } = null!;

    [EmailAddress, StringLength(120)]
    public string? Email { get; set; }

    [StringLength(30)]
    public string? Telefono { get; set; }

    [Display(Name="Dirección de contacto"), StringLength(200)]
    public string? DireccionContacto { get; set; }


}
