using System.Data;
using MySql.Data.MySqlClient;
using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories
{
    public class InquilinoRepo : IInquilinoRepo
    {
        private readonly Database _db;

        public InquilinoRepo(Database db)
        {
            _db = db;
        }

        private MySqlConnection Conn()
        {
            return _db.GetConnection();
        }

        public async Task<int> AgregarAsync(Inquilino i, CancellationToken ct = default)
        {
            using var conn = Conn();
            var sql = @"INSERT INTO inquilinos (documento, apellido_nombres, domicilio, telefono, email)
                        VALUES (@doc, @apeNom, @dom, @tel, @mail);
                        SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@doc", i.Documento);
            cmd.Parameters.AddWithValue("@apeNom", i.ApellidoNombres);
            cmd.Parameters.AddWithValue("@dom", i.Domicilio);
            cmd.Parameters.AddWithValue("@tel", i.Telefono);
            cmd.Parameters.AddWithValue("@mail", i.Email);

            await conn.OpenAsync(ct);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
        }

        public async Task<int> ActualizarAsync(Inquilino i, CancellationToken ct = default)
        {
            using var conn = Conn();
            var sql = @"UPDATE inquilinos SET
                            documento = @doc,
                            apellido_nombres = @apeNom,
                            domicilio = @dom,
                            telefono = @tel,
                            email = @mail
                        WHERE id_inquilino = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@doc", i.Documento);
            cmd.Parameters.AddWithValue("@apeNom", i.ApellidoNombres);
            cmd.Parameters.AddWithValue("@dom", i.Domicilio);
            cmd.Parameters.AddWithValue("@tel", i.Telefono);
            cmd.Parameters.AddWithValue("@mail", i.Email);
            cmd.Parameters.AddWithValue("@id", i.IdInquilino);

            await conn.OpenAsync(ct);
            return await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task<List<Inquilino>> ListarTodosAsync(CancellationToken ct = default)
        {
            var lista = new List<Inquilino>();
            using var conn = Conn();
            var sql = "SELECT * FROM inquilinos";
            using var cmd = new MySqlCommand(sql, conn);

            await conn.OpenAsync(ct);
            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                lista.Add(new Inquilino
                {
                    IdInquilino = reader.GetInt32("id_inquilino"),
                    Documento = reader.GetString("documento"),
                    ApellidoNombres = reader.GetString("apellido_nombres"),
                    Domicilio = reader.GetString("domicilio"),
                    Telefono = reader.IsDBNull("telefono") ? null : reader.GetString("telefono"),
                    Email = reader.IsDBNull("email") ? null : reader.GetString("email")
                });
            }
            return lista;
        }

        public async Task<(List<Inquilino> registros, int totalRegistros)> ListarTodosPaginadoAsync(
            int pagina, int cantidadPorPagina, string? searchString = null, CancellationToken ct = default)
        {
            var lista = new List<Inquilino>();
            int offset = (pagina - 1) * cantidadPorPagina;
            int totalRegistros = 0;

            using var conn = Conn();
            await conn.OpenAsync(ct);

            string where = "";
            if (!string.IsNullOrEmpty(searchString))
            {
                where = @"WHERE apellido_nombres LIKE @search 
                        OR documento LIKE @search
                        OR telefono LIKE @search"; 
            }

            var sql = $@"SELECT * FROM inquilinos 
                        {where}
                        ORDER BY apellido_nombres 
                        LIMIT @cantidad OFFSET @offset";

            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@cantidad", cantidadPorPagina);
                cmd.Parameters.AddWithValue("@offset", offset);

                if (!string.IsNullOrEmpty(searchString))
                    cmd.Parameters.AddWithValue("@search", "%" + searchString + "%");

                using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    lista.Add(new Inquilino
                    {
                        IdInquilino = reader.GetInt32("id_inquilino"),
                        Documento = reader.GetString("documento"),
                        ApellidoNombres = reader.GetString("apellido_nombres"),
                        Domicilio = reader.GetString("domicilio"),
                        Telefono = reader.IsDBNull("telefono") ? null : reader.GetString("telefono"),
                        Email = reader.IsDBNull("email") ? null : reader.GetString("email")
                    });
                }
                await reader.CloseAsync();
            }

            var sqlTotal = "SELECT COUNT(*) FROM inquilinos";
            using (var cmdTotal = new MySqlCommand(sqlTotal, conn))
            {
                totalRegistros = Convert.ToInt32(await cmdTotal.ExecuteScalarAsync(ct));
            }

            return (lista, totalRegistros);
        }

        public async Task<Inquilino?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        {
            Inquilino? i = null;
            using var conn = Conn();
            var sql = "SELECT * FROM inquilinos WHERE id_inquilino = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync(ct);
            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                i = new Inquilino
                {
                    IdInquilino = reader.GetInt32("id_inquilino"),
                    Documento = reader.GetString("documento"),
                    ApellidoNombres = reader.GetString("apellido_nombres"),
                    Domicilio = reader.GetString("domicilio"),
                    Telefono = reader.IsDBNull("telefono") ? null : reader.GetString("telefono"),
                    Email = reader.IsDBNull("email") ? null : reader.GetString("email")
                };
            }
            return i;
        }

        public async Task<Inquilino?> ObtenerPorDocumentoAsync(string documento, CancellationToken ct = default)
        {
            Inquilino? i = null;
            using var conn = Conn();
            var sql = @"SELECT id_inquilino, documento, apellido_nombres, domicilio, telefono, email 
                        FROM inquilinos 
                        WHERE documento = @doc";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@doc", documento);

            await conn.OpenAsync(ct);
            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                i = new Inquilino
                {
                    IdInquilino = reader.GetInt32("id_inquilino"),
                    Documento = reader.GetString("documento"),
                    ApellidoNombres = reader.GetString("apellido_nombres"),
                    Domicilio = reader.GetString("domicilio"),
                    Telefono = reader.IsDBNull("telefono") ? null : reader.GetString("telefono"),
                    Email = reader.IsDBNull("email") ? null : reader.GetString("email")
                };
            }
            return i;
        }

        public async Task<int> BorrarAsync(int id, CancellationToken ct = default)
        {
            using var conn = Conn();
            var sql = "DELETE FROM inquilinos WHERE id_inquilino = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync(ct);
            return await cmd.ExecuteNonQueryAsync(ct);
        }
    }
}
