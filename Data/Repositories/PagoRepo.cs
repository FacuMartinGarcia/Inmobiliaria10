using System.Text.Json;
using Inmobiliaria10.Models;
using MySql.Data.MySqlClient;
using System.Data; 

namespace Inmobiliaria10.Data.Repositories
{
    public class PagoRepo : IPagoRepo
    {
        private readonly Database _db;
        public PagoRepo(Database db) => _db = db;

        // ------------------ Mapping helper ------------------
        private static Pago Map(IDataRecord r) => new Pago
        {
            IdPago          = r["IdPago"]          is DBNull ? 0  : Convert.ToInt32(r["IdPago"]),
            IdContrato      = r["IdContrato"]      is DBNull ? 0  : Convert.ToInt32(r["IdContrato"]),
            FechaPago       = r["FechaPago"]       is DBNull ? DateTime.MinValue : Convert.ToDateTime(r["FechaPago"]),
            Detalle         = r["Detalle"]         is DBNull ? null : Convert.ToString(r["Detalle"]),
            IdConcepto      = r["IdConcepto"]      is DBNull ? 0  : Convert.ToInt32(r["IdConcepto"]),
            Importe         = r["Importe"]         is DBNull ? 0m : Convert.ToDecimal(r["Importe"]),
            MotivoAnulacion = r["MotivoAnulacion"] is DBNull ? null : Convert.ToString(r["MotivoAnulacion"]),
            CreatedBy       = r["CreatedBy"]       is DBNull ? 0  : Convert.ToInt32(r["CreatedBy"]),
            CreatedAt       = r["CreatedAt"]       is DBNull ? DateTime.MinValue : Convert.ToDateTime(r["CreatedAt"]),
            DeletedAt       = r["DeletedAt"]       is DBNull ? (DateTime?)null : Convert.ToDateTime(r["DeletedAt"]),
            DeletedBy       = r["DeletedBy"]       is DBNull ? (int?)null : Convert.ToInt32(r["DeletedBy"]),
        };

        // ------------------ CRUD ------------------

        public async Task<Pago?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
                SELECT  p.id_pago         AS IdPago,
                        p.id_contrato     AS IdContrato,
                        p.fecha_pago      AS FechaPago,
                        p.detalle         AS Detalle,
                        p.id_concepto     AS IdConcepto,
                        p.importe         AS Importe,
                        p.motivo_anulacion AS MotivoAnulacion,
                        p.created_by      AS CreatedBy,
                        p.created_at      AS CreatedAt,
                        p.deleted_at      AS DeletedAt,
                        p.deleted_by      AS DeletedBy
                FROM pagos p
                WHERE p.id_pago = @id;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var r = await cmd.ExecuteReaderAsync(ct);
            return await r.ReadAsync(ct) ? Map(r) : null;
        }

        public async Task<(IReadOnlyList<Pago> Items, int Total)> ListAsync(
            int? idContrato = null,
            int? idConcepto = null,
            bool? soloActivos = null,
            int pageIndex = 1,
            int pageSize = 20,
            CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            var where = " WHERE 1=1 ";
            var pars = new List<MySqlParameter>();

            if (idContrato is > 0)
            {
                where += " AND p.id_contrato = @idContrato ";
                pars.Add(new MySqlParameter("@idContrato", idContrato));
            }
            if (idConcepto is > 0)
            {
                where += " AND p.id_concepto = @idConcepto ";
                pars.Add(new MySqlParameter("@idConcepto", idConcepto));
            }
            if (soloActivos == true) where += " AND p.deleted_at IS NULL ";
            else if (soloActivos == false) where += " AND p.deleted_at IS NOT NULL ";

            // Total
            var sqlCount = $"SELECT COUNT(*) FROM pagos p {where};";
            using var cmdCount = new MySqlCommand(sqlCount, conn);
            cmdCount.Parameters.AddRange(pars.ToArray());
            var total = Convert.ToInt32(await cmdCount.ExecuteScalarAsync(ct));

            // Items
            var sqlItems = $@"
                SELECT  p.id_pago         AS IdPago,
                        p.id_contrato     AS IdContrato,
                        p.fecha_pago      AS FechaPago,
                        p.detalle         AS Detalle,
                        p.id_concepto     AS IdConcepto,
                        p.importe         AS Importe,
                        p.motivo_anulacion AS MotivoAnulacion,
                        p.created_by      AS CreatedBy,
                        p.created_at      AS CreatedAt,
                        p.deleted_at      AS DeletedAt,
                        p.deleted_by      AS DeletedBy
                FROM pagos p
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

            var items = new List<Pago>();
            using var r = await cmdItems.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct)) items.Add(Map(r));

            return (items, total);
        }

        public async Task<int> CreateAsync(Pago e, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
                INSERT INTO pagos
                    (id_contrato, fecha_pago, detalle, id_concepto, importe,
                     motivo_anulacion, created_by, created_at, deleted_at, deleted_by)
                VALUES
                    (@IdContrato, @FechaPago, @Detalle, @IdConcepto, @Importe,
                     @Motivo, @CreatedBy, @CreatedAt, NULL, NULL);
                SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@IdContrato", e.IdContrato);
            cmd.Parameters.Add("@FechaPago", MySqlDbType.Date).Value = e.FechaPago.Date; // columna DATE
            cmd.Parameters.AddWithValue("@Detalle", (object?)e.Detalle ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IdConcepto", e.IdConcepto);
            cmd.Parameters.Add("@Importe", MySqlDbType.Decimal).Value = e.Importe;
            cmd.Parameters.AddWithValue("@Motivo", (object?)e.MotivoAnulacion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedBy", e.CreatedBy);
            cmd.Parameters.Add("@CreatedAt", MySqlDbType.DateTime).Value = e.CreatedAt == default ? DateTime.UtcNow : e.CreatedAt;

            var id = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));

            await InsertAuditAsync(conn, null, id, "CREATE", e.CreatedBy, null, JsonSerializer.Serialize(e), ct);
            return id;
        }

        public async Task UpdateAsync(Pago e, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            // snapshot previo
            var old = await GetByIdAsync(e.IdPago, ct);
            var oldJson = old is null ? null : JsonSerializer.Serialize(old);

            const string sql = @"
                UPDATE pagos
                SET id_contrato=@IdContrato,
                    fecha_pago=@FechaPago,
                    detalle=@Detalle,
                    id_concepto=@IdConcepto,
                    importe=@Importe,
                    motivo_anulacion=@Motivo
                WHERE id_pago=@IdPago;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@IdPago", e.IdPago);
            cmd.Parameters.AddWithValue("@IdContrato", e.IdContrato);
            cmd.Parameters.Add("@FechaPago", MySqlDbType.Date).Value = e.FechaPago.Date;
            cmd.Parameters.AddWithValue("@Detalle", (object?)e.Detalle ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IdConcepto", e.IdConcepto);
            cmd.Parameters.Add("@Importe", MySqlDbType.Decimal).Value = e.Importe;
            cmd.Parameters.AddWithValue("@Motivo", (object?)e.MotivoAnulacion ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync(ct);

            // No tenés "modified_by" en la tabla; uso CreatedBy como actor.
            var actor = e.CreatedBy > 0 ? e.CreatedBy : 1;
            await InsertAuditAsync(conn, null, e.IdPago, "UPDATE", actor, oldJson, JsonSerializer.Serialize(e), ct);
        }

        public async Task<bool> SoftDeleteAsync(int id, int deletedBy, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            var old = await GetByIdAsync(id, ct);
            var oldJson = old is null ? null : JsonSerializer.Serialize(old);

            const string sql = @"
                UPDATE pagos
                SET deleted_at = UTC_TIMESTAMP(),
                    deleted_by = @By
                WHERE id_pago=@Id AND deleted_at IS NULL;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@By", deletedBy);
            cmd.Parameters.AddWithValue("@Id", id);
            var rows = await cmd.ExecuteNonQueryAsync(ct);

            if (rows > 0)
                await InsertAuditAsync(conn, null, id, "DELETE", deletedBy, oldJson, null, ct);

            return rows > 0;
        }

        // ------------------ Soporte selects ------------------

        public async Task<IReadOnlyList<(int Id, string Nombre)>> GetConceptosAsync(CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
                SELECT id_concepto AS Id, denominacion_concepto AS Nombre
                FROM conceptos
                ORDER BY denominacion_concepto;";

            using var cmd = new MySqlCommand(sql, conn);
            var list = new List<(int, string)>();
            using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
            {
                var id  = r["Id"]     is DBNull ? 0 : Convert.ToInt32(r["Id"]);
                var nom = r["Nombre"] is DBNull ? "" : Convert.ToString(r["Nombre"])!;
                list.Add((id, nom));
            }
            return list;
        }

        // ------------------ Auditoría ------------------

        public async Task<IReadOnlyList<PagoAudit>> GetAuditoriaAsync(int idPago, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
                SELECT id_audit AS IdAudit,
                       id_pago  AS IdPago,
                       accion   AS Accion,
                       accion_at AS AccionAt,
                       accion_by AS AccionBy,
                       old_data  AS OldData,
                       new_data  AS NewData
                FROM pagos_audit
                WHERE id_pago=@id
                ORDER BY accion_at DESC, id_audit DESC;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", idPago);

            var list = new List<PagoAudit>();
            using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
            {
                list.Add(new PagoAudit
                {
                    IdAudit  = r["IdAudit"]  is DBNull ? 0L : Convert.ToInt64(r["IdAudit"]),
                    IdPago   = r["IdPago"]   is DBNull ? 0  : Convert.ToInt32(r["IdPago"]),
                    Accion   = r["Accion"]   is DBNull ? "" : Convert.ToString(r["Accion"])!,
                    AccionAt = r["AccionAt"] is DBNull ? DateTime.MinValue : Convert.ToDateTime(r["AccionAt"]),
                    AccionBy = r["AccionBy"] is DBNull ? 0  : Convert.ToInt32(r["AccionBy"]),
                    OldData  = r["OldData"]  is DBNull ? null : Convert.ToString(r["OldData"]),
                    NewData  = r["NewData"]  is DBNull ? null : Convert.ToString(r["NewData"]),
                });
            }


            return list;
        }

        private static async Task InsertAuditAsync(
            MySqlConnection conn,
            MySqlTransaction? tx,
            int idPago,
            string accion,
            int by,
            string? oldJson,
            string? newJson,
            CancellationToken ct)
        {
            const string sql = @"
                INSERT INTO pagos_audit
                    (id_pago, accion, accion_at, accion_by, old_data, new_data)
                VALUES
                    (@Id, @Accion, UTC_TIMESTAMP(), @By, @Old, @New);";

            using var cmd = new MySqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("@Id", idPago);
            cmd.Parameters.AddWithValue("@Accion", accion);
            cmd.Parameters.AddWithValue("@By", by);
            cmd.Parameters.AddWithValue("@Old", (object?)oldJson ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@New", (object?)newJson ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync(ct);
        }
    }
}
