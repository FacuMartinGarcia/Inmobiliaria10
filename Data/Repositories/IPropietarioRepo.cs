// Repositories/IPropietarioRepository.cs
using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories;

public interface IPropietarioRepo
{
    Task<IEnumerable<Propietario>> GetAllAsync(string? q = null);
    Task<Propietario?> GetByIdAsync(int id);
    Task<int> CreateAsync(Propietario p);
    Task<bool> UpdateAsync(Propietario p);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsDocumentoAsync(string documento, int? exceptId = null);
}
