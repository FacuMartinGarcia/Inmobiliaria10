using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories
{
    public interface IUsuarioRepo
    {
        Task<int> Agregar(Usuario u);
        Task<int> Actualizar(Usuario u);
        Task<List<Usuario>> ListarTodos();
        Task<(List<Usuario> registros, int totalRegistros)> ListarTodosPaginado(int pagina, int cantidadPorPagina, string? searchString = null);
        Task<Usuario?> ObtenerPorId(int id);
        Task<int> Eliminar(int id);
        Task<Usuario?> ObtenerPorAlias(string alias);
    }
}
