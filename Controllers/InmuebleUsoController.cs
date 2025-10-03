using Microsoft.AspNetCore.Mvc;
using Inmobiliaria10.Models;
using Inmobiliaria10.Data.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace Inmobiliaria10.Controllers
{
    [Authorize]
    public class InmuebleUsoController : Controller
    {
        private readonly IInmuebleUsoRepo repo;

        public InmuebleUsoController(IInmuebleUsoRepo repo)
        {
            this.repo = repo;
        }

        public IActionResult Index()
        {
            var usos = repo.MostrarTodosInmuebleUsos();
            return View(usos);
        }

        public IActionResult Detalle(int id)
        {
            var uso = repo.ObtenerPorId(id);
            if (uso == null)
                return NotFound();

            return View(uso);
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(InmuebleUso uso)
        {
            if (!ModelState.IsValid)
                return View(uso);

            try
            {
                repo.Agregar(uso);
                TempData["Mensaje"] = "Uso de inmueble creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(uso);
            }
        }

        public IActionResult Editar(int id)
        {
            var uso = repo.ObtenerPorId(id);
            if (uso == null)
                return NotFound();

            return View(uso);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(InmuebleUso uso)
        {
            if (!ModelState.IsValid)
                return View(uso);

            try
            {
                repo.Editar(uso);
                TempData["Mensaje"] = "Uso de inmueble modificado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(uso);
            }
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Eliminar(int id)
        {
            var uso = repo.ObtenerPorId(id);
            if (uso == null)
            {
                TempData["Error"] = "El uso de inmueble no existe.";
                return RedirectToAction(nameof(Index));
            }

            return View(uso);
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarConfirmado(int id)
        {
            try
            {
                repo.Eliminar(id);
                TempData["Mensaje"] = "Uso de inmueble eliminado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (MySql.Data.MySqlClient.MySqlException ex) when (ex.Number == 1451)
            {
                TempData["Error"] = "No se puede eliminar este uso porque tiene inmuebles asociados.";
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
