using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria10.Models
{
    public class Pago
    {
        public int IdPago { get; set; }              // PK
        public int IdContrato { get; set; }          // FK contratos

        [Required]
        [DataType(DataType.Date)]
        public DateTime FechaPago { get; set; }      // DATE NOT NULL

        // 🔹 Nuevo: FK a tabla Meses
        public int? IdMes { get; set; }              // FK meses (nullable para compatibilidad)

        // 🔹 Nuevo: Año del pago
        [Range(1900, 2100, ErrorMessage = "El año debe estar entre 1900 y 2100.")]
        public int Anio { get; set; }                // INT NOT NULL

        [StringLength(50)]
        public string? Detalle { get; set; }         // varchar(50) NULL

        public int IdConcepto { get; set; }          // FK conceptos

        [Range(0, double.MaxValue, ErrorMessage = "El importe no puede ser negativo.")]
        public decimal Importe { get; set; }         // DECIMAL(15,2) NOT NULL

        [StringLength(255)]
        public string? MotivoAnulacion { get; set; } // varchar(255) NULL

        public int? CreatedBy { get; set; }          // FK usuarios
        public DateTime? CreatedAt { get; set; }     // DEFAULT NOW
        public DateTime? DeletedAt { get; set; }     // NULL
        public int? DeletedBy { get; set; }          // NULL FK usuarios

        public int NumeroPago { get; set; }          // Secuencia dentro del contrato
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
        public string? UsuarioAlias { get; set; }
        public string AccionTraducida =>
        Accion switch
        {
            "INSERT" => "Creación",
            "UPDATE" => "Modificación",
            "DELETE" => "Eliminación",
            _ => Accion
        };

        
    }
}
