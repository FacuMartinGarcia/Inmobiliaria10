using Inmobiliaria10.Models;

namespace Inmobiliaria10.Repositories;

public interface IInquilinoRepo
{
    Task<IList<Inquilino>> ObtenerTodo();
    Task<Inquilino?> ObtenerPorId(int id);
    Task<Inquilino?> ObtenerPorDocumento(string documento);
    Task<int> Crear(Inquilino i);
    Task<bool> Modificar(Inquilino i);
    Task<bool> Borrar(int id);
    Task<bool> ExistsDocumento(string documento, int? exceptId = null);
}
