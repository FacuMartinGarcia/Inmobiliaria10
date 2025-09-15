using System.Data;
using MySql.Data.MySqlClient;
using System.Data.Common;
using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories
{
    public class InmuebleRepo : IInmuebleRepo
    {
        private readonly Database _db;

        public InmuebleRepo(Database db)
        {
            _db = db;
        }

        public async Task<int> Agregar(Inmueble i)
        {
            using var conn = _db.GetConnection();
            var sql = @"INSERT INTO inmuebles 
                        (id_propietario, id_uso, id_tipo, direccion, piso, depto, lat, lon, ambientes, precio, activo, created_at, updated_at)
                        VALUES (@idPropietario, @idUso, @idTipo, @direccion, @piso, @depto, @lat, @lon, @ambientes, @precio, @activo, @createdAt, @updatedAt);
                        SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idPropietario", i.IdPropietario);
            cmd.Parameters.AddWithValue("@idUso", i.IdUso);
            cmd.Parameters.AddWithValue("@idTipo", i.IdTipo);
            cmd.Parameters.AddWithValue("@direccion", i.Direccion);
            cmd.Parameters.AddWithValue("@piso", (object?)i.Piso ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@depto", (object?)i.Depto ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@lat", (object?)i.Lat ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@lon", (object?)i.Lon ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ambientes", (object?)i.Ambientes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@precio", (object?)i.Precio ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@activo", i.Activo);
            cmd.Parameters.AddWithValue("@createdAt", DateTime.Now);
            cmd.Parameters.AddWithValue("@updatedAt", DateTime.Now);

            await conn.OpenAsync();
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<int> Actualizar(Inmueble i)
        {
            using var conn = _db.GetConnection();
            var sql = @"UPDATE inmuebles SET
                            id_propietario = @idPropietario,
                            id_uso = @idUso,
                            id_tipo = @idTipo,
                            direccion = @direccion,
                            piso = @piso,
                            depto = @depto,
                            lat = @lat,
                            lon = @lon,
                            ambientes = @ambientes,
                            precio = @precio,
                            activo = @activo,
                            portada = @portada,         -- 👈 agregado
                            updated_at = @updatedAt
                        WHERE id_inmueble = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idPropietario", i.IdPropietario);
            cmd.Parameters.AddWithValue("@idUso", i.IdUso);
            cmd.Parameters.AddWithValue("@idTipo", i.IdTipo);
            cmd.Parameters.AddWithValue("@direccion", i.Direccion);
            cmd.Parameters.AddWithValue("@piso", (object?)i.Piso ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@depto", (object?)i.Depto ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@lat", (object?)i.Lat ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@lon", (object?)i.Lon ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ambientes", (object?)i.Ambientes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@precio", (object?)i.Precio ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@activo", i.Activo);
            cmd.Parameters.AddWithValue("@portada", (object?)i.Portada ?? DBNull.Value); 
            cmd.Parameters.AddWithValue("@updatedAt", DateTime.Now);
            cmd.Parameters.AddWithValue("@id", i.IdInmueble);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }


        public async Task<List<Inmueble>> ListarTodos()
        {
            var lista = new List<Inmueble>();
            using var conn = _db.GetConnection();

            var sql = @"
                SELECT i.*,
                    u.id_uso AS uso_id_uso, u.denominacion_uso AS denominacion_uso,
                    t.id_tipo AS tipo_id_tipo, t.denominacion_tipo AS denominacion_tipo,
                    p.id_propietario, p.documento, p.apellido_nombres, p.domicilio, 
                    p.telefono, p.email
                FROM inmuebles i
                LEFT JOIN inmuebles_usos u ON i.id_uso = u.id_uso
                LEFT JOIN inmuebles_tipos t ON i.id_tipo = t.id_tipo
                LEFT JOIN propietarios p ON i.id_propietario = p.id_propietario
            ";

            using var cmd = new MySqlCommand(sql, conn);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(Map(reader));
            }
            return lista;
        }

        public async Task<(List<Inmueble> registros, int totalRegistros)> ListarTodosPaginado(
            int pagina, int cantidadPorPagina, string? searchString = null)
        {
            var lista = new List<Inmueble>();
            using var conn = _db.GetConnection();

            int offset = (pagina - 1) * cantidadPorPagina;

            string where = "";
            if (!string.IsNullOrEmpty(searchString))
            {
                where = @"WHERE i.direccion LIKE @search 
                        OR i.ambientes LIKE @search
                        OR t.denominacion_tipo LIKE @search 
                        OR p.apellido_nombres LIKE @search";
            }

            var sql = $@"
                SELECT i.*,
                    u.id_uso AS uso_id_uso, u.denominacion_uso AS denominacion_uso,
                    t.id_tipo AS tipo_id_tipo, t.denominacion_tipo AS denominacion_tipo,
                    p.id_propietario, p.documento, p.apellido_nombres, p.domicilio, 
                    p.telefono, p.email
                FROM inmuebles i
                LEFT JOIN inmuebles_usos u ON i.id_uso = u.id_uso
                LEFT JOIN inmuebles_tipos t ON i.id_tipo = t.id_tipo
                LEFT JOIN propietarios p ON i.id_propietario = p.id_propietario
                {where}
                ORDER BY i.direccion
                LIMIT @cantidad OFFSET @offset
            ";

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
                FROM inmuebles i
                LEFT JOIN inmuebles_tipos t ON i.id_tipo = t.id_tipo
                LEFT JOIN propietarios p ON i.id_propietario = p.id_propietario
                {where}";

            using var cmdTotal = new MySqlCommand(sqlTotal, conn);

            if (!string.IsNullOrEmpty(searchString))
                cmdTotal.Parameters.AddWithValue("@search", "%" + searchString + "%");

            int totalRegistros = Convert.ToInt32(await cmdTotal.ExecuteScalarAsync());

            return (lista, totalRegistros);
        }

        public async Task<Inmueble?> ObtenerPorId(int id)
        {
            Inmueble? i = null;
            using var conn = _db.GetConnection();

            var sql = @"
                SELECT i.*,
                    u.id_uso AS uso_id_uso, u.denominacion_uso AS denominacion_uso,
                    t.id_tipo AS tipo_id_tipo, t.denominacion_tipo AS denominacion_tipo,
                    p.id_propietario, p.documento, p.apellido_nombres, p.domicilio, 
                    p.telefono, p.email
                FROM inmuebles i
                LEFT JOIN inmuebles_usos u ON i.id_uso = u.id_uso
                LEFT JOIN inmuebles_tipos t ON i.id_tipo = t.id_tipo
                LEFT JOIN propietarios p ON i.id_propietario = p.id_propietario
                WHERE i.id_inmueble = @id
            ";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                i = Map(reader);
            }
            return i;
        }

        public async Task<int> Eliminar(int id)
        {
            using var conn = _db.GetConnection();
            var sql = "DELETE FROM inmuebles WHERE id_inmueble = @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<Inmueble>> BuscarInmueble(string term)
        {
            var lista = new List<Inmueble>();
            using var conn = _db.GetConnection();

            var sql = @"
                SELECT i.*,
                    u.id_uso AS uso_id_uso, u.denominacion_uso AS denominacion_uso,
                    t.id_tipo AS tipo_id_tipo, t.denominacion_tipo AS denominacion_tipo,
                    p.id_propietario, p.documento, p.apellido_nombres, p.domicilio, 
                    p.telefono, p.email
                FROM inmuebles i
                LEFT JOIN inmuebles_usos u ON i.id_uso = u.id_uso
                LEFT JOIN inmuebles_tipos t ON i.id_tipo = t.id_tipo
                LEFT JOIN propietarios p ON i.id_propietario = p.id_propietario
                WHERE i.direccion LIKE @term
                AND i.activo = 1
                ORDER BY i.direccion, i.piso, i.depto
                LIMIT 20
            ";

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.Add("@term", MySqlDbType.VarChar).Value = $"%{term}%";

            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(Map(reader));
            }
            return lista;
        }
        private Inmueble Map(DbDataReader reader)
        {
            return new Inmueble
            {
                IdInmueble = reader.GetInt32(reader.GetOrdinal("id_inmueble")),
                IdPropietario = reader.GetInt32(reader.GetOrdinal("id_propietario")),
                Propietario = new Propietario
                {
                    IdPropietario = reader.GetInt32(reader.GetOrdinal("id_propietario")),
                    Documento = reader.GetString(reader.GetOrdinal("documento")),
                    ApellidoNombres = reader.GetString(reader.GetOrdinal("apellido_nombres")),
                    Domicilio = reader.GetString(reader.GetOrdinal("domicilio")),
                    Telefono = reader.IsDBNull(reader.GetOrdinal("telefono")) ? null : reader.GetString(reader.GetOrdinal("telefono")),
                    Email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString(reader.GetOrdinal("email"))
                },
                IdUso = reader.GetInt32(reader.GetOrdinal("id_uso")),
                Uso = new InmuebleUso
                {
                    IdUso = reader.GetInt32(reader.GetOrdinal("uso_id_uso")),
                    DenominacionUso = reader.GetString(reader.GetOrdinal("denominacion_uso"))
                },
                IdTipo = reader.GetInt32(reader.GetOrdinal("id_tipo")),
                Tipo = new InmuebleTipo
                {
                    IdTipo = reader.GetInt32(reader.GetOrdinal("tipo_id_tipo")),
                    DenominacionTipo = reader.GetString(reader.GetOrdinal("denominacion_tipo"))
                },
                Direccion = reader.GetString(reader.GetOrdinal("direccion")),
                Piso = reader.IsDBNull(reader.GetOrdinal("piso")) ? null : reader.GetString(reader.GetOrdinal("piso")),
                Depto = reader.IsDBNull(reader.GetOrdinal("depto")) ? null : reader.GetString(reader.GetOrdinal("depto")),
                Lat = reader.IsDBNull(reader.GetOrdinal("lat")) ? null : reader.GetDecimal(reader.GetOrdinal("lat")),
                Lon = reader.IsDBNull(reader.GetOrdinal("lon")) ? null : reader.GetDecimal(reader.GetOrdinal("lon")),
                Ambientes = reader.IsDBNull(reader.GetOrdinal("ambientes")) ? null : reader.GetInt32(reader.GetOrdinal("ambientes")),
                Precio = reader.IsDBNull(reader.GetOrdinal("precio")) ? null : reader.GetDecimal(reader.GetOrdinal("precio")),
                Portada = reader.IsDBNull(reader.GetOrdinal("portada")) ? null : reader.GetString(reader.GetOrdinal("portada")),
                Activo = reader.GetBoolean(reader.GetOrdinal("activo")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at"))
            };
        }
    }
}
