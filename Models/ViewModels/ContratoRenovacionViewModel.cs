namespace Inmobiliaria10.Models
{
    public class RenovacionContratoViewModel
    {
        public int IdContratoPadre { get; set; }
        public int IdInmueble { get; set; }
        public int IdInquilino { get; set; }

        public DateTime FechaInicio { get; set; }   //fecha fin + 1
        public DateTime FechaFin { get; set; }      // +AddYears(PlazoAnios)


        public decimal MontoMensual { get; set; }

        public int PlazoAnios { get; set; }
    }
}
