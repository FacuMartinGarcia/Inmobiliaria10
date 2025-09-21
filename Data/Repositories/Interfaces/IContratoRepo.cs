using Inmobiliaria10.Models;

namespace Inmobiliaria10.Data.Repositories
{
    public interface IContratoRepo
    {
        Task<Contrato?> GetByIdAsync(int id, CancellationToken ct = default);

        /// <summary>
        /// Lista contratos con filtros opcionales (por inmueble, inquilino y estado).
        /// pageIndex base 1. Si pageSize es 0, devuelve todo sin paginar.
        /// </summary>
        Task<(IReadOnlyList<Contrato> Items, int Total)> ListAsync(
            int? tipo = null,   
            int? idInmueble = null,
            int? idInquilino = null,
            bool? soloActivos = null,
            int pageIndex = 1,
            int pageSize = 20,
            CancellationToken ct = default);

        Task<int> CreateAsync(Contrato entity, CancellationToken ct = default);
        Task UpdateAsync(Contrato entity, CancellationToken ct = default);

        /// <summary>
        /// Soft delete: marca DeletedAt/DeletedBy.
        /// </summary>
        Task<bool> SoftDeleteAsync(int id, int deletedBy, CancellationToken ct = default);

        /// <summary>
        /// Verifica si existe solapamiento de fechas para el mismo inmueble.
        /// excludeContratoId: para ignorar el propio contrato en edición.
        /// </summary>
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
    }
}
