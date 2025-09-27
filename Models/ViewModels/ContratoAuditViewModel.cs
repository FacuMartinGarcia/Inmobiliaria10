using System;
using System.Collections.Generic;

namespace Inmobiliaria10.Models.ViewModels
{
    public class ContratoAuditViewModel
    {
        public int IdAudit { get; set; }
        public int IdContrato { get; set; }
        public string Accion { get; set; } = string.Empty;
        public DateTime AccionAt { get; set; }
        public string Usuario { get; set; } = string.Empty;

        // 🔹 OldData y NewData parseados como diccionarios para mostrar en la vista
        public Dictionary<string, string>? OldData { get; set; }
        public Dictionary<string, string>? NewData { get; set; }
    }
}
