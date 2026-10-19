using Biblioteca.Data.Entities;
using Biblioteca.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Controllers;

[Authorize]
public class LivrosController : Controller
{
    private readonly ILivroRepository _livroRepository;
    private readonly IAutorRepository _autorRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly ILogger<LivrosController> _logger;

    public LivrosController(
        ILivroRepository livroRepository,
        IAutorRepository autorRepository,
        ICategoriaRepository categoriaRepository,
        ILogger<LivrosController> logger)
    {
        _livroRepository = livroRepository;
        _autorRepository = autorRepository;
        _categoriaRepository = categoriaRepository;
        _logger = logger;
    }

    // GET: Livros?pesquisa=...&categoriaId=...
    public async Task<IActionResult> Index(string? pesquisa, int? categoriaId)
    {
        ViewData["Pesquisa"] = pesquisa;
        ViewBag.Categorias = new SelectList(_categoriaRepository.GetComboCategorias(), "Value", "Text", categoriaId);

        return View(await _livroRepository.GetAllWithDetailsAsync(pesquisa, categoriaId));
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return View("RegistoNaoEncontrado");
        }

        var livro = await _livroRepository.GetByIdWithDetailsAsync(id.Value);
        if (livro == null)
        {
            return View("RegistoNaoEncontrado");
        }

        return View(livro);
    }

    public IActionResult Create()
    {
        CarregarCombos();
        return View(new Livro { AnoPublicacao = DateTime.Today.Year });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Livro livro)
    {
        await ValidarLivroAsync(livro);

        if (!ModelState.IsValid)
        {
            CarregarCombos(livro);
            return View(livro);
        }

        try
        {
            await _livroRepository.CreateAsync(livro);
            TempData["Sucesso"] = $"Livro \"{livro.Titulo}\" criado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao criar livro");
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao gravar o livro.");
            CarregarCombos(livro);
            return View(livro);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return View("RegistoNaoEncontrado");
        }

        var livro = await _livroRepository.GetByIdAsync(id.Value);
        if (livro == null)
        {
            return View("RegistoNaoEncontrado");
        }

        CarregarCombos(livro);
        return View(livro);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Livro livro)
    {
        if (id != livro.Id)
        {
            return View("RegistoNaoEncontrado");
        }

        await ValidarLivroAsync(livro);

        // Conflito: não pode haver menos exemplares do que os que estão emprestados
        var ativos = await _livroRepository.CountEmprestimosAtivosAsync(livro.Id);
        if (livro.NumeroExemplares < ativos)
        {
            ModelState.AddModelError(nameof(livro.NumeroExemplares),
                $"Existem {ativos} exemplar(es) emprestado(s). O número de exemplares não pode ser inferior.");
        }

        if (!ModelState.IsValid)
        {
            CarregarCombos(livro);
            return View(livro);
        }

        try
        {
            await _livroRepository.UpdateAsync(livro);
            TempData["Sucesso"] = "Livro atualizado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _livroRepository.ExistAsync(livro.Id))
            {
                return View("RegistoNaoEncontrado");
            }

            ModelState.AddModelError(string.Empty, "O registo foi alterado por outro utilizador. Recarregue a página.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao editar livro {Id}", id);
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao gravar as alterações.");
        }

        CarregarCombos(livro);
        return View(livro);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return View("RegistoNaoEncontrado");
        }

        var livro = await _livroRepository.GetByIdWithDetailsAsync(id.Value);
        if (livro == null)
        {
            return View("RegistoNaoEncontrado");
        }

        return View(livro);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var livro = await _livroRepository.GetByIdAsync(id);
        if (livro == null)
        {
            return View("RegistoNaoEncontrado");
        }

        // Conflito: o histórico de empréstimos tem de ser preservado
        if (await _livroRepository.HasEmprestimosAsync(id))
        {
            TempData["Erro"] = $"Não é possível eliminar \"{livro.Titulo}\" porque tem empréstimos registados.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _livroRepository.DeleteAsync(livro);
            TempData["Sucesso"] = "Livro eliminado com sucesso.";
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao eliminar livro {Id}", id);
            TempData["Erro"] = "Não foi possível eliminar o livro, pois existem registos relacionados.";
        }

        return RedirectToAction(nameof(Index));
    }

    // ---------- Métodos auxiliares ----------

    private async Task ValidarLivroAsync(Livro livro)
    {
        if (livro.AnoPublicacao > DateTime.Today.Year)
        {
            ModelState.AddModelError(nameof(livro.AnoPublicacao), "O ano de publicação não pode ser futuro.");
        }

        if (!string.IsNullOrWhiteSpace(livro.ISBN) && await _livroRepository.IsbnExistsAsync(livro.ISBN, livro.Id))
        {
            ModelState.AddModelError(nameof(livro.ISBN), "Já existe um livro com este ISBN.");
        }

        if (livro.AutorId.HasValue && !await _autorRepository.ExistAsync(livro.AutorId.Value))
        {
            ModelState.AddModelError(nameof(livro.AutorId), "O autor selecionado não existe.");
        }

        if (livro.CategoriaId.HasValue && !await _categoriaRepository.ExistAsync(livro.CategoriaId.Value))
        {
            ModelState.AddModelError(nameof(livro.CategoriaId), "A categoria selecionada não existe.");
        }
    }

    private void CarregarCombos(Livro? livro = null)
    {
        ViewBag.Autores = new SelectList(_autorRepository.GetComboAutores(), "Value", "Text", livro?.AutorId);
        ViewBag.Categorias = new SelectList(_categoriaRepository.GetComboCategorias(), "Value", "Text", livro?.CategoriaId);
    }
}
