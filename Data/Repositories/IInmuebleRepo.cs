using Inmobiliaria10.Models;
//using System.Collections.Generic;

namespace Inmobiliaria10.Data.Repositories
{
    public interface IInmuebleRepo
    {
        int Agregar(Inmueble inmueble);
        int Actualizar(Inmueble inmueble);
        int Eliminar(int id);
        Inmueble? ObtenerPorId(int id);
        List<Inmueble> ListarTodos();
        (List<Inmueble> registros, int totalRegistros) ListarTodosPaginado(int pagina, int cantidadPorPagina, string? searchString = null);
    }
}
