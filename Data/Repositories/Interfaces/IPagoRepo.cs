using Inmobiliaria10.Models.ViewModels;
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
            CancellationToken ct = default,
            int? idInquilino = null
        );

        Task<int> CreateAsync(Pago entity, CancellationToken ct = default);
        Task UpdateAsync(Pago entity, CancellationToken ct = default);
        Task<bool> SoftDeleteAsync(int id, int deletedBy, CancellationToken ct = default);
        Task<bool> AnularPagoAsync(int id, string motivo, int userId, CancellationToken ct = default);
        Task<int> GetNextNumeroPagoAsync(int idContrato, CancellationToken ct = default);
        Task<int> RegistrarMultaAsync(int contratoId, DateTime fecha, int userId, CancellationToken ct = default);
        Task UpdateConceptoAsync(int idPago, int idConcepto, CancellationToken ct = default);
        // Combos
        Task<IReadOnlyList<(int Id, string Nombre)>> GetConceptosAsync(CancellationToken ct = default);

        // Auditoría
        Task<IReadOnlyList<PagoAudit>> GetAuditoriaAsync(int idPago, CancellationToken ct = default);

        // --- Select2 ---
        Task<IReadOnlyList<(int Id, string Text)>> SearchInquilinosAsync(string? term, int take, CancellationToken ct = default);
        Task<(int Id, string Text)?> GetInquilinoItemAsync(int id, CancellationToken ct = default);

        Task<IReadOnlyList<(int Id, string Text)>> SearchContratosAsync(string? term, int take, CancellationToken ct = default);
        Task<(int Id, string Text)?> GetContratoItemAsync(int id, CancellationToken ct = default);

        Task<IReadOnlyList<(int Id, string Text)>> SearchContratosPorInquilinoAsync(
            int idInquilino, string? term, int take, bool soloVigentesActivos = true, CancellationToken ct = default);

        // 🔹 Nuevo método para detalle
        Task<PagoDetalleViewModel?> GetDetalleAsync(int id, CancellationToken ct = default);
    }
}
