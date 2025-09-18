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
                        VALUES (@ApellidoNombres, @Alias, @Password, @Email, @IdRol, @CreatedAt, @UpdatedAt);
                        SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ApellidoNombres", u.ApellidoNombres);
            cmd.Parameters.AddWithValue("@Alias", u.Alias);
            cmd.Parameters.AddWithValue("@Password", u.Password);
            cmd.Parameters.AddWithValue("@Email", u.Email);
            cmd.Parameters.AddWithValue("@IdRol", u.IdRol);
            cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
            cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

            await conn.OpenAsync();
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }
        public async Task<int> Actualizar(Usuario u)
        {
            using var conn = _db.GetConnection();

            var sql = @"UPDATE usuarios SET
                            apellido_nombres = @ApellidoNombres,
                            alias = @Alias,
                            email = @Email,
                            id_rol = @IdRol,
                            updated_at = @UpdatedAt";

            if (!string.IsNullOrEmpty(u.Password))
                sql += ", password = @Password";

            sql += " WHERE id_usuario = @IdUsuario";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ApellidoNombres", u.ApellidoNombres);
            cmd.Parameters.AddWithValue("@Alias", u.Alias);
            cmd.Parameters.AddWithValue("@Email", u.Email ?? "");
            cmd.Parameters.AddWithValue("@IdRol", u.IdRol);
            cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
            cmd.Parameters.AddWithValue("@IdUsuario", u.IdUsuario);

            if (!string.IsNullOrEmpty(u.Password))
                cmd.Parameters.AddWithValue("@Password", u.Password);

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
                WHERE u.id_usuario = @Id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

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
            var sql = "DELETE FROM usuarios WHERE id_usuario = @Id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }
        public async Task<Usuario?> ObtenerPorAlias(string alias)
        {
            Usuario? u = null;
            using var conn = _db.GetConnection();
            var sql = @"SELECT id_usuario, apellido_nombres, alias, password, email, id_rol, created_at, updated_at
                        FROM usuarios
                        WHERE UPPER(alias) = @Alias";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Alias", alias.ToUpper());

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
                    DenominacionRol = reader.IsDBNull(reader.GetOrdinal("denominacion_rol")) ? "" : reader.GetString("denominacion_rol")
                },
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at"))
            };
        }
        public async Task<Usuario?> ObtenerPorToken(string token)
        {
            using var conn = _db.GetConnection();
            const string sql = @"
                SELECT u.*, r.id_rol, r.denominacion_rol
                FROM usuarios u
                LEFT JOIN roles r ON u.id_rol = r.id_rol
                WHERE u.reset_token = @Token
                LIMIT 1;";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Token", token);
            await conn.OpenAsync();
            using var rd = await cmd.ExecuteReaderAsync();
            if (await rd.ReadAsync())
            {
                return new Usuario
                {
                    IdUsuario = rd.GetInt32("id_usuario"),
                    ApellidoNombres = rd.GetString("apellido_nombres"),
                    Alias = rd.GetString("alias"),
                    Email = rd.GetString("email"),
                    Password = rd.GetString("password"),
                    IdRol = rd.GetInt32("id_rol"),
                    ResetToken = rd.IsDBNull(rd.GetOrdinal("reset_token")) ? null : rd.GetString("reset_token"),
                    ResetTokenExpira = rd.IsDBNull(rd.GetOrdinal("reset_token_expira")) ? (DateTime?)null : rd.GetDateTime("reset_token_expira")
                };
            }
            return null;
        }

        public async Task<int> GuardarTokenReset(int idUsuario, string token, DateTime expiraUtc)
        {
            using var conn = _db.GetConnection();
            const string sql = @"UPDATE usuarios 
                                SET reset_token = @Token, reset_token_expira = @Expira 
                                WHERE id_usuario = @Id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Token", token);
            cmd.Parameters.AddWithValue("@Expira", expiraUtc);
            cmd.Parameters.AddWithValue("@Id", idUsuario);
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> LimpiarTokenReset(int idUsuario)
        {
            using var conn = _db.GetConnection();
            const string sql = @"UPDATE usuarios 
                                SET reset_token = NULL, reset_token_expira = NULL 
                                WHERE id_usuario = @Id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", idUsuario);
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

         public async Task<int> ActualizarPassword(int idUsuario, string passwordHash)
        {
            using var conn = _db.GetConnection();
            const string sql = @"UPDATE usuarios 
                                SET password = @Password, updated_at = @Updated 
                                WHERE id_usuario = @Id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Password", passwordHash);
            cmd.Parameters.AddWithValue("@Updated", DateTime.Now);
            cmd.Parameters.AddWithValue("@Id", idUsuario);
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<Usuario?> ObtenerPorEmail(string email)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string sql = @"SELECT id_usuario, apellido_nombres, alias, email, password, id_rol, 
                                        reset_token, reset_token_expira
                                 FROM usuarios
                                 WHERE email = @Email";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Email", email);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Usuario
                {
                    IdUsuario = reader.GetInt32("id_usuario"),
                    ApellidoNombres = reader["apellido_nombres"]?.ToString() ?? "",
                    Alias = reader["alias"]?.ToString() ?? "",
                    Email = reader["email"]?.ToString() ?? "",
                    Password = reader["password"]?.ToString() ?? "",
                    IdRol = reader.GetInt32("id_rol"),
                    ResetToken = reader["reset_token"] as string,
                    ResetTokenExpira = reader["reset_token_expira"] as DateTime?
                };
            }

            return null;
        }

    }
}
