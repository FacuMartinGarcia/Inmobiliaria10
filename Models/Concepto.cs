
namespace Inmobiliaria10.Models
{
    public class Concepto
    {
        public int IdConcepto { get; set; }                 // PK (id_concepto)
        public string DenominacionConcepto { get; set; } = ""; // UNIQUE, NOT NULL (denominacion_concepto)
    }
}
