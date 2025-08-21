namespace Inmobiliaria10.Models
{
    public class Pago
    {
        public int IdPago { get; set; }              // PK
        public int IdContrato { get; set; }          // FK contratos
        public DateTime FechaPago { get; set; }      // DATE NOT NULL
        public string? Detalle { get; set; }         // varchar(50) NULL
        public int IdConcepto { get; set; }          // FK conceptos
        public decimal Importe { get; set; }         // DECIMAL(15,2) NOT NULL
        public string? MotivoAnulacion { get; set; } // varchar(255) NULL
        public int CreatedBy { get; set; }           // FK usuarios
        public DateTime CreatedAt { get; set; }      // DEFAULT NOW
        public DateTime? DeletedAt { get; set; }     // NULL
        public int? DeletedBy { get; set; }          // NULL FK usuarios
    }

    public class PagoAudit
    {
        public long IdAudit { get; set; }            // PK (bigint)
        public int IdPago { get; set; }              // FK pagos
        public string Accion { get; set; } = "";     // varchar(12)
        public DateTime AccionAt { get; set; }       // DEFAULT NOW
        public int? AccionBy { get; set; }           // NULL FK usuarios
        public string? OldData { get; set; }         // longtext
        public string? NewData { get; set; }         // longtext
    }

    public class Concepto
    {
        public int IdConcepto { get; set; }                  // PK
        public string DenominacionConcepto { get; set; } = ""; // UNIQUE, NOT NULL
    }
}


