using Inmobiliaria10.Models;
using Inmobiliaria10.Models.ViewModels;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text.Json; 

namespace Inmobiliaria10.Data.Repositories
{
    public class ContratoRepo : IContratoRepo
    {
        private readonly Database _db;

        public ContratoRepo(Database db)
        {
            _db = db;
        }

        // --------------- Helpers ----------------

        private static Contrato MapContrato(MySqlDataReader r)
        {
            return new Contrato
            {
                IdContrato = r.GetInt32("IdContrato"),
                FechaFirma = r.IsDBNull(r.GetOrdinal("FechaFirma")) ? null : r.GetDateTime("FechaFirma"),
                IdInmueble = r.GetInt32("IdInmueble"),
                IdInquilino = r.GetInt32("IdInquilino"),
                FechaInicio = r.GetDateTime("FechaInicio"),
                FechaFin = r.GetDateTime("FechaFin"),
                MontoMensual = r.GetDecimal("MontoMensual"),
                Rescision = r.IsDBNull(r.GetOrdinal("Rescision")) ? null : r.GetDateTime("Rescision"),
                MontoMulta = r.IsDBNull(r.GetOrdinal("MontoMulta")) ? null : r.GetDecimal("MontoMulta"),
                CreatedBy = r.GetInt32("CreatedBy"),
                CreatedAt = r.GetDateTime("CreatedAt"),
                DeletedAt = r.IsDBNull(r.GetOrdinal("DeletedAt")) ? null : r.GetDateTime("DeletedAt"),
                DeletedBy = r.IsDBNull(r.GetOrdinal("DeletedBy")) ? null : r.GetInt32("DeletedBy")
            };
        }

        private static void AddParam(MySqlCommand cmd, string name, object? value, MySqlDbType type)
        {
            var p = cmd.Parameters.Add(name, type);
            p.Value = value ?? DBNull.Value;
        }

        // --------------- CRUD ----------------

        public async Task<Contrato?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT  c.id_contrato   AS IdContrato,
                        c.fecha_firma   AS FechaFirma,
                        c.id_inmueble   AS IdInmueble,
                        i.direccion     AS InmuebleDireccion,
                        c.id_inquilino  AS IdInquilino,
                        q.apellido_nombres AS InquilinoNombre,
                        c.fecha_inicio  AS FechaInicio,
                        c.fecha_fin     AS FechaFin,
                        c.monto_mensual AS MontoMensual,
                        c.rescision     AS Rescision,
                        c.monto_multa   AS MontoMulta,
                        c.created_by    AS CreatedBy,
                        c.created_at    AS CreatedAt,
                        c.deleted_at    AS DeletedAt,
                        c.deleted_by    AS DeletedBy
                FROM contratos c
                INNER JOIN inmuebles i ON i.id_inmueble = c.id_inmueble
                INNER JOIN inquilinos q ON q.id_inquilino = c.id_inquilino
                WHERE c.id_contrato = @id;";

            using var cmd = new MySqlCommand(sql, conn);
            AddParam(cmd, "@id", id, MySqlDbType.Int32);

            using var r = await cmd.ExecuteReaderAsync(ct);
            if (await r.ReadAsync(ct))
            {
                var contrato = MapContrato((MySqlDataReader)r);
                contrato.Inmueble = new Inmueble
                {
                    IdInmueble = contrato.IdInmueble,
                    Direccion = r.GetString("InmuebleDireccion")
                };
                contrato.Inquilino = new Inquilino
                {
                    IdInquilino = contrato.IdInquilino,
                    ApellidoNombres = r.GetString("InquilinoNombre")
                };

                return contrato;
            }

            return null;
        }

        public async Task<(IReadOnlyList<ContratoAuditViewModel> Items, int Total)> ListAuditoriaAsync(
            int? usuarioId = null,
            int pageIndex = 1,
            int pageSize = 10,
            CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            var pars = new List<MySqlParameter>();
            var where = " WHERE 1=1 ";

            if (usuarioId is > 0)
            {
                where += " AND a.accion_by = @usuarioId ";
                pars.Add(new MySqlParameter("@usuarioId", usuarioId));
            }

            var sqlCount = $@"
                SELECT COUNT(*)
                FROM contratos_audit a
                {where};";

            using var cmdCount = new MySqlCommand(sqlCount, conn);
            cmdCount.Parameters.AddRange(pars.ToArray());
            var total = Convert.ToInt32(await cmdCount.ExecuteScalarAsync(ct));

            var sqlItems = $@"
                SELECT  a.id_audit   AS IdAudit,
                        a.id_contrato AS IdContrato,
                        a.accion     AS Accion,
                        a.accion_at  AS AccionAt,
                        u.alias      AS Usuario,
                        a.old_data   AS OldData,
                        a.new_data   AS NewData
                FROM contratos_audit a
                LEFT JOIN usuarios u ON u.id_usuario = a.accion_by
                {where}
                ORDER BY a.id_audit DESC
                LIMIT @limit OFFSET @offset;";

            pars.Add(new MySqlParameter("@limit", pageSize));
            pars.Add(new MySqlParameter("@offset", (pageIndex - 1) * pageSize));

            using var cmdItems = new MySqlCommand(sqlItems, conn);
            cmdItems.Parameters.AddRange(pars.ToArray());

            var list = new List<ContratoAuditViewModel>();
            using var r = await cmdItems.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
            {
                var vm = new ContratoAuditViewModel
                {
                    IdAudit = Convert.ToInt32(r["IdAudit"]),
                    IdContrato = Convert.ToInt32(r["IdContrato"]),
                    Accion = r["Accion"].ToString()!,
                    AccionAt = Convert.ToDateTime(r["AccionAt"]),
                    Usuario = r["Usuario"]?.ToString() ?? "(sistema)"
                };

                // 🔹 Llamadas async después de inicializar el objeto
                vm.OldData = await ParseAndTranslateContratoAsync(r["OldData"] as string, ct);
                vm.NewData = await ParseAndTranslateContratoAsync(r["NewData"] as string, ct);

                list.Add(vm);

            }

            return (list, total);
        }

        private async Task<Dictionary<string, string>> ParseAndTranslateContratoAsync(
            string? json, CancellationToken ct = default)
        {
            var dict = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(json)) return dict;

            var doc = JsonDocument.Parse(json);
            foreach (var kv in doc.RootElement.EnumerateObject())
            {
                var key = kv.Name;
                var value = kv.Value.ValueKind == JsonValueKind.Null ? null : kv.Value.ToString();

                switch (key.ToLower())
                {
                    case "id_inquilino":
                        if (int.TryParse(value, out var idInquilino))
                            value = await GetInquilinoTextoAsync(idInquilino, ct);
                        break;

                    case "id_inmueble":
                        if (int.TryParse(value, out var idInmueble))
                            value = await GetInmuebleTextoAsync(idInmueble, ct);
                        break;

                    case "fecha_inicio":
                    case "fecha_fin":
                        if (DateTime.TryParse(value, out var fecha))
                            value = fecha.ToString("dd/MM/yyyy");
                        break;

                    case "monto_mensual":
                        if (decimal.TryParse(value, out var monto))
                            value = monto.ToString("C"); // formato moneda
                        break;
                }

                dict[key] = value ?? string.Empty;
            }

            return dict;
        }

        private async Task<string> GetInquilinoTextoAsync(int id, CancellationToken ct)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"SELECT apellido_nombres FROM inquilinos WHERE id_inquilino = @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            var res = await cmd.ExecuteScalarAsync(ct);
            return res?.ToString() ?? $"Inquilino #{id}";
        }

        private async Task<string> GetInmuebleTextoAsync(int id, CancellationToken ct)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"SELECT direccion FROM inmuebles WHERE id_inmueble = @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            var res = await cmd.ExecuteScalarAsync(ct);
            return res?.ToString() ?? $"Inmueble #{id}";
        }


        public async Task<(IReadOnlyList<Contrato> Items, int Total)> ListAsync(
            int? tipo = null,
            int? idInmueble = null,
            int? idInquilino = null,
            bool? soloActivos = null,
            int pageIndex = 1,
            int pageSize = 20,
            CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            var where = " WHERE c.deleted_at IS NULL ";
            var pars = new List<MySqlParameter>();

            if (tipo.HasValue && tipo.Value > 0)
            {
                where += " AND i.id_tipo = @Tipo ";
                pars.Add(new MySqlParameter("@Tipo", MySqlDbType.Int32) { Value = tipo.Value });
            }

            if (idInmueble.HasValue && idInmueble.Value > 0)
            {
                where += " AND c.id_inmueble = @idInmueble ";
                pars.Add(new MySqlParameter("@idInmueble", MySqlDbType.Int32) { Value = idInmueble.Value });
            }

            if (idInquilino.HasValue && idInquilino.Value > 0)
            {
                where += " AND c.id_inquilino = @idInquilino ";
                pars.Add(new MySqlParameter("@idInquilino", MySqlDbType.Int32) { Value = idInquilino.Value });
            }

            if (soloActivos.HasValue)
            {
                var hoy = DateTime.Today;
                pars.Add(new MySqlParameter("@Hoy", MySqlDbType.Date) { Value = hoy });

                if (soloActivos.Value)
                    where += " AND c.fecha_fin >= @Hoy ";
                else
                    where += " AND c.fecha_fin < @Hoy ";
            }

            // agregamos el join con inmuebles
            var sqlCount = $@"
                SELECT COUNT(*)
                FROM contratos c
                INNER JOIN inmuebles i ON i.id_inmueble = c.id_inmueble
                {where};";

            using (var cmdCount = new MySqlCommand(sqlCount, conn))
            {
                cmdCount.Parameters.AddRange(pars.ToArray());
                var total = Convert.ToInt32(await cmdCount.ExecuteScalarAsync(ct));

                var sqlItems = $@"
                    SELECT  c.id_contrato   AS IdContrato,
                            c.fecha_firma   AS FechaFirma,
                            c.id_inmueble   AS IdInmueble,
                            c.id_inquilino  AS IdInquilino,
                            c.fecha_inicio  AS FechaInicio,
                            c.fecha_fin     AS FechaFin,
                            c.monto_mensual AS MontoMensual,
                            c.rescision     AS Rescision,
                            c.monto_multa   AS MontoMulta,
                            c.created_by    AS CreatedBy,
                            c.created_at    AS CreatedAt,
                            c.deleted_at    AS DeletedAt,
                            c.deleted_by    AS DeletedBy
                    FROM contratos c
                    INNER JOIN inmuebles i ON i.id_inmueble = c.id_inmueble
                    {where}
                    ORDER BY c.created_at DESC";

                if (pageSize > 0)
                {
                    int offset = Math.Max(0, (pageIndex - 1) * pageSize);
                    sqlItems += " LIMIT @limit OFFSET @offset;";
                    pars.Add(new MySqlParameter("@limit", MySqlDbType.Int32) { Value = pageSize });
                    pars.Add(new MySqlParameter("@offset", MySqlDbType.Int32) { Value = offset });
                }
                else
                {
                    sqlItems += ";";
                }

                using var cmdItems = new MySqlCommand(sqlItems, conn);
                cmdItems.Parameters.AddRange(pars.ToArray());

                var items = new List<Contrato>();
                using var r = await cmdItems.ExecuteReaderAsync(ct);
                while (await r.ReadAsync(ct))
                    items.Add(MapContrato((MySqlDataReader)r));

                return (items, total);
            }
        }

    public async Task<(IReadOnlyList<Contrato> Items, int Total)> ListByFechasAsync(
        int? tipo = null,
        int? idInmueble = null,
        int? idInquilino = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        int pageIndex = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        using var conn = _db.GetConnection();
        await conn.OpenAsync(ct);

        var where = " WHERE c.deleted_at IS NULL ";
        var pars = new List<MySqlParameter>();

        if (tipo.HasValue && tipo.Value > 0)
        {
            where += " AND i.id_tipo = @Tipo ";
            pars.Add(new MySqlParameter("@Tipo", MySqlDbType.Int32) { Value = tipo.Value });
        }

        if (idInmueble.HasValue && idInmueble.Value > 0)
        {
            where += " AND c.id_inmueble = @idInmueble ";
            pars.Add(new MySqlParameter("@idInmueble", MySqlDbType.Int32) { Value = idInmueble.Value });
        }

        if (idInquilino.HasValue && idInquilino.Value > 0)
        {
            where += " AND c.id_inquilino = @idInquilino ";
            pars.Add(new MySqlParameter("@idInquilino", MySqlDbType.Int32) { Value = idInquilino.Value });
        }

        if (fechaDesde.HasValue)
        {
            where += " AND COALESCE(c.rescision, c.fecha_fin) >= @fechaDesde ";
            pars.Add(new MySqlParameter("@fechaDesde", MySqlDbType.Date) { Value = fechaDesde.Value });
        }

        if (fechaHasta.HasValue)
        {
            where += " AND c.fecha_inicio <= @fechaHasta ";
            pars.Add(new MySqlParameter("@fechaHasta", MySqlDbType.Date) { Value = fechaHasta.Value });
        }


        // Count
        var sqlCount = $@"
            SELECT COUNT(*)
            FROM contratos c
            INNER JOIN inmuebles i ON i.id_inmueble = c.id_inmueble
            {where};";

        using var cmdCount = new MySqlCommand(sqlCount, conn);
        cmdCount.Parameters.AddRange(pars.ToArray());
        var total = Convert.ToInt32(await cmdCount.ExecuteScalarAsync(ct));

        // Items
        var sqlItems = $@"
            SELECT  c.id_contrato   AS IdContrato,
                    c.fecha_firma   AS FechaFirma,
                    c.id_inmueble   AS IdInmueble,
                    c.id_inquilino  AS IdInquilino,
                    c.fecha_inicio  AS FechaInicio,
                    c.fecha_fin     AS FechaFin,
                    c.monto_mensual AS MontoMensual,
                    c.rescision     AS Rescision,
                    c.monto_multa   AS MontoMulta,
                    c.created_by    AS CreatedBy,
                    c.created_at    AS CreatedAt,
                    c.deleted_at    AS DeletedAt,
                    c.deleted_by    AS DeletedBy
            FROM contratos c
            INNER JOIN inmuebles i ON i.id_inmueble = c.id_inmueble
            {where}
            ORDER BY c.created_at DESC";

        if (pageSize > 0)
        {
            int offset = Math.Max(0, (pageIndex - 1) * pageSize);
            sqlItems += " LIMIT @limit OFFSET @offset;";
            pars.Add(new MySqlParameter("@limit", MySqlDbType.Int32) { Value = pageSize });
            pars.Add(new MySqlParameter("@offset", MySqlDbType.Int32) { Value = offset });
        }
        else
        {
            sqlItems += ";";
        }

        using var cmdItems = new MySqlCommand(sqlItems, conn);
        cmdItems.Parameters.AddRange(pars.ToArray());

        var items = new List<Contrato>();
        using var r = await cmdItems.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
            items.Add(MapContrato((MySqlDataReader)r));

        return (items, total);
    }


        public async Task<int> CreateAsync(Contrato entity, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            var overlap = await ExistsOverlapAsync(entity.IdInmueble, entity.FechaInicio, entity.FechaFin, entity.Rescision, null, ct);
            if (overlap)
                throw new InvalidOperationException("Ya existe un contrato que se superpone para este inmueble.");

            var sql = @"
                INSERT INTO contratos
                    (fecha_firma, id_inmueble, id_inquilino, fecha_inicio, fecha_fin,
                     monto_mensual, rescision, monto_multa, created_by, created_at, deleted_at, deleted_by)
                VALUES
                    (@FechaFirma, @IdInmueble, @IdInquilino, @FechaInicio, @FechaFin,
                     @MontoMensual, @Rescision, @MontoMulta, @CreatedBy, @CreatedAt, NULL, NULL);
                SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(sql, conn);

            AddParam(cmd, "@FechaFirma", entity.FechaFirma, MySqlDbType.DateTime);
            AddParam(cmd, "@IdInmueble", entity.IdInmueble, MySqlDbType.Int32);
            AddParam(cmd, "@IdInquilino", entity.IdInquilino, MySqlDbType.Int32);
            AddParam(cmd, "@FechaInicio", entity.FechaInicio, MySqlDbType.DateTime);
            AddParam(cmd, "@FechaFin", entity.FechaFin, MySqlDbType.DateTime);
            AddParam(cmd, "@MontoMensual", entity.MontoMensual, MySqlDbType.Decimal);
            AddParam(cmd, "@Rescision", entity.Rescision, MySqlDbType.DateTime);
            AddParam(cmd, "@MontoMulta", entity.MontoMulta, MySqlDbType.Decimal);
            AddParam(cmd, "@CreatedBy", entity.CreatedBy, MySqlDbType.Int32);
            AddParam(cmd, "@CreatedAt", entity.CreatedAt == default ? DateTime.UtcNow : entity.CreatedAt, MySqlDbType.DateTime);

            var id = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
            return id;
        }
        public async Task UpdateAsync(Contrato entity, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            var overlap = await ExistsOverlapAsync(entity.IdInmueble, entity.FechaInicio, entity.FechaFin, entity.Rescision, entity.IdContrato, ct);
            if (overlap)
                throw new InvalidOperationException("La modificación genera un solapamiento con otro contrato del inmueble.");

            var sql = @"
                        UPDATE contratos
                        SET fecha_firma=@FechaFirma,
                            id_inmueble=@IdInmueble,
                            id_inquilino=@IdInquilino,
                            fecha_inicio=@FechaInicio,
                            fecha_fin=@FechaFin,
                            monto_mensual=@MontoMensual,
                            rescision=@Rescision,
                            monto_multa=@MontoMulta
                        WHERE id_contrato=@IdContrato;";

            using var cmd = new MySqlCommand(sql, conn);
            AddParam(cmd, "@FechaFirma", entity.FechaFirma ?? (object)DBNull.Value, MySqlDbType.DateTime);
            AddParam(cmd, "@IdInmueble", entity.IdInmueble, MySqlDbType.Int32);
            AddParam(cmd, "@IdInquilino", entity.IdInquilino, MySqlDbType.Int32);
            AddParam(cmd, "@FechaInicio", entity.FechaInicio, MySqlDbType.DateTime);
            AddParam(cmd, "@FechaFin", entity.FechaFin, MySqlDbType.DateTime);
            AddParam(cmd, "@MontoMensual", entity.MontoMensual, MySqlDbType.Decimal);
            AddParam(cmd, "@Rescision", entity.Rescision ?? (object)DBNull.Value, MySqlDbType.DateTime);
            AddParam(cmd, "@MontoMulta", entity.MontoMulta ?? (object)DBNull.Value, MySqlDbType.Decimal);
            AddParam(cmd, "@IdContrato", entity.IdContrato, MySqlDbType.Int32);


            var filas = await cmd.ExecuteNonQueryAsync(ct);
            if (filas == 0)
            {
                throw new InvalidOperationException("No se actualizó ningún contrato. Verifique el ID.");
            }
        }

        public async Task<bool> SoftDeleteAsync(int id, int deletedBy, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            var sql = @"
                UPDATE contratos
                SET deleted_at = UTC_TIMESTAMP(),
                    deleted_by = @DeletedBy
                WHERE id_contrato = @IdContrato AND deleted_at IS NULL;";

            using var cmd = new MySqlCommand(sql, conn);
            AddParam(cmd, "@DeletedBy", deletedBy, MySqlDbType.Int32);
            AddParam(cmd, "@IdContrato", id, MySqlDbType.Int32);

            var rows = await cmd.ExecuteNonQueryAsync(ct);
            return rows > 0;
        }

        public async Task<bool> ExistsOverlapAsync(
            int idInmueble,
            DateTime fechaInicio,
            DateTime fechaFin,
            DateTime? rescision = null,
            int? excludeContratoId = null,
            CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            var ini = fechaInicio.Date;
            var efFin = (rescision.HasValue && rescision.Value.Date < fechaFin.Date)
                        ? rescision.Value.Date
                        : fechaFin.Date;

            var sql = @"
                SELECT COUNT(*)
                FROM contratos c
                WHERE c.id_inmueble = @IdInmueble
                AND c.deleted_at IS NULL
                /**exclude**/
                AND (
                        DATE(c.fecha_inicio) <= LEAST(DATE(c.fecha_fin), COALESCE(DATE(c.rescision), DATE(c.fecha_fin)))
                        AND @Ini <= LEAST(DATE(c.fecha_fin), COALESCE(DATE(c.rescision), DATE(c.fecha_fin)))
                        AND DATE(c.fecha_inicio) <= @Fin
                );";

            if (excludeContratoId.HasValue)
                sql = sql.Replace("/**exclude**/", "AND c.id_contrato <> @ExcludeId");
            else
                sql = sql.Replace("/**exclude**/", "");

            using var cmd = new MySqlCommand(sql, conn);
            AddParam(cmd, "@IdInmueble", idInmueble, MySqlDbType.Int32);
            AddParam(cmd, "@Ini", ini, MySqlDbType.Date);
            AddParam(cmd, "@Fin", efFin, MySqlDbType.Date);
            if (excludeContratoId.HasValue)
                AddParam(cmd, "@ExcludeId", excludeContratoId.Value, MySqlDbType.Int32);

            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
            return count > 0;
        }

        // --------------- Selects auxiliares (para Controller) ----------------

        public async Task<IReadOnlyList<(int Id, string Direccion)>> GetInmueblesAsync(CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"SELECT id_inmueble AS Id, direccion AS Direccion FROM inmuebles ORDER BY direccion;";
            using var cmd = new MySqlCommand(sql, conn);

            var list = new List<(int, string)>();
            using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
            {
                var id = r.GetInt32("Id");
                var dir = r.GetString("Direccion");
                list.Add((id, dir));
            }
            return list;
        }

        public async Task<IReadOnlyList<(int Id, string Nombre)>> GetInquilinosAsync(CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"SELECT id_inquilino AS Id, apellido_nombres AS Nombre FROM inquilinos ORDER BY apellido_nombres;";
            using var cmd = new MySqlCommand(sql, conn);

            var list = new List<(int, string)>();
            using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
            {
                var id = r.GetInt32("Id");
                var nom = r.GetString("Nombre");
                list.Add((id, nom));
            }
            return list;
        }

        public async Task<IReadOnlyList<ContratoAudit>> GetAuditoriaAsync(int contratoId, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
                SELECT  id_audit      AS IdAudit,
                        id_contrato   AS IdContrato,
                        accion        AS Accion,
                        accion_at     AS AccionAt,
                        accion_by     AS AccionBy,
                        old_data      AS OldData,
                        new_data      AS NewData
                FROM contratos_audit
                WHERE id_contrato = @id
                ORDER BY accion_at DESC, id_audit DESC;";

            using var cmd = new MySql.Data.MySqlClient.MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", contratoId);

            var list = new List<ContratoAudit>();
            using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
            {
                list.Add(new ContratoAudit
                {
                    IdAudit = r.GetInt64("IdAudit"),
                    IdContrato = r.GetInt32("IdContrato"),
                    Accion = r.GetString("Accion"),
                    AccionAt = r.GetDateTime("AccionAt"),
                    AccionBy = r.GetInt32("AccionBy"),
                    OldData = r.IsDBNull(r.GetOrdinal("OldData")) ? null : r.GetString("OldData"),
                    NewData = r.IsDBNull(r.GetOrdinal("NewData")) ? null : r.GetString("NewData"),
                });
            }
            return list;
        }

        public async Task<decimal?> CalcularMultaAsync(int idContrato, DateTime fechaRescision, CancellationToken ct = default)
        {

            Console.WriteLine($"Calculando multa para contrato {idContrato} con fecha {fechaRescision}");
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT fecha_inicio, fecha_fin, monto_mensual
                FROM contratos
                WHERE id_contrato = @id AND deleted_at IS NULL;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.Add(new MySqlParameter("@id", MySqlDbType.Int32) { Value = idContrato });

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
                return null;

            var fechaInicio = reader.GetDateTime("fecha_inicio");
            var fechaFin = reader.GetDateTime("fecha_fin");
            var montoMensual = reader.GetDecimal("monto_mensual");

            if (fechaRescision < fechaInicio || fechaRescision > fechaFin)
                throw new ArgumentException("La fecha de rescisión debe estar dentro del período del contrato.");

            var mitadContrato = fechaInicio.AddDays((fechaFin - fechaInicio).TotalDays / 2);

            decimal multa = fechaRescision < mitadContrato ? montoMensual * 2 : montoMensual * 1;

            return multa;
        }

        public async Task<IReadOnlyList<(int Id, string Direccion, string Inquilino)>>
            GetContratosInfoAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            var idList = ids?.Distinct().Where(x => x > 0).ToList() ?? new List<int>();
            if (idList.Count == 0) return Array.Empty<(int, string, string)>();

            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            var pars = new List<MySqlParameter>();
            var inParams = new List<string>();
            for (int i = 0; i < idList.Count; i++)
            {
                var p = new MySqlParameter($"@id{i}", MySqlDbType.Int32) { Value = idList[i] };
                pars.Add(p);
                inParams.Add(p.ParameterName);
            }

            var sql = $@"
                SELECT  c.id_contrato       AS Id,
                        i.direccion         AS Direccion,
                        q.apellido_nombres  AS Inquilino
                FROM contratos c
                INNER JOIN inmuebles  i ON i.id_inmueble  = c.id_inmueble
                INNER JOIN inquilinos q ON q.id_inquilino = c.id_inquilino
                WHERE c.id_contrato IN ({string.Join(",", inParams)})";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddRange(pars.ToArray());

            var list = new List<(int, string, string)>();
            using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
            {
                list.Add((r.GetInt32("Id"), r.GetString("Direccion"), r.GetString("Inquilino")));
            }
            return list;
        }
        
        public async Task<IReadOnlyList<Contrato>> GetContratosPorInmuebleAsync(
            int idInmueble,
            CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            var sql = @"
                SELECT  c.id_contrato      AS IdContrato,
                        c.fecha_firma      AS FechaFirma,
                        c.id_inmueble      AS IdInmueble,
                        i.direccion        AS InmuebleDireccion,
                        c.id_inquilino     AS IdInquilino,
                        q.apellido_nombres AS InquilinoNombre,
                        q.documento        AS InquilinoDni,
                        c.fecha_inicio     AS FechaInicio,
                        c.fecha_fin        AS FechaFin,
                        c.monto_mensual    AS MontoMensual,
                        c.rescision        AS Rescision,
                        c.monto_multa      AS MontoMulta,
                        c.created_by       AS CreatedBy,
                        c.created_at       AS CreatedAt,
                        c.deleted_at       AS DeletedAt,
                        c.deleted_by       AS DeletedBy
                FROM contratos c
                INNER JOIN inmuebles i ON i.id_inmueble = c.id_inmueble
                INNER JOIN inquilinos q ON q.id_inquilino = c.id_inquilino
                WHERE c.id_inmueble = @IdInmueble
                AND c.deleted_at IS NULL
                ORDER BY c.fecha_inicio DESC;";

            using var cmd = new MySqlCommand(sql, conn);
            AddParam(cmd, "@IdInmueble", idInmueble, MySqlDbType.Int32);

            var list = new List<Contrato>();
            using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
            {
                var contrato = MapContrato((MySqlDataReader)r);

                contrato.Inmueble = new Inmueble
                {
                    IdInmueble = contrato.IdInmueble,
                    Direccion = r.GetString("InmuebleDireccion")
                };

                contrato.Inquilino = new Inquilino
                {
                    IdInquilino = contrato.IdInquilino,
                    ApellidoNombres = r.GetString("InquilinoNombre"),
                    Documento = r.GetString("InquilinoDni")
                };

                list.Add(contrato);
            }

            return list;
        }

        public async Task<bool> ExisteContratoParaInmueble(int idInmueble)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            var sql = "SELECT COUNT(1) FROM contratos WHERE id_inmueble = @IdInmueble AND deleted_at IS NULL";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.Add("@IdInmueble", MySqlDbType.Int32).Value = idInmueble;

            var result = await cmd.ExecuteScalarAsync();
            int count = Convert.ToInt32(result);

            return count > 0;
        }

    }
}
