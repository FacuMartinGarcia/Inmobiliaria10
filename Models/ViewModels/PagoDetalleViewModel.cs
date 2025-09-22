using System;

namespace Inmobiliaria10.Models.ViewModels
{
    public class PagoDetalleViewModel
    {
        public int IdPago { get; set; }
        public DateTime FechaPago { get; set; }

        // 🔹 Nuevo: mes y año
        public int? IdMes { get; set; }          // FK meses (nullable)
        public string? MesTexto { get; set; }    // Nombre del mes (JOIN o calculado)
        public int Anio { get; set; }            // Año del pago

        public string? Detalle { get; set; }
        public decimal Importe { get; set; }

        // Concepto
        public int IdConcepto { get; set; }
        public string? ConceptoTexto { get; set; }

        // Contrato
        public int IdContrato { get; set; }
        public string? ContratoTexto { get; set; }
        public int NumeroPago { get; set; }

        // Auditoría
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }
        public string? MotivoAnulacion { get; set; }

        public string? CreatedByAlias { get; set; }
        public string? DeletedByAlias { get; set; }
    }
}
