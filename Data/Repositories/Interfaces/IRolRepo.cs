using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories
{
    public interface IRolRepo
    {
        Task<int> Agregar(Rol rol);
        Task<int> Actualizar(Rol rol);
        Task<int> Eliminar(int id);
        Task<Rol?> ObtenerPorId(int id);
        Task<List<Rol>> ListarTodos();
        Task<bool> ExisteDenominacion(string denominacion, int? idExcluir = null);
    }
}
