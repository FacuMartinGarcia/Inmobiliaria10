namespace Inmobiliaria10.Models
{
    public class Inmueble
    {
        public int IdInmueble { get; set; }             // PK
        public int IdPropietario { get; set; }          // FK propietarios
        public int IdUso { get; set; }                  // FK inmuebles_usos
        public int IdTipo { get; set; }                 // FK inmuebles_tipos
        public string Direccion { get; set; } = "";     // NOT NULL
        public string? Piso { get; set; }
        public string? Depto { get; set; }
        public decimal? Lat { get; set; }               // DECIMAL(9,6) -> decimal?
        public decimal? Lon { get; set; }
        public int? Ambientes { get; set; }             // UNSIGNED -> int
        public decimal? Precio { get; set; }            // DECIMAL(15,2)
        public bool Activo { get; set; } = true;        // tinyint(1)
        public DateTime CreatedAt { get; set; }         // default CURRENT_TIMESTAMP
        public DateTime UpdatedAt { get; set; }         // ON UPDATE CURRENT_TIMESTAMP

        // (Opcional) propiedades de navegación si las querés usar en vistas:
        // public Propietario? Propietario { get; set; }
        // public InmuebleTipo? Tipo { get; set; }
        // public InmuebleUso? Uso { get; set; }
    }

    public class InmuebleTipo
    {
        public int IdTipo { get; set; }                      // PK
        public string DenominacionTipo { get; set; } = "";   // UNIQUE
    }

    public class InmuebleUso
    {
        public int IdUso { get; set; }                       // PK
        public string DenominacionUso { get; set; } = "";    // UNIQUE
    }
}
