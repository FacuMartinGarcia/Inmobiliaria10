using Inmobiliaria10.Models;
using Inmobiliaria10.Models.ViewModels;

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
            int? idInquilino = null);
        Task<int> CreateAsync(Pago e, CancellationToken ct = default);
        Task<bool> UpdateConceptoAsync(Pago e, CancellationToken ct = default);
        Task<int> GetNextNumeroPagoAsync(int idContrato, CancellationToken ct = default);
        Task<bool> AnularPagoAsync(int id, string motivo, int? userId, CancellationToken ct = default);
        Task<bool> SoftDeleteAsync(int id, int deletedBy, CancellationToken ct = default);
        Task<IReadOnlyList<(int Id, string Nombre)>> GetConceptosAsync(CancellationToken ct = default);
        Task<IReadOnlyList<PagoAuditViewModel>> GetAuditoriaAsync(int idPago, CancellationToken ct = default);
        Task<IReadOnlyList<PagoAuditViewModel>> GetAuditoriaGeneralAsync(CancellationToken ct = default);
        Task<(IReadOnlyList<PagoAuditViewModel> Items, int Total)> ListAuditoriaAsync(
            int? usuarioId = null,
            int? contratoId = null,
            int? conceptoId = null,
            int pageIndex = 1,
            int pageSize = 10,
            CancellationToken ct = default);
        Task<IReadOnlyList<(int Id, string Text)>> SearchInquilinosAsync(string? term, int take, CancellationToken ct = default);
        Task<(int Id, string Text)?> GetInquilinoItemAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<(int Id, string Text)>> SearchContratosPorInquilinoAsync(
            int idInquilino, string? term, int take, bool soloVigentesActivos = true, CancellationToken ct = default);
        Task<IReadOnlyList<(int Id, string Text)>> SearchContratosAsync(string? term, int take, CancellationToken ct = default);
        Task<(int Id, string Text)?> GetContratoItemAsync(int id, CancellationToken ct = default);
        Task<PagoDetalleViewModel?> GetDetalleAsync(int id, CancellationToken ct = default);
        Task<(IReadOnlyList<MorosoViewModel> Items, int Total)> GetMorososAsync(
            int? inquilinoId = null,
            int pageIndex = 1,
            int pageSize = 10,
            CancellationToken ct = default);
        Task<IReadOnlyList<(int Id, string Text)>> SearchUsuariosAsync(string? term, int take, CancellationToken ct = default);
    }
}
