using System.Security.Claims;
using Inmobiliaria10.Data.Repositories;
using Inmobiliaria10.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inmobiliaria10.Controllers
{
    public class ContratoController : Controller
    {
        private readonly IContratoRepo _repo;
        private readonly IInmuebleRepo _repoInmueble;
        private readonly IInmuebleTipoRepo _repoTipo;

        public ContratoController(
            IContratoRepo repo,
            IInmuebleRepo repoInmueble,
            IInmuebleTipoRepo repoTipo)
        {
            _repo = repo;
            _repoInmueble = repoInmueble;
            _repoTipo = repoTipo;
        }

        // ------------------- INDEX -------------------
        // /Contrato?tipo=1&inmueble=10&inquilino=2&soloActivos=true&page=1&pageSize=20
        public async Task<IActionResult> Index(
            int? tipo,
            int? inmueble,
            int? inquilino,
            bool? soloActivos,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default)
        {
            await CargarSelectsAsync(tipo, inmueble, inquilino, ct);

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
            ViewBag.TipoSel = tipo;

            return View(items);
        }

        // ------------------- CREAR -------------------
        [HttpGet]
        public async Task<IActionResult> Crear(int? tipo, int? inmueble, int? inquilino, CancellationToken ct = default)
        {
            // Pre-cargamos combos (si viene ?tipo= lo usamos para filtrar Inmuebles)
            await CargarSelectsAsync(tipo, inmueble, inquilino, ct);
            ViewBag.TipoSel = tipo; // para que el JS del form sepa el tipo seleccionado

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
            // 1) leemos el tipo desde el form para reconstruir la cascada si falla
            int? tipoForm = null;
            if (int.TryParse(Request.Form["tipo"], out var _tipo)) tipoForm = _tipo;

            if (!tipoForm.HasValue && model.IdInmueble > 0)
            {
                var inm = _repoInmueble.ObtenerPorId(model.IdInmueble); // sync en tu repo
                tipoForm = inm?.IdTipo;
            }

            // 2) Asignar audit fields ANTES de validar y limpiar entradas previas del binder
            model.CreatedBy = GetUserIdOrDefault();
            model.CreatedAt = DateTime.UtcNow;

            ModelState.Remove(nameof(Contrato.CreatedBy));
            ModelState.Remove(nameof(Contrato.CreatedAt));

            // 3) Validar y, si falla, reconstruir combos manteniendo tipo/inmueble/inq
            if (!ModelState.IsValid)
            {
                await CargarSelectsAsync(tipoForm, model.IdInmueble, model.IdInquilino, ct);
                ViewBag.TipoSel = tipoForm;
                return View(model);
            }

            // 4) Guardar
            try
            {
                await _repo.CreateAsync(model, ct);
                TempData["Ok"] = "Contrato creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "No se pudo guardar el contrato. " + ex.Message);
                await CargarSelectsAsync(tipoForm, model.IdInmueble, model.IdInquilino, ct);
                ViewBag.TipoSel = tipoForm;
                return View(model);
            }
        }


        // ------------------- EDITAR -------------------
        [HttpGet]
        public async Task<IActionResult> Editar(int id, CancellationToken ct = default)
        {
            var contrato = await _repo.GetByIdAsync(id, ct);
            if (contrato == null) return NotFound();

            // ObtenerPorId es SINCRÓNICO → NO usar await aquí
            var inm = _repoInmueble.ObtenerPorId(contrato.IdInmueble);
            int? tipo = inm?.IdTipo;

            await CargarSelectsAsync(tipo, contrato.IdInmueble, contrato.IdInquilino, ct);
            return View(contrato);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Contrato model, CancellationToken ct = default)
        {
            if (id != model.IdContrato) return BadRequest();

            if (!ModelState.IsValid)
            {
                await CargarSelectsAsync(null, model.IdInmueble, model.IdInquilino, ct);
                return View(model);
            }

            try
            {
                await _repo.UpdateAsync(model, ct);
                TempData["Ok"] = "Contrato actualizado correctamente.";
                return RedirectToAction(nameof(Detalles), new { id = model.IdContrato });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "No se pudo actualizar el contrato. " + ex.Message);
            }

            await CargarSelectsAsync(null, model.IdInmueble, model.IdInquilino, ct);
            return View(model);
        }

        // ------------------- DETALLES / ELIMINAR -------------------
        public async Task<IActionResult> Detalles(int id, CancellationToken ct = default)
        {
            var contrato = await _repo.GetByIdAsync(id, ct);
            if (contrato == null) return NotFound();
            return View(contrato);
        }

        [HttpGet]
        public async Task<IActionResult> Eliminar(int id, CancellationToken ct = default)
        {
            var contrato = await _repo.GetByIdAsync(id, ct);
            if (contrato == null) return NotFound();
            return View(contrato);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrar(int id, CancellationToken ct = default)
        {
            var ok = await _repo.SoftDeleteAsync(id, GetUserIdOrDefault(), ct);
            if (!ok) return NotFound();
            TempData["Ok"] = "Contrato eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ------------------- API: Inmuebles por Tipo (para AJAX) -------------------
        [HttpGet]
        public async Task<IActionResult> InmueblesPorTipo(int idTipo, CancellationToken ct = default)
        {
            var lista = _repoInmueble
                .ListarTodos() // o un método async si lo tenés
                .Where(x => x.IdTipo == idTipo)
                .Select(x => new
                {
                    id = x.IdInmueble,
                    texto = $"{x.Direccion}"
                            + (string.IsNullOrWhiteSpace(x.Piso) ? "" : $" Piso {x.Piso}")
                            + (string.IsNullOrWhiteSpace(x.Depto) ? "" : $" Dpto {x.Depto}")
                })
                .OrderBy(x => x.texto)
                .ToList();

            return Json(lista);
        }

        // =================== Helpers ===================
        private int GetUserIdOrDefault()
        {
            var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(str, out var id) ? id : 1;
        }

        private async Task CargarSelectsAsync(int? idTipo, int? idInmueble, int? idInquilino, CancellationToken ct)
        {
            // Tipos
            var tipos = _repoTipo.MostrarTodosInmuebleTipos()
                .Select(t => new SelectListItem
                {
                    Value = t.IdTipo.ToString(),
                    Text = t.DenominacionTipo
                })
                .ToList();

            ViewBag.Tipos = new SelectList(tipos, "Value", "Text", idTipo?.ToString());

            // Inmuebles (filtrados si viene tipo)
            var inmuebles = _repoInmueble.ListarTodos()
                .Where(i => !idTipo.HasValue || i.IdTipo == idTipo)
                .Select(i => new SelectListItem
                {
                    Value = i.IdInmueble.ToString(),
                    Text = $"{i.Direccion}"
                           + (string.IsNullOrWhiteSpace(i.Piso) ? "" : $" Piso {i.Piso}")
                           + (string.IsNullOrWhiteSpace(i.Depto) ? "" : $" Dpto {i.Depto}")
                })
                .OrderBy(x => x.Text)
                .ToList();

            ViewBag.Inmuebles = new SelectList(inmuebles, "Value", "Text", idInmueble?.ToString());

            // Inquilinos (igual que venías usando)
            var inquilinos = await _repo.GetInquilinosAsync(ct);
            ViewBag.Inquilinos = new SelectList(
                inquilinos.Select(x => new { Id = x.Id, Texto = x.Nombre }),
                "Id", "Texto", idInquilino
            );
        }

        // ------------------- Auditoría (sin cambios) -------------------
        [HttpGet]
        public async Task<IActionResult> Auditoria(int id, CancellationToken ct = default)
        {
            var contrato = await _repo.GetByIdAsync(id, ct);
            if (contrato == null) return NotFound();

            var eventos = await _repo.GetAuditoriaAsync(id, ct);
            ViewBag.IdContrato = id;
            return View(eventos);
        }
    }
}
