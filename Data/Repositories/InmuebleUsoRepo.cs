using System.Data;
using MySql.Data.MySqlClient;
using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories
{
    public class InmuebleUsoRepo : IInmuebleUsoRepo
    {
        private readonly Database _db;

        public InmuebleUsoRepo(Database db)
        {
            _db = db;
        }

        private MySqlConnection Conn()
        {
            return _db.GetConnection();
        }

        public bool ExisteDenominacion(string denominacion, int? idExcluir = null)
        {
            using var conn = Conn();
            var sql = "SELECT COUNT(*) FROM inmuebles_usos WHERE denominacion_uso = @denom";

            if (idExcluir.HasValue)
                sql += " AND id_uso <> @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@denom", denominacion);

            if (idExcluir.HasValue)
                cmd.Parameters.AddWithValue("@id", idExcluir.Value);

            conn.Open();
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }

        public int Agregar(InmuebleUso u)
        {
            if (ExisteDenominacion(u.DenominacionUso))
                throw new Exception("Ya existe un uso con esa denominación");

            using var conn = Conn();
            var sql = @"INSERT INTO inmuebles_usos (denominacion_uso) VALUES (@denom);
                        SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@denom", u.DenominacionUso);

            conn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void Editar(InmuebleUso u)
        {
            if (ExisteDenominacion(u.DenominacionUso, u.IdUso))
                throw new Exception("Ya existe otro uso con esa denominación");

            using var conn = Conn();
            var sql = "UPDATE inmuebles_usos SET denominacion_uso = @denom WHERE id_uso = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@denom", u.DenominacionUso);
            cmd.Parameters.AddWithValue("@id", u.IdUso);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void Eliminar(int id)
        {
            using var conn = Conn();
            var sql = "DELETE FROM inmuebles_usos WHERE id_uso = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public InmuebleUso? ObtenerPorId(int id)
        {
            using var conn = Conn();
            var sql = "SELECT * FROM inmuebles_usos WHERE id_uso = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            conn.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new InmuebleUso
                {
                    IdUso = reader.GetInt32("id_uso"),
                    DenominacionUso = reader.GetString("denominacion_uso"),
                };
            }
            return null;
        }

        public List<InmuebleUso> MostrarTodosInmuebleUsos()
        {
            var inmuebleUsos = new List<InmuebleUso>();

            using var conn = Conn();
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

            return inmuebleUsos;
        }
    }
}
