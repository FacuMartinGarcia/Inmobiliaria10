using System.Security.Claims;
using Inmobiliaria10.Data.Repositories;
using Inmobiliaria10.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;


namespace Inmobiliaria10.Controllers
{
    [Authorize] 
    public class ContratoController : Controller
    {
        private readonly IContratoRepo _repo;
        private readonly IInmuebleRepo _repoInmueble;
        private readonly IInmuebleTipoRepo _repoTipo;

        private readonly IPagoRepo _repoPago;

        public ContratoController(
            IContratoRepo repo,
            IInmuebleRepo repoInmueble,
            IInmuebleTipoRepo repoTipo,
            IPagoRepo repoPago)
        {
            _repo = repo;
            _repoInmueble = repoInmueble;
            _repoTipo = repoTipo;
            _repoPago = repoPago;
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

            var inmuebles = await _repoInmueble.ListarTodos();
            ViewBag.TipoMap = inmuebles.ToDictionary(
                i => i.IdInmueble.ToString(),
                i => i.Tipo?.DenominacionTipo ?? "-"
);


            var (items, total) = await _repo.ListAsync(
                tipo: tipo,
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
                TempData["Mensaje"] = "Contrato creado correctamente.";
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
        public async Task<IActionResult> Editar(int id, Contrato model, bool generarPagoMulta = false, CancellationToken ct = default)
        {
            if (id != model.IdContrato)
            {
                ModelState.AddModelError(string.Empty, "El ID del contrato no coincide.");
                await CargarSelectsAsync(model.IdInmueble, model.IdInquilino, ct);
                return View(model);
            }

            // Si no hay rescisión, anulamos multa
            if (!model.Rescision.HasValue)
            {
                model.MontoMulta = null;
            }

            if (!ModelState.IsValid)
            {
                await CargarSelectsAsync(model.IdInmueble, model.IdInquilino, ct);
                return View(model);
            }

            try
            {
                await _repo.UpdateAsync(model, ct);

                // Lógica de generación de pago de multa
                if (model.Rescision.HasValue && generarPagoMulta && model.MontoMulta > 0)
                {
                    // Validar que no exista pago anterior de multa
                    
                    bool existePago = false; 
                    if (!existePago)
                    {
                        var pagoMulta = new Pago
                        {
                            IdContrato = model.IdContrato,
                            IdConcepto = 2, 
                            FechaPago = DateTime.Today,
                            Importe = model.MontoMulta.GetValueOrDefault(),
                            Detalle = $"Multa por rescisión del contrato {model.IdContrato}",
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = GetUserIdOrDefault()
                        };
                        await _repoPago.CreateAsync(pagoMulta, ct);
                    }
                }

                TempData["Ok"] = "Contrato actualizado correctamente.";
                return RedirectToAction("Detalles", new { id = model.IdContrato });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"No se pudo actualizar el contrato. {ex.Message}");
                await CargarSelectsAsync(model.IdInmueble, model.IdInquilino, ct);
                return View(model);
            }
        }
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

            TempData[ok ? "Ok" : "Err"] = ok
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

        [HttpGet]
        public async Task<IActionResult> SearchInquilinos(string? term, int? id, CancellationToken ct = default)
        {
            var inquilinos = await _repo.GetInquilinosAsync(ct);

            if (id.HasValue)
            {
                var item = inquilinos.FirstOrDefault(x => x.Id == id.Value);
                if (item.Id > 0)
                    return Json(new { item = new { id = item.Id, text = item.Nombre } });
                return Json(new { item = (object?)null });
            }

            var results = inquilinos
                .Where(x => string.IsNullOrEmpty(term) || x.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(x => new { id = x.Id, text = x.Nombre })
                .Take(20)
                .ToList();

            return Json(new { results });
        }



        [HttpGet]
        public async Task<IActionResult> SearchInmuebles(string? term, int? id, CancellationToken ct = default)
        {
            var inmuebles = await _repoInmueble.ListarTodos();

            if (id.HasValue)
            {
                var item = inmuebles.FirstOrDefault(x => x.IdInmueble == id.Value);
                if (item != null)
                {
                    var txt = $"{item.Direccion}" +
                            (string.IsNullOrWhiteSpace(item.Piso) ? "" : $" Piso {item.Piso}") +
                            (string.IsNullOrWhiteSpace(item.Depto) ? "" : $" Dpto {item.Depto}") +
                            $" - ${item.Precio:0.00}";

                    return Json(new
                    {
                        item = new
                        {
                            id = item.IdInmueble,
                            text = txt,
                            piso = item.Piso,
                            depto = item.Depto,
                            precio = item.Precio
                        }
                    });
                }
                return Json(new { item = (object?)null });
            }

            var results = inmuebles
                .Where(x => string.IsNullOrEmpty(term) ||
                            x.Direccion.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(x => new
                {
                    id = x.IdInmueble,
                    text = $"{x.Direccion}" +
                        (string.IsNullOrWhiteSpace(x.Piso) ? "" : $" Piso {x.Piso}") +
                        (string.IsNullOrWhiteSpace(x.Depto) ? "" : $" Dpto {x.Depto}") +
                        $" - ${x.Precio:0.00}",
                    piso = x.Piso,
                    depto = x.Depto,
                    precio = x.Precio
                })
                .Take(20)
                .ToList();

            return Json(new { results });
        }
        // Para Crear/Editar: solo inmuebles activos
        private async Task CargarSelectsAsync(int? idInmueble, int? idInquilino, CancellationToken ct)
        {
            var inmuebles = (await _repoInmueble.ListarTodos())
                .Where(i => i.Activo)
                .ToList();

            ViewBag.Inmuebles = new SelectList(
                inmuebles,
                "IdInmueble",
                "Direccion",
                idInmueble?.ToString()
            );

            // Creamos la lista completa para iterar en el Razor y agregar datos adicionales
            ViewBag.InmueblesRaw = inmuebles.Select(i => new
            {
                i.IdInmueble,
                i.Direccion,
                i.Piso,
                i.Depto,
                i.Precio,
                DenominacionTipo = i.Tipo?.DenominacionTipo ?? ""
            }).ToList();
            //ViewBag.InmueblesRaw = inmuebles;

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

        [HttpPost]
        public async Task<IActionResult> CalcularMulta(int idContrato, DateTime fechaRescision)
        {
            try
            {
                Console.WriteLine($"Recibido idContrato={idContrato}, fechaRescision={fechaRescision}");

                var multa = await _repo.CalcularMultaAsync(idContrato, fechaRescision);
                return Json(new { ok = true, multa });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }
    }

}
