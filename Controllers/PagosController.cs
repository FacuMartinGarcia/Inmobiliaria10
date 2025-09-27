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
        private readonly IMesRepo _mesRepo;

        public PagosController(IPagoRepo repo, IContratoRepo contratoRepo, IMesRepo mesRepo)
        {
            _repo = repo;
            _contratoRepo = contratoRepo;
            _mesRepo = mesRepo;
        }
    
        [HttpGet("")]
        public async Task<IActionResult> Index(
            int? contrato, int? inquilino, int? concepto, bool? soloActivos,
            CancellationToken ct = default)
        {
            await CargarSelectsAsync(concepto, null, null, ct);
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
            int pageIndex = Math.Max(1, (start / Math.Max(1, length)) + 1);
            int pageSize  = Math.Max(1, length);

            var (items, total) = await _repo.ListAsync(
                idContrato : contrato,
                idConcepto : null,
                soloActivos: null,
                pageIndex  : pageIndex,
                pageSize   : pageSize,
                ct         : ct,
                idInquilino: inquilino
            );

            var conceptos   = await _repo.GetConceptosAsync(ct);
            var conceptoMap = conceptos.ToDictionary(x => x.Id, x => x.Nombre);

            var contratoIds   = items.Select(p => p.IdContrato).Distinct().ToList();
            var contratosInfo = await _contratoRepo.GetContratosInfoAsync(contratoIds, ct);
            var contratoMap   = contratosInfo.ToDictionary(c => c.Id, c => $"{c.Direccion} - {c.Inquilino}");

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
            var hoy = DateTime.Today;

            await CargarSelectsAsync(idConcepto: null, idMes: hoy.Month, anio: hoy.Year, ct);

            return View(new Pago {
                IdContrato = contrato ?? 0,
                FechaPago  = hoy,
                IdMes      = hoy.Month,
                Anio       = hoy.Year
            });
        }

        [HttpPost("Crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Pago m, CancellationToken ct = default)
        {
            var contrato = await _contratoRepo.GetByIdAsync(m.IdContrato, ct);
            if (contrato == null || contrato.Rescision.HasValue || contrato.DeletedAt.HasValue)
            {
                TempData["Error"] = "No se puede registrar un pago en un contrato caducado o rescindido.";
                await CargarSelectsAsync(m.IdConcepto, m.IdMes, m.Anio, ct);
                return View(m);
            }

            if (!ModelState.IsValid)
            { 
                await CargarSelectsAsync(m.IdConcepto, m.IdMes, m.Anio, ct); 
                return View(m); 
            }

            m.CreatedBy = GetUserIdOrDefault(); 
            m.CreatedAt = DateTime.UtcNow;

            var id = await _repo.CreateAsync(m, ct);

            TempData["Mensaje"] = "Pago registrado.";
            return RedirectToAction(nameof(Detalles), new { id });
        }

        [HttpGet("Editar/{id:int}")]
        public async Task<IActionResult> Editar(int id, CancellationToken ct = default)
        {
            var pago = await _repo.GetByIdAsync(id, ct);
            if (pago == null) return NotFound();

            await CargarSelectsAsync(pago.IdConcepto, pago.IdMes, pago.Anio, ct);

            var contrato = await _repo.GetContratoItemAsync(pago.IdContrato, ct);
            ViewBag.ContratoTexto = contrato?.Text;

            // 🔹 Obtener nombre del mes
            var meses = await _mesRepo.GetAllAsync(ct);
            ViewBag.MesTexto = meses.FirstOrDefault(m => m.IdMes == pago.IdMes)?.Nombre;

            // 🔹 Pasar el año como texto
            ViewBag.AnioTexto = pago.Anio.ToString();

            return View(pago);
        }

        [HttpPost("Editar/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Pago m, CancellationToken ct = default)
        {
            if (id != m.IdPago) return BadRequest();

            // Validar contrato (opcional, por si está rescindido)
            var contrato = await _contratoRepo.GetByIdAsync(m.IdContrato, ct);
            if (contrato == null || contrato.Rescision.HasValue || contrato.DeletedAt.HasValue)
            {
                TempData["Error"] = "No se puede editar un pago de un contrato caducado o rescindido.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                
                var originalPago = await _repo.GetByIdAsync(id, ct);
                if (originalPago == null) return NotFound();
                
                originalPago.IdConcepto = m.IdConcepto;
                originalPago.Detalle = m.Detalle;
                originalPago.MotivoAnulacion = m.MotivoAnulacion;
                
                await CargarSelectsAsync(originalPago.IdConcepto, originalPago.IdMes, originalPago.Anio, ct);
                
                var contratoItem = await _repo.GetContratoItemAsync(originalPago.IdContrato, ct);
                ViewBag.ContratoTexto = contratoItem?.Text;

                var meses = await _mesRepo.GetAllAsync(ct);
                ViewBag.MesTexto = meses.FirstOrDefault(mes => mes.IdMes == originalPago.IdMes)?.Nombre;

                ViewBag.AnioTexto = originalPago.Anio.ToString();
                
                return View(originalPago);
            }

            var ok = await _repo.UpdateConceptoAsync(m, ct);
            if (!ok) return NotFound();

            TempData["Mensaje"] = "Pago actualizado correctamente.";
            return RedirectToAction(nameof(Detalles), new { id = m.IdPago });
        }

        [HttpGet("Eliminar/{id:int}")]
        public async Task<IActionResult> Eliminar(int id, CancellationToken ct = default)
        {
            var vm = await _repo.GetDetalleAsync(id, ct);
            return vm is null ? NotFound() : View(vm);
        }
       
        [HttpPost("Eliminar/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id, string motivo, CancellationToken ct = default)
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

        [HttpPost("Borrar/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrar(int id, CancellationToken ct = default)
        {
            var ok = await _repo.SoftDeleteAsync(id, GetUserIdOrDefault(), ct);
            if (!ok) return NotFound();
            TempData["Mensaje"] = "Pago eliminado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Auditoria")]
        public async Task<IActionResult> Auditoria(
            [FromServices] IMesRepo mesRepo, 
            CancellationToken ct = default)
        {
            var list = await _repo.GetAuditoriaGeneralAsync(ct);

            ViewBag.Conceptos = (await _repo.GetConceptosAsync(ct))
                .ToDictionary(x => x.Id.ToString(), x => x.Nombre);

            ViewBag.Meses = (await mesRepo.GetAllAsync(ct))
                .ToDictionary(x => x.IdMes.ToString(), x => x.Nombre);

            return View(list);
        }

        [HttpGet("data-auditoria")]
        public async Task<IActionResult> DataAuditoria(
            int? usuario, int? contrato, int? concepto,
            int draw, int start = 0, int length = 10,
            CancellationToken ct = default)
        {
            int pageIndex = Math.Max(1, (start / Math.Max(1, length)) + 1);
            int pageSize  = Math.Max(1, length);

            var (items, total) = await _repo.ListAuditoriaAsync(
                usuarioId : usuario,
                contratoId: contrato,
                conceptoId: concepto,
                pageIndex : pageIndex,
                pageSize  : pageSize,
                ct        : ct
            );

            var data = items.Select(a => new {
                accion    = a.Accion == "INSERT" ? "Alta" :
                        a.Accion == "UPDATE" ? "Modificación" :
                        a.Accion == "DELETE" ? "Borrado" : a.Accion,
                fecha     = a.AccionAt.ToString("dd/MM/yyyy HH:mm"),
                usuario   = a.Usuario,
                oldData   = string.Join("<br/>", (a.OldData ?? new Dictionary<string,string>())
                                                .Select(kv => $"<b>{kv.Key}:</b> {kv.Value}")),
                newData   = string.Join("<br/>", (a.NewData ?? new Dictionary<string,string>())
                                                .Select(kv => $"<b>{kv.Key}:</b> {kv.Value}"))
            }).ToList();

            return Json(new {
                draw,
                recordsTotal = total,
                recordsFiltered = total,
                data
            });
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

        [HttpGet("Morosos")]
        public async Task<IActionResult> Morosos(int? inquilino, int page = 1, CancellationToken ct = default)
        {
            const int pageSize = 10;
            var (items, total) = await _repo.GetMorososAsync(inquilino, page, pageSize, ct);

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;
            ViewBag.Inquilino = inquilino; // guardamos el id para reusar en la vista

            return View(items);
        }

        [HttpGet("BuscarUsuarios")]
        public async Task<IActionResult> BuscarUsuarios(string? term, int take = 20, CancellationToken ct = default)
        {
            var items = await _repo.SearchUsuariosAsync(term, take, ct);
            return Json(new { results = items.Select(x => new { id = x.Id, text = x.Text }) });
        }


        [HttpGet("BuscarInquilinos")]
        public async Task<IActionResult> BuscarInquilinos(string? term, CancellationToken ct)
        {
            var items = await _repo.SearchInquilinosAsync(term, 20, ct);
            return Json(new { results = items.Select(x => new { id = x.Id, text = x.Text }) });
        }

        // --- Helpers ---
        private async Task CargarSelectsAsync(int? idConcepto, int? idMes, int? anio, CancellationToken ct)
        {
            var conceptos = await _repo.GetConceptosAsync(ct);
            ViewBag.Conceptos = new SelectList(
                conceptos.Select(c => new { IdConcepto = c.Id, Nombre = c.Nombre }),
                "IdConcepto", "Nombre", idConcepto);

            var meses = await _mesRepo.GetAllAsync(ct);
            ViewBag.Meses = new SelectList(meses, "IdMes", "Nombre", idMes);

            var anioActual = DateTime.Today.Year;
            var anios = Enumerable.Range(anioActual - 5, 11); // ints
            ViewBag.Anios = new SelectList(anios, anio ?? anioActual);
        }

        private int GetUserIdOrDefault()
        {
            var s = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(s, out var id) ? id : 1;
        }
    }
}
