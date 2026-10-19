using Biblioteca.Data.Entities;
using Biblioteca.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Controllers;

[Authorize]
public class CategoriasController : Controller
{
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly ILogger<CategoriasController> _logger;

    public CategoriasController(ICategoriaRepository categoriaRepository, ILogger<CategoriasController> logger)
    {
        _categoriaRepository = categoriaRepository;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _categoriaRepository.GetAllWithLivrosAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return View("RegistoNaoEncontrado");
        }

        var categoria = await _categoriaRepository.GetByIdWithLivrosAsync(id.Value);
        if (categoria == null)
        {
            return View("RegistoNaoEncontrado");
        }

        return View(categoria);
    }

    public IActionResult Create()
    {
        return View(new Categoria());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Categoria categoria)
    {
        if (await _categoriaRepository.NomeExistsAsync(categoria.Nome))
        {
            ModelState.AddModelError(nameof(categoria.Nome), "Já existe uma categoria com este nome.");
        }

        if (!ModelState.IsValid)
        {
            return View(categoria);
        }

        try
        {
            await _categoriaRepository.CreateAsync(categoria);
            TempData["Sucesso"] = $"Categoria \"{categoria.Nome}\" criada com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao criar categoria");
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao gravar a categoria.");
            return View(categoria);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return View("RegistoNaoEncontrado");
        }

        var categoria = await _categoriaRepository.GetByIdAsync(id.Value);
        if (categoria == null)
        {
            return View("RegistoNaoEncontrado");
        }

        return View(categoria);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Categoria categoria)
    {
        if (id != categoria.Id)
        {
            return View("RegistoNaoEncontrado");
        }

        if (await _categoriaRepository.NomeExistsAsync(categoria.Nome, categoria.Id))
        {
            ModelState.AddModelError(nameof(categoria.Nome), "Já existe uma categoria com este nome.");
        }

        if (!ModelState.IsValid)
        {
            return View(categoria);
        }

        try
        {
            await _categoriaRepository.UpdateAsync(categoria);
            TempData["Sucesso"] = "Categoria atualizada com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _categoriaRepository.ExistAsync(categoria.Id))
            {
                return View("RegistoNaoEncontrado");
            }

            ModelState.AddModelError(string.Empty, "O registo foi alterado por outro utilizador. Recarregue a página.");
            return View(categoria);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao editar categoria {Id}", id);
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao gravar as alterações.");
            return View(categoria);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return View("RegistoNaoEncontrado");
        }

        var categoria = await _categoriaRepository.GetByIdWithLivrosAsync(id.Value);
        if (categoria == null)
        {
            return View("RegistoNaoEncontrado");
        }

        return View(categoria);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var categoria = await _categoriaRepository.GetByIdAsync(id);
        if (categoria == null)
        {
            return View("RegistoNaoEncontrado");
        }

        if (await _categoriaRepository.HasLivrosAsync(id))
        {
            TempData["Erro"] = $"Não é possível eliminar a categoria \"{categoria.Nome}\" porque tem livros associados.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _categoriaRepository.DeleteAsync(categoria);
            TempData["Sucesso"] = "Categoria eliminada com sucesso.";
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao eliminar categoria {Id}", id);
            TempData["Erro"] = "Não foi possível eliminar a categoria, pois existem registos relacionados.";
        }

        return RedirectToAction(nameof(Index));
    }
}
