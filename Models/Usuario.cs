namespace Inmobiliaria10.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }               // PK
        public string ApellidoNombres { get; set; } = ""; // NOT NULL
        public string Alias { get; set; } = "";          // UNIQUE, NOT NULL
        public string Password { get; set; } = "";       // NOT NULL (hash)
        public string Email { get; set; } = "";          // UNIQUE, NOT NULL
        public int IdRol { get; set; }                   // FK roles
        public DateTime CreatedAt { get; set; }          // DEFAULT NOW
        public DateTime UpdatedAt { get; set; }          // ON UPDATE NOW
    }

    public class Rol
    {
        public int IdRol { get; set; }                   // PK
        public string DenominacionRol { get; set; } = ""; // UNIQUE
    }
}
