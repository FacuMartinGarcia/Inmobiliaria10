using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Cryptography;
using System.Text;
using Inmobiliaria10.Data.Repositories;
using Inmobiliaria10.Models;
using Inmobiliaria10.Models.ViewModels;
using Inmobiliaria10.Services;
using System.Security.Claims;                     // Para Claim, ClaimTypes, ClaimsIdentity
using Microsoft.AspNetCore.Authentication;        // Para HttpContext.SignInAsync
using Microsoft.AspNetCore.Authentication.Cookies;// Para CookieAuthenticationDefaults
using Microsoft.AspNetCore.Authorization;

namespace Inmobiliaria10.Controllers
{
    
    public class UsuarioController : Controller
    {
        private readonly IUsuarioRepo _repo;
        private readonly IRolRepo _rolRepo;
        private readonly IEmailService _emailService;
        public UsuarioController(IUsuarioRepo repo, IRolRepo rolRepo, IEmailService emailService)
        {
            _repo = repo;
            _rolRepo = rolRepo;
            _emailService = emailService;
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

            // 🔹 Validar alias duplicado
            var existente = await _repo.ObtenerPorAlias(vm.Alias);
            if (existente != null && existente.IdUsuario != vm.IdUsuario)
            {
                ModelState.AddModelError("Alias", "El alias ya está en uso por otro usuario.");
                ViewBag.Roles = await ObtenerRolesSelectList();
                return View(vm);
            }

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

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            var idClaim = User.FindFirst("IdUsuario");
            if (idClaim == null) return RedirectToAction("Login");

            int idUsuario = int.Parse(idClaim.Value);
            var usuario = await _repo.ObtenerPorId(idUsuario);

            if (usuario == null) return NotFound();

            return View(usuario); // le manda el usuario a la vista
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Perfil(Usuario vm, IFormFile? ImagenPerfil, [FromServices] IWebHostEnvironment env)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var usuarioActual = await _repo.ObtenerPorId(vm.IdUsuario);
            if (usuarioActual == null)
                return NotFound();

            usuarioActual.ApellidoNombres = vm.ApellidoNombres;
            usuarioActual.Email = vm.Email;

            // Subida de imagen
            if (ImagenPerfil != null && ImagenPerfil.Length > 0)
            {
                string carpeta = Path.Combine(env.WebRootPath, "Uploads", "Perfiles", vm.IdUsuario.ToString());
                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);

                var extension = Path.GetExtension(ImagenPerfil.FileName);
                var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                var ruta = Path.Combine(carpeta, nombreArchivo);

                using var stream = new FileStream(ruta, FileMode.Create);
                await ImagenPerfil.CopyToAsync(stream);

                usuarioActual.ImagenPerfil = $"/Uploads/Perfiles/{vm.IdUsuario}/{nombreArchivo}";
            }

            await _repo.Actualizar(usuarioActual);
            TempData["Mensaje"] = "Perfil actualizado correctamente.";
            return RedirectToAction("Perfil");
        }


        // CAMBIAR PASSWORD
        [HttpGet]
        public async Task<IActionResult> CambiarPassword(int id)
        {
            var usuario = await _repo.ObtenerPorId(id);
            if (usuario == null)
                return NotFound();

            var vm = new CambiarPasswordViewModel { IdUsuario = usuario.IdUsuario };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPassword(CambiarPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var usuarioActual = await _repo.ObtenerPorId(vm.IdUsuario);
            if (usuarioActual == null)
                return NotFound();

            // Verificar contraseña actual
            var passwordActualHash = HashPassword(vm.PasswordActual);
            if (usuarioActual.Password != passwordActualHash)
            {
                ModelState.AddModelError("PasswordActual", "La contraseña actual es incorrecta.");
                return View(vm);
            }

            // Guardar nueva contraseña
            usuarioActual.Password = HashPassword(vm.NuevaPassword);
            await _repo.Actualizar(usuarioActual);

            TempData["Mensaje"] = "Contraseña cambiada correctamente.";
            return RedirectToAction("Perfil");
        }

        // RECUPERAR PASSWORD
        [HttpGet]
        public IActionResult RecuperarPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecuperarPassword(RecuperarPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var usuario = (await _repo.ListarTodos()).FirstOrDefault(u => u.Email == vm.Email);
            if (usuario == null)
            {
                TempData["Error"] = "No existe un usuario con ese correo.";
                return View(vm);
            }

            // Generar token
            var token = Guid.NewGuid().ToString();
            usuario.ResetToken = token;
            usuario.ResetTokenExpira = DateTime.UtcNow.AddHours(1);
            await _repo.Actualizar(usuario);

            // Enviar correo 
            string link = Url.Action("ResetPassword", "Usuario", new { token = token }, Request.Scheme)!;
            await _emailService.Enviar(
                vm.Email,
                "Recuperar contraseña",
                $"<p>Hacé click en el siguiente enlace para restablecer tu contraseña:</p><p><a href='{link}'>Restablecer Contraseña</a></p>"
            );
            TempData["Mensaje"] = "Se ha enviado un enlace a tu correo.";
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            var vm = new ResetPasswordViewModel { Token = token };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var usuario = (await _repo.ListarTodos()).FirstOrDefault(u => u.ResetToken == vm.Token && u.ResetTokenExpira > DateTime.UtcNow);
            if (usuario == null)
            {
                TempData["Error"] = "El enlace no es válido o ha expirado.";
                return RedirectToAction("RecuperarPassword");
            }

            usuario.Password = HashPassword(vm.NuevaPassword);
            usuario.ResetToken = null;
            usuario.ResetTokenExpira = null;
            await _repo.Actualizar(usuario);

            TempData["Mensaje"] = "Tu contraseña ha sido restablecida correctamente.";
            return RedirectToAction("Login");
        }

        // LOGIN
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            // 1. Buscar usuario en BD
            var usuario = (await _repo.ListarTodos())
                            .FirstOrDefault(u => u.Email == email);

            if (usuario == null || usuario.Password != HashPassword(password))
            {
                ModelState.AddModelError("", "Credenciales incorrectas.");
                return View();
            }

            // 2. Crear Claims
            var claims = new List<Claim>
            {
                new Claim("IdUsuario", usuario.IdUsuario.ToString()), 
                new Claim(ClaimTypes.Name, usuario.Email),
                new Claim("FullName", usuario.ApellidoNombres),
                new Claim(ClaimTypes.Role, usuario.Rol?.DenominacionRol ?? "Empleado"),
                new Claim("Foto", usuario.ImagenPerfil ?? "/img/user.png")
            };

            // 3. Identidad y cookie
            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            // 4. Redirigir a página principal
            return RedirectToAction("Index", "Home");
        }
        
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Usuario");
        }

    }
}
