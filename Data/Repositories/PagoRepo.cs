using System.Data;
using Inmobiliaria10.Models;
using MySql.Data.MySqlClient;
using Inmobiliaria10.Models.ViewModels;

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
            NewData = r["NewData"] as string
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

            const string sql = @"
                INSERT INTO pagos
                    (id_contrato, fecha_pago, id_mes, anio, detalle, id_concepto, importe, motivo_anulacion, created_by, created_at)
                VALUES
                    (@id_contrato, @fecha_pago, @id_mes, @anio, @detalle, @id_concepto, @importe, @motivo, @created_by, UTC_TIMESTAMP());
                SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id_contrato", e.IdContrato);
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
        
        public async Task UpdateAsync(Pago e, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
                UPDATE pagos
                   SET id_contrato     = @id_contrato,
                       fecha_pago      = @fecha_pago,
                       id_mes          = @id_mes,
                       anio            = @anio,
                       detalle         = @detalle,
                       id_concepto     = @id_concepto,
                       importe         = @importe,
                       motivo_anulacion= @motivo
                 WHERE id_pago = @id;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", e.IdPago);
            cmd.Parameters.AddWithValue("@id_contrato", e.IdContrato);
            cmd.Parameters.AddWithValue("@fecha_pago", e.FechaPago);
            cmd.Parameters.AddWithValue("@id_mes", (object?)e.IdMes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@anio", e.Anio);
            cmd.Parameters.AddWithValue("@detalle", e.Detalle ?? string.Empty);
            cmd.Parameters.AddWithValue("@id_concepto", e.IdConcepto);
            cmd.Parameters.AddWithValue("@importe", e.Importe);
            cmd.Parameters.AddWithValue("@motivo", (object?)e.MotivoAnulacion ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task<bool> UpdateConceptoAsync(int idPago, int idConcepto, string detalle, CancellationToken ct)
        {
                using var conn = _db.GetConnection();
                await conn.OpenAsync(ct);

                var sql = @"UPDATE pagos 
                            SET id_concepto = @idConcepto, 
                                detalle = @detalle 
                            WHERE id_pago = @idPago";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@idConcepto", idConcepto);
                cmd.Parameters.AddWithValue("@detalle", detalle ?? "");
                cmd.Parameters.AddWithValue("@idPago", idPago);

                var rows = await cmd.ExecuteNonQueryAsync(ct);
                return rows > 0;
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

        public async Task<bool> AnularPagoAsync(int id, string motivo, int userId, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
                UPDATE pagos
                SET deleted_at = UTC_TIMESTAMP(),
                    deleted_by = @by,
                    motivo_anulacion = @motivo
                WHERE id_pago = @id AND deleted_at IS NULL;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@by", userId);
            cmd.Parameters.AddWithValue("@motivo", motivo);

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
        public async Task<IReadOnlyList<PagoAudit>> GetAuditoriaAsync(int idPago, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
                SELECT  a.id_audit AS IdAudit,
                        a.id_pago  AS IdPago,
                        a.accion   AS Accion,
                        a.accion_at AS AccionAt,
                        a.accion_by AS AccionBy,
                        a.old_data AS OldData,
                        a.new_data AS NewData
                FROM pagos_audit a
                WHERE a.id_pago = @id
                ORDER BY a.id_audit DESC;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", idPago);

            var list = new List<PagoAudit>();
            using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct)) list.Add(MapAudit(r));
            return list;
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
                SELECT  p.id_pago         AS IdPago,
                        p.id_contrato     AS IdContrato,
                        p.numero_pago     AS NumeroPago, 
                        p.fecha_pago      AS FechaPago,
                        p.id_mes          AS IdMes,
                        m.nombre          AS MesTexto,
                        p.anio            AS Anio,
                        p.detalle         AS Detalle,
                        p.id_concepto     AS IdConcepto,
                        p.importe         AS Importe,
                        p.motivo_anulacion AS MotivoAnulacion,
                        p.created_by      AS CreatedBy,
                        u1.alias          AS CreatedByAlias,
                        p.created_at      AS CreatedAt,
                        p.deleted_at      AS DeletedAt,
                        p.deleted_by      AS DeletedBy,
                        u2.alias          AS DeletedByAlias
                FROM pagos p
                LEFT JOIN usuarios u1 ON u1.id_usuario = p.created_by
                LEFT JOIN usuarios u2 ON u2.id_usuario = p.deleted_by
                LEFT JOIN meses m ON m.id_mes = p.id_mes
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
                ConceptoTexto = null, 
                IdContrato = Convert.ToInt32(r["IdContrato"]),
                ContratoTexto = null, 
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

    }
}
