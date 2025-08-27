using System.Data;
using MySql.Data.MySqlClient;
using Inmobiliaria10.Models;

namespace Inmobiliaria10.Repositories
{
    public class InmuebleUsoRepo
    {
        private readonly string connectionString;

        public InmuebleUsoRepo(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public int Agregar(InmuebleUso u)
        {
            int idGenerado = 0;
            using var conn = new MySqlConnection(connectionString);
            var sql = @"INSERT INTO inmuebles_usos (denominacion_uso) VALUES (@denom);
                        SELECT LAST_INSERT_ID();";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@denom", u.DenominacionUso);
            conn.Open();
            idGenerado = Convert.ToInt32(cmd.ExecuteScalar());
            conn.Close();
            return idGenerado;
        }

        public List<InmuebleUso> MostrarTodosInmuebleUsos()
        {
            var inmuebleUsos = new List<InmuebleUso>();

            using var conn = new MySqlConnection(connectionString);
            var sql = "SELECT * FROM inmuebles_usos";
            using var cmd = new MySqlCommand(sql, conn);
            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var uso = new InmuebleUso
                {
                    IdUso = reader.GetInt32("id_uso"),
                    DenominacionUso = reader.GetString("denominacion_uso"),
                };
                inmuebleUsos.Add(uso);
            }

            conn.Close();
            return inmuebleUsos;
        }
    }
}
