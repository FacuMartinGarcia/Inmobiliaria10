namespace Inmobiliaria10.Models
{
    public class Contrato
    {
        public int IdContrato { get; set; }          // PK
        public DateTime? FechaFirma { get; set; }    // NULL
        public int IdInmueble { get; set; }          // FK inmuebles
        public int IdInquilino { get; set; }         // FK inquilinos
        public DateTime FechaInicio { get; set; }    // NOT NULL (DATE)
        public DateTime FechaFin { get; set; }       // NOT NULL (DATE)
        public DateTime? Rescision { get; set; }     // NULL (DATE)
        public decimal? MontoMulta { get; set; }     // NULL DEC(15,2)
        public int CreatedBy { get; set; }           // FK usuarios
        public DateTime CreatedAt { get; set; }      // DEFAULT NOW
        public DateTime? DeletedAt { get; set; }     // NULL
        public int? DeletedBy { get; set; }          // NULL FK usuarios

        // Navigación opcional
        // public Inmueble? Inmueble { get; set; }
        // public Inquilino? Inquilino { get; set; }
        // public Usuario? Creador { get; set; }
        // public Usuario? Eliminador { get; set; }
    }

    public class ContratoAudit
    {
        public long IdAudit { get; set; }            // PK (bigint)
        public int IdContrato { get; set; }          // FK contratos (ON DELETE CASCADE)
        public string Accion { get; set; } = "";     // varchar(12) (ej: INSERT/UPDATE/DELETE)
        public DateTime AccionAt { get; set; }       // DEFAULT NOW
        public int? AccionBy { get; set; }           // NULL FK usuarios
        public string? OldData { get; set; }         // longtext (JSON con antes)
        public string? NewData { get; set; }         // longtext (JSON con después)
    }
}
