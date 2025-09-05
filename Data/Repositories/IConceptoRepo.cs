
using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories
{
    public interface IConceptoRepo
    {
        Task<Concepto?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<(IReadOnlyList<Concepto> Items, int Total)> ListAsync(
            string? buscar = null,                // filtra por LIKE en denominación
            int pageIndex = 1,
            int pageSize = 20,
            CancellationToken ct = default);

        Task<int> CreateAsync(Concepto entity, CancellationToken ct = default);
        Task UpdateAsync(Concepto entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default); // borrado duro
        Task<bool> ExistsByNameAsync(string denominacion, int? excludeId = null, CancellationToken ct = default);
    }
}
