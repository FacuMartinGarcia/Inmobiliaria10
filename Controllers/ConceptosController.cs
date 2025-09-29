// Controllers/ConceptosController.cs
using Inmobiliaria10.Data.Repositories;
using Inmobiliaria10.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace Inmobiliaria10.Controllers
{
    [Route("Conceptos")]
    [Authorize] 
    public class ConceptosController : Controller
    {
        private readonly IConceptoRepo _repo;
        public ConceptosController(IConceptoRepo repo) => _repo = repo;

        // GET /Conceptos
        [HttpGet("")]
        public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 20, CancellationToken ct = default)
        {
            var (items, total) = await _repo.ListAsync(q, page, pageSize, ct);
            ViewBag.Total = total;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Query = q;
            return View(items);
        }


        // GET /Conceptos/Crear
        [HttpGet("Crear")]
        public IActionResult Crear() => View(new Concepto());

        // POST /Conceptos/Crear
        [HttpPost("Crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Concepto m, CancellationToken ct = default)
        {
            // Validación simple
            if (string.IsNullOrWhiteSpace(m.DenominacionConcepto))
                ModelState.AddModelError(nameof(m.DenominacionConcepto), "La denominación es obligatoria.");

            // Unicidad
            if (ModelState.IsValid && await _repo.ExistsByNameAsync(m.DenominacionConcepto, null, ct))
                ModelState.AddModelError(nameof(m.DenominacionConcepto), "Ya existe un concepto con esa denominación.");

            if (!ModelState.IsValid) return View(m);

            var id = await _repo.CreateAsync(m, ct);
            TempData["Mensaje"] = "Concepto creado.";
            return RedirectToAction(nameof(Index), new { id });
        }

        [HttpGet("Editar/{id:int}")]
        public async Task<IActionResult> Editar(int id, CancellationToken ct = default)
        {
            var x = await _repo.GetByIdAsync(id, ct);
            return x is null ? NotFound() : View(x);
        }

        [HttpPost("Editar/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Concepto m, CancellationToken ct = default)
        {
            if (id <= 0) id = m.IdConcepto;     // fallback por si la URL no trae id
            if (id != m.IdConcepto) return BadRequest();

            if (string.IsNullOrWhiteSpace(m.DenominacionConcepto))
                ModelState.AddModelError(nameof(m.DenominacionConcepto), "La denominación es obligatoria.");

            if (ModelState.IsValid && await _repo.ExistsByNameAsync(m.DenominacionConcepto, id, ct))
                ModelState.AddModelError(nameof(m.DenominacionConcepto), "Ya existe un concepto con esa denominación.");

            if (!ModelState.IsValid) return View(m);

            await _repo.UpdateAsync(m, ct);
            TempData["Mensaje"] = "Concepto actualizado.";
            return RedirectToAction(nameof(Index), new { id });
        }

        [HttpPost("Editar")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> EditarSinId(Concepto m, CancellationToken ct = default)
            => Editar(m.IdConcepto, m, ct);

        [Authorize(Roles = "Administrador")]
        [HttpGet("Eliminar/{id:int}")]
        public async Task<IActionResult> Eliminar(int id, CancellationToken ct = default)
        {
            var x = await _repo.GetByIdAsync(id, ct);
            return x is null ? NotFound() : View(x);
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost("Borrar/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrar(int id, CancellationToken ct = default)
        {
            try
            {
                var ok = await _repo.DeleteAsync(id, ct);
                if (!ok) return NotFound();
                TempData["Mensaje"] = "Concepto eliminado.";
                return RedirectToAction(nameof(Index));
            }
            catch (MySql.Data.MySqlClient.MySqlException ex) when (ex.Number == 1451) // FK constraint
            {
                TempData["Error"] = "No se puede eliminar: el concepto tiene pagos asociados.";
                return RedirectToAction(nameof(Index), new { id });
            }
        }
    }
}
