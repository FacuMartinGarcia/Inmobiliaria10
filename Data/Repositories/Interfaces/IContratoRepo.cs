using Inmobiliaria10.Models;
using Inmobiliaria10.Models.ViewModels;

namespace Inmobiliaria10.Data.Repositories
{
    public interface IContratoRepo
    {
        Task<Contrato?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<(IReadOnlyList<Contrato> Items, int Total)> ListAsync(
            int? tipo = null,
            int? idInmueble = null,
            int? idInquilino = null,
            bool? soloActivos = null,
            int pageIndex = 1,
            int pageSize = 20,
            CancellationToken ct = default);

        Task<(IReadOnlyList<Contrato> Items, int Total)> ListByFechasAsync(
                int? tipo = null,
                int? idInmueble = null,
                int? idInquilino = null,
                DateTime? fechaDesde = null,
                DateTime? fechaHasta = null,
                int pageIndex = 1,
                int pageSize = 20,
                CancellationToken ct = default);
        Task<int> CreateAsync(Contrato entity, CancellationToken ct = default);
        Task UpdateAsync(Contrato entity, CancellationToken ct = default);

        Task<bool> SoftDeleteAsync(int id, int deletedBy, CancellationToken ct = default);

        Task<bool> ExistsOverlapAsync(
            int idInmueble,
            DateTime fechaInicio,
            DateTime fechaFin,
            DateTime? rescision = null,
            int? excludeContratoId = null,
            CancellationToken ct = default);

        // --------- Soporte para selects en el Controller (sin EF) ----------
        Task<IReadOnlyList<(int Id, string Direccion)>> GetInmueblesAsync(CancellationToken ct = default);
        Task<IReadOnlyList<(int Id, string Nombre)>> GetInquilinosAsync(CancellationToken ct = default);
        Task<IReadOnlyList<ContratoAudit>> GetAuditoriaAsync(int contratoId, CancellationToken ct = default);
        Task<decimal?> CalcularMultaAsync(int idContrato, DateTime fechaRescision, CancellationToken ct = default);
        Task<IReadOnlyList<(int Id, string Direccion, string Inquilino)>>
        GetContratosInfoAsync(IEnumerable<int> ids, CancellationToken ct = default);
        Task<IReadOnlyList<Contrato>> GetContratosPorInmuebleAsync(int idInmueble, CancellationToken ct = default);
        Task<(IReadOnlyList<ContratoAuditViewModel> Items, int Total)> ListAuditoriaAsync(
            int? usuarioId = null,
            int pageIndex = 1,
            int pageSize = 10,
            CancellationToken ct = default);
    }
}
