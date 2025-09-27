namespace Inmobiliaria10.Models.ViewModels
{
    public class PagoAuditViewModel
    {
        public int IdAudit { get; set; }
        public int IdPago { get; set; }
        public string Accion { get; set; } = "";
        public DateTime AccionAt { get; set; }
        public string Usuario { get; set; } = "";

        public Dictionary<string, string>? OldData { get; set; }
        public Dictionary<string, string>? NewData { get; set; }
    }
}
