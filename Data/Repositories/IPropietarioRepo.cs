// Repositories/IPropietarioRepository.cs
using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories;

public interface IPropietarioRepo
{
    Task<IEnumerable<Propietario>> ObtenerTodoAsync(string? q = null);
    Task<Propietario?> ObtenerPorIdAsync(int id);
    Task<int> CrearAsync(Propietario p);
    Task<bool> ModificarAsync(Propietario p);
    Task<bool> BorrarAsync(int id);
    Task<bool> ExistsDocumentoAsync(string documento, int? exceptId = null);
}
