using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Inmobiliaria10.Models
{
    public class Inmueble
    {
        private string _direccion = string.Empty;
        private string? _piso;
        private string? _depto;

        [Key]
        [Display(Name = "Cód Inmueble")]
        public int IdInmueble { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un Propietario")]
        [Display(Name = "Propietario")]
        public int IdPropietario { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un uso para el inmueble")]
        [Display(Name = "Uso")]
        public int IdUso { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un tipo de inmueble")]
        [Display(Name = "Tipo")]
        public int IdTipo { get; set; }

        [Required(ErrorMessage = "Debe ingresar una dirección")]
        [StringLength(255)]
        [Display(Name = "Dirección")]
        public string Direccion
        {
            get => _direccion;
            set => _direccion = (value ?? string.Empty).ToUpper();
        }

        [StringLength(20)]
        [Display(Name = "Piso")]
        public string? Piso
        {
            get => _piso;
            set => _piso = value?.ToUpper();
        }

        [StringLength(20)]
        [Display(Name = "Depto")]
        public string? Depto
        {
            get => _depto;
            set => _depto = value?.ToUpper();
        }

        [RegularExpression(@"^[-+]?([1-8]?\d(\.\d+)?|90(\.0+)?)$",
            ErrorMessage = "Latitud inválida. Debe estar entre -90 y 90.")]
        [Display(Name = "Latitud")]
        public decimal? Lat { get; set; }

        [RegularExpression(@"^[-+]?((1[0-7]\d)|(\d{1,2}))(\.\d+)?|180(\.0+)?$",
            ErrorMessage = "Longitud inválida. Debe estar entre -180 y 180.")]
        [Display(Name = "Longitud")]
        public decimal? Lon { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El número de ambientes debe ser al menos 1")]
        [Display(Name = "Ambientes")]
        public int? Ambientes { get; set; } = 1;

        [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser mayor o igual a 0")]
        [Display(Name = "Precio")]
        public decimal? Precio { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        [Display(Name = "Creado")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Actualizado")]
        public DateTime UpdatedAt { get; set; }

        [ForeignKey(nameof(IdPropietario))]
        public Propietario? Propietario { get; set; }

        [ForeignKey(nameof(IdTipo))]
        public InmuebleTipo? Tipo { get; set; }

        [ForeignKey(nameof(IdUso))]
        public InmuebleUso? Uso { get; set; }
        public string? Portada { get; set; }

        [NotMapped]
        public IFormFile? PortadaFile { get; set; }

        public IList<Imagen> Imagenes { get; set; } = new List<Imagen>();
        public bool EstaAlquilado { get; set; }
        public bool TieneRescisionFutura { get; set; }
    }
}
