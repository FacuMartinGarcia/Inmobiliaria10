
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
        _cs = cfg.GetConnectionString("DefaultConnection")!;
    }

    private MySqlConnection Conn() => new MySqlConnection(_cs);

    public async Task<IEnumerable<Propietario>> GetAllAsync(string? q = null)
    {
        var list = new List<Propietario>();
        await using var cn = Conn();
        await cn.OpenAsync();
        var sql = @"SELECT id_propietario, documento, apellido_nombres, domicilio, telefono, email
                    FROM propietarios
                    /**where**/
                    ORDER BY apellido_nombres";
        if (!string.IsNullOrWhiteSpace(q))
            sql = sql.Replace("/**where**/", "WHERE apellido_nombres LIKE @q OR documento LIKE @q");
        else
            sql = sql.Replace("/**where**/", "");

        await using var cmd = new MySqlCommand(sql, cn);
        if (!string.IsNullOrWhiteSpace(q)) cmd.Parameters.AddWithValue("@q", $"%{q}%");

        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new Propietario{
                IdPropietario = rd.GetInt32("id_propietario"),
                Documento = rd.GetString("documento"),
                ApellidoNombres = rd.GetString("apellido_nombres"),
                Domicilio = rd.IsDBNull("domicilio") ? null : rd.GetString("domicilio"),
                Telefono = rd.IsDBNull("telefono") ? null : rd.GetString("telefono"),
                Email = rd.IsDBNull("email") ? null : rd.GetString("email"),
            });
        }
        return list;
    }

    public async Task<Propietario?> GetByIdAsync(int id)
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

    public async Task<int> CreateAsync(Propietario p)
    {
        await using var cn = Conn();
        await cn.OpenAsync();
        var sql = @"INSERT INTO propietarios (documento, apellido_nombres, domicilio, telefono, email)
                    VALUES (@doc, @ape, @dom, @tel, @mail);
                    SELECT LAST_INSERT_ID();";
        await using var cmd = new MySqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@doc", p.Documento);
        cmd.Parameters.AddWithValue("@ape", p.ApellidoNombres);
        cmd.Parameters.AddWithValue("@dom", (object?)p.Domicilio ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@tel", (object?)p.Telefono ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@mail", (object?)p.Email ?? DBNull.Value);
        var id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        return id;
    }

    public async Task<bool> UpdateAsync(Propietario p)
    {
        await using var cn = Conn();
        await cn.OpenAsync();
        var sql = @"UPDATE propietarios
                    SET documento=@doc, apellido_nombres=@ape, domicilio=@dom, telefono=@tel, email=@mail
                    WHERE id_propietario=@id";
        await using var cmd = new MySqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@doc", p.Documento);
        cmd.Parameters.AddWithValue("@ape", p.ApellidoNombres);
        cmd.Parameters.AddWithValue("@dom", (object?)p.Domicilio ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@tel", (object?)p.Telefono ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@mail", (object?)p.Email ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@id", p.IdPropietario);
        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var cn = Conn();
        await cn.OpenAsync();
        var sql = "DELETE FROM propietarios WHERE id_propietario=@id";
        await using var cmd = new MySqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@id", id);
        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> ExistsDocumentoAsync(string documento, int? exceptId = null)
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
