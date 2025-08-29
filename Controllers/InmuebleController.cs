using Microsoft.AspNetCore.Mvc;
using Inmobiliaria10.Models;
using Inmobiliaria10.Data.Repositories;

namespace Inmobiliaria10.Controllers
{
    public class InmuebleController : Controller
    {
        private readonly IInmuebleRepo repo;

        public InmuebleController(IInmuebleRepo repo)
        {
            this.repo = repo;
        }

        // GET: /Inmueble
        public IActionResult Index(int pagina = 1, int cantidadPorPagina = 10)
        {
            var (registros, total) = repo.ListarTodosPaginado(pagina, cantidadPorPagina);

            ViewBag.TotalRegistros = total;
            ViewBag.Pagina = pagina;
            ViewBag.CantidadPorPagina = cantidadPorPagina;

            return View(registros);
        }

        // GET: /Inmueble/Details/5
        public IActionResult Detalles(int id)
        {
            var inmueble = repo.ObtenerPorId(id);
            if (inmueble == null)
            {
                return NotFound();
            }
            return View(inmueble);
        }

        // GET: /Inmueble/Create
        public IActionResult Crear()
        {
            return View();
        }

        // POST: /Inmueble/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Inmueble i)
        {
            if (ModelState.IsValid)
            {
                repo.Agregar(i);
                return RedirectToAction(nameof(Index));
            }
            return View(i);
        }

        // GET: /Inmueble/Edit/5
        public IActionResult Editar(int id)
        {
            var inmueble = repo.ObtenerPorId(id);
            if (inmueble == null)
            {
                return NotFound();
            }
            return View(inmueble);
        }

        // POST: /Inmueble/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Inmueble i)
        {
            if (id != i.IdInmueble)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                repo.Actualizar(i);
                return RedirectToAction(nameof(Index));
            }
            return View(i);
        }

        // GET: /Inmueble/Delete/5
        public IActionResult Eliminar(int id)
        {
            var inmueble = repo.ObtenerPorId(id);
            if (inmueble == null)
            {
                return NotFound();
            }
            return View(inmueble);
        }

        // POST: /Inmueble/Delete/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarConfirmado(int id)
        {
            repo.Borrar(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
