using MySql.Data.MySqlClient;
using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories
{
    public class MesRepo : IMesRepo
    {
        private readonly Database _db;
        public MesRepo(Database db) => _db = db;

        public async Task<IReadOnlyList<Mes>> GetAllAsync(CancellationToken ct = default)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync(ct);

            const string sql = @"SELECT id_mes AS IdMes, nombre AS Nombre
                                 FROM meses
                                 ORDER BY id_mes;";

            using var cmd = new MySqlCommand(sql, conn);
            var list = new List<Mes>();

            using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
                list.Add(new Mes
                {
                    IdMes = Convert.ToInt32(r["IdMes"]),
                    Nombre = Convert.ToString(r["Nombre"])!
                });

            return list; // nunca null
        }
    }
}
