using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories
{
    public interface IPagoRepo
    {
        Task<Pago?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<(IReadOnlyList<Pago> Items, int Total)> ListAsync(
            int? idContrato = null,
            int? idConcepto = null,
            bool? soloActivos = null,
            int pageIndex = 1,
            int pageSize = 20,
            CancellationToken ct = default);

        Task<int> CreateAsync(Pago entity, CancellationToken ct = default);

        Task UpdateAsync(Pago entity, CancellationToken ct = default);

        Task<bool> SoftDeleteAsync(int id, int deletedBy, CancellationToken ct = default);

        // Para el combo de conceptos
        Task<IReadOnlyList<(int Id, string Nombre)>> GetConceptosAsync(CancellationToken ct = default);

        // Auditoría
        Task<IReadOnlyList<PagoAudit>> GetAuditoriaAsync(int idPago, CancellationToken ct = default);
    }
}

