using System.Data;
using MySql.Data.MySqlClient;
using Inmobiliaria10.Models;

namespace Inmobiliaria10.Repositories
{
    public class InmuebleRepo
    {
        private readonly string connectionString;

        public InmuebleRepo(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public int Agregar(Inmueble i)
        {
            int idGenerado = 0;
            using var conn = new MySqlConnection(connectionString);
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

            conn.Open();
            idGenerado = Convert.ToInt32(cmd.ExecuteScalar());
            conn.Close();

            return idGenerado;
        }

        public int Actualizar(Inmueble i)
        {
            int filas = 0;
            using var conn = new MySqlConnection(connectionString);
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
            cmd.Parameters.AddWithValue("@updatedAt", DateTime.Now);
            cmd.Parameters.AddWithValue("@id", i.IdInmueble);

            conn.Open();
            filas = cmd.ExecuteNonQuery();
            conn.Close();

            return filas;
        }

        public List<Inmueble> ListarTodos()
        {
            var lista = new List<Inmueble>();
            using var conn = new MySqlConnection(connectionString);

            var sql = @"
                SELECT i.*, 
                    u.id_uso AS uso_id_uso, u.nombre AS uso_nombre,
                    t.id_tipo AS tipo_id_tipo, t.nombre AS tipo_nombre
                FROM inmuebles i
                LEFT JOIN inmueble_usos u ON i.id_uso = u.id_uso
                LEFT JOIN inmueble_tipos t ON i.id_tipo = t.id_tipo
            ";

            using var cmd = new MySqlCommand(sql, conn);
            conn.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(Map(reader));
            }
            conn.Close();
            return lista;
        }


        public (List<Inmueble> registros, int totalRegistros) ListarTodosPaginado(
            int pagina, int cantidadPorPagina)
        {
            var lista = new List<Inmueble>();
            using var conn = new MySqlConnection(connectionString);

            int offset = (pagina - 1) * cantidadPorPagina;

            var sql = @"
                SELECT i.*, 
                    u.id_uso AS uso_id_uso, u.nombre AS uso_nombre,
                    t.id_tipo AS tipo_id_tipo, t.nombre AS tipo_nombre
                FROM inmuebles i
                LEFT JOIN inmueble_usos u ON i.id_uso = u.id_uso
                LEFT JOIN inmueble_tipos t ON i.id_tipo = t.id_tipo
                ORDER BY i.direccion
                LIMIT @cantidad OFFSET @offset
            ";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@cantidad", cantidadPorPagina);
            cmd.Parameters.AddWithValue("@offset", offset);

            conn.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(Map(reader)); 
            }
            reader.Close();

            var sqlTotal = "SELECT COUNT(*) FROM inmuebles";
            using var cmdTotal = new MySqlCommand(sqlTotal, conn);
            int totalRegistros = Convert.ToInt32(cmdTotal.ExecuteScalar());

            conn.Close();
            return (lista, totalRegistros);
        }

        public Inmueble? ObtenerPorId(int id)
        {
            Inmueble? i = null;
            using var conn = new MySqlConnection(connectionString);

            var sql = @"
                SELECT i.*, 
                    u.id_uso AS uso_id_uso, u.nombre AS uso_nombre,
                    t.id_tipo AS tipo_id_tipo, t.nombre AS tipo_nombre
                FROM inmuebles i
                LEFT JOIN inmueble_usos u ON i.id_uso = u.id_uso
                LEFT JOIN inmueble_tipos t ON i.id_tipo = t.id_tipo
                WHERE i.id_inmueble = @id
            ";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            conn.Open();
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                i = Map(reader);
            }
            conn.Close();
            return i;
        }


        public int Borrar(int id)
        {
            int filas = 0;
            using var conn = new MySqlConnection(connectionString);
            var sql = "DELETE FROM inmuebles WHERE id_inmueble = @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            conn.Open();
            filas = cmd.ExecuteNonQuery();
            conn.Close();

            return filas;
        }

        private Inmueble Map(MySqlDataReader reader)
        {
            return new Inmueble
            {
                IdInmueble = reader.GetInt32("id_inmueble"),
                IdPropietario = reader.GetInt32("id_propietario"),
                IdUso = reader.GetInt32("id_uso"),
                Uso = new InmuebleUso
                {
                    IdUso = reader.GetInt32("id_uso"),
                    DenominacionUso= reader.GetString("denominacion_uso")
                },
                IdTipo = reader.GetInt32("id_tipo"),
                Tipo = new InmuebleTipo
                {
                    IdTipo = reader.GetInt32("id_tipo"),
                    DenominacionTipo = reader.GetString("denominacion_tipo")
                },
                Direccion = reader.GetString("direccion"),
                Piso = reader.IsDBNull("piso") ? null : reader.GetString("piso"),
                Depto = reader.IsDBNull("depto") ? null : reader.GetString("depto"),
                Lat = reader.IsDBNull("lat") ? null : reader.GetDecimal("lat"),
                Lon = reader.IsDBNull("lon") ? null : reader.GetDecimal("lon"),
                Ambientes = reader.IsDBNull("ambientes") ? null : reader.GetInt32("ambientes"),
                Precio = reader.IsDBNull("precio") ? null : reader.GetDecimal("precio"),
                Activo = reader.GetBoolean("activo"),
                CreatedAt = reader.GetDateTime("created_at"),
                UpdatedAt = reader.GetDateTime("updated_at")
            };
        }

    }
}
