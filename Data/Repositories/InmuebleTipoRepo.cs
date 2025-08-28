using System.Data;
using MySql.Data.MySqlClient;
using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories
{
    public class InmuebleTipoRepo : RepositorioBase, IInmuebleTipoRepo
    {
        public InmuebleTipoRepo(IConfiguration cfg) : base(cfg) { }

        public int Agregar(InmuebleTipo i)
        {
            using var conn = Conn();
            var sql = @"INSERT INTO inmuebles_tipos (denominacion_tipo) VALUES (@denom);
                        SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@denom", i.DenominacionTipo);

            conn.Open();
            int idGenerado = Convert.ToInt32(cmd.ExecuteScalar());
            return idGenerado;
        }

        public List<InmuebleTipo> MostrarTodosInmuebleTipos()
        {
            var inmuebleTipos = new List<InmuebleTipo>();

            using var conn = Conn();
            var sql = "SELECT * FROM inmuebles_tipos";

            using var cmd = new MySqlCommand(sql, conn);
            conn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var inmuebleTipo = new InmuebleTipo
                {
                    IdTipo = reader.GetInt32("id_tipo"),
                    DenominacionTipo = reader.GetString("denominacion_tipo"),
                };
                inmuebleTipos.Add(inmuebleTipo);
            }

            return inmuebleTipos;
        }
    }
}
