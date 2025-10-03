using Microsoft.AspNetCore.Mvc;
using Inmobiliaria10.Models;
using Inmobiliaria10.Data.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace Inmobiliaria10.Controllers
{
    [Authorize]
    public class InmuebleTipoController : Controller
    {
        private readonly IInmuebleTipoRepo repo;

        public InmuebleTipoController(IInmuebleTipoRepo repo)
        {
            this.repo = repo;
        }

        public IActionResult Index()
        {
            var tipos = repo.MostrarTodosInmuebleTipos();
            return View(tipos);
        }

        public IActionResult Detalle(int id)
        {
            var tipo = repo.ObtenerPorId(id);
            if (tipo == null)
                return NotFound();

            return View(tipo);
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(InmuebleTipo tipo)
        {
            if (!ModelState.IsValid)
                return View(tipo);

            try
            {
                repo.Agregar(tipo);
                TempData["Mensaje"] = "Tipo de inmueble creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(tipo);
            }
        }

        public IActionResult Editar(int id)
        {
            var tipo = repo.ObtenerPorId(id);
            if (tipo == null)
                return NotFound();

            return View(tipo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(InmuebleTipo tipo)
        {
            if (!ModelState.IsValid)
                return View(tipo);

            try
            {
                repo.Editar(tipo);
                TempData["Mensaje"] = "Tipo de inmueble modificado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(tipo);
            }
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Eliminar(int id)
        {
            var tipo = repo.ObtenerPorId(id);
            if (tipo == null)
                return NotFound();

            return View(tipo);
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarConfirmado(int id)
        {
            try
            {
                repo.Eliminar(id);
                TempData["Mensaje"] = "Tipo de inmueble eliminado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (MySql.Data.MySqlClient.MySqlException ex) when (ex.Number == 1451)
            {
                TempData["Error"] = "No se puede eliminar el tipo porque tiene inmuebles asociados.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

    }
}
