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

        // LISTADO
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

         // DETALLE
        public async Task<IActionResult> Detalle(int id, CancellationToken ct = default)
        {
            var usu = await _repo.ObtenerPorId(id, ct);
            if (usu == null)
                return NotFound();

            return View(usu);
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

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await ObtenerRolesSelectList();
                return View(u);
            }

            u.Password = HashPassword(u.Password);

            await _repo.Agregar(u);
            TempData["Mensaje"] = "Usuario creado correctamente.";
            return RedirectToAction("Index");
        }

        // EDITAR (GET)
        public async Task<IActionResult> Editar(int id)
        {
            var usuario = await _repo.ObtenerPorId(id);
            if (usuario == null)
                return NotFound();

            var vm = new UsuarioEditViewModel
            {
                IdUsuario = usuario.IdUsuario,
                ApellidoNombres = usuario.ApellidoNombres,
                Alias = usuario.Alias,
                Email = usuario.Email,
                IdRol = usuario.IdRol

            };

            ViewBag.Roles = await ObtenerRolesSelectList();
            return View(vm);
        }

        // EDITAR (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(UsuarioEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await ObtenerRolesSelectList();
                return View(vm);
            }

            var usuarioActual = await _repo.ObtenerPorId(vm.IdUsuario);
            if (usuarioActual == null)
                return NotFound();

            usuarioActual.ApellidoNombres = vm.ApellidoNombres;
            usuarioActual.Alias = vm.Alias;
            usuarioActual.Email = vm.Email;
            usuarioActual.IdRol = vm.IdRol;

            if (!string.IsNullOrEmpty(vm.Password))
            {
                usuarioActual.Password = HashPassword(vm.Password);
            }

            await _repo.Actualizar(usuarioActual);

            TempData["Mensaje"] = "Usuario actualizado correctamente.";
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
            TempData["Mensaje"] = "Usuario eliminado correctamente.";
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
