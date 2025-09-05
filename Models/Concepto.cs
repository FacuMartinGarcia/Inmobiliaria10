
namespace Inmobiliaria10.Models
{
    public class Conceptos
    {
        public int IdConcepto { get; set; }                 // PK (id_concepto)
        public string DenominacionConcepto { get; set; } = ""; // UNIQUE, NOT NULL (denominacion_concepto)
    }
}
