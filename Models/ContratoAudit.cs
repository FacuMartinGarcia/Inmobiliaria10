namespace Inmobiliaria10.Models
{
    public class ContratoAudit
    {
        public long IdAudit { get; set; }
        public int IdContrato { get; set; }
        public string Accion { get; set; } = ""; // CREATE | UPDATE | DELETE (o lo que uses)
        public DateTime AccionAt { get; set; }
        public int AccionBy { get; set; }
        public string? OldData { get; set; }
        public string? NewData { get; set; }
    }
}
