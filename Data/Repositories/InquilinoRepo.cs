using System.Data;
using MySql.Data.MySqlClient;
using Inmobiliaria10.Models;

namespace Inmobiliaria10.Repositories
{
    public class InquilinoRepo
    {
        private readonly string connectionString;

        public InquilinoRepo(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public int Agregar(Inquilino i)
        {
            int idGenerado = 0;
            using var conn = new MySqlConnection(connectionString);
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
            idGenerado = Convert.ToInt32(cmd.ExecuteScalar());
            conn.Close();

            return idGenerado;
        }

        public int Actualizar(Inquilino i)
        {
            int filas = 0;
            using var conn = new MySqlConnection(connectionString);
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
            filas = cmd.ExecuteNonQuery();
            conn.Close();

            return filas;
        }

        public List<Inquilino> ListarTodos()
        {
            var lista = new List<Inquilino>();
            using var conn = new MySqlConnection(connectionString);
            var sql = "SELECT * FROM inquilinos";
            using var cmd = new MySqlCommand(sql, conn);

            conn.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var i = new Inquilino
                {
                    IdInquilino = reader.GetInt32("id_inquilino"),
                    Documento = reader.GetString("documento"),
                    ApellidoNombres = reader.GetString("apellido_nombres"),
                    Domicilio = reader.GetString("domicilio"),
                    Telefono = reader.IsDBNull("telefono") ? null : reader.GetString("telefono"),
                    Email = reader.IsDBNull("email") ? null : reader.GetString("email")
                };
                lista.Add(i);
            }
            conn.Close();
            return lista;
        }

        public Inquilino? ObtenerPorId(int id)
        {
            Inquilino? i = null;
            using var conn = new MySqlConnection(connectionString);
            var sql = "SELECT * FROM inquilinos WHERE id_inquilino = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            conn.Open();
            var reader = cmd.ExecuteReader();
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
            conn.Close();
            return i;
        }
        public Inquilino? ObtenerPorDocumento(string documento)
        {
            Inquilino? i = null;
            using var conn = new MySqlConnection(connectionString);
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
                    Telefono = reader.GetString("telefono"),
                    Email = reader.GetString("email")
                };
            }
            conn.Close();
            return i;
        }
    }
}
