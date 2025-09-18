using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using Inmobiliaria10.Data.Repositories;
using Inmobiliaria10.Models;
using Inmobiliaria10.Models.ViewModels;
using Inmobiliaria10.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using BCrypt.Net; // 🔹 BCrypt.Net-Next

namespace Inmobiliaria10.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly IUsuarioRepo _repo;
        private readonly IRolRepo _rolRepo;
        private readonly IEmailService _emailService;
        private readonly IImagenRepo _imagenRepo;

        public UsuarioController(IUsuarioRepo repo, IRolRepo rolRepo, IEmailService emailService, IImagenRepo imagenRepo)
        {
            _repo = repo;
            _rolRepo = rolRepo;
            _emailService = emailService;
            _imagenRepo = imagenRepo;
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

            // Validar alias duplicado
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

        // ELIMINAR
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

        // PERFIL
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            var idClaim = User.FindFirst("IdUsuario");
            if (idClaim == null) return RedirectToAction("Login");

            int idUsuario = int.Parse(idClaim.Value);
            var usuario = await _repo.ObtenerPorId(idUsuario);

            if (usuario == null) return NotFound();

            return View(usuario);
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

            // Subida de imagen (si decidís mantener ImagenPerfil en el modelo Usuario)
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
            if (!VerifyPassword(vm.PasswordActual, usuarioActual.Password))
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
            if (!ModelState.IsValid) return View(vm);

            var usuario = await _repo.ObtenerPorEmail(vm.Email);
            if (usuario == null)
            {
                TempData["Error"] = "No existe ningún usuario con ese correo.";
                return View(vm);
            }

            var token = Guid.NewGuid().ToString("N");
            await _repo.GuardarTokenReset(usuario.IdUsuario, token, DateTime.UtcNow.AddHours(1));

            var link = Url.Action("ResetPassword", "Usuario",
                new { token }, protocol: HttpContext.Request.Scheme);

            await _emailService.Enviar(vm.Email, "Recuperar contraseña",
                $"<p>Hacé click en el siguiente enlace para restablecer tu contraseña:</p><p><a href='{link}'>Restablecer Contraseña</a></p>");

            TempData["Mensaje"] = "Se ha enviado un enlace a tu correo.";
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "El enlace de recuperación no es válido.";
                return RedirectToAction("RecuperarPassword");
            }

            var vm = new ResetPasswordViewModel { Token = token };
            return View(vm);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var usuario = await _repo.ObtenerPorToken(vm.Token);
            if (usuario == null || usuario.ResetTokenExpira <= DateTime.UtcNow)
            {
                TempData["Error"] = "El enlace de recuperación es inválido o ha expirado.";
                return RedirectToAction("RecuperarPassword");
            }

            await _repo.ActualizarPassword(usuario.IdUsuario, HashPassword(vm.NuevaPassword));
            await _repo.LimpiarTokenReset(usuario.IdUsuario);

            TempData["Mensaje"] = "Tu contraseña ha sido restablecida correctamente. Ahora puedes iniciar sesión.";
            return RedirectToAction("Login", "Usuario");
        }

        // LOGIN
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var input = (vm.Email ?? "").Trim();
            var emailNorm = input.ToLowerInvariant();
            var aliasNorm = input.ToUpperInvariant();

            var usuarios = await _repo.ListarTodos();
            var usuario = usuarios.FirstOrDefault(u =>
                (u.Email?.ToLowerInvariant() == emailNorm) ||
                (u.Alias?.ToUpperInvariant() == aliasNorm));

            if (usuario == null || !VerifyPassword(vm.Password, usuario.Password))
            {
                ModelState.AddModelError("", "Credenciales incorrectas.");
                return View(vm);
            }

            var claims = new List<Claim>
            {
                new Claim("IdUsuario", usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.Email),
                new Claim("FullName", usuario.ApellidoNombres),
                new Claim(ClaimTypes.Role, usuario.Rol?.DenominacionRol ?? "Empleado"),
                new Claim("Foto", usuario.ImagenPerfil ?? "/img/default-user.png"),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

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

        // Helpers
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
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Perfil(Usuario vm, IFormFile? AvatarFile, [FromServices] IImageStorageService storage)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var usuarioActual = await _repo.ObtenerPorId(vm.IdUsuario);
            if (usuarioActual == null)
                return NotFound();

            usuarioActual.ApellidoNombres = vm.ApellidoNombres;
            usuarioActual.Email = vm.Email;

            if (AvatarFile != null && AvatarFile.Length > 0)
            {
                // Guardar en carpeta /wwwroot/uploads/Usuarios/{id}
                var url = await storage.UploadAsync(AvatarFile, $"Usuarios/{vm.IdUsuario}");

                // Registrar en la tabla Imagenes
                await _imagenRepo.AltaPerfil(vm.IdUsuario, url);
            }

            await _repo.Actualizar(usuarioActual);

            TempData["Mensaje"] = "Perfil actualizado correctamente.";
            return RedirectToAction("Perfil");
        }


    }
}
