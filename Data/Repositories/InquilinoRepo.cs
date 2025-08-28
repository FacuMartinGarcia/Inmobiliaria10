using System.Data;
using MySql.Data.MySqlClient;
using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories
{
    public class InquilinoRepo : RepositorioBase, IInquilinoRepo
    {
        // el constructor recibe IConfiguration y lo pasa a la base
        public InquilinoRepo(IConfiguration cfg) : base(cfg) { }

        public int Agregar(Inquilino i)
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

            conn.Open();
            int idGenerado = Convert.ToInt32(cmd.ExecuteScalar());
            return idGenerado;
        }

        public int Actualizar(Inquilino i)
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

            conn.Open();
            int filas = cmd.ExecuteNonQuery();
            return filas;
        }

        public List<Inquilino> ListarTodos()
        {
            var lista = new List<Inquilino>();
            using var conn = Conn();
            var sql = "SELECT * FROM inquilinos";
            using var cmd = new MySqlCommand(sql, conn);

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
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

        public (List<Inquilino> registros, int totalRegistros) ListarTodosPaginado(int pagina, int cantidadPorPagina)
        {
            var lista = new List<Inquilino>();
            int totalRegistros = 0;
            int offset = (pagina - 1) * cantidadPorPagina;

            using var conn = Conn();
            conn.Open();

            // obtener registros paginados
            var sql = @"SELECT * FROM inquilinos 
                        ORDER BY apellido_nombres 
                        LIMIT @cantidad OFFSET @offset";

            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@cantidad", cantidadPorPagina);
                cmd.Parameters.AddWithValue("@offset", offset);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
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
                reader.Close();
            }

            // contar total
            var sqlTotal = "SELECT COUNT(*) FROM inquilinos";
            using (var cmdTotal = new MySqlCommand(sqlTotal, conn))
            {
                totalRegistros = Convert.ToInt32(cmdTotal.ExecuteScalar());
            }

            return (lista, totalRegistros);
        }

        public Inquilino? ObtenerPorId(int id)
        {
            Inquilino? i = null;
            using var conn = Conn();
            var sql = "SELECT * FROM inquilinos WHERE id_inquilino = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            conn.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
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

        public Inquilino? ObtenerPorDocumento(string documento)
        {
            Inquilino? i = null;
            using var conn = Conn();
            var sql = @"SELECT id_inquilino, documento, apellido_nombres, domicilio, telefono, email 
                        FROM inquilinos 
                        WHERE documento = @doc";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@doc", documento);

            conn.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
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

        public int Borrar(int id)
        {
            using var conn = Conn();
            var sql = "DELETE FROM inquilinos WHERE id_inquilino = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            conn.Open();
            int filas = cmd.ExecuteNonQuery();
            return filas;
        }
    }
}
