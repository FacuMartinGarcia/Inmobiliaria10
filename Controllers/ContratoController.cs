using System.Security.Claims;
using Inmobiliaria10.Data.Repositories;
using Inmobiliaria10.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        public async Task<IActionResult> Index(
            int? tipo,
            int? inmueble,
            int? inquilino,
            bool? soloActivos,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default)
        {
            // Selects para filtro del index
            await CargarSelectsIndexAsync(tipo, inmueble, inquilino, ct);
            ViewBag.TipoSel = tipo;

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

            return View(items);
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
            model.CreatedBy = GetUserIdOrDefault();
            model.CreatedAt = DateTime.UtcNow;

            ModelState.Remove(nameof(Contrato.CreatedBy));
            ModelState.Remove(nameof(Contrato.CreatedAt));

            if (!ModelState.IsValid)
            {
                await CargarSelectsAsync(model.IdInmueble, model.IdInquilino, ct);
                return View(model);
            }

            try
            {
                await _repo.CreateAsync(model, ct);
                TempData["Ok"] = "Contrato creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "No se pudo guardar el contrato. " + ex.Message);
                await CargarSelectsAsync(model.IdInmueble, model.IdInquilino, ct);
                return View(model);
            }
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
            if (id != model.IdContrato)
            {
                ModelState.AddModelError(string.Empty, "El ID del contrato no coincide.");
                await CargarSelectsAsync(model.IdInmueble, model.IdInquilino, ct);
                return View(model);
            }

            // Permitir que los campos opcionales nulos no rompan la validación
            ModelState.Remove(nameof(Contrato.Rescision));
            ModelState.Remove(nameof(Contrato.MontoMulta));

            if (!ModelState.IsValid)
            {
                await CargarSelectsAsync(model.IdInmueble, model.IdInquilino, ct);
                return View(model);
            }

            try
            {
                // Ejecutar update y verificar filas afectadas
                await _repo.UpdateAsync(model, ct);
                  TempData["Ok"] = "Contrato actualizado correctamente.";
                  return RedirectToAction(nameof(Detalles), new { id = model.IdContrato });
              
            }
            catch (Exception ex)
            {
                // Mostrar mensaje de error completo
                ModelState.AddModelError(string.Empty, $"No se pudo actualizar el contrato. {ex.Message}\n{ex.StackTrace}");
                await CargarSelectsAsync(model.IdInmueble, model.IdInquilino, ct);
                return View(model);
            }
        }


/*
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
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "No se pudo actualizar el contrato. " + ex.Message);
                await CargarSelectsAsync(model.IdInmueble, model.IdInquilino, ct);
                return View(model);
            }
        }
*/
        // ------------------- DETALLES -------------------
        public async Task<IActionResult> Detalles(int id, CancellationToken ct = default)
        {
            var contrato = await _repo.GetByIdAsync(id, ct);
            if (contrato == null) return NotFound();

            await SetContratoEtiquetasAsync(contrato, ct);
            return View(contrato);
        }

        // ------------------- ELIMINAR -------------------
        [HttpGet]
        public async Task<IActionResult> Eliminar(int id, CancellationToken ct = default)
        {
            var contrato = await _repo.GetByIdAsync(id, ct);
            if (contrato == null) return NotFound();

            await SetContratoEtiquetasAsync(contrato, ct);
            return View(contrato);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrar(int id, CancellationToken ct = default)
        {
            var ok = await _repo.SoftDeleteAsync(id, GetUserIdOrDefault(), ct);

            TempData[ ok ? "Ok" : "Err" ] = ok
                ? "Contrato eliminado correctamente."
                : "El contrato no existe o ya fue eliminado.";

            return RedirectToAction(nameof(Index));
        }

        // =================== HELPERS ===================

        private async Task SetContratoEtiquetasAsync(Contrato contrato, CancellationToken ct)
        {
            var inm = await _repoInmueble.ObtenerPorId(contrato.IdInmueble);

            string inmuebleTxt = contrato.IdInmueble.ToString();
            if (inm != null)
            {
                var sb = new System.Text.StringBuilder();
                if (!string.IsNullOrWhiteSpace(inm.Direccion)) sb.Append(inm.Direccion);
                if (!string.IsNullOrWhiteSpace(inm.Piso)) sb.Append(" Piso ").Append(inm.Piso);
                if (!string.IsNullOrWhiteSpace(inm.Depto)) sb.Append(" Dpto ").Append(inm.Depto);

                var texto = sb.ToString();
                if (!string.IsNullOrWhiteSpace(texto)) inmuebleTxt = texto;
            }
            ViewBag.InmuebleTxt = inmuebleTxt;

            string inquilinoTxt = contrato.IdInquilino.ToString();
            var inqs = await _repo.GetInquilinosAsync(ct);
            if (inqs != null)
            {
                foreach (var item in inqs)
                {
                    if (item.Id == contrato.IdInquilino && !string.IsNullOrWhiteSpace(item.Nombre))
                    {
                        inquilinoTxt = item.Nombre;
                        break;
                    }
                }
            }
            ViewBag.InquilinoTxt = inquilinoTxt;
        }

        private int GetUserIdOrDefault()
        {
            var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(str, out var id) ? id : 1;
        }

        // ------------------- CARGA SELECTS -------------------

        // Para Crear/Editar: solo inmuebles activos
        private async Task CargarSelectsAsync(int? idInmueble, int? idInquilino, CancellationToken ct)
        {
            var inmuebles = (await _repoInmueble.ListarTodos())
                .Where(i => i.Activo)
                .Select(i => new SelectListItem
                {
                    Value = i.IdInmueble.ToString(),
                    Text = $"{i.Direccion}" +
                           (string.IsNullOrWhiteSpace(i.Piso) ? "" : $" Piso {i.Piso}") +
                           (string.IsNullOrWhiteSpace(i.Depto) ? "" : $" Dpto {i.Depto}")
                })
                .OrderBy(x => x.Text)
                .ToList();
            ViewBag.Inmuebles = new SelectList(inmuebles, "Value", "Text", idInmueble?.ToString());

            var inquilinos = await _repo.GetInquilinosAsync(ct);
            ViewBag.Inquilinos = new SelectList(
                inquilinos.Select(x => new { Id = x.Id, Texto = x.Nombre }),
                "Id", "Texto", idInquilino
            );
        }

        // Para Index: filtra por tipo
        private async Task CargarSelectsIndexAsync(int? tipo, int? idInmueble, int? idInquilino, CancellationToken ct)
        {
            // Tipos
            var tipos = _repoTipo.MostrarTodosInmuebleTipos()
                .Select(t => new SelectListItem
                {
                    Value = t.IdTipo.ToString(),
                    Text = t.DenominacionTipo
                })
                .OrderBy(x => x.Text)
                .ToList();
            ViewBag.Tipos = new SelectList(tipos, "Value", "Text", tipo?.ToString());

            // Inmuebles filtrados por tipo (si aplica)
            var inmuebles = (await _repoInmueble.ListarTodos())
                .Where(i => !tipo.HasValue || i.IdTipo == tipo)
                .Select(i => new SelectListItem
                {
                    Value = i.IdInmueble.ToString(),
                    Text = $"{i.Direccion}" +
                           (string.IsNullOrWhiteSpace(i.Piso) ? "" : $" Piso {i.Piso}") +
                           (string.IsNullOrWhiteSpace(i.Depto) ? "" : $" Dpto {i.Depto}")
                })
                .OrderBy(x => x.Text)
                .ToList();
            ViewBag.Inmuebles = new SelectList(inmuebles, "Value", "Text", idInmueble?.ToString());

            // Inquilinos
            var inquilinos = await _repo.GetInquilinosAsync(ct);
            ViewBag.Inquilinos = new SelectList(
                inquilinos.Select(x => new { Id = x.Id, Texto = x.Nombre }),
                "Id", "Texto", idInquilino
            );
        }
    }
}
