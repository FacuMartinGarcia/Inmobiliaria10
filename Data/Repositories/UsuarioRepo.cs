using System.Data;
using MySql.Data.MySqlClient;
using System.Data.Common;
using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories
{
    public class UsuarioRepo : IUsuarioRepo
    {
        private readonly Database _db;

        public UsuarioRepo(Database db)
        {
            _db = db;
        }

        public async Task<int> Agregar(Usuario u)
        {
            using var conn = _db.GetConnection();
            var sql = @"INSERT INTO usuarios
                        (apellido_nombres, alias, password, email, id_rol, created_at, updated_at)
                        VALUES (@apellidoNombres, @alias, @password, @email, @idRol, @createdAt, @updatedAt);
                        SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@apellidoNombres", u.ApellidoNombres);
            cmd.Parameters.AddWithValue("@alias", u.Alias);
            cmd.Parameters.AddWithValue("@password", u.Password);
            cmd.Parameters.AddWithValue("@email", u.Email);
            cmd.Parameters.AddWithValue("@idRol", u.IdRol);
            cmd.Parameters.AddWithValue("@createdAt", DateTime.Now);
            cmd.Parameters.AddWithValue("@updatedAt", DateTime.Now);

            await conn.OpenAsync();
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<int> Actualizar(Usuario u)
        {
            using var conn = _db.GetConnection();
            var sql = @"UPDATE usuarios SET
                            apellido_nombres = @apellidoNombres,
                            alias = @alias,
                            password = @password,
                            email = @email,
                            id_rol = @idRol,
                            updated_at = @updatedAt
                        WHERE id_usuario = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@apellidoNombres", u.ApellidoNombres);
            cmd.Parameters.AddWithValue("@alias", u.Alias);
            cmd.Parameters.AddWithValue("@password", u.Password);
            cmd.Parameters.AddWithValue("@email", u.Email);
            cmd.Parameters.AddWithValue("@idRol", u.IdRol);
            cmd.Parameters.AddWithValue("@updatedAt", DateTime.Now);
            cmd.Parameters.AddWithValue("@id", u.IdUsuario);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<Usuario>> ListarTodos()
        {
            var lista = new List<Usuario>();
            using var conn = _db.GetConnection();

            var sql = @"
                SELECT u.*, r.id_rol, r.denominacion_rol
                FROM usuarios u
                LEFT JOIN roles r ON u.id_rol = r.id_rol";

            using var cmd = new MySqlCommand(sql, conn);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(Map(reader));
            }
            return lista;
        }

        public async Task<(List<Usuario> registros, int totalRegistros)> ListarTodosPaginado(int pagina, int cantidadPorPagina, string? searchString = null)
        {
            var lista = new List<Usuario>();
            using var conn = _db.GetConnection();

            int offset = (pagina - 1) * cantidadPorPagina;
            string where = "";

            if (!string.IsNullOrEmpty(searchString))
            {
                where = @"WHERE u.apellido_nombres LIKE @search
                        OR u.alias LIKE @search
                        OR u.email LIKE @search";
            }

            var sql = $@"
                SELECT u.*, r.id_rol, r.denominacion_rol
                FROM usuarios u
                LEFT JOIN roles r ON u.id_rol = r.id_rol
                {where}
                ORDER BY u.apellido_nombres
                LIMIT @cantidad OFFSET @offset";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@cantidad", cantidadPorPagina);
            cmd.Parameters.AddWithValue("@offset", offset);

            if (!string.IsNullOrEmpty(searchString))
                cmd.Parameters.AddWithValue("@search", "%" + searchString + "%");

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(Map(reader));
            }
            await reader.CloseAsync();

            var sqlTotal = $@"
                SELECT COUNT(*)
                FROM usuarios u
                {where}";

            using var cmdTotal = new MySqlCommand(sqlTotal, conn);
            if (!string.IsNullOrEmpty(searchString))
                cmdTotal.Parameters.AddWithValue("@search", "%" + searchString + "%");

            int totalRegistros = Convert.ToInt32(await cmdTotal.ExecuteScalarAsync());

            return (lista, totalRegistros);
        }

        public async Task<Usuario?> ObtenerPorId(int id, CancellationToken cancellationToken = default)
        {
            Usuario? u = null;
            using var conn = _db.GetConnection();

            var sql = @"
                SELECT u.*, r.id_rol, r.denominacion_rol
                FROM usuarios u
                LEFT JOIN roles r ON u.id_rol = r.id_rol
                WHERE u.id_usuario = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync(cancellationToken);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                u = Map(reader);
            }
            return u;
        }

        public async Task<int> Eliminar(int id)
        {
            using var conn = _db.GetConnection();
            var sql = "DELETE FROM usuarios WHERE id_usuario = @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<Usuario?> ObtenerPorAlias(string alias)
        {
            Usuario? u = null;
            using var conn = _db.GetConnection();
            var sql = @"SELECT id_usuario, apellido_nombres, alias, password, email, id_rol, created_at, updated_at
                        FROM usuarios
                        WHERE alias = @alias";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alias", alias.ToUpper());

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                u = new Usuario
                {
                    IdUsuario = reader.GetInt32("id_usuario"),
                    ApellidoNombres = reader.GetString("apellido_nombres"),
                    Alias = reader.GetString("alias"),
                    Password = reader.GetString("password"),
                    Email = reader.GetString("email"),
                    IdRol = reader.GetInt32("id_rol"),
                    CreatedAt = reader.GetDateTime("created_at"),
                    UpdatedAt = reader.GetDateTime("updated_at")
                };
            }

            return u;
        }

        private Usuario Map(DbDataReader reader)
        {
            return new Usuario
            {
                IdUsuario = reader.GetInt32(reader.GetOrdinal("id_usuario")),
                ApellidoNombres = reader.GetString(reader.GetOrdinal("apellido_nombres")),
                Alias = reader.GetString(reader.GetOrdinal("alias")),
                Password = reader.GetString(reader.GetOrdinal("password")),
                Email = reader.GetString(reader.GetOrdinal("email")),
                IdRol = reader.GetInt32(reader.GetOrdinal("id_rol")),
                Rol = new Rol
                {
                    IdRol = reader.GetInt32(reader.GetOrdinal("id_rol")),
                    DenominacionRol = reader.GetString(reader.GetOrdinal("denominacion_rol"))
                },
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at"))
            };
        }
    }
}
