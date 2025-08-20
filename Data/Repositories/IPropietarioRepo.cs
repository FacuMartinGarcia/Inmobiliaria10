// Repositories/IPropietarioRepository.cs
using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories;

public interface IPropietarioRepo
{
    Task<IEnumerable<Propietario>> ObtenerTodo(string? q = null);
    Task<Propietario?> ObtenerPorId(int id);
    Task<int> Crear(Propietario p);
    Task<bool> Modificar(Propietario p);
    Task<bool> Borrar(int id);
    Task<bool> ExistsDocumento(string documento, int? exceptId = null);
}
