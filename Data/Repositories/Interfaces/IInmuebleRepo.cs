using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories
{
    public interface IInmuebleRepo
    {
        Task<int> Agregar(Inmueble inmueble);
        Task<int> Actualizar(Inmueble inmueble);
        Task<List<Inmueble>> BuscarInmueble(string term);
        Task<int> Eliminar(int id);
        Task<Inmueble?> ObtenerPorId(int id);
        Task<List<Inmueble>> ListarTodos();
        Task<(List<Inmueble> registros, int totalRegistros)> ListarTodosPaginado(
            int pagina, int cantidadPorPagina, string? searchString = null,
            bool soloDisponibles = false, DateTime? fechaInicio = null,
            DateTime? fechaFin = null);
        Task<(List<Inmueble> registros, int totalRegistros)> ListarPorPropietario(
        int idPropietario, int pagina, int cantidadPorPagina, string? searchString = null);
    }
}
