using System.Data;
using MySql.Data.MySqlClient;
using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories
{
    public class InmuebleTipoRepo : IInmuebleTipoRepo
    {
        private readonly Database _db;

        public InmuebleTipoRepo(Database db)
        {
            _db = db;
        }

        public bool ExisteDenominacion(string denominacion, int? idExcluir = null)
        {
            using var conn = _db.GetConnection();
            var sql = "SELECT COUNT(*) FROM inmuebles_tipos WHERE denominacion_tipo = @denom";

            if (idExcluir.HasValue)
                sql += " AND id_tipo <> @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@denom", denominacion);

            if (idExcluir.HasValue)
                cmd.Parameters.AddWithValue("@id", idExcluir.Value);

            conn.Open();
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }

        public int Agregar(InmuebleTipo i)
        {
            if (ExisteDenominacion(i.DenominacionTipo))
                throw new Exception("Ya existe un tipo con esa denominación");

            using var conn = _db.GetConnection();
            var sql = @"INSERT INTO inmuebles_tipos (denominacion_tipo) VALUES (@denom);
                        SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@denom", i.DenominacionTipo);

            conn.Open();
            int idGenerado = Convert.ToInt32(cmd.ExecuteScalar());
            return idGenerado;
        }

        public void Editar(InmuebleTipo i)
        {
            if (ExisteDenominacion(i.DenominacionTipo, i.IdTipo))
                throw new Exception("Ya existe otro tipo con esa denominación");

            using var conn = _db.GetConnection();
            var sql = "UPDATE inmuebles_tipos SET denominacion_tipo = @denom WHERE id_tipo = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@denom", i.DenominacionTipo);
            cmd.Parameters.AddWithValue("@id", i.IdTipo);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void Eliminar(int id)
        {
            using var conn = _db.GetConnection();
            var sql = "DELETE FROM inmuebles_tipos WHERE id_tipo = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public InmuebleTipo? ObtenerPorId(int id)
        {
            using var conn = _db.GetConnection();
            var sql = "SELECT * FROM inmuebles_tipos WHERE id_tipo = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            conn.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new InmuebleTipo
                {
                    IdTipo = reader.GetInt32("id_tipo"),
                    DenominacionTipo = reader.GetString("denominacion_tipo"),
                };
            }
            return null;
        }

        public List<InmuebleTipo> MostrarTodosInmuebleTipos()
        {
            var inmuebleTipos = new List<InmuebleTipo>();

            using var conn = _db.GetConnection();
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

        public List<InmuebleTipo> BuscarInmuebleTipos(string term)
        {
            var inmuebleTipos = new List<InmuebleTipo>();

            using var conn = _db.GetConnection();
            var sql = @"SELECT id_tipo, denominacion_tipo 
                        FROM inmuebles_tipos
                        WHERE denominacion_tipo LIKE @term
                        LIMIT 20";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@term", "%" + term + "%");
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
