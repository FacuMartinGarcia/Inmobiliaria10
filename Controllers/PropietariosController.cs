// Controllers/PropietariosController.cs
using Inmobiliaria10.Models;
using Inmobiliaria10.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria10.Controllers;

public class PropietariosController : Controller
{
    private readonly IPropietarioRepo _repo;

    public PropietariosController(IPropietarioRepo repo)
    {
        _repo = repo;
    }

    // GET: /Propietarios
    public async Task<IActionResult> Index(string? q)
    {
        var lista = await _repo.ObtenerTodoAsync(q);
        ViewBag.q = q;
        return View(lista);
    }

    // GET: /Propietarios/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var p = await _repo.ObtenerPorIdAsync(id);
        if (p == null) return NotFound();
        return View(p);
    }

    // GET: /Propietarios/Create
    public IActionResult Create() => View(new Propietario());

    // POST: /Propietarios/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Propietario p)
    {
        if (await _repo.ExistsDocumentoAsync(p.Documento))
            ModelState.AddModelError(nameof(p.Documento), "Ya existe un propietario con ese documento.");

        if (!ModelState.IsValid) return View(p);

        p.IdPropietario = await _repo.CrearAsync(p);
        TempData["ok"] = "Propietario creado.";
        return RedirectToAction(nameof(Details), new { id = p.IdPropietario });
    }

    // GET: /Propietarios/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var p = await _repo.ObtenerPorIdAsync(id);
        if (p == null) return NotFound();
        return View(p);
    }

    // POST: /Propietarios/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Propietario p)
    {
        if (id != p.IdPropietario) return BadRequest();

        if (await _repo.ExistsDocumentoAsync(p.Documento, id))
            ModelState.AddModelError(nameof(p.Documento), "Ya existe otro propietario con ese documento.");

        if (!ModelState.IsValid) return View(p);

        var ok = await _repo.ModificarAsync(p);
        if (!ok) return NotFound();

        TempData["ok"] = "Propietario actualizado.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: /Propietarios/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _repo.ObtenerPorIdAsync(id);
        if (p == null) return NotFound();
        return View(p);
    }

    // POST: /Propietarios/DeleteConfirmed/5
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var ok = await _repo.BorrarAsync(id);
        if (!ok) return NotFound();
        TempData["ok"] = "Propietario eliminado.";
        return RedirectToAction(nameof(Index));
    }
}
