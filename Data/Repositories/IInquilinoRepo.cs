using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories
{
    public interface IInquilinoRepo
    {
        Task<int> AgregarAsync(Inquilino i, CancellationToken ct = default);
        Task<int> ActualizarAsync(Inquilino i, CancellationToken ct = default);
        Task<List<Inquilino>> ListarTodosAsync(CancellationToken ct = default);
        Task<(List<Inquilino> registros, int totalRegistros)> ListarTodosPaginadoAsync(
            int pagina, int cantidadPorPagina, string? searchString = null, CancellationToken ct = default);
        Task<Inquilino?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
        Task<Inquilino?> ObtenerPorDocumentoAsync(string documento, CancellationToken ct = default);
        Task<int> BorrarAsync(int id, CancellationToken ct = default);
    }
}
