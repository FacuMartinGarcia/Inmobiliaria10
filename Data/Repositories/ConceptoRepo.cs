
using System.Data;
using Inmobiliaria10.Models;
using MySql.Data.MySqlClient;

namespace Inmobiliaria10.Data.Repositories
{
    public class ConceptoRepo : IConceptoRepo
    {
        private readonly Database _db;
        public ConceptoRepo(Database db) => _db = db;

        private static Concepto Map(IDataRecord r) => new Concepto
        {
            IdConcepto = r["IdConcepto"] is DBNull ? 0 : Convert.ToInt32(r["IdConcepto"]),
            DenominacionConcepto = r["DenominacionConcepto"] is DBNull ? "" : Convert.ToString(r["DenominacionConcepto"])!
        };

        public async Task<Concepto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
                SELECT  c.id_concepto AS IdConcepto,
                        c.denominacion_concepto AS DenominacionConcepto
                FROM conceptos c
                WHERE c.id_concepto = @id;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var r = await cmd.ExecuteReaderAsync(ct);
            return await r.ReadAsync(ct) ? Map(r) : null;
        }

        public async Task<(IReadOnlyList<Concepto> Items, int Total)> ListAsync(
            string? buscar = null, int pageIndex = 1, int pageSize = 20, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            var where = " WHERE 1=1 ";
            var pars = new List<MySqlParameter>();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                where += " AND c.denominacion_concepto LIKE @q ";
                pars.Add(new MySqlParameter("@q", $"%{buscar.Trim()}%"));
            }

            // total
            var sqlCount = $"SELECT COUNT(*) FROM conceptos c {where};";
            using var cmdCount = new MySqlCommand(sqlCount, conn);
            cmdCount.Parameters.AddRange(pars.ToArray());
            var total = Convert.ToInt32(await cmdCount.ExecuteScalarAsync(ct));

            // items
            var sqlItems = $@"
                SELECT  c.id_concepto AS IdConcepto,
                        c.denominacion_concepto AS DenominacionConcepto
                FROM conceptos c
                {where}
                ORDER BY c.denominacion_concepto";

            if (pageSize > 0)
            {
                sqlItems += " LIMIT @limit OFFSET @offset;";
                pars.Add(new MySqlParameter("@limit", pageSize));
                pars.Add(new MySqlParameter("@offset", Math.Max(0, (pageIndex - 1) * pageSize)));
            }
            else sqlItems += ";";

            using var cmdItems = new MySqlCommand(sqlItems, conn);
            cmdItems.Parameters.AddRange(pars.ToArray());

            var list = new List<Concepto>();
            using var r = await cmdItems.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct)) list.Add(Map(r));

            return (list, total);
        }

        public async Task<int> CreateAsync(Concepto e, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
                INSERT INTO conceptos (denominacion_concepto)
                VALUES (@denom);
                SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@denom", e.DenominacionConcepto.Trim());

            var id = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
            return id;
        }

        public async Task UpdateAsync(Concepto e, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
                UPDATE conceptos
                SET denominacion_concepto = @denom
                WHERE id_concepto = @id;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@denom", e.DenominacionConcepto.Trim());
            cmd.Parameters.AddWithValue("@id", e.IdConcepto);

            await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"DELETE FROM conceptos WHERE id_concepto = @id;";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            var rows = await cmd.ExecuteNonQueryAsync(ct);
            return rows > 0;
        }

        public async Task<bool> ExistsByNameAsync(string denominacion, int? excludeId = null, CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            var sql = @"SELECT 1 FROM conceptos WHERE denominacion_concepto = @denom";
            if (excludeId is > 0) sql += " AND id_concepto <> @id";
            sql += " LIMIT 1;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@denom", denominacion.Trim());
            if (excludeId is > 0) cmd.Parameters.AddWithValue("@id", excludeId);

            var obj = await cmd.ExecuteScalarAsync(ct);
            return obj is not null;
        }
    }
}
