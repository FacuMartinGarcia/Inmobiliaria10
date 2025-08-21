// Data/Repositories/PropietarioRepo.cs
using System.Data;
using Inmobiliaria10.Models;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace Inmobiliaria10.Data.Repositories
{
    public class PropietarioRepo : IPropietarioRepo
    {
        private readonly string _cs;

        public PropietarioRepo(IConfiguration cfg)
        {
            _cs = cfg.GetConnectionString("Inmogenial")
                ?? throw new InvalidOperationException("Falta ConnectionStrings:Inmogenial");
        }

        private MySqlConnection Conn() => new MySqlConnection(_cs);

        public async Task<IList<Propietario>> ObtenerTodo(string? q = null)
        {
            var list = new List<Propietario>();
            var sql = @"SELECT id_propietario, documento, apellido_nombres, domicilio, telefono, email
                        FROM propietarios /**where**/ ORDER BY apellido_nombres";
            sql = string.IsNullOrWhiteSpace(q)
                ? sql.Replace("/**where**/", "")
                : sql.Replace("/**where**/", "WHERE apellido_nombres LIKE @q OR documento LIKE @q");

            await using var cn = Conn();
            await cn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, cn);
            if (!string.IsNullOrWhiteSpace(q))
                cmd.Parameters.Add("@q", MySqlDbType.VarChar).Value = $"%{q}%";

            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                list.Add(Map(rd));
            }
            return list;
        }
        public async Task<Propietario?> ObtenerPorId(int id)
        {
            const string sql = @"SELECT id_propietario, documento, apellido_nombres, domicilio, telefono, email
                                 FROM propietarios WHERE id_propietario=@id";
            await using var cn = Conn();
            await cn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", id);

            await using var rd = await cmd.ExecuteReaderAsync();
            return await rd.ReadAsync() ? Map(rd) : null;
        }
        public async Task<int> Crear(Propietario p)
        {
            if (string.IsNullOrWhiteSpace(p.Documento))
                throw new ArgumentException("El documento es obligatorio.");
            if (string.IsNullOrWhiteSpace(p.ApellidoNombres))
                throw new ArgumentException("El nombre y apellido son obligatorios.");

            const string sql = @"
                INSERT INTO propietarios (documento, apellido_nombres, domicilio, telefono, email)
                VALUES (@doc, @ape, @dom, @tel, @mail);";

            try
            {
                await using var cn = Conn();
                await cn.OpenAsync();
                await using var cmd = new MySqlCommand(sql, cn);
                cmd.Parameters.Add("@doc", MySqlDbType.VarChar).Value = p.Documento;
                cmd.Parameters.Add("@ape", MySqlDbType.VarChar).Value = p.ApellidoNombres;
                cmd.Parameters.Add("@dom", MySqlDbType.VarChar).Value = (object?)p.Domicilio ?? DBNull.Value;
                cmd.Parameters.Add("@tel", MySqlDbType.VarChar).Value = (object?)p.Telefono ?? DBNull.Value;
                cmd.Parameters.Add("@mail", MySqlDbType.VarChar).Value = (object?)p.Email ?? DBNull.Value;

                await cmd.ExecuteNonQueryAsync();
                return Convert.ToInt32(cmd.LastInsertedId);
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                throw new Exception("Documento duplicado.", ex);
            }
        }
        public async Task<bool> Modificar(Propietario p)
        {
            if (p.IdPropietario <= 0)
                throw new ArgumentException("IdPropietario inválido.");
            if (string.IsNullOrWhiteSpace(p.Documento))
                throw new ArgumentException("Documento obligatorio.");
            if (string.IsNullOrWhiteSpace(p.ApellidoNombres))
                throw new ArgumentException("Apellido y Nombres obligatorios.");

            const string sql = @"
                UPDATE propietarios
                   SET documento=@doc, apellido_nombres=@ape, domicilio=@dom, telefono=@tel, email=@mail
                 WHERE id_propietario=@id;";

            try
            {
                await using var cn = Conn();
                await cn.OpenAsync();
                await using var cmd = new MySqlCommand(sql, cn);

                cmd.Parameters.Add("@doc", MySqlDbType.VarChar).Value = p.Documento;
                cmd.Parameters.Add("@ape", MySqlDbType.VarChar).Value = p.ApellidoNombres;
                cmd.Parameters.Add("@dom", MySqlDbType.VarChar).Value = (object?)p.Domicilio ?? DBNull.Value;
                cmd.Parameters.Add("@tel", MySqlDbType.VarChar).Value = (object?)p.Telefono ?? DBNull.Value;
                cmd.Parameters.Add("@mail", MySqlDbType.VarChar).Value = (object?)p.Email ?? DBNull.Value;
                cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = p.IdPropietario;

                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                throw new Exception("Documento duplicado. Verifique que no exista otro propietario con el mismo documento.", ex);
            }
        }
        public async Task<bool> Borrar(int id)
        {
            const string sql = "DELETE FROM propietarios WHERE id_propietario=@id";
            await using var cn = Conn();
            await cn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", id);
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
        public async Task<bool> ExistsDocumento(string documento, int? exceptId = null)
        {
            var sql = "SELECT COUNT(1) FROM propietarios WHERE documento=@doc";
            if (exceptId.HasValue) sql += " AND id_propietario<>@id";

            await using var cn = Conn();
            await cn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@doc", documento);
            if (exceptId.HasValue) cmd.Parameters.AddWithValue("@id", exceptId.Value);
            var n = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return n > 0;
        }

        // === NUEVO: Paginado con búsqueda ===
        public async Task<PagedResult<Propietario>> BuscarPaginado(string? q, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var result = new PagedResult<Propietario>
            {
                Page = page,
                PageSize = pageSize,
                Query = q
            };

            // 1) contar total
            var sqlCount = "SELECT COUNT(*) FROM propietarios /**where**/";
            sqlCount = string.IsNullOrWhiteSpace(q)
                ? sqlCount.Replace("/**where**/", "")
                : sqlCount.Replace("/**where**/", "WHERE apellido_nombres LIKE @q OR documento LIKE @q");

            // 2) traer página
            var sqlPage = @"
                SELECT id_propietario, documento, apellido_nombres, domicilio, telefono, email
                FROM propietarios
                /**where**/
                ORDER BY apellido_nombres
                LIMIT @take OFFSET @skip;";
            sqlPage = string.IsNullOrWhiteSpace(q)
                ? sqlPage.Replace("/**where**/", "")
                : sqlPage.Replace("/**where**/", "WHERE apellido_nombres LIKE @q OR documento LIKE @q");

            await using var cn = Conn();
            await cn.OpenAsync();

            // COUNT
            await using (var cmdCount = new MySqlCommand(sqlCount, cn))
            {
                if (!string.IsNullOrWhiteSpace(q))
                    cmdCount.Parameters.Add("@q", MySqlDbType.VarChar).Value = $"%{q}%";
                result.TotalItems = Convert.ToInt32(await cmdCount.ExecuteScalarAsync());
            }

            // PAGE
            var items = new List<Propietario>();
            await using (var cmdPage = new MySqlCommand(sqlPage, cn))
            {
                if (!string.IsNullOrWhiteSpace(q))
                    cmdPage.Parameters.Add("@q", MySqlDbType.VarChar).Value = $"%{q}%";
                cmdPage.Parameters.Add("@take", MySqlDbType.Int32).Value = pageSize;
                cmdPage.Parameters.Add("@skip", MySqlDbType.Int32).Value = (page - 1) * pageSize;

                await using var rd = await cmdPage.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                    items.Add(Map(rd));
            }

            result.Items = items;
            return result;
        }

        // ==== mapper único para no repetir código ====
        private static Propietario Map(IDataRecord rd)
        {
            return new Propietario
            {
                IdPropietario   = rd.GetInt32(rd.GetOrdinal("id_propietario")),
                Documento       = rd.GetString(rd.GetOrdinal("documento")),
                ApellidoNombres = rd.GetString(rd.GetOrdinal("apellido_nombres")),
                Domicilio       = IsNull(rd, "domicilio") ? string.Empty : rd.GetString(rd.GetOrdinal("domicilio")),
                Telefono        = IsNull(rd, "telefono")  ? string.Empty : rd.GetString(rd.GetOrdinal("telefono")),
                Email           = IsNull(rd, "email")     ? string.Empty : rd.GetString(rd.GetOrdinal("email")),
            };
        }
        private static bool IsNull(IDataRecord rd, string col)
        {
            var i = rd.GetOrdinal(col);
            return rd.IsDBNull(i);
        }
    }
}
