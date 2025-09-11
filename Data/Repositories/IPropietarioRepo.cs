using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories;

public interface IPropietarioRepo
{
    Task<IList<Propietario>> ObtenerTodo(string? q = null);
    Task<List<Propietario>> BuscarPropietarioAsync(string term);
    Task<Propietario?> ObtenerPorId(int id);
    Task<int> Crear(Propietario p);
    Task<bool> Modificar(Propietario p);
    Task<bool> Borrar(int id);
    Task<bool> ExistsDocumento(string documento, int? exceptId = null);
    Task<PagedResult<Propietario>> BuscarPaginado(string? q, int page, int pageSize);
}
