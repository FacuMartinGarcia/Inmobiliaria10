
using System.Data;
using Inmobiliaria10.Models;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace Inmobiliaria10.Data.Repositories;

public class PropietarioRepo : IPropietarioRepo
{
    private readonly string _cs;

    public PropietarioRepo(IConfiguration cfg)
    {
         _cs = cfg.GetConnectionString("Inmogenial")
            ?? throw new InvalidOperationException("Falta ConnectionStrings:Inmogenial");
    }

    private MySqlConnection Conn() => new MySqlConnection(_cs);
    public async Task<IList<Propietario>> ObtenerTodo(string? q = null)
    {
        var list = new List<Propietario>();
        await using var cn = Conn();
        await cn.OpenAsync();

        var sql = @"SELECT id_propietario, documento, apellido_nombres, domicilio, telefono, email
                    FROM propietarios
                    /**where**/
                    ORDER BY apellido_nombres";
        sql = string.IsNullOrWhiteSpace(q)
            ? sql.Replace("/**where**/", "")
            : sql.Replace("/**where**/", "WHERE apellido_nombres LIKE @q OR documento LIKE @q");

        await using var cmd = new MySqlCommand(sql, cn);
        if (!string.IsNullOrWhiteSpace(q))
            cmd.Parameters.Add("@q", MySqlDbType.VarChar).Value = $"%{q}%";

        await using var rd = await cmd.ExecuteReaderAsync();

        var oId   = rd.GetOrdinal("id_propietario");
        var oDoc  = rd.GetOrdinal("documento");
        var oApe  = rd.GetOrdinal("apellido_nombres");
        var oDom  = rd.GetOrdinal("domicilio");
        var oTel  = rd.GetOrdinal("telefono");
        var oMail = rd.GetOrdinal("email");

        while (await rd.ReadAsync())
        {
            list.Add(new Propietario{
                IdPropietario   = rd.GetInt32(oId),
                Documento       = rd.GetString(oDoc),
                ApellidoNombres = rd.GetString(oApe),
                Domicilio       = rd.IsDBNull(oDom)  ? null : rd.GetString(oDom),
                Telefono        = rd.IsDBNull(oTel)  ? null : rd.GetString(oTel),
                Email           = rd.IsDBNull(oMail) ? null : rd.GetString(oMail),
            });
        }
        return list;
    }

    public IList<Propietario> ObtenerLista(int paginaNro, int tamPagina)
    {
        if (paginaNro < 1) paginaNro = 1;
        if (tamPagina < 1) tamPagina = 10;

        var res = new List<Propietario>();
        using var cn = Conn();
        cn.Open();

        const string sql = @"
            SELECT id_propietario, documento, apellido_nombres, domicilio, telefono, email
            FROM propietarios
            ORDER BY apellido_nombres
            LIMIT @take OFFSET @skip;";

        using var cmd = new MySqlCommand(sql, cn);
        cmd.Parameters.Add("@take", MySqlDbType.Int32).Value = tamPagina;
        cmd.Parameters.Add("@skip", MySqlDbType.Int32).Value = (paginaNro - 1) * tamPagina;

        using var rd = cmd.ExecuteReader();

        var oId   = rd.GetOrdinal("id_propietario");
        var oDoc  = rd.GetOrdinal("documento");
        var oApe  = rd.GetOrdinal("apellido_nombres");
        var oDom  = rd.GetOrdinal("domicilio");
        var oTel  = rd.GetOrdinal("telefono");
        var oMail = rd.GetOrdinal("email");

        while (rd.Read())
        {
            res.Add(new Propietario{
                IdPropietario   = rd.GetInt32(oId),
                Documento       = rd.GetString(oDoc),
                ApellidoNombres = rd.GetString(oApe),
                Domicilio       = rd.IsDBNull(oDom)  ? null : rd.GetString(oDom),
                Telefono        = rd.IsDBNull(oTel)  ? null : rd.GetString(oTel),
                Email           = rd.IsDBNull(oMail) ? null : rd.GetString(oMail),
            });
        }
        return res;
    }

    public int ObtenerCantidad()
    {
        using var cn = Conn();
        cn.Open();
        const string sql = "SELECT COUNT(*) FROM propietarios;";
        using var cmd = new MySqlCommand(sql, cn);
        var obj = cmd.ExecuteScalar();
        return Convert.ToInt32(obj);
    }

    public async Task<Propietario?> ObtenerPorId(int id)
    {
        await using var cn = Conn();
        await cn.OpenAsync();
        var sql = @"SELECT id_propietario, documento, apellido_nombres, domicilio, telefono, email
                    FROM propietarios WHERE id_propietario=@id";
        await using var cmd = new MySqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@id", id);
        await using var rd = await cmd.ExecuteReaderAsync();
        if (await rd.ReadAsync())
        {
            return new Propietario{
                IdPropietario = rd.GetInt32("id_propietario"),
                Documento = rd.GetString("documento"),
                ApellidoNombres = rd.GetString("apellido_nombres"),
                Domicilio = rd.IsDBNull("domicilio") ? null : rd.GetString("domicilio"),
                Telefono = rd.IsDBNull("telefono") ? null : rd.GetString("telefono"),
                Email = rd.IsDBNull("email") ? null : rd.GetString("email"),
            };
        }
        return null;
    }
    public async Task<int> Crear(Propietario p)
    {
        if (string.IsNullOrWhiteSpace(p.Documento))
            throw new ArgumentException("El documento es obligatorio.");
        if (string.IsNullOrWhiteSpace(p.ApellidoNombres))
            throw new ArgumentException("El nombre y apellido son obligatorios.");

        try
        {
            await using var cn = Conn();
            await cn.OpenAsync();

            const string sql = @"
                INSERT INTO propietarios (documento, apellido_nombres, domicilio, telefono, email)
                VALUES (@doc, @ape, @dom, @tel, @mail);";

            await using var cmd = new MySqlCommand(sql, cn);
            cmd.Parameters.Add("@doc",  MySqlDbType.VarChar).Value = p.Documento;
            cmd.Parameters.Add("@ape",  MySqlDbType.VarChar).Value = p.ApellidoNombres;
            cmd.Parameters.Add("@dom",  MySqlDbType.VarChar).Value = (object?)p.Domicilio ?? DBNull.Value;
            cmd.Parameters.Add("@tel",  MySqlDbType.VarChar).Value = (object?)p.Telefono ?? DBNull.Value;
            cmd.Parameters.Add("@mail", MySqlDbType.VarChar).Value = (object?)p.Email ?? DBNull.Value;

            await cmd.ExecuteNonQueryAsync();
            return Convert.ToInt32(cmd.LastInsertedId);
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            throw new Exception("Documento duplicado.", ex);
        }
        catch (MySqlException ex)
        {
            throw new Exception("Error al crear propietario.", ex);
        }
    }

    public async Task<bool> Modificar(Propietario p)
    {

        if (p.IdPropietario <= 0) 
            throw new ArgumentException("IdPropietario inválido.");
        if (string.IsNullOrWhiteSpace(p.Documento)) 
            throw new ArgumentException("Documento obligatorio.");
        if (string.IsNullOrWhiteSpace(p.ApellidoNombres)) 
            throw new ArgumentException("Apellido y Nombres obligatorio.");

        const string sql = @"
                    UPDATE propietarios
                    SET documento=@doc, apellido_nombres=@ape, domicilio=@dom, telefono=@tel, email=@mail
                    WHERE id_propietario=@id;";

        try
        {
            await using var cn = Conn();
            await cn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, cn);

            cmd.Parameters.Add("@doc",  MySqlDbType.VarChar).Value = p.Documento;
            cmd.Parameters.Add("@ape",  MySqlDbType.VarChar).Value = p.ApellidoNombres;
            cmd.Parameters.Add("@dom",  MySqlDbType.VarChar).Value = (object?)p.Domicilio ?? DBNull.Value;
            cmd.Parameters.Add("@tel",  MySqlDbType.VarChar).Value = (object?)p.Telefono ?? DBNull.Value;
            cmd.Parameters.Add("@mail", MySqlDbType.VarChar).Value = (object?)p.Email ?? DBNull.Value;
            cmd.Parameters.Add("@id",   MySqlDbType.Int32 ).Value = p.IdPropietario;

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;  // true si actualizó al menos 1 fila
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Código de error para duplicados
        {
            throw new Exception("Documento duplicado. Verifique que no exista otro propietario con el mismo documento.", ex);
        }
        catch (MySqlException ex)
        {
            throw new Exception("Error al modificar propietario.", ex);
        }
    }
    public async Task<bool> Borrar(int id)
    {
        await using var cn = Conn();
        await cn.OpenAsync();
        var sql = "DELETE FROM propietarios WHERE id_propietario=@id";
        await using var cmd = new MySqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@id", id);
        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }
    public async Task<bool> ExistsDocumento(string documento, int? exceptId = null)
    {
        await using var cn = Conn();
        await cn.OpenAsync();
        var sql = "SELECT COUNT(1) FROM propietarios WHERE documento=@doc";
        if (exceptId.HasValue) sql += " AND id_propietario<>@id";
        await using var cmd = new MySqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@doc", documento);
        if (exceptId.HasValue) cmd.Parameters.AddWithValue("@id", exceptId.Value);
        var n = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        return n > 0;
    }
}
