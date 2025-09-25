namespace Inmobiliaria10.Models
{
    public class RenovacionContratoViewModel
    {
        public int IdContratoPadre { get; set; }
        public int IdInmueble { get; set; }
        public int IdInquilino { get; set; }

        // Pre-cargados en GET
        public DateTime FechaInicio { get; set; }   // = contrato.FechaFin.AddDays(1)
        public DateTime FechaFin { get; set; }      // = FechaInicio.AddYears(PlazoAnios)

        // Editable por usuario
        public decimal MontoMensual { get; set; }

        // Seleccionado por usuario (1|2|3 años)
        public int PlazoAnios { get; set; }
    }
}
