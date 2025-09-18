using Inmobiliaria10.Models;
using MySql.Data.MySqlClient;

namespace Inmobiliaria10.Data.Repositories
{
    public class ImagenRepo : IImagenRepo
    {
        private readonly Database _db;

        public ImagenRepo(Database db)
        {
            _db = db;
        }

        public async Task<int> Alta(Imagen p)
        {
            using var conn = _db.GetConnection();
            string sql = @"INSERT INTO Imagenes (id_inmueble, ruta) 
                        VALUES (@idInmueble, @ruta)";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idInmueble", p.IdInmueble);
            cmd.Parameters.AddWithValue("@ruta", p.Url);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> Baja(int id)
        {
            using var conn = _db.GetConnection();
            string sql = "DELETE FROM Imagenes WHERE id_imagen = @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> Modificacion(Imagen p)
        {
            using var conn = _db.GetConnection();
            string sql = @"UPDATE Imagenes 
                        SET ruta=@ruta, id_inmueble=@idInmueble
                        WHERE id_imagen=@id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", p.IdImagen);
            cmd.Parameters.AddWithValue("@idInmueble", p.IdInmueble);
            cmd.Parameters.AddWithValue("@ruta", p.Url);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<Imagen?> ObtenerPorId(int id)
        {
            Imagen? res = null;
            using var conn = _db.GetConnection();
            string sql = @"SELECT id_imagen, id_inmueble, ruta
                        FROM Imagenes
                        WHERE id_imagen=@id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                res = new Imagen
                {
                    IdImagen = reader.GetInt32(reader.GetOrdinal("id_imagen")),
                    IdInmueble = reader.GetInt32(reader.GetOrdinal("id_inmueble")),
                    Url = reader.GetString(reader.GetOrdinal("ruta"))
                };
            }
            return res;
        }

        public async Task<IList<Imagen>> ObtenerTodos()
        {
            var res = new List<Imagen>();
            using var conn = _db.GetConnection();
            string sql = @"SELECT id_imagen, id_inmueble, ruta FROM Imagenes";
            using var cmd = new MySqlCommand(sql, conn);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                res.Add(new Imagen
                {
                    IdImagen = reader.GetInt32(reader.GetOrdinal("id_imagen")),
                    IdInmueble = reader.GetInt32(reader.GetOrdinal("id_inmueble")),
                    Url = reader.GetString(reader.GetOrdinal("ruta"))
                });
            }
            return res;
        }

        public async Task<IList<Imagen>> BuscarPorInmueble(int idInmueble)
        {
            var res = new List<Imagen>();
            using var conn = _db.GetConnection();
            string sql = @"SELECT id_imagen, id_inmueble, ruta 
                        FROM Imagenes
                        WHERE id_inmueble=@idInmueble";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idInmueble", idInmueble);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                res.Add(new Imagen
                {
                    IdImagen = reader.GetInt32(reader.GetOrdinal("id_imagen")),
                    IdInmueble = reader.GetInt32(reader.GetOrdinal("id_inmueble")),
                    Url = reader.GetString(reader.GetOrdinal("ruta"))
                });
            }
            return res;
        }
        public async Task<int> AltaPerfil(int idUsuario, string ruta)
        {
            using var conn = _db.GetConnection();
            string sql = @"INSERT INTO Imagenes (id_usuario, ruta) VALUES (@idUsuario, @ruta)";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
            cmd.Parameters.AddWithValue("@ruta", ruta);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IList<Imagen>> BuscarPorUsuario(int idUsuario)
        {
            var res = new List<Imagen>();
            using var conn = _db.GetConnection();
            string sql = @"SELECT id_imagen, id_usuario, ruta
                        FROM Imagenes
                        WHERE id_usuario=@idUsuario";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idUsuario", idUsuario);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                res.Add(new Imagen
                {
                    IdImagen = reader.GetInt32(reader.GetOrdinal("id_imagen")),
                    IdUsuario = reader.IsDBNull(reader.GetOrdinal("id_usuario")) 
                                ? null 
                                : reader.GetInt32(reader.GetOrdinal("id_usuario")),
                    Url = reader.GetString(reader.GetOrdinal("ruta"))
                });
            }
            return res;
        }

        
    }
}
