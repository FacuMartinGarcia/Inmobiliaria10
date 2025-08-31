using Inmobiliaria10.Models;
using Microsoft.EntityFrameworkCore;

namespace Inmobiliaria10.Data.Repositories
{
    public class ContratoRepo : IContratoRepo
    {
        private readonly AppDbContext _db;

        public ContratoRepo(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Contrato?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Contratos
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdContrato == id, ct);
        }

        public async Task<(IReadOnlyList<Contrato> Items, int Total)> ListAsync(
            int? idInmueble = null,
            int? idInquilino = null,
            bool? soloActivos = null,
            int pageIndex = 1,
            int pageSize = 20,
            CancellationToken ct = default)
        {
            var q = _db.Contratos.AsNoTracking().AsQueryable();

            if (idInmueble.HasValue && idInmueble.Value > 0)
                q = q.Where(c => c.IdInmueble == idInmueble.Value);

            if (idInquilino.HasValue && idInquilino.Value > 0)
                q = q.Where(c => c.IdInquilino == idInquilino.Value);

            if (soloActivos == true)
                q = q.Where(c => c.DeletedAt == null);
            else if (soloActivos == false)
                q = q.Where(c => c.DeletedAt != null);

            q = q.OrderByDescending(c => c.CreatedAt);

            int total = await q.CountAsync(ct);

            if (pageSize > 0)
            {
                // pageIndex base 1
                int skip = Math.Max(0, (pageIndex - 1) * pageSize);
                q = q.Skip(skip).Take(pageSize);
            }

            var items = await q.ToListAsync(ct);
            return (items, total);
        }

        public async Task<int> CreateAsync(Contrato entity, CancellationToken ct = default)
        {
            // Normalizar fechas a Date si querés comparar solo por día
            entity.CreatedAt = DateTime.UtcNow;

            // (Opcional) Validación de solape acá, o hacela en el servicio antes de llamar al repo
            bool overlap = await ExistsOverlapAsync(
                entity.IdInmueble,
                entity.FechaInicio,
                entity.FechaFin,
                entity.Rescision,
                null,
                ct);

            if (overlap)
                throw new InvalidOperationException("Ya existe un contrato que se superpone para este inmueble.");

            _db.Contratos.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity.IdContrato;
        }

        public async Task UpdateAsync(Contrato entity, CancellationToken ct = default)
        {
            // (Opcional) Validación de solape antes de guardar
            bool overlap = await ExistsOverlapAsync(
                entity.IdInmueble,
                entity.FechaInicio,
                entity.FechaFin,
                entity.Rescision,
                entity.IdContrato,
                ct);

            if (overlap)
                throw new InvalidOperationException("La modificación genera un solapamiento con otro contrato del inmueble.");

            _db.Contratos.Update(entity);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<bool> SoftDeleteAsync(int id, int deletedBy, CancellationToken ct = default)
        {
            var entity = await _db.Contratos.FirstOrDefaultAsync(c => c.IdContrato == id, ct);
            if (entity == null) return false;

            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedBy = deletedBy;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> ExistsOverlapAsync(
            int idInmueble,
            DateTime fechaInicio,
            DateTime fechaFin,
            DateTime? rescision = null,
            int? excludeContratoId = null,
            CancellationToken ct = default)
        {
            // Rango efectivo del nuevo/actual contrato
            GetRangoEfectivo(fechaInicio, fechaFin, rescision, out var ini, out var fin);

            var q = _db.Contratos
                .Where(c => c.IdInmueble == idInmueble)
                .Where(c => c.DeletedAt == null); // ignorar eliminados

            if (excludeContratoId.HasValue)
                q = q.Where(c => c.IdContrato != excludeContratoId.Value);

            return await q.AnyAsync(c =>
                RangoSeSolapa(
                    ini, fin,
                    RangoInicioExistente(c), RangoFinExistente(c)
                ), ct);
        }

        // ===== Helpers de rango (consideran rescisión) =====
        private static void GetRangoEfectivo(DateTime inicio, DateTime fin, DateTime? rescision,
                                             out DateTime efIni, out DateTime efFin)
        {
            efIni = inicio.Date;
            var finEfectivo = fin.Date;

            if (rescision.HasValue && rescision.Value.Date < finEfectivo)
                finEfectivo = rescision.Value.Date;

            efFin = finEfectivo;
        }

        private static DateTime RangoInicioExistente(Contrato c) => c.FechaInicio.Date;

        private static DateTime RangoFinExistente(Contrato c)
        {
            var finEfectivo = c.FechaFin.Date;
            if (c.Rescision.HasValue && c.Rescision.Value.Date < finEfectivo)
                finEfectivo = c.Rescision.Value.Date;
            return finEfectivo;
        }

        /// <summary>
        /// Solapamiento inclusivo en extremos: [a1, a2] con [b1, b2]
        /// Si querés que fin=inicio NO cuente como solape, cambiá por (a1 < b2 && b1 < a2).
        /// </summary>
        private static bool RangoSeSolapa(DateTime a1, DateTime a2, DateTime b1, DateTime b2)
            => b1 <= a2 && a1 <= b2;
    }
}
