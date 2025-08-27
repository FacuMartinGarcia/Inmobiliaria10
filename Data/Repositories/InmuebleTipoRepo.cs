using System.Data;
using MySql.Data.MySqlClient;
using Inmobiliaria10.Models;

namespace Inmobiliaria10.Repositories
{
    public class InmuebleTipoRepo 
    {
        private readonly string connectionString;

        public InmuebleTipoRepo(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public int Agregar(InmuebleTipo i)
        {
            int idGenerado = 0;
            using var conn = new MySqlConnection(connectionString);
            var sql = @"INSERT INTO inmuebles_tipos (denominacion_tipo) VALUES (@denom);
                        SELECT LAST_INSERT_ID();";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@denom", i.DenominacionTipo); 
            conn.Open();
            idGenerado = Convert.ToInt32(cmd.ExecuteScalar());
            conn.Close();
            return idGenerado;
        }

        public List<InmuebleTipo> MostrarTodosInmuebleTipos()
        {
            var inmuebleTipos = new List<InmuebleTipo>();

            using var conn = new MySqlConnection(connectionString);
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

            conn.Close();
            return inmuebleTipos;
        }
    }
}
