// Repositories/IPropietarioRepository.cs
using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories;

public interface IPropietarioRepo
{
    Task<IList<Propietario>> ObtenerTodo(string? q = null);
    Task<Propietario?> ObtenerPorId(int id);
    Task<int> Crear(Propietario p);
    Task<bool> Modificar(Propietario p);
    Task<bool> Borrar(int id);
    IList<Propietario> ObtenerLista(int paginaNro, int tamPagina);
    int ObtenerCantidad();
    Task<bool> ExistsDocumento(string documento, int? exceptId = null);
}
