using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inmobiliaria10.Models
{
    public class Contrato : IValidatableObject
    {
        public int IdContrato { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Firma")]
        public DateTime? FechaFirma { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un inmueble válido.")]
        public int IdInmueble { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un inquilino válido.")]
        public int IdInquilino { get; set; }

        [Required, DataType(DataType.Date)]
        [Display(Name = "Fecha Inicio")]
        public DateTime FechaInicio { get; set; }

        [Required, DataType(DataType.Date)]
        [Display(Name = "Fecha Fin")]
        public DateTime FechaFin { get; set; }
        
        [Required(ErrorMessage = "El monto mensual es obligatorio")]
        [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser mayor o igual a 0")]
        [Column(TypeName = "decimal(15,2)")]
        [Display(Name = "Monto Mensual")]

        public decimal MontoMensual { get; set; }

        [DataType(DataType.Date)]
        public DateTime? Rescision { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        [Range(typeof(decimal), "0", "79228162514264337593543950335",
                    ErrorMessage = "La multa no puede ser negativa.")]
        [Display(Name = "Multa Aplicada")]
        public decimal? MontoMulta { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "DeletedBy debe ser válido si DeletedAt tiene valor.")]
        public int? DeletedBy { get; set; }
        
        [ForeignKey(nameof(IdInquilino))]
        public Inquilino? Inquilino { get; set; }

        [ForeignKey(nameof(IdInmueble))]
        public Inmueble? Inmueble { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // 1) Fin después de inicio
            if (FechaFin <= FechaInicio)
                yield return new ValidationResult("La fecha de fin debe ser posterior a la fecha de inicio.", new[] { nameof(FechaFin) });

            // 2) Firma (si existe) no después del inicio
            if (FechaFirma.HasValue && FechaFirma.Value.Date > FechaInicio.Date)
                yield return new ValidationResult("La fecha de firma no puede ser posterior al inicio del contrato.", new[] { nameof(FechaFirma) });

            // 3) Rescisión (si existe) dentro del intervalo
            if (Rescision.HasValue && (Rescision.Value.Date < FechaInicio.Date || Rescision.Value.Date > FechaFin.Date))
                yield return new ValidationResult("La rescisión debe estar dentro del período del contrato.", new[] { nameof(Rescision) });

            // 4) Si hay DeletedAt, debe haber DeletedBy
            if (DeletedAt.HasValue && (!DeletedBy.HasValue || DeletedBy.Value <= 0))
                yield return new ValidationResult("Si el contrato está eliminado, 'DeletedBy' es obligatorio.", new[] { nameof(DeletedBy) });

            // 5) En casi que quiera rescindir el contrato, que  controle que multa no puede ser 0
            if (Rescision.HasValue)
            {
                if (!MontoMulta.HasValue || MontoMulta.Value <= 0)
                {
                    yield return new ValidationResult(
                        "Si existe rescisión, la multa es obligatoria y debe ser mayor a 0.",
                        new[] { nameof(MontoMulta) });
                }
            }     
            // 6) El caso inverso, si no hay fecha de rescicion, no deberia haber multa
            if (!Rescision.HasValue && MontoMulta.HasValue && MontoMulta.Value > 0)
            {
                yield return new ValidationResult(
                    "Si no hay rescisión, no se puede asignar una multa.",
                    new[] { nameof(MontoMulta) });
            }
        }
    }
}
