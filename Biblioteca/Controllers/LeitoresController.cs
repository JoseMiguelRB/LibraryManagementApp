using Biblioteca.Data.Entities;
using Biblioteca.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Controllers;

[Authorize]
public class LeitoresController : Controller
{
    private readonly ILeitorRepository _leitorRepository;
    private readonly ILogger<LeitoresController> _logger;

    public LeitoresController(ILeitorRepository leitorRepository, ILogger<LeitoresController> logger)
    {
        _leitorRepository = leitorRepository;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? pesquisa)
    {
        ViewData["Pesquisa"] = pesquisa;
        return View(await _leitorRepository.GetAllWithEmprestimosAsync(pesquisa));
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return View("RegistoNaoEncontrado");
        }

        var leitor = await _leitorRepository.GetByIdWithEmprestimosAsync(id.Value);
        if (leitor == null)
        {
            return View("RegistoNaoEncontrado");
        }

        return View(leitor);
    }

    public IActionResult Create()
    {
        return View(new Leitor());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Leitor leitor)
    {
        await ValidarLeitorAsync(leitor);

        if (!ModelState.IsValid)
        {
            return View(leitor);
        }

        try
        {
            await _leitorRepository.CreateAsync(leitor);
            TempData["Sucesso"] = $"Leitor \"{leitor.Nome}\" criado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao criar leitor");
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao gravar o leitor.");
            return View(leitor);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return View("RegistoNaoEncontrado");
        }

        var leitor = await _leitorRepository.GetByIdAsync(id.Value);
        if (leitor == null)
        {
            return View("RegistoNaoEncontrado");
        }

        return View(leitor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Leitor leitor)
    {
        if (id != leitor.Id)
        {
            return View("RegistoNaoEncontrado");
        }

        await ValidarLeitorAsync(leitor);

        if (!ModelState.IsValid)
        {
            return View(leitor);
        }

        try
        {
            await _leitorRepository.UpdateAsync(leitor);
            TempData["Sucesso"] = "Leitor atualizado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _leitorRepository.ExistAsync(leitor.Id))
            {
                return View("RegistoNaoEncontrado");
            }

            ModelState.AddModelError(string.Empty, "O registo foi alterado por outro utilizador. Recarregue a página.");
            return View(leitor);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao editar leitor {Id}", id);
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao gravar as alterações.");
            return View(leitor);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return View("RegistoNaoEncontrado");
        }

        var leitor = await _leitorRepository.GetByIdWithEmprestimosAsync(id.Value);
        if (leitor == null)
        {
            return View("RegistoNaoEncontrado");
        }

        return View(leitor);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var leitor = await _leitorRepository.GetByIdAsync(id);
        if (leitor == null)
        {
            return View("RegistoNaoEncontrado");
        }

        if (await _leitorRepository.HasEmprestimosAsync(id))
        {
            TempData["Erro"] = $"Não é possível eliminar \"{leitor.Nome}\" porque tem empréstimos registados.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _leitorRepository.DeleteAsync(leitor);
            TempData["Sucesso"] = "Leitor eliminado com sucesso.";
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao eliminar leitor {Id}", id);
            TempData["Erro"] = "Não foi possível eliminar o leitor, pois existem registos relacionados.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarLeitorAsync(Leitor leitor)
    {
        if (leitor.DataRegisto > DateTime.Today)
        {
            ModelState.AddModelError(nameof(leitor.DataRegisto), "A data de registo não pode ser futura.");
        }

        if (!string.IsNullOrWhiteSpace(leitor.Email) && await _leitorRepository.EmailExistsAsync(leitor.Email, leitor.Id))
        {
            ModelState.AddModelError(nameof(leitor.Email), "Já existe um leitor com este email.");
        }
    }
}
