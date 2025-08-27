using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inmobiliaria10.Models;

public class InmuebleTipo
{
    private string _denominacionTipo = string.Empty;

    [Key]
    [Display(Name = "Cód Tipo")]
    public int IdTipo { get; set; }   

    [Required(ErrorMessage = "Debe ingresar una denominación")]
    [StringLength(50)]
    [Display(Name = "Denominación")]
    public string DenominacionTipo
    {
        get => _denominacionTipo;
        set => _denominacionTipo = (value ?? string.Empty).ToUpper();
    }

    public ICollection<Inmueble>? Inmuebles { get; set; }
    
}