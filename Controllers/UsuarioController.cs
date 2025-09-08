using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Cryptography;
using System.Text;
using Inmobiliaria10.Data.Repositories;
using Inmobiliaria10.Models;

namespace Inmobiliaria10.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly IUsuarioRepo _repo;
        private readonly IRolRepo _rolRepo;

        public UsuarioController(IUsuarioRepo repo, IRolRepo rolRepo)
        {
            _repo = repo;
            _rolRepo = rolRepo;
        }

        // LISTADO CON PAGINACIÓN
        public async Task<IActionResult> Index(int pagina = 1, string? search = null)
        {
            int cantidadPorPagina = 10;
            var (usuarios, totalRegistros) = await _repo.ListarTodosPaginado(pagina, cantidadPorPagina, search);

            ViewBag.TotalRegistros = totalRegistros;
            ViewBag.PaginaActual = pagina;
            ViewBag.CantidadPorPagina = cantidadPorPagina;
            ViewBag.Search = search;

            return View(usuarios);
        }

        // CREAR
        public async Task<IActionResult> Crear()
        {
            ViewBag.Roles = await ObtenerRolesSelectList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Usuario u)
        {
            var existente = await _repo.ObtenerPorAlias(u.Alias);
            if (existente != null)
            {
                ModelState.AddModelError("Alias", "El alias ya está en uso por otro usuario.");
            }

            if (string.IsNullOrEmpty(u.Password))
            {
                ModelState.AddModelError("Password", "La contraseña es obligatoria.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await ObtenerRolesSelectList();
                return View(u);
            }


            u.Password = HashPassword(u.Password);

            await _repo.Agregar(u);
            TempData["Success"] = "Usuario creado correctamente.";
            return RedirectToAction("Index");
        }

        // EDITAR (GET)
        public async Task<IActionResult> Editar(int id)
        {
            var u = await _repo.ObtenerPorId(id);
            if (u == null)
                return NotFound();

            ViewBag.Roles = await ObtenerRolesSelectList();

            u.Password = string.Empty;

            return View(u);
        }

        // EDITAR (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Usuario u)
        {
            var existente = await _repo.ObtenerPorAlias(u.Alias);
            if (existente != null && existente.IdUsuario != u.IdUsuario)
            {
                ModelState.AddModelError("Alias", "El alias ya está en uso por otro usuario.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await ObtenerRolesSelectList();
                return View(u);
            }

            var usuarioActual = await _repo.ObtenerPorId(u.IdUsuario);
            if (usuarioActual == null)
                return NotFound();

            usuarioActual.ApellidoNombres = u.ApellidoNombres;
            usuarioActual.Alias = u.Alias;
            usuarioActual.Email = u.Email;
            usuarioActual.IdRol = u.IdRol;


            if (!string.IsNullOrEmpty(u.Password))
            {
                usuarioActual.Password = HashPassword(u.Password);
            }

            await _repo.Actualizar(usuarioActual);

            TempData["Success"] = "Usuario actualizado correctamente.";
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Eliminar(int id)
        {
            var u = await _repo.ObtenerPorId(id);
            if (u == null)
                return NotFound();

            return View(u);
        }

        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            await _repo.Eliminar(id);
            TempData["Success"] = "Usuario eliminado correctamente.";
            return RedirectToAction("Index");
        }


        private async Task<List<SelectListItem>> ObtenerRolesSelectList()
        {
            var roles = await _rolRepo.ListarTodos();
            return roles.Select(r => new SelectListItem
            {
                Value = r.IdRol.ToString(),
                Text = r.DenominacionRol
            }).ToList();
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hashBytes = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
