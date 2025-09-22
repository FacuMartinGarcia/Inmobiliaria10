using System.Security.Claims;
using Inmobiliaria10.Data.Repositories;
using Inmobiliaria10.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;


namespace Inmobiliaria10.Controllers
{
    [Authorize]
    [Route("Pagos")]
    public class PagosController : Controller
    {
        private readonly IPagoRepo _repo;
        private readonly IContratoRepo _contratoRepo; 

        public PagosController(IPagoRepo repo, IContratoRepo contratoRepo) 
        {
            _repo = repo;
            _contratoRepo = contratoRepo;               
        }
        // Index: solo prepara filtros; la grilla se llena por AJAX (DataTables)
        [HttpGet("")]
        public async Task<IActionResult> Index(
            int? contrato, int? inquilino, int? concepto, bool? soloActivos,
            CancellationToken ct = default)
        {
            await CargarSelectsAsync(concepto, ct);
            ViewBag.Contrato = contrato;
            ViewBag.Inquilino = inquilino;
            ViewBag.SoloActivos = soloActivos;
            return View(Enumerable.Empty<Pago>());
        }

        // DataTables server-side
        [HttpGet("data")]
        public async Task<IActionResult> Data(
            int? contrato, int? inquilino,
            int draw, int start = 0, int length = 10,
            CancellationToken ct = default)
        {
            // Calcular paginación
            int pageIndex = Math.Max(1, (start / Math.Max(1, length)) + 1);
            int pageSize  = Math.Max(1, length);

            // Traer datos desde el repo
            var (items, total) = await _repo.ListAsync(
                idContrato : contrato,
                idConcepto : null,
                soloActivos: null,
                pageIndex  : pageIndex,
                pageSize   : pageSize,
                ct         : ct,
                idInquilino: inquilino
            );

            // Conceptos para mapear Id -> Nombre
            var conceptos   = await _repo.GetConceptosAsync(ct);
            var conceptoMap = conceptos.ToDictionary(x => x.Id, x => x.Nombre);

            // Contratos de la página actual
            var contratoIds   = items.Select(p => p.IdContrato).Distinct().ToList();
            var contratosInfo = await _contratoRepo.GetContratosInfoAsync(contratoIds, ct);
            var contratoMap   = contratosInfo.ToDictionary(c => c.Id, c => $"{c.Direccion} - {c.Inquilino}");

            // Orden recibido desde DataTables
            var orderColIdx = int.TryParse(Request.Query["order[0][column]"], out var oc) ? oc : 0;
            var orderDir    = (Request.Query["order[0][dir]"].FirstOrDefault() ?? "desc").ToLower();

            Func<Pago, object?> keySelector = orderColIdx switch
            {
                0 => p => p.FechaPago,
                1 => p => p.Detalle,
                2 => p => conceptoMap.TryGetValue(p.IdConcepto, out var nom) ? nom : "",
                3 => p => p.Importe,
                4 => p => contratoMap.TryGetValue(p.IdContrato, out var txt) ? txt : $"Contrato #{p.IdContrato}",
                5 => p => p.DeletedAt.HasValue ? "Eliminado" : "Activo",
                _ => p => p.FechaPago
            };

            var ordered = (orderDir == "asc")
                ? items.OrderBy(keySelector)
                : items.OrderByDescending(keySelector);

            // Proyección al objeto que DataTables consume
            var data = ordered.Select(p => new
            {
                idPago    = p.IdPago,
                fechaPago = p.FechaPago.ToString("dd/MM/yyyy"),
                mes       = p.IdMes.HasValue 
                    ? System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(p.IdMes.Value) 
                    : "",
                anio      = p.Anio,
                detalle   = p.Detalle,
                conceptoDenominacion = conceptoMap.TryGetValue(p.IdConcepto, out var nom) ? nom : "",
                importe   = p.Importe,
                contratoTexto = contratoMap.TryGetValue(p.IdContrato, out var txt) ? txt : $"Contrato #{p.IdContrato}",
                estado = p.DeletedAt.HasValue ? "Eliminado" : "Activo"
            }).ToList();

            // 🔹 Devolver el mismo draw que vino en la request
            var safeDraw = draw < 0 ? 0 : draw;

            return Json(new {
                draw = safeDraw,
                recordsTotal = total,
                recordsFiltered = total,
                data
            });
        }


        [HttpGet("Detalles/{id:int}")]
        public async Task<IActionResult> Detalles(int id, CancellationToken ct = default)
        {
            var vm = await _repo.GetDetalleAsync(id, ct);
            if (vm == null) return NotFound();
            return View(vm);
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
            if (!ModelState.IsValid) 
            { 
                await CargarSelectsAsync(m.IdConcepto, ct); 
                return View(m); 
            }

            // 🔹 Calcular mes y año automáticamente
            m.IdMes = m.FechaPago.Month;   // 1–12 → coincide con tabla meses
            m.Anio = m.FechaPago.Year;

            m.CreatedBy = GetUserIdOrDefault(); 
            m.CreatedAt = DateTime.UtcNow;

            var id = await _repo.CreateAsync(m, ct);

            TempData["Mensaje"] = "Pago registrado.";
            return RedirectToAction(nameof(Detalles), new { id });
        }


        [HttpPost("RegistrarMulta/{contratoId:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarMulta(int contratoId, CancellationToken ct = default)
        {
            var idPago = await _repo.RegistrarMultaAsync(contratoId, DateTime.Today, GetUserIdOrDefault(), ct);
            TempData["Mensaje"] = "Multa registrada como pago.";
            return RedirectToAction("Detalles", "Pagos", new { id = idPago });
        }

        [HttpGet("Editar/{id:int}")]
        public async Task<IActionResult> Editar(int id, CancellationToken ct = default)
        {
            var pago = await _repo.GetByIdAsync(id, ct);
            if (pago == null) return NotFound();

            await CargarSelectsAsync(pago.IdConcepto, ct); 

            var contrato = await _repo.GetContratoItemAsync(pago.IdContrato, ct);
            ViewBag.ContratoTexto = contrato?.Text;

            return View(pago);
        }

        [HttpPost("Editar/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Pago m, CancellationToken ct = default)
        {
            if (id != m.IdPago) return BadRequest();
            if (!ModelState.IsValid)
            { 
                await CargarSelectsAsync(m.IdConcepto, ct); 
                return View(m); 
            }

            // 🔹 Recalcular mes y año si cambió la fecha
            m.IdMes = m.FechaPago.Month;
            m.Anio = m.FechaPago.Year;

            await _repo.UpdateAsync(m, ct);

            TempData["Mensaje"] = "Pago actualizado correctamente.";
            return RedirectToAction(nameof(Detalles), new { id = m.IdPago });
        }

        [HttpPost("Anular/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Anular(int id, string motivo, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(motivo))
            {
                TempData["Error"] = "Debe ingresar un motivo para anular el pago.";
                return RedirectToAction(nameof(Eliminar), new { id });
            }

            var ok = await _repo.AnularPagoAsync(id, motivo, GetUserIdOrDefault(), ct);
            if (!ok) return NotFound();

            TempData["Mensaje"] = "Pago anulado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Editar")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> EditarSinId(Pago m, CancellationToken ct = default)
            => Editar(m.IdPago, m, ct);

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
            TempData["Mensaje"] = "Pago eliminado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Auditoria/{id:int}")]
        public async Task<IActionResult> Auditoria(int id, CancellationToken ct = default)
        {
            var eventos = await _repo.GetAuditoriaAsync(id, ct);
            ViewBag.IdPago = id;
            return View(eventos);
        }

        // --- Endpoints Select2 ---

        [HttpGet("search-contratos")]
        public async Task<IActionResult> SearchContratos(string? term, int take = 20, int? id = null, CancellationToken ct = default)
        {
            if (id is > 0)
            {
                var it = await _repo.GetContratoItemAsync(id.Value, ct);
                return Json(new { item = it.HasValue ? new { id = it.Value.Id, text = it.Value.Text } : null });
            }
            var items = await _repo.SearchContratosAsync(term, take, ct);
            return Json(new { results = items.Select(x => new { id = x.Id, text = x.Text }) });
        }

        [HttpGet("search-contratos-by-inquilino")]
        public async Task<IActionResult> SearchContratosByInquilino(int idInquilino, string? term, int take = 20, CancellationToken ct = default)
        {
            var items = await _repo.SearchContratosPorInquilinoAsync(idInquilino, term, take, soloVigentesActivos: true, ct);
            return Json(new { results = items.Select(x => new { id = x.Id, text = x.Text }) });
        }

        // --- Helpers ---
        private async Task CargarSelectsAsync(int? idConcepto, CancellationToken ct)
        {
            var conceptos = await _repo.GetConceptosAsync(ct);
            ViewBag.Conceptos = new SelectList(conceptos.Select(x => new { IdConcepto = x.Id, Nombre = x.Nombre }),
                                               "IdConcepto", "Nombre", idConcepto);
        }

        private int GetUserIdOrDefault()
        {
            var s = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(s, out var id) ? id : 1;
        }
    }
}
