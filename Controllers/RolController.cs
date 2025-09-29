using Microsoft.AspNetCore.Mvc;
using Inmobiliaria10.Data.Repositories;
using Inmobiliaria10.Models;
using Microsoft.AspNetCore.Authorization;

namespace Inmobiliaria10.Controllers
{
    [Authorize]
    public class RolController : Controller
    {
        private readonly IRolRepo _repo;

        public RolController(IRolRepo repo)
        {
            _repo = repo;
        }

        // GET: Rol
        public async Task<IActionResult> Index()
        {
            var roles = await _repo.ListarTodos();
            return View(roles);
        }

        // GET: Rol/Crear
        public IActionResult Crear()
        {
            return View();
        }

        // POST: Rol/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Rol rol)
        {
            if (!ModelState.IsValid)
                return View(rol);

            if (await _repo.ExisteDenominacion(rol.DenominacionRol))
            {
                ModelState.AddModelError("DenominacionRol", "Ya existe un rol con esa denominación");
                return View(rol);
            }

            await _repo.Agregar(rol);
            return RedirectToAction(nameof(Index));
        }

        // GET: Rol/Editar
        public async Task<IActionResult> Editar(int id)
        {
            var rol = await _repo.ObtenerPorId(id);
            if (rol == null)
                return NotFound();

            return View(rol);
        }

        // POST: Rol/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Rol rol)
        {
            if (id != rol.IdRol)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(rol);

            if (await _repo.ExisteDenominacion(rol.DenominacionRol, rol.IdRol))
            {
                ModelState.AddModelError("DenominacionRol", "Ya existe otro rol con esa denominación");
                return View(rol);
            }

            await _repo.Actualizar(rol);
            return RedirectToAction(nameof(Index));
        }

        // GET: Rol/Eliminar
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var rol = await _repo.ObtenerPorId(id);
            if (rol == null)
                return NotFound();

            return View(rol);
        }

        // POST: Rol/Eliminar
        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            await _repo.Eliminar(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: Rol/Detalle/5
        public async Task<IActionResult> Detalle(int id)
        {
            var rol = await _repo.ObtenerPorId(id);
            if (rol == null)
                return NotFound();

            return View(rol);
        }
    }
}
