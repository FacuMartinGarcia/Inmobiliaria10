using System.Security.Claims;
using Inmobiliaria10.Data;
using Inmobiliaria10.Data.Repositories;
using Inmobiliaria10.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Inmobiliaria10.Controllers
{
    [Authorize] // opcional
    public class ContratoController : Controller
    {
        private readonly IContratoRepo _repo;
        private readonly AppDbContext _db;

        public ContratoController(IContratoRepo repo, AppDbContext db)
        {
            _repo = repo;
            _db = db;
        }

        // ------------------- INDEX -------------------
        // /Contrato?inmueble=1&inquilino=2&soloActivos=true&page=1&pageSize=20
        public async Task<IActionResult> Index(
            int? inmueble,
            int? inquilino,
            bool? soloActivos,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default)
        {
            await CargarSelectsAsync(inmueble, inquilino, ct);

            var (items, total) = await _repo.ListAsync(
                idInmueble: inmueble,
                idInquilino: inquilino,
                soloActivos: soloActivos,
                pageIndex: page,
                pageSize: pageSize,
                ct);

            ViewBag.Total = total;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.SoloActivos = soloActivos;

            return View(items); // Podés crear un ViewModel si preferís
        }

        // ------------------- DETALLES -------------------
        public async Task<IActionResult> Detalles(int id, CancellationToken ct = default)
        {
            var contrato = await _repo.GetByIdAsync(id, ct);
            if (contrato == null) return NotFound();
            return View(contrato);
        }

        // ------------------- CREAR -------------------
        [HttpGet]
        public async Task<IActionResult> Crear(int? inmueble, int? inquilino, CancellationToken ct = default)
        {
            await CargarSelectsAsync(inmueble, inquilino, ct);
            return View(new Contrato
            {
                IdInmueble = inmueble ?? 0,
                IdInquilino = inquilino ?? 0,
                FechaInicio = DateTime.Today,
                FechaFin = DateTime.Today.AddYears(1)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Contrato model, CancellationToken ct = default)
        {
            // Validaciones por anotaciones + IValidatableObject
            if (!ModelState.IsValid)
            {
                await CargarSelectsAsync(model.IdInmueble, model.IdInquilino, ct);
                return View(model);
            }

            // Set de auditoría
            model.CreatedBy = GetUserIdOrDefault();
            model.CreatedAt = DateTime.UtcNow;

            try
            {
                await _repo.CreateAsync(model, ct);
                TempData["Ok"] = "Contrato creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                // p.ej. solapamiento
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, "No se pudo guardar el contrato. " + ex.Message);
            }

            await CargarSelectsAsync(model.IdInmueble, model.IdInquilino, ct);
            return View(model);
        }

        // ------------------- EDITAR -------------------
        [HttpGet]
        public async Task<IActionResult> Editar(int id, CancellationToken ct = default)
        {
            var contrato = await _repo.GetByIdAsync(id, ct);
            if (contrato == null) return NotFound();

            await CargarSelectsAsync(contrato.IdInmueble, contrato.IdInquilino, ct);
            return View(contrato);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Contrato model, CancellationToken ct = default)
        {
            if (id != model.IdContrato) return BadRequest();

            if (!ModelState.IsValid)
            {
                await CargarSelectsAsync(model.IdInmueble, model.IdInquilino, ct);
                return View(model);
            }

            try
            {
                await _repo.UpdateAsync(model, ct);
                TempData["Ok"] = "Contrato actualizado correctamente.";
                return RedirectToAction(nameof(Detalles), new { id = model.IdContrato });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, "No se pudo actualizar el contrato. " + ex.Message);
            }

            await CargarSelectsAsync(model.IdInmueble, model.IdInquilino, ct);
            return View(model);
        }

        // ------------------- ELIMINAR (Confirmación) -------------------
        [HttpGet]
        public async Task<IActionResult> Eliminar(int id, CancellationToken ct = default)
        {
            var contrato = await _repo.GetByIdAsync(id, ct);
            if (contrato == null) return NotFound();
            return View(contrato); // vista de confirmación
        }

        // ------------------- BORRAR (Soft Delete) -------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrar(int id, CancellationToken ct = default)
        {
            var userId = GetUserIdOrDefault();
            var ok = await _repo.SoftDeleteAsync(id, userId, ct);
            if (!ok) return NotFound();

            TempData["Ok"] = "Contrato eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // =================== Helpers ===================

        private int GetUserIdOrDefault()
        {
            // Ajustá este claim según cómo guardes el Id de usuario
            var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(str, out var id) ? id : 1; // fallback 1
        }

        private async Task CargarSelectsAsync(int? idInmueble, int? idInquilino, CancellationToken ct)
        {
            // Si tus entidades tienen otros nombres de campos, ajustá aquí:
            var inmuebles = await _db.Inmuebles
                .AsNoTracking()
                .Select(i => new { i.IdInmueble, i.Direccion })
                .OrderBy(i => i.Direccion)
                .ToListAsync(ct);

            var inquilinos = await _db.Inquilinos
                .AsNoTracking()
                .Select(q => new { q.IdInquilino, q.ApellidoNombres })
                .OrderBy(q => q.ApellidoNombres)
                .ToListAsync(ct);

            ViewBag.Inmuebles = new SelectList(inmuebles, "IdInmueble", "Direccion", idInmueble);
            ViewBag.Inquilinos = new SelectList(inquilinos, "IdInquilino", "ApellidoNombres", idInquilino);
        }
    }
}
