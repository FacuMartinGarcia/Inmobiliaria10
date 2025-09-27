namespace Inmobiliaria10.Models.ViewModels
{
    public class MorosoViewModel
    {
        public int IdInquilino { get; set; }
        public string Inquilino { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string Inmueble { get; set; } = string.Empty;

        public int IdContrato { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public int IdMes { get; set; }
        public string MesAdeudado { get; set; } = string.Empty;
        public int AnioAdeudado { get; set; }

        public decimal MontoMensual { get; set; }
    }
}
