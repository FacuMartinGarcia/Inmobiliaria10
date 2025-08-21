// Controllers/PropietariosController.cs
using Inmobiliaria10.Models;
using Inmobiliaria10.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria10.Controllers;

public class PropietarioController : Controller
{
    private readonly IPropietarioRepo _repo;

    public PropietarioController(IPropietarioRepo repo)
    {
        _repo = repo;
    }

    // GET: /Propietarios
    public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10)
    {
        var data = await _repo.BuscarPaginado(q, page, pageSize);
        var vm = new PropietarioIndexVm { Data = data };
        return View(vm);
    }

    // GET: /Propietarios/Details/5
    public async Task<IActionResult> Detalles(int id)
    {
        var p = await _repo.ObtenerPorId(id);
        if (p == null) return NotFound();
        return View(p);
    }

    // GET: /Propietarios/Create
    public IActionResult Crear() => View(new Propietario());

    // POST: /Propietarios/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Propietario p)
    {
        if (await _repo.ExistsDocumento(p.Documento))
            ModelState.AddModelError(nameof(p.Documento), "Ya existe un propietario con ese documento.");

        if (!ModelState.IsValid) return View(p);

        p.IdPropietario = await _repo.Crear(p);
        TempData["ok"] = "Propietario creado.";
        return RedirectToAction(nameof(Detalles), new { id = p.IdPropietario });
    }

    // GET: /Propietarios/Edit/5
    public async Task<IActionResult> Editar(int id)
    {
        var p = await _repo.ObtenerPorId(id);
        if (p == null) return NotFound();
        return View(p);
    }

    // POST: /Propietarios/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, Propietario p)
    {
        if (id != p.IdPropietario) return BadRequest();

        if (await _repo.ExistsDocumento(p.Documento, id))
            ModelState.AddModelError(nameof(p.Documento), "Ya existe otro propietario con ese documento.");

        if (!ModelState.IsValid) return View(p);

        var ok = await _repo.Modificar(p);
        if (!ok) return NotFound();

        TempData["ok"] = "Propietario actualizado.";
        return RedirectToAction(nameof(Detalles), new { id });
    }

    // GET: /Propietarios/Delete/5
    public async Task<IActionResult> Borrar(int id)
    {
        var p = await _repo.ObtenerPorId(id);
        if (p == null) return NotFound();
        return View(p);
    }

    // POST: /Propietarios/DeleteConfirmed/5
    [HttpPost, ActionName("Borrar"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var ok = await _repo.Borrar(id);
        if (!ok) return NotFound();
        TempData["ok"] = "Propietario eliminado.";
        return RedirectToAction(nameof(Index));
    }
}
