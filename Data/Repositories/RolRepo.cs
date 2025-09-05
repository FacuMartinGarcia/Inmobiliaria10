using System.Data;
using MySql.Data.MySqlClient;
using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories
{
    public class RolRepo : IRolRepo
    {
        private readonly Database _db;

        public RolRepo(Database db)
        {
            _db = db;
        }

        public async Task<int> Agregar(Rol rol)
        {
            using var conn = _db.GetConnection();
            var sql = @"INSERT INTO roles (denominacion_rol) 
                        VALUES (@denom);
                        SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@denom", rol.DenominacionRol);

            await conn.OpenAsync();
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<int> Actualizar(Rol rol)
        {
            using var conn = _db.GetConnection();
            var sql = @"UPDATE roles SET denominacion_rol = @denom
                        WHERE id_rol = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@denom", rol.DenominacionRol);
            cmd.Parameters.AddWithValue("@id", rol.IdRol);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> Eliminar(int id)
        {
            using var conn = _db.GetConnection();
            var sql = "DELETE FROM roles WHERE id_rol = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<Rol?> ObtenerPorId(int id)
        {
            Rol? rol = null;
            using var conn = _db.GetConnection();
            var sql = "SELECT * FROM roles WHERE id_rol = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                rol = new Rol
                {
                    IdRol = reader.GetInt32("id_rol"),
                    DenominacionRol = reader.GetString("denominacion_rol")
                };
            }
            return rol;
        }

        public async Task<List<Rol>> ListarTodos()
        {
            var lista = new List<Rol>();
            using var conn = _db.GetConnection();
            var sql = "SELECT * FROM roles ORDER BY denominacion_rol";

            using var cmd = new MySqlCommand(sql, conn);
            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new Rol
                {
                    IdRol = reader.GetInt32("id_rol"),
                    DenominacionRol = reader.GetString("denominacion_rol")
                });
            }
            return lista;
        }

        public async Task<bool> ExisteDenominacion(string denominacion, int? idExcluir = null)
        {
            using var conn = _db.GetConnection();
            string sql = "SELECT COUNT(*) FROM roles WHERE denominacion_rol = @denom";

            if (idExcluir.HasValue)
                sql += " AND id_rol <> @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@denom", denominacion);
            if (idExcluir.HasValue)
                cmd.Parameters.AddWithValue("@id", idExcluir.Value);

            await conn.OpenAsync();
            int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return count > 0;
        }
    }
}
