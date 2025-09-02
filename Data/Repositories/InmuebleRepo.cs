using System.Data;
using MySql.Data.MySqlClient;
using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories
{
    public class InmuebleRepo: RepositorioBase, IInmuebleRepo
    {
        public InmuebleRepo(IConfiguration cfg) : base(cfg) { }

        public int Agregar(Inmueble i)
        {
            using var conn = Conn();
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
            int idGenerado = Convert.ToInt32(cmd.ExecuteScalar());
            return idGenerado;
        }

        public int Actualizar(Inmueble i)
        {
            int filas = 0;
            using var conn = Conn();
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
            return filas;
        }

        public List<Inmueble> ListarTodos()
        {
            var lista = new List<Inmueble>();
            using var conn = Conn();

            var sql = @"
                SELECT i.*,
                    u.id_uso AS uso_id_uso, u.denominacion_uso AS denominacion_uso,
                    t.id_tipo AS tipo_id_tipo, t.denominacion_tipo AS denominacion_tipo,
                    p.id_propietario, p.documento, p.apellido_nombres, p.domicilio, 
                    p.telefono, p.email
                FROM inmuebles i
                LEFT JOIN inmuebles_usos u ON i.id_uso = u.id_uso
                LEFT JOIN inmuebles_tipos t ON i.id_tipo = t.id_tipo
                LEFT JOIN propietarios p ON i.id_propietario = p.id_propietario
            ";

            using var cmd = new MySqlCommand(sql, conn);
            conn.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(Map(reader));
            }
            return lista;
        }


        public (List<Inmueble> registros, int totalRegistros) ListarTodosPaginado(
            int pagina, int cantidadPorPagina, string? searchString = null)
        {
            var lista = new List<Inmueble>();
            using var conn = Conn();

            int offset = (pagina - 1) * cantidadPorPagina;

            string where = "";
            if (!string.IsNullOrEmpty(searchString))
            {
                where = @"WHERE i.direccion LIKE @search 
                        OR i.ambientes LIKE @search
                        OR t.denominacion_tipo LIKE @search 
                        OR p.apellido_nombres LIKE @search";
            }

            var sql = $@"
                SELECT i.*,
                    u.id_uso AS uso_id_uso, u.denominacion_uso AS denominacion_uso,
                    t.id_tipo AS tipo_id_tipo, t.denominacion_tipo AS denominacion_tipo,
                    p.id_propietario, p.documento, p.apellido_nombres, p.domicilio, 
                    p.telefono, p.email
                FROM inmuebles i
                LEFT JOIN inmuebles_usos u ON i.id_uso = u.id_uso
                LEFT JOIN inmuebles_tipos t ON i.id_tipo = t.id_tipo
                LEFT JOIN propietarios p ON i.id_propietario = p.id_propietario
                {where}
                ORDER BY i.direccion
                LIMIT @cantidad OFFSET @offset
            ";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@cantidad", cantidadPorPagina);
            cmd.Parameters.AddWithValue("@offset", offset);

            if (!string.IsNullOrEmpty(searchString))
                cmd.Parameters.AddWithValue("@search", "%" + searchString + "%");

            conn.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(Map(reader));
            }
            reader.Close();

            var sqlTotal = $@"
                SELECT COUNT(*) 
                FROM inmuebles i
                LEFT JOIN inmuebles_tipos t ON i.id_tipo = t.id_tipo
                LEFT JOIN propietarios p ON i.id_propietario = p.id_propietario
                {where}";

            using var cmdTotal = new MySqlCommand(sqlTotal, conn);

            if (!string.IsNullOrEmpty(searchString))
                cmdTotal.Parameters.AddWithValue("@search", "%" + searchString + "%");

            int totalRegistros = Convert.ToInt32(cmdTotal.ExecuteScalar());

            return (lista, totalRegistros);
        }

        public Inmueble? ObtenerPorId(int id)
        {
            Inmueble? i = null;
            using var conn = Conn();

            var sql = @"
                SELECT i.*,
                    u.id_uso AS uso_id_uso, u.denominacion_uso AS denominacion_uso,
                    t.id_tipo AS tipo_id_tipo, t.denominacion_tipo AS denominacion_tipo,
                    p.id_propietario, p.documento, p.apellido_nombres, p.domicilio, 
                    p.telefono, p.email
                FROM inmuebles i
                LEFT JOIN inmuebles_usos u ON i.id_uso = u.id_uso
                LEFT JOIN inmuebles_tipos t ON i.id_tipo = t.id_tipo
                LEFT JOIN propietarios p ON i.id_propietario = p.id_propietario
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
            return i;
        }


        public int Eliminar(int id)
        {
            int filas = 0;
            using var conn = Conn();
            var sql = "DELETE FROM inmuebles WHERE id_inmueble = @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            conn.Open();
            filas = cmd.ExecuteNonQuery();
            return filas;
        }

        private Inmueble Map(MySqlDataReader reader)
        {
            return new Inmueble
            {
                IdInmueble = reader.GetInt32("id_inmueble"),
                IdPropietario = reader.GetInt32("id_propietario"),
                Propietario = new Propietario
                {
                    IdPropietario = reader.GetInt32("id_propietario"),
                    Documento = reader.GetString("documento"),
                    ApellidoNombres = reader.GetString("apellido_nombres"),
                    Domicilio = reader.GetString("domicilio"),
                    Telefono = reader.IsDBNull(reader.GetOrdinal("telefono")) ? null : reader.GetString("telefono"),
                    Email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString("email")
                },
                IdUso = reader.GetInt32("id_uso"),
                Uso = new InmuebleUso
                {
                    IdUso = reader.GetInt32("uso_id_uso"),
                    DenominacionUso = reader.GetString("denominacion_uso")
                },
                IdTipo = reader.GetInt32("id_tipo"),
                Tipo = new InmuebleTipo
                {
                    IdTipo = reader.GetInt32("tipo_id_tipo"),
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
