using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories;

public interface IInquilinoRepo
{
    int Agregar(Inquilino i);
    int Actualizar(Inquilino i);
    List<Inquilino> ListarTodos();
    (List<Inquilino> registros, int totalRegistros) ListarTodosPaginado(int pagina, int cantidadPorPagina, string? searchString = null);
    Inquilino? ObtenerPorId(int id);
    Inquilino? ObtenerPorDocumento(string documento);
    int Borrar(int id);
    
    
}