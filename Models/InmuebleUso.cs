using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inmobiliaria10.Models;

public class InmuebleUso
{
    private string _denominacionUso = string.Empty;

    [Key]
    [Display(Name = "Cód Uso")]
    public int IdUso { get; set; }   

    [Required(ErrorMessage = "Debe ingresar una denominación")]
    [StringLength(50)]
    [Display(Name = "Denominación")]
    public string DenominacionUso
    {
        get => _denominacionUso;
        set => _denominacionUso = (value ?? string.Empty).ToUpper();
    }

    public ICollection<Inmueble>? Inmuebles { get; set; }
    
}