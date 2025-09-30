using MySql.Data.MySqlClient;
using System.Data.Common;
using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories
{
    public class InmuebleRepo : IInmuebleRepo
    {
        private readonly Database _db;
        private readonly IImagenRepo _imagenRepo;

        public InmuebleRepo(Database db)
        {
            _db = db;
            _imagenRepo = new ImagenRepo(db);
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
            int pagina,
            int cantidadPorPagina,
            string? searchString = null,
            bool soloDisponibles = false,
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null)
        {
            var lista = new List<Inmueble>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            int offset = (pagina - 1) * cantidadPorPagina;

            var condiciones = new List<string>();


            if (!string.IsNullOrEmpty(searchString))
            {
                condiciones.Add(@"(i.direccion LIKE @search 
                                OR i.ambientes LIKE @search
                                OR t.denominacion_tipo LIKE @search 
                                OR p.apellido_nombres LIKE @search)");
            }


            if (soloDisponibles)
            {
                condiciones.Add($@"NOT EXISTS (
                    SELECT 1 
                    FROM contratos c2
                    WHERE c2.id_inmueble = i.id_inmueble
                    AND (c2.rescision IS NULL OR c2.rescision > @hoy)
                    AND c2.fecha_fin >= @hoy
                )");
            }


            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                condiciones.Add($@"NOT EXISTS (
                    SELECT 1 
                    FROM contratos c2
                    WHERE c2.id_inmueble = i.id_inmueble
                    AND c2.fecha_inicio <= @fechaFin
                    AND c2.fecha_fin >= @fechaInicio
                    AND (c2.rescision IS NULL OR c2.rescision > @fechaInicio)
                )");
            }

            string where = condiciones.Any() ? "WHERE " + string.Join(" AND ", condiciones) : "";

            // SQL principal
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

            if (soloDisponibles || fechaInicio.HasValue)
                cmd.Parameters.AddWithValue("@hoy", DateTime.Today);

            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio.Value);
                cmd.Parameters.AddWithValue("@fechaFin", fechaFin.Value);
            }

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(Map(reader));
            }
            await reader.CloseAsync();


            var sqlTotal = $@"
                SELECT COUNT(DISTINCT i.id_inmueble)
                FROM inmuebles i
                LEFT JOIN inmuebles_usos u ON i.id_uso = u.id_uso
                LEFT JOIN inmuebles_tipos t ON i.id_tipo = t.id_tipo
                LEFT JOIN propietarios p ON i.id_propietario = p.id_propietario
                {where}
            ";

            using var cmdTotal = new MySqlCommand(sqlTotal, conn);

            if (!string.IsNullOrEmpty(searchString))
                cmdTotal.Parameters.AddWithValue("@search", "%" + searchString + "%");

            if (soloDisponibles || fechaInicio.HasValue)
                cmdTotal.Parameters.AddWithValue("@hoy", DateTime.Today);

            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                cmdTotal.Parameters.AddWithValue("@fechaInicio", fechaInicio.Value);
                cmdTotal.Parameters.AddWithValue("@fechaFin", fechaFin.Value);
            }

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


            if (i != null)
            {
                i.Imagenes = (await _imagenRepo.BuscarPorInmueble(id)).ToList();
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
            var inmueble = new Inmueble
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

            // 📌 Generar la ruta de la portada dinámicamente
            //            inmueble.Portada = $"/uploads/Inmuebles/{inmueble.IdInmueble}.jpeg";

            return inmueble;
        }

    public async Task<(List<Inmueble> registros, int totalRegistros)> ListarPorPropietario(
        int idPropietario, int pagina, int cantidadPorPagina, string? searchString = null)
    {
        var lista = new List<Inmueble>();
        using var conn = _db.GetConnection();

        int offset = (pagina - 1) * cantidadPorPagina;

        string where = "WHERE i.id_propietario = @idPropietario";
        if (!string.IsNullOrEmpty(searchString))
        {
            where += @" AND (
                            i.direccion LIKE @search
                            OR CAST(i.ambientes AS CHAR) LIKE @search
                            OR t.denominacion_tipo LIKE @search
                            OR u.denominacion_uso LIKE @search
                        )";
        }

        var sql = $@"
            SELECT i.*,
                u.id_uso AS uso_id_uso, u.denominacion_uso AS denominacion_uso,
                t.id_tipo AS tipo_id_tipo, t.denominacion_tipo AS denominacion_tipo,
                p.id_propietario, p.documento, p.apellido_nombres, p.domicilio,
                p.telefono, p.email,
                CASE
                    WHEN EXISTS (
                        SELECT 1
                        FROM contratos c
                        WHERE c.id_inmueble = i.id_inmueble
                        AND c.deleted_at IS NULL
                        AND (
                            (c.rescision IS NULL OR DATE(c.rescision) > CURDATE())
                        )
                        AND DATE(c.fecha_inicio) <= CURDATE()
                        AND DATE(c.fecha_fin) >= CURDATE()
                    ) THEN 1 ELSE 0
                END AS esta_alquilado,

                CASE
                    WHEN EXISTS (
                        SELECT 1
                        FROM contratos c
                        WHERE c.id_inmueble = i.id_inmueble
                        AND c.deleted_at IS NULL
                        AND DATE(c.rescision) > CURDATE()
                        AND DATE(c.fecha_inicio) <= CURDATE()
                        AND DATE(c.fecha_fin) >= CURDATE()
                    ) THEN 1 ELSE 0
                END AS tiene_rescision_futura

            FROM inmuebles i
            LEFT JOIN inmuebles_usos u ON i.id_uso = u.id_uso
            LEFT JOIN inmuebles_tipos t ON i.id_tipo = t.id_tipo
            LEFT JOIN propietarios p ON i.id_propietario = p.id_propietario
            {where}
            ORDER BY i.direccion
            LIMIT @cantidad OFFSET @offset;
        ";

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@idPropietario", idPropietario);
        cmd.Parameters.AddWithValue("@cantidad", cantidadPorPagina);
        cmd.Parameters.AddWithValue("@offset", offset);

        if (!string.IsNullOrEmpty(searchString))
            cmd.Parameters.AddWithValue("@search", "%" + searchString + "%");

        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {

            var inmueble = Map(reader);

            int ordEsta = reader.GetOrdinal("esta_alquilado");
            inmueble.EstaAlquilado = !reader.IsDBNull(ordEsta) && reader.GetInt32(ordEsta) == 1;

            int ordResc = reader.GetOrdinal("tiene_rescision_futura");
            inmueble.TieneRescisionFutura = !reader.IsDBNull(ordResc) && reader.GetInt32(ordResc) == 1;


            lista.Add(inmueble);
        }
        await reader.CloseAsync();

        var sqlTotal = $@"
            SELECT COUNT(*)
            FROM inmuebles i
            LEFT JOIN inmuebles_usos u ON i.id_uso = u.id_uso
            LEFT JOIN inmuebles_tipos t ON i.id_tipo = t.id_tipo
            LEFT JOIN propietarios p ON i.id_propietario = p.id_propietario
            {where};
        ";

        using var cmdTotal = new MySqlCommand(sqlTotal, conn);
        cmdTotal.Parameters.AddWithValue("@idPropietario", idPropietario);
        if (!string.IsNullOrEmpty(searchString))
            cmdTotal.Parameters.AddWithValue("@search", "%" + searchString + "%");

        int totalRegistros = Convert.ToInt32(await cmdTotal.ExecuteScalarAsync());

        return (lista, totalRegistros);
    }

    }
}
