using System.Data;
using Inmobiliaria10.Models;
using MySql.Data.MySqlClient;
using Inmobiliaria10.Models.ViewModels;
using System.Text.Json;

namespace Inmobiliaria10.Data.Repositories
{
    public class PagoRepo : IPagoRepo
    {
        private readonly Database _db;
        public PagoRepo(Database db) => _db = db;

        private const string ONLY_ACTIVE_WHERE = @"
            c.deleted_at IS NULL
            AND IFNULL(c.activo, 1) = 1
            AND (c.fecha_inicio IS NULL OR c.fecha_inicio <= CURDATE())
            AND (c.fecha_fin IS NULL OR c.fecha_fin >= CURDATE())
        ";

        // -----------------------------
        // Mappers
        // -----------------------------
        // Helpers para leer DBNull de forma segura
        private static int? IntN(IDataRecord r, string n) => r[n] is DBNull ? (int?)null : Convert.ToInt32(r[n]);
        private static DateTime? DateN(IDataRecord r, string n) => r[n] is DBNull ? (DateTime?)null : Convert.ToDateTime(r[n]);
        private static string? StrN(IDataRecord r, string n) => r[n] is DBNull ? null : Convert.ToString(r[n]);

        private static Pago MapPago(IDataRecord r) => new()
        {
            IdPago = Convert.ToInt32(r["IdPago"]),
            IdContrato = Convert.ToInt32(r["IdContrato"]),
            FechaPago = Convert.ToDateTime(r["FechaPago"]),
            IdMes = IntN(r, "IdMes"),
            Anio = Convert.ToInt32(r["Anio"]),
            Detalle = StrN(r, "Detalle") ?? string.Empty,
            IdConcepto = Convert.ToInt32(r["IdConcepto"]),
            Importe = Convert.ToDecimal(r["Importe"]),
            MotivoAnulacion = StrN(r, "MotivoAnulacion"),
            CreatedBy = IntN(r, "CreatedBy"),
            CreatedAt = DateN(r, "CreatedAt"),
            DeletedAt = DateN(r, "DeletedAt"),
            DeletedBy = IntN(r, "DeletedBy"),
            NumeroPago = Convert.ToInt32(r["NumeroPago"])
        };

        private static PagoAudit MapAudit(IDataRecord r) => new PagoAudit
        {
            IdAudit = Convert.ToInt32(r["IdAudit"]),
            IdPago = Convert.ToInt32(r["IdPago"]),
            Accion = r["Accion"].ToString()!,
            AccionAt = Convert.ToDateTime(r["AccionAt"]),
            AccionBy = r["AccionBy"] is DBNull ? null : (int?)Convert.ToInt32(r["AccionBy"]),
            OldData = r["OldData"] as string,
            NewData = r["NewData"] as string,
            UsuarioAlias = r["UsuarioAlias"] as string
        };

        // -----------------------------
        // CRUD
        // -----------------------------
        public async Task<Pago?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
                SELECT  p.id_pago         AS IdPago,
                        p.numero_pago     AS NumeroPago,
                        p.id_contrato     AS IdContrato,
                        p.fecha_pago      AS FechaPago,
                        p.id_mes          AS IdMes,
                        p.anio            AS Anio,
                        p.detalle         AS Detalle,
                        p.id_concepto     AS IdConcepto,
                        p.importe         AS Importe,
                        p.motivo_anulacion AS MotivoAnulacion,
                        p.created_by      AS CreatedBy,
                        p.created_at      AS CreatedAt,
                        p.deleted_at      AS DeletedAt,
                        p.deleted_by      AS DeletedBy
                FROM pagos p
                WHERE p.id_pago = @id
                LIMIT 1;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var r = await cmd.ExecuteReaderAsync(ct);
            if (await r.ReadAsync(ct)) return MapPago(r);
            return null;
        }

        public async Task<(IReadOnlyList<Pago> Items, int Total)> ListAsync(
            int? idContrato = null,
            int? idConcepto = null,
            bool? soloActivos = null,
            int pageIndex = 1,
            int pageSize = 20,
            CancellationToken ct = default,
            int? idInquilino = null)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            var pars = new List<MySqlParameter>();
            var where = " WHERE 1=1 ";
            var from = " FROM pagos p ";

            if (idInquilino is > 0)
            {
                from += " JOIN contratos c ON c.id_contrato = p.id_contrato ";
                where += " AND c.id_inquilino = @idInquilino ";
                pars.Add(new MySqlParameter("@idInquilino", idInquilino));
            }

            if (idContrato is > 0) { where += " AND p.id_contrato = @idContrato "; pars.Add(new MySqlParameter("@idContrato", idContrato)); }
            if (idConcepto is > 0) { where += " AND p.id_concepto = @idConcepto "; pars.Add(new MySqlParameter("@idConcepto", idConcepto)); }

            if (soloActivos == true) where += " AND p.deleted_at IS NULL ";
            else if (soloActivos == false) where += " AND p.deleted_at IS NOT NULL ";

            // Total
            var sqlCount = $"SELECT COUNT(*) {from} {where};";
            using var cmdCount = new MySqlCommand(sqlCount, conn);
            cmdCount.Parameters.AddRange(pars.ToArray());
            var total = Convert.ToInt32(await cmdCount.ExecuteScalarAsync(ct));

            // Items
            var sqlItems = $@"
                SELECT  p.id_pago         AS IdPago,
                        p.numero_pago     AS NumeroPago,
                        p.id_contrato     AS IdContrato,
                        p.fecha_pago      AS FechaPago,
                        p.id_mes          AS IdMes,
                        p.anio            AS Anio,
                        p.detalle         AS Detalle,
                        p.id_concepto     AS IdConcepto,
                        p.importe         AS Importe,
                        p.motivo_anulacion AS MotivoAnulacion,
                        p.created_by      AS CreatedBy,
                        p.created_at      AS CreatedAt,
                        p.deleted_at      AS DeletedAt,
                        p.deleted_by      AS DeletedBy
                {from}
                {where}
                ORDER BY p.fecha_pago DESC, p.id_pago DESC";

            if (pageSize > 0)
            {
                sqlItems += " LIMIT @limit OFFSET @offset;";
                pars.Add(new MySqlParameter("@limit", pageSize));
                pars.Add(new MySqlParameter("@offset", Math.Max(0, (pageIndex - 1) * pageSize)));
            }
            else sqlItems += ";";

            using var cmdItems = new MySqlCommand(sqlItems, conn);
            cmdItems.Parameters.AddRange(pars.ToArray());

            var list = new List<Pago>();
            using var r = await cmdItems.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct)) list.Add(MapPago(r));

            return (list, total);
        }

        public async Task<int> CreateAsync(Pago e, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            // Calcular número de pago por contrato
            const string sqlNum = @"SELECT IFNULL(MAX(numero_pago), 0) + 1
                                    FROM pagos
                                    WHERE id_contrato = @idContrato";

            using (var cmdNum = new MySqlCommand(sqlNum, conn))
            {
                cmdNum.Parameters.AddWithValue("@idContrato", e.IdContrato);
                e.NumeroPago = Convert.ToInt32(await cmdNum.ExecuteScalarAsync(ct));
            }

            // Insertar con numero_pago incluido
            const string sql = @"
                INSERT INTO pagos
                    (id_contrato, numero_pago, fecha_pago, id_mes, anio, detalle, id_concepto, importe, motivo_anulacion, created_by, created_at)
                VALUES
                    (@id_contrato, @numero_pago, @fecha_pago, @id_mes, @anio, @detalle, @id_concepto, @importe, @motivo, @created_by, UTC_TIMESTAMP());
                SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id_contrato", e.IdContrato);
            cmd.Parameters.AddWithValue("@numero_pago", e.NumeroPago);
            cmd.Parameters.AddWithValue("@fecha_pago", e.FechaPago);
            cmd.Parameters.AddWithValue("@id_mes", (object?)e.IdMes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@anio", e.Anio);
            cmd.Parameters.AddWithValue("@detalle", e.Detalle ?? string.Empty);
            cmd.Parameters.AddWithValue("@id_concepto", e.IdConcepto);
            cmd.Parameters.AddWithValue("@importe", e.Importe);
            cmd.Parameters.AddWithValue("@motivo", (object?)e.MotivoAnulacion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@created_by", (object?)e.CreatedBy ?? DBNull.Value);

            var id = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
            return id;
        }
        public async Task<bool> UpdateConceptoAsync(Pago e, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();

            await conn.OpenAsync(ct);

            const string sql = @"
                UPDATE pagos
                SET id_concepto = @idConcepto,
                    detalle = @detalle,
                    motivo_anulacion = @motivo
                WHERE id_pago = @idPago;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idPago", e.IdPago);
            cmd.Parameters.AddWithValue("@idConcepto", e.IdConcepto);
            cmd.Parameters.AddWithValue("@detalle", e.Detalle ?? "");
            cmd.Parameters.AddWithValue("@motivo", (object?)e.MotivoAnulacion ?? DBNull.Value);

            return await cmd.ExecuteNonQueryAsync(ct) > 0;
        }

        public async Task<int> RegistrarMultaAsync(int contratoId, DateTime fecha, int userId, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            // 1. Traemos info del contrato (inicio, fin, monto mensual)
            const string sqlContrato = @"
                SELECT fecha_inicio, fecha_fin, monto
                FROM contratos
                WHERE id_contrato = @id;";

            DateTime inicio, fin;
            decimal montoMensual;

            using (var cmd = new MySqlCommand(sqlContrato, conn))
            {
                cmd.Parameters.AddWithValue("@id", contratoId);
                using var r = await cmd.ExecuteReaderAsync(ct);
                if (!await r.ReadAsync(ct)) throw new InvalidOperationException("Contrato no encontrado.");

                inicio = Convert.ToDateTime(r["fecha_inicio"]);
                fin = Convert.ToDateTime(r["fecha_fin"]);
                montoMensual = Convert.ToDecimal(r["monto"]);
            }

            // 2. Calcular la mitad de duración
            var duracion = (fin - inicio).TotalDays;
            var mitad = inicio.AddDays(duracion / 2);

            // 3. Determinar multa
            decimal importeMulta = fecha < mitad
                ? montoMensual * 2   // antes de la mitad → 2 meses
                : montoMensual;      // después de la mitad → 1 mes

            // 4. Generar número de pago secuencial
            var numeroPago = await GetNextNumeroPagoAsync(contratoId, ct);

            // 5. Insertar el pago de multa
            const string sqlInsert = @"
                INSERT INTO pagos
                    (id_contrato, numero_pago, fecha_pago, detalle, id_concepto, importe, created_by, created_at)
                VALUES
                    (@id_contrato, @numero_pago, @fecha_pago, @detalle, @id_concepto, @importe, @created_by, UTC_TIMESTAMP());
                SELECT LAST_INSERT_ID();";

            using var cmdInsert = new MySqlCommand(sqlInsert, conn);
            cmdInsert.Parameters.AddWithValue("@id_contrato", contratoId);
            cmdInsert.Parameters.AddWithValue("@numero_pago", numeroPago);
            cmdInsert.Parameters.AddWithValue("@fecha_pago", fecha);
            cmdInsert.Parameters.AddWithValue("@detalle", "Multa por rescisión anticipada");
            cmdInsert.Parameters.AddWithValue("@id_concepto", /* id del concepto "Multa" */ 3);
            cmdInsert.Parameters.AddWithValue("@importe", importeMulta);
            cmdInsert.Parameters.AddWithValue("@created_by", userId);

            var idPago = Convert.ToInt32(await cmdInsert.ExecuteScalarAsync(ct));
            return idPago;
        }

        public async Task<int> GetNextNumeroPagoAsync(int idContrato, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"SELECT IFNULL(MAX(numero_pago), 0) + 1
                                FROM pagos
                                WHERE id_contrato = @idContrato;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idContrato", idContrato);

            var result = await cmd.ExecuteScalarAsync(ct);
            return Convert.ToInt32(result);
        }

        public async Task<bool> AnularPagoAsync(int id, string motivo, int? userId, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
                UPDATE pagos
                SET motivo_anulacion = @motivo, 
                    deleted_at = NOW(),
                    deleted_by = @userId
                WHERE id_pago = @id;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@motivo", motivo);
            cmd.Parameters.AddWithValue("@userId", (object?)userId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id", id);

            var rows = await cmd.ExecuteNonQueryAsync(ct);
            return rows > 0;
        }

        public async Task<bool> SoftDeleteAsync(int id, int deletedBy, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
                UPDATE pagos
                   SET deleted_at = UTC_TIMESTAMP(),
                       deleted_by = @by
                 WHERE id_pago = @id AND deleted_at IS NULL;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@by", deletedBy);

            var rows = await cmd.ExecuteNonQueryAsync(ct);
            return rows > 0;
        }

        // -----------------------------
        // Conceptos
        // -----------------------------
        public async Task<IReadOnlyList<(int Id, string Nombre)>> GetConceptosAsync(CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
                SELECT c.id_concepto AS Id, c.denominacion_concepto AS Nombre
                FROM conceptos c
                ORDER BY c.denominacion_concepto;";

            using var cmd = new MySqlCommand(sql, conn);

            var list = new List<(int, string)>();
            using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
                list.Add((Convert.ToInt32(r["Id"]), Convert.ToString(r["Nombre"])!));
            return list;
        }

        // -----------------------------
        // Auditoría 
        // -----------------------------
        public async Task<IReadOnlyList<PagoAuditViewModel>> GetAuditoriaGeneralAsync(CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
                SELECT  a.id_audit   AS IdAudit,
                        a.id_pago    AS IdPago,
                        a.accion     AS Accion,
                        a.accion_at  AS AccionAt,
                        u.alias      AS Usuario,
                        a.old_data   AS OldData,
                        a.new_data   AS NewData
                FROM pagos_audit a
                LEFT JOIN usuarios u ON u.id_usuario = a.accion_by
                ORDER BY a.id_audit DESC;";

            using var cmd = new MySqlCommand(sql, conn);

            var list = new List<PagoAuditViewModel>();
            using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
            {
                var vm = new PagoAuditViewModel
                {
                    IdAudit = Convert.ToInt32(r["IdAudit"]),
                    IdPago = Convert.ToInt32(r["IdPago"]),
                    Accion = r["Accion"].ToString()!,
                    AccionAt = Convert.ToDateTime(r["AccionAt"]),
                    Usuario = r["Usuario"]?.ToString() ?? "(sistema)"
                };

                vm.OldData = await ParseAndTranslateAsync(r["OldData"] as string, ct);
                vm.NewData = await ParseAndTranslateAsync(r["NewData"] as string, ct);


                list.Add(vm);
            }
            return list;
        }

        public async Task<IReadOnlyList<PagoAuditViewModel>> GetAuditoriaAsync(int idPago, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
                SELECT  a.id_audit   AS IdAudit,
                        a.id_pago    AS IdPago,
                        a.accion     AS Accion,
                        a.accion_at  AS AccionAt,
                        u.alias      AS Usuario,
                        a.old_data   AS OldData,
                        a.new_data   AS NewData
                FROM pagos_audit a
                LEFT JOIN usuarios u ON u.id_usuario = a.accion_by
                WHERE a.id_pago = @id
                ORDER BY a.id_audit DESC;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", idPago);

            // 1. Leer todo del DataReader y guardarlo en memoria
            var tempList = new List<(int IdAudit, int IdPago, string Accion, DateTime AccionAt, string Usuario, string? OldData, string? NewData)>();
            using (var r = await cmd.ExecuteReaderAsync(ct))
            {
                while (await r.ReadAsync(ct))
                {
                    tempList.Add((
                        Convert.ToInt32(r["IdAudit"]),
                        Convert.ToInt32(r["IdPago"]),
                        r["Accion"].ToString()!,
                        Convert.ToDateTime(r["AccionAt"]),
                        r["Usuario"]?.ToString() ?? "(sistema)",
                        r["OldData"] as string,
                        r["NewData"] as string
                    ));
                }
            } // 🔹 Ahora se cierra el DataReader

            // 2. Traducir después de haber cerrado el DataReader
            var list = new List<PagoAuditViewModel>();
            foreach (var item in tempList)
            {
                var vm = new PagoAuditViewModel
                {
                    IdAudit = item.IdAudit,
                    IdPago = item.IdPago,
                    Accion = item.Accion,
                    AccionAt = item.AccionAt,
                    Usuario = item.Usuario,
                    OldData = await ParseAndTranslateAsync(item.OldData, ct),
                    NewData = await ParseAndTranslateAsync(item.NewData, ct)
                };

                list.Add(vm);
            }

            return list;
        }
        public async Task<(IReadOnlyList<PagoAuditViewModel> Items, int Total)> ListAuditoriaAsync(
            int? usuarioId = null,
            int? contratoId = null,
            int? conceptoId = null,
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

            if (contratoId is > 0)
            {
                where += " AND p.id_contrato = @contratoId ";
                pars.Add(new MySqlParameter("@contratoId", contratoId));
            }

            if (conceptoId is > 0)
            {
                where += " AND p.id_concepto = @conceptoId ";
                pars.Add(new MySqlParameter("@conceptoId", conceptoId));
            }

            // --- Total
            var sqlCount = $@"
                SELECT COUNT(*)
                FROM pagos_audit a
                INNER JOIN pagos p ON p.id_pago = a.id_pago
                {where};";

            using var cmdCount = new MySqlCommand(sqlCount, conn);
            cmdCount.Parameters.AddRange(pars.ToArray());
            var total = Convert.ToInt32(await cmdCount.ExecuteScalarAsync(ct));

            // --- Items con paginado
            var sqlItems = $@"
                SELECT  a.id_audit   AS IdAudit,
                        a.id_pago    AS IdPago,
                        a.accion     AS Accion,
                        a.accion_at  AS AccionAt,
                        u.alias      AS Usuario,
                        a.old_data   AS OldData,
                        a.new_data   AS NewData
                FROM pagos_audit a
                INNER JOIN pagos p ON p.id_pago = a.id_pago
                LEFT JOIN usuarios u ON u.id_usuario = a.accion_by
                {where}
                ORDER BY a.id_audit DESC
                LIMIT @limit OFFSET @offset;";

            pars.Add(new MySqlParameter("@limit", pageSize));
            pars.Add(new MySqlParameter("@offset", (pageIndex - 1) * pageSize));

            using var cmdItems = new MySqlCommand(sqlItems, conn);
            cmdItems.Parameters.AddRange(pars.ToArray());

            var list = new List<PagoAuditViewModel>();
            using var r = await cmdItems.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
            {
                var vm = new PagoAuditViewModel
                {
                    IdAudit = Convert.ToInt32(r["IdAudit"]),
                    IdPago = Convert.ToInt32(r["IdPago"]),
                    Accion = r["Accion"].ToString()!,
                    AccionAt = Convert.ToDateTime(r["AccionAt"]),
                    Usuario = r["Usuario"]?.ToString() ?? "(sistema)"
                };

                vm.OldData = await ParseAndTranslateAsync(r["OldData"] as string, ct);
                vm.NewData = await ParseAndTranslateAsync(r["NewData"] as string, ct);

                list.Add(vm);
            }

            return (list, total);
        }

        private async Task<Dictionary<string, string>> ParseAndTranslateAsync(string? json, CancellationToken ct = default)
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
                    case "id_contrato":
                        if (int.TryParse(value, out var idContrato))
                            value = await GetContratoTextoAsync(idContrato, ct);
                        break;

                    case "id_concepto":
                        if (int.TryParse(value, out var idConcepto))
                            value = await GetConceptoTextoAsync(idConcepto, ct);
                        break;

                    case "id_mes":
                        if (int.TryParse(value, out var idMes))
                            value = await GetMesTextoAsync(idMes, ct);
                        break;

                    case "fecha_pago":
                        if (DateTime.TryParse(value, out var fecha))
                            value = fecha.ToString("dd/MM/yyyy");
                        break;

                    case "importe":
                        if (decimal.TryParse(value, out var importe))
                            value = importe.ToString("C"); // formato moneda
                        break;
                }

                dict[key] = value ?? string.Empty;
            }

            return dict;
        }
        private async Task<string> GetContratoTextoAsync(int id, CancellationToken ct)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"SELECT CONCAT('C#', c.id_contrato, ' - ', IFNULL(i.direccion,'(sin dirección)'), 
                                ' · ', IFNULL(inq.apellido_nombres,'')) 
                                FROM contratos c
                                LEFT JOIN inmuebles i ON i.id_inmueble = c.id_inmueble
                                LEFT JOIN inquilinos inq ON inq.id_inquilino = c.id_inquilino
                                WHERE c.id_contrato = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            var res = await cmd.ExecuteScalarAsync(ct);
            return res?.ToString() ?? $"Contrato #{id}";
        }

        private async Task<string> GetConceptoTextoAsync(int id, CancellationToken ct)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"SELECT denominacion_concepto FROM conceptos WHERE id_concepto = @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            var res = await cmd.ExecuteScalarAsync(ct);
            return res?.ToString() ?? $"Concepto #{id}";
        }

        private async Task<string> GetMesTextoAsync(int id, CancellationToken ct)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"SELECT nombre FROM meses WHERE id_mes = @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            var res = await cmd.ExecuteScalarAsync(ct);
            return res?.ToString() ?? $"Mes #{id}";
        }


        // -----------------------------
        // Select2 — Inquilinos
        // -----------------------------
        public async Task<IReadOnlyList<(int Id, string Text)>> SearchInquilinosAsync(string? term, int take, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
              SELECT  i.id_inquilino AS Id,
                      CONCAT(i.apellido_nombres, ' (', i.documento, ')') AS Text
              FROM inquilinos i
              WHERE (@q IS NULL OR @q = '' OR i.apellido_nombres LIKE @q OR i.documento LIKE @q)
              ORDER BY i.apellido_nombres
              LIMIT @take;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@q", $"%{(term ?? "").Trim()}%");
            cmd.Parameters.AddWithValue("@take", take);

            var list = new List<(int, string)>();
            using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
                list.Add((Convert.ToInt32(r["Id"]), Convert.ToString(r["Text"])!));
            return list;
        }

        public async Task<(int Id, string Text)?> GetInquilinoItemAsync(int id, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
              SELECT  i.id_inquilino AS Id,
                      CONCAT(i.apellido_nombres, ' (', i.documento, ')') AS Text
              FROM inquilinos i
              WHERE i.id_inquilino = @id
              LIMIT 1;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var r = await cmd.ExecuteReaderAsync(ct);
            if (await r.ReadAsync(ct))
                return (Convert.ToInt32(r["Id"]), Convert.ToString(r["Text"])!);
            return null;
        }
        // -----------------------------
        // Select2 — Contratos por Inquilino (solo vigentes/activos)
        // -----------------------------
        // Contratos por Inquilino (para el Select2 dependiente en Crear/Editar Pago)
        public async Task<IReadOnlyList<(int Id, string Text)>> SearchContratosPorInquilinoAsync(
            int idInquilino, string? term, int take, bool soloVigentesActivos = true, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            var where = " WHERE c.id_inquilino = @inq ";
            if (soloVigentesActivos)
                where += $" AND ({ONLY_ACTIVE_WHERE}) ";

            if (!string.IsNullOrWhiteSpace(term))
                where += " AND (CAST(c.id_contrato AS CHAR) LIKE @q OR im.direccion LIKE @q) ";

            var sql = $@"
            SELECT  c.id_contrato AS Id,
                    CONCAT('C#', c.id_contrato, ' - ', IFNULL(im.direccion,'(sin dirección)'),
                            ' · ', DATE_FORMAT(c.fecha_inicio, '%d/%m/%Y'), ' → ', DATE_FORMAT(c.fecha_fin, '%d/%m/%Y')) AS Text
            FROM contratos c
            LEFT JOIN inmuebles im ON im.id_inmueble = c.id_inmueble
            {where}
            ORDER BY c.id_contrato DESC
            LIMIT @take;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@inq", idInquilino);
            cmd.Parameters.AddWithValue("@take", take);
            if (!string.IsNullOrWhiteSpace(term))
                cmd.Parameters.AddWithValue("@q", $"%{term!.Trim()}%");

            var list = new List<(int, string)>();
            using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
                list.Add((Convert.ToInt32(r["Id"]), Convert.ToString(r["Text"])!));
            return list;
        }

        // Búsqueda genérica de contratos (también con inmueble en el texto)
        public async Task<IReadOnlyList<(int Id, string Text)>> SearchContratosAsync(string? term, int take, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
            SELECT  c.id_contrato AS Id,
                    CONCAT('C#', c.id_contrato, ' - ', IFNULL(im.direccion,'(sin dirección)'),
                            ' · ', IFNULL(i.apellido_nombres,'')) AS Text
            FROM contratos c
            LEFT JOIN inmuebles im ON im.id_inmueble = c.id_inmueble
            LEFT JOIN inquilinos i ON i.id_inquilino = c.id_inquilino
            WHERE (@q IS NULL OR @q = ''
                    OR CAST(c.id_contrato AS CHAR) LIKE @q
                    OR im.direccion LIKE @q
                    OR i.apellido_nombres LIKE @q
                    OR i.documento LIKE @q)
            ORDER BY c.id_contrato DESC
            LIMIT @take;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@q", $"%{(term ?? "").Trim()}%");
            cmd.Parameters.AddWithValue("@take", take);

            var list = new List<(int, string)>();
            using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
                list.Add((Convert.ToInt32(r["Id"]), Convert.ToString(r["Text"])!));
            return list;
        }

        // Item de contrato por Id (con inmueble)
        public async Task<(int Id, string Text)?> GetContratoItemAsync(int id, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
            SELECT  c.id_contrato AS Id,
                    CONCAT('C#', c.id_contrato, ' - ', IFNULL(im.direccion,'(sin dirección)'),
                            ' · ', IFNULL(i.apellido_nombres,'')) AS Text
            FROM contratos c
            LEFT JOIN inmuebles im ON im.id_inmueble = c.id_inmueble
            LEFT JOIN inquilinos i ON i.id_inquilino = c.id_inquilino
            WHERE c.id_contrato = @id
            LIMIT 1;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var r = await cmd.ExecuteReaderAsync(ct);
            if (await r.ReadAsync(ct))
                return (Convert.ToInt32(r["Id"]), Convert.ToString(r["Text"])!);
            return null;
        }

        public async Task<PagoDetalleViewModel?> GetDetalleAsync(int id, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
                SELECT  
                    p.id_pago          AS IdPago,
                    p.id_contrato      AS IdContrato,
                    CONCAT(i.direccion, ' · ', inq.apellido_nombres) AS ContratoTexto,
                    p.numero_pago      AS NumeroPago, 
                    p.fecha_pago       AS FechaPago,
                    p.id_mes           AS IdMes,
                    m.nombre           AS MesTexto,
                    p.anio             AS Anio,
                    p.detalle          AS Detalle,
                    p.id_concepto      AS IdConcepto,
                    c.denominacion_concepto AS ConceptoTexto,
                    p.importe          AS Importe,
                    p.motivo_anulacion AS MotivoAnulacion,
                    p.created_by       AS CreatedBy,
                    u1.alias           AS CreatedByAlias,
                    p.created_at       AS CreatedAt,
                    p.deleted_at       AS DeletedAt,
                    p.deleted_by       AS DeletedBy,
                    u2.alias           AS DeletedByAlias
                FROM pagos p
                LEFT JOIN usuarios u1    ON u1.id_usuario = p.created_by
                LEFT JOIN usuarios u2    ON u2.id_usuario = p.deleted_by
                LEFT JOIN meses m        ON m.id_mes = p.id_mes
                LEFT JOIN conceptos c    ON c.id_concepto = p.id_concepto
                LEFT JOIN contratos co   ON co.id_contrato = p.id_contrato
                LEFT JOIN inmuebles i    ON i.id_inmueble = co.id_inmueble
                LEFT JOIN inquilinos inq ON inq.id_inquilino = co.id_inquilino
                WHERE p.id_pago = @id
                LIMIT 1;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var r = await cmd.ExecuteReaderAsync(ct);
            if (!await r.ReadAsync(ct)) return null;

            return new PagoDetalleViewModel
            {
                IdPago = Convert.ToInt32(r["IdPago"]),
                FechaPago = Convert.ToDateTime(r["FechaPago"]),
                IdMes = IntN(r, "IdMes"),
                MesTexto = r["MesTexto"] as string,
                Anio = Convert.ToInt32(r["Anio"]),
                Detalle = r["Detalle"] as string,
                Importe = Convert.ToDecimal(r["Importe"]),
                IdConcepto = Convert.ToInt32(r["IdConcepto"]),
                ConceptoTexto = r["ConceptoTexto"] as string,
                IdContrato = Convert.ToInt32(r["IdContrato"]),
                ContratoTexto = r["ContratoTexto"] as string,
                NumeroPago = Convert.ToInt32(r["NumeroPago"]),
                CreatedAt = DateN(r, "CreatedAt"),
                CreatedBy = IntN(r, "CreatedBy"),
                CreatedByAlias = r["CreatedByAlias"] as string,
                DeletedAt = DateN(r, "DeletedAt"),
                DeletedBy = IntN(r, "DeletedBy"),
                DeletedByAlias = r["DeletedByAlias"] as string,
                MotivoAnulacion = r["MotivoAnulacion"] as string
            };
        }
        public async Task<(IReadOnlyList<MorosoViewModel> Items, int Total)> GetMorososAsync(
            int? inquilinoId = null,
            int pageIndex = 1,
            int pageSize = 10,
            CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            var pars = new List<MySqlParameter>();
            if (inquilinoId.HasValue)
                pars.Add(new MySqlParameter("@inq", inquilinoId.Value));

            pars.Add(new MySqlParameter("@limit", pageSize));
            pars.Add(new MySqlParameter("@offset", (pageIndex - 1) * pageSize));

            // ------------------------
            // COUNT (total de registros)
            // ------------------------
            var sqlCount = @"
                SELECT COUNT(*)
                FROM contratos c
                INNER JOIN inquilinos i ON i.id_inquilino = c.id_inquilino
                INNER JOIN inmuebles im ON im.id_inmueble = c.id_inmueble
                -- Generar últimos 5 años (se puede ampliar)
                JOIN (
                    SELECT YEAR(CURDATE()) as anio
                    UNION SELECT YEAR(CURDATE())-1
                    UNION SELECT YEAR(CURDATE())-2
                    UNION SELECT YEAR(CURDATE())-3
                    UNION SELECT YEAR(CURDATE())-4
                    UNION SELECT YEAR(CURDATE())-5
                ) y
                JOIN meses m ON 1=1
                WHERE STR_TO_DATE(CONCAT(y.anio,'-',m.id_mes,'-01'),'%Y-%m-%d')
                    BETWEEN c.fecha_inicio AND c.fecha_fin
                AND NOT EXISTS (
                    SELECT 1
                    FROM pagos p
                    WHERE p.id_contrato = c.id_contrato
                        AND p.anio = y.anio
                        AND p.id_mes = m.id_mes
                        AND p.id_concepto = 1   
                        AND p.deleted_at IS NULL
                )
                AND (
                    y.anio < YEAR(CURDATE())
                    OR (y.anio = YEAR(CURDATE()) AND m.id_mes < MONTH(CURDATE()))
                    OR (y.anio = YEAR(CURDATE()) AND m.id_mes = MONTH(CURDATE()) AND DAY(CURDATE()) > 10)
                )
                " + (inquilinoId.HasValue ? " AND i.id_inquilino = @inq" : "");

            using var cmdCount = new MySqlCommand(sqlCount, conn);
            cmdCount.Parameters.AddRange(pars.Where(p => p.ParameterName == "@inq").ToArray());
            var total = Convert.ToInt32(await cmdCount.ExecuteScalarAsync(ct));

            // ------------------------
            // ITEMS (con paginado)
            // ------------------------
            var sqlItems = @"
                SELECT 
                    i.id_inquilino       AS IdInquilino,
                    i.apellido_nombres   AS Inquilino,
                    i.telefono           AS Telefono,
                    CONCAT(im.direccion,
                        IFNULL(CONCAT(' Piso ', im.piso), ''),
                        IFNULL(CONCAT(' Depto ', im.depto), '')
                    ) AS Inmueble,
                    c.id_contrato        AS IdContrato,
                    c.fecha_inicio       AS FechaInicio,
                    c.fecha_fin          AS FechaFin,
                    m.id_mes             AS IdMes,
                    m.nombre             AS MesAdeudado,
                    y.anio               AS AnioAdeudado,
                    c.monto_mensual      AS MontoMensual
                FROM contratos c
                INNER JOIN inquilinos i ON i.id_inquilino = c.id_inquilino
                INNER JOIN inmuebles im ON im.id_inmueble = c.id_inmueble
                JOIN (
                    SELECT YEAR(CURDATE()) as anio
                    UNION SELECT YEAR(CURDATE())-1
                    UNION SELECT YEAR(CURDATE())-2
                    UNION SELECT YEAR(CURDATE())-3
                    UNION SELECT YEAR(CURDATE())-4
                    UNION SELECT YEAR(CURDATE())-5
                ) y
                JOIN meses m ON 1=1
                WHERE STR_TO_DATE(CONCAT(y.anio,'-',m.id_mes,'-01'),'%Y-%m-%d')
                    BETWEEN c.fecha_inicio AND c.fecha_fin
                AND NOT EXISTS (
                    SELECT 1
                    FROM pagos p
                    WHERE p.id_contrato = c.id_contrato
                        AND p.anio = y.anio
                        AND p.id_mes = m.id_mes
                        AND p.id_concepto = 1
                        AND p.deleted_at IS NULL
                )
                AND (
                    y.anio < YEAR(CURDATE())
                    OR (y.anio = YEAR(CURDATE()) AND m.id_mes < MONTH(CURDATE()))
                    OR (y.anio = YEAR(CURDATE()) AND m.id_mes = MONTH(CURDATE()) AND DAY(CURDATE()) > 10)
                )
                " + (inquilinoId.HasValue ? " AND i.id_inquilino = @inq" : "") + @"
                ORDER BY Inquilino, AnioAdeudado, IdMes
                LIMIT @limit OFFSET @offset;";

            using var cmd = new MySqlCommand(sqlItems, conn);
            cmd.Parameters.AddRange(pars.ToArray());

            var list = new List<MorosoViewModel>();
            using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
            {
                list.Add(new MorosoViewModel
                {
                    IdInquilino = Convert.ToInt32(r["IdInquilino"]),
                    Inquilino = r["Inquilino"].ToString()!,
                    Telefono = r["Telefono"]?.ToString(),
                    Inmueble = r["Inmueble"].ToString()!,
                    IdContrato = Convert.ToInt32(r["IdContrato"]),
                    FechaInicio = Convert.ToDateTime(r["FechaInicio"]),
                    FechaFin = Convert.ToDateTime(r["FechaFin"]),
                    IdMes = Convert.ToInt32(r["IdMes"]),
                    MesAdeudado = r["MesAdeudado"].ToString()!,
                    AnioAdeudado = Convert.ToInt32(r["AnioAdeudado"]),
                    MontoMensual = Convert.ToDecimal(r["MontoMensual"])
                });
            }

            return (list, total);
        }
        
        public async Task<IReadOnlyList<(int Id, string Text)>> SearchUsuariosAsync(string? term, int take, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
                SELECT u.id_usuario AS Id,
                    u.alias AS Text
                FROM usuarios u
                WHERE (@q IS NULL OR @q = '' OR u.alias LIKE @q)
                ORDER BY u.alias
                LIMIT @take;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@q", $"%{(term ?? "").Trim()}%");
            cmd.Parameters.AddWithValue("@take", take);

            var list = new List<(int, string)>();
            using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
                list.Add((Convert.ToInt32(r["Id"]), Convert.ToString(r["Text"])!));
            return list;
        }

    }
}
