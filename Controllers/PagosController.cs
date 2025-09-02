using System.Security.Claims;
using Inmobiliaria10.Data.Repositories;
using Inmobiliaria10.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inmobiliaria10.Controllers
{
    [Route("Pagos")]
    public class PagosController : Controller
    {
        private readonly IPagoRepo _repo;
        public PagosController(IPagoRepo repo) { _repo = repo; }

        [HttpGet("")]
        public async Task<IActionResult> Index(int? contrato, int? concepto, bool? soloActivos, int page = 1, int pageSize = 20, CancellationToken ct = default)
        {
            await CargarSelectsAsync(concepto, ct);
            var (items, total) = await _repo.ListAsync(contrato, concepto, soloActivos, page, pageSize, ct);
            ViewBag.Total = total; ViewBag.Page = page; ViewBag.PageSize = pageSize; ViewBag.SoloActivos = soloActivos; ViewBag.Contrato = contrato;
            return View(items);
        }

        [HttpGet("Detalles/{id:int}")]
        public async Task<IActionResult> Detalles(int id, CancellationToken ct = default)
        {
            var x = await _repo.GetByIdAsync(id, ct);
            return x is null ? NotFound() : View(x);
        }

        [HttpGet("Crear")]
        public async Task<IActionResult> Crear(int? contrato, CancellationToken ct = default)
        {
            await CargarSelectsAsync(null, ct);
            return View(new Pago { IdContrato = contrato ?? 0, FechaPago = DateTime.Today });
        }

        [HttpPost("Crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Pago m, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) { await CargarSelectsAsync(m.IdConcepto, ct); return View(m); }
            m.CreatedBy = GetUserIdOrDefault(); m.CreatedAt = DateTime.UtcNow;
            await _repo.CreateAsync(m, ct);
            TempData["Ok"] = "Pago registrado.";
            return RedirectToAction(nameof(Index), new { contrato = m.IdContrato });
        }

        [HttpGet("Editar/{id:int}")]
        public async Task<IActionResult> Editar(int id, CancellationToken ct = default)
        {
            var x = await _repo.GetByIdAsync(id, ct);
            if (x is null) return NotFound();
            await CargarSelectsAsync(x.IdConcepto, ct);
            return View(x);
        }

        [HttpPost("Editar/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Pago m, CancellationToken ct = default)
        {
            if (id != m.IdPago) return BadRequest();
            if (!ModelState.IsValid) { await CargarSelectsAsync(m.IdConcepto, ct); return View(m); }
            await _repo.UpdateAsync(m, ct);
            TempData["Ok"] = "Pago actualizado.";
            return RedirectToAction(nameof(Detalles), new { id = m.IdPago });
        }

        [HttpGet("Eliminar/{id:int}")]
        public async Task<IActionResult> Eliminar(int id, CancellationToken ct = default)
        {
            var x = await _repo.GetByIdAsync(id, ct);
            return x is null ? NotFound() : View(x);
        }

        [HttpPost("Borrar/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrar(int id, CancellationToken ct = default)
        {
            var ok = await _repo.SoftDeleteAsync(id, GetUserIdOrDefault(), ct);
            if (!ok) return NotFound();
            TempData["Ok"] = "Pago eliminado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Auditoria/{id:int}")]
        public async Task<IActionResult> Auditoria(int id, CancellationToken ct = default)
        {
            var eventos = await _repo.GetAuditoriaAsync(id, ct);
            ViewBag.IdPago = id;
            return View(eventos);
        }

        private async Task CargarSelectsAsync(int? idConcepto, CancellationToken ct)
        {
            var conceptos = await _repo.GetConceptosAsync(ct);
            ViewBag.Conceptos = new SelectList(
                conceptos.Select(x => new { IdConcepto = x.Id, Nombre = x.Nombre }),
                "IdConcepto", "Nombre", idConcepto);
        }

        private int GetUserIdOrDefault()
        {
            var s = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(s, out var id) ? id : 1;
        }
    }
}
