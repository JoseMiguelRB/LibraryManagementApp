using Biblioteca.Data.Entities;
using Biblioteca.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Controllers;

[Authorize]
public class AutoresController : Controller
{
    private readonly IAutorRepository _autorRepository;
    private readonly ILogger<AutoresController> _logger;

    public AutoresController(IAutorRepository autorRepository, ILogger<AutoresController> logger)
    {
        _autorRepository = autorRepository;
        _logger = logger;
    }

    // GET: Autores
    public async Task<IActionResult> Index(string? pesquisa)
    {
        ViewData["Pesquisa"] = pesquisa;
        return View(await _autorRepository.GetAllWithLivrosAsync(pesquisa));
    }

    // GET: Autores/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return View("RegistoNaoEncontrado");
        }

        var autor = await _autorRepository.GetByIdWithLivrosAsync(id.Value);
        if (autor == null)
        {
            return View("RegistoNaoEncontrado");
        }

        return View(autor);
    }

    // GET: Autores/Create
    public IActionResult Create()
    {
        return View(new Autor());
    }

    // POST: Autores/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Autor autor)
    {
        if (autor.DataNascimento > DateTime.Today)
        {
            ModelState.AddModelError(nameof(autor.DataNascimento), "A data de nascimento não pode ser futura.");
        }

        if (!ModelState.IsValid)
        {
            return View(autor);
        }

        try
        {
            await _autorRepository.CreateAsync(autor);
            TempData["Sucesso"] = $"Autor \"{autor.Nome}\" criado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao criar autor");
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao gravar o autor. Tente novamente.");
            return View(autor);
        }
    }

    // GET: Autores/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return View("RegistoNaoEncontrado");
        }

        var autor = await _autorRepository.GetByIdAsync(id.Value);
        if (autor == null)
        {
            return View("RegistoNaoEncontrado");
        }

        return View(autor);
    }

    // POST: Autores/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Autor autor)
    {
        if (id != autor.Id)
        {
            return View("RegistoNaoEncontrado");
        }

        if (autor.DataNascimento > DateTime.Today)
        {
            ModelState.AddModelError(nameof(autor.DataNascimento), "A data de nascimento não pode ser futura.");
        }

        if (!ModelState.IsValid)
        {
            return View(autor);
        }

        try
        {
            await _autorRepository.UpdateAsync(autor);
            TempData["Sucesso"] = "Autor atualizado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _autorRepository.ExistAsync(autor.Id))
            {
                return View("RegistoNaoEncontrado");
            }

            ModelState.AddModelError(string.Empty, "O registo foi alterado por outro utilizador. Recarregue a página.");
            return View(autor);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao editar autor {Id}", id);
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao gravar as alterações.");
            return View(autor);
        }
    }

    // GET: Autores/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return View("RegistoNaoEncontrado");
        }

        var autor = await _autorRepository.GetByIdWithLivrosAsync(id.Value);
        if (autor == null)
        {
            return View("RegistoNaoEncontrado");
        }

        return View(autor);
    }

    // POST: Autores/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var autor = await _autorRepository.GetByIdAsync(id);
        if (autor == null)
        {
            return View("RegistoNaoEncontrado");
        }

        // Conflito: não se pode eliminar um autor que tenha livros associados
        if (await _autorRepository.HasLivrosAsync(id))
        {
            TempData["Erro"] = $"Não é possível eliminar \"{autor.Nome}\" porque tem livros associados. Elimine ou altere primeiro esses livros.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _autorRepository.DeleteAsync(autor);
            TempData["Sucesso"] = "Autor eliminado com sucesso.";
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao eliminar autor {Id}", id);
            TempData["Erro"] = "Não foi possível eliminar o autor, pois existem registos relacionados.";
        }

        return RedirectToAction(nameof(Index));
    }
}
