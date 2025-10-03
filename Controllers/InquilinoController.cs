using Microsoft.AspNetCore.Mvc;
using Inmobiliaria10.Models;
using Inmobiliaria10.Data.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace Inmobiliaria10.Controllers
{
    [Authorize]
    public class InquilinoController : Controller
    {
        private readonly IInquilinoRepo _repo;
        private readonly IContratoRepo _repoContratos;

        public InquilinoController(
            IInquilinoRepo repo,
            IContratoRepo repoContratos
        )
        {
            _repo = repo;
            _repoContratos = repoContratos;
            
        }

        // GET: /Inquilino - paginado
        public async Task<IActionResult> Index(int pagina = 1, string? searchString = null, CancellationToken ct = default)
        {
            int cantidadPorPagina = 8;
            var resultado = await _repo.ListarTodosPaginadoAsync(pagina, cantidadPorPagina, searchString, ct);

            ViewData["TotalPaginas"] = (int)Math.Ceiling((double)resultado.totalRegistros / cantidadPorPagina);
            ViewData["PaginaActual"] = pagina;
            ViewData["SearchString"] = searchString;

            return View(resultado.registros);
        }

        // GET: /Inquilino/Detalle/5
        public async Task<IActionResult> Detalle(int id, CancellationToken ct = default)
        {
            var inq = await _repo.ObtenerPorIdAsync(id, ct);
            if (inq == null)
                return NotFound();

            return View(inq);
        }

        // GET: /Inquilino/Crear
        public IActionResult Crear()
        {
            return View();
        }

        // POST: /Inquilino/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Inquilino i, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
                return View(i);

            try
            {
                await _repo.AgregarAsync(i, ct);
                TempData["Mensaje"] = "Inquilino creado correctamente";
                return RedirectToAction("Index");
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                if (ex.Number == 1062)
                    ModelState.AddModelError("Documento", "El documento ya existe en el sistema.");
                else
                    ModelState.AddModelError("", "Error al guardar: " + ex.Message);

                return View(i);
            }
        }

        // GET: /Inquilino/Editar/5
        public async Task<IActionResult> Editar(int id, CancellationToken ct = default)
        {
            var inq = await _repo.ObtenerPorIdAsync(id, ct);
            if (inq == null)
                return NotFound();

            return View(inq);
        }

        // POST: /Inquilino/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Inquilino i, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
                return View(i);

            var existente = await _repo.ObtenerPorDocumentoAsync(i.Documento, ct);
            if (existente != null && existente.IdInquilino != i.IdInquilino)
            {
                ModelState.AddModelError("Documento", "El documento ya está registrado en otro inquilino.");
                return View(i);
            }

            await _repo.ActualizarAsync(i, ct);
            TempData["Mensaje"] = "El inquilino ha sido actualizado correctamente.";
            return RedirectToAction("Index");
        }

        // GET: /Inquilino/Eliminar/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Borrar(int id, CancellationToken ct = default)
        {
            var inq = await _repo.ObtenerPorIdAsync(id, ct);
            if (inq == null)
                return NotFound();

            var tieneContratos = await _repoContratos.ExisteContratoPorInquilinoAsync(id, ct);
            if (tieneContratos)
            {
                TempData["Error"] = "No se puede eliminar el inquilino porque tiene contratos asociados.";
                return RedirectToAction("Index");
            }

            return View(inq);
        }

        // POST: /Inquilino/Eliminar/5
        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Borrar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BorrarConfirmado(int id, CancellationToken ct = default)
        {
            var inq = await _repo.ObtenerPorIdAsync(id, ct);
            if (inq == null)
                return NotFound();

            try
            {
                await _repo.BorrarAsync(id, ct);
                TempData["Mensaje"] = "Inquilino eliminado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "No se pudo eliminar el inquilino: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
