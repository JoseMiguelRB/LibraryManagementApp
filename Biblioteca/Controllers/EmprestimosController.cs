using Biblioteca.Data.Entities;
using Biblioteca.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Controllers;

[Authorize]
public class EmprestimosController : Controller
{
    private const int MaxEmprestimosAtivosPorLeitor = 3;

    private readonly IEmprestimoRepository _emprestimoRepository;
    private readonly ILivroRepository _livroRepository;
    private readonly ILeitorRepository _leitorRepository;
    private readonly ILogger<EmprestimosController> _logger;

    public EmprestimosController(
        IEmprestimoRepository emprestimoRepository,
        ILivroRepository livroRepository,
        ILeitorRepository leitorRepository,
        ILogger<EmprestimosController> logger)
    {
        _emprestimoRepository = emprestimoRepository;
        _livroRepository = livroRepository;
        _leitorRepository = leitorRepository;
        _logger = logger;
    }

    // GET: Emprestimos?estado=ativos|atraso|devolvidos&pesquisa=...
    public async Task<IActionResult> Index(string? estado, string? pesquisa)
    {
        ViewData["Estado"] = estado;
        ViewData["Pesquisa"] = pesquisa;
        return View(await _emprestimoRepository.GetAllWithDetailsAsync(estado, pesquisa));
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return View("RegistoNaoEncontrado");
        }

        var emprestimo = await _emprestimoRepository.GetByIdWithDetailsAsync(id.Value);
        if (emprestimo == null)
        {
            return View("RegistoNaoEncontrado");
        }

        return View(emprestimo);
    }

    // GET: Emprestimos/Create  (pode vir de Livros/Details ou Leitores/Details com o id pré-selecionado)
    public async Task<IActionResult> Create(int? livroId, int? leitorId)
    {
        var emprestimo = new Emprestimo { LivroId = livroId, LeitorId = leitorId };
        await CarregarCombosAsync(emprestimo);
        return View(emprestimo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Emprestimo emprestimo)
    {
        ValidarDatas(emprestimo);

        if (emprestimo.LivroId.HasValue && emprestimo.LeitorId.HasValue)
        {
            await ValidarRegrasNegocioAsync(emprestimo.LivroId.Value, emprestimo.LeitorId.Value);
        }

        if (!ModelState.IsValid)
        {
            await CarregarCombosAsync(emprestimo);
            return View(emprestimo);
        }

        try
        {
            emprestimo.DataDevolucao = null;
            await _emprestimoRepository.CreateAsync(emprestimo);
            TempData["Sucesso"] = "Empréstimo registado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao criar empréstimo");
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao registar o empréstimo.");
            await CarregarCombosAsync(emprestimo);
            return View(emprestimo);
        }
    }

    // Só é possível alterar as datas de um empréstimo ativo
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return View("RegistoNaoEncontrado");
        }

        var emprestimo = await _emprestimoRepository.GetByIdWithDetailsAsync(id.Value);
        if (emprestimo == null)
        {
            return View("RegistoNaoEncontrado");
        }

        if (emprestimo.Devolvido)
        {
            TempData["Erro"] = "Não é possível alterar um empréstimo que já foi devolvido.";
            return RedirectToAction(nameof(Details), new { id });
        }

        return View(emprestimo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Emprestimo dados)
    {
        if (id != dados.Id)
        {
            return View("RegistoNaoEncontrado");
        }

        // Carrega o registo original: só as datas podem ser alteradas
        var emprestimo = await _emprestimoRepository.GetByIdWithDetailsAsync(id);
        if (emprestimo == null)
        {
            return View("RegistoNaoEncontrado");
        }

        if (emprestimo.Devolvido)
        {
            TempData["Erro"] = "Não é possível alterar um empréstimo que já foi devolvido.";
            return RedirectToAction(nameof(Details), new { id });
        }

        emprestimo.DataEmprestimo = dados.DataEmprestimo;
        emprestimo.DataPrevistaDevolucao = dados.DataPrevistaDevolucao;

        ValidarDatas(emprestimo);

        if (!ModelState.IsValid)
        {
            return View(emprestimo);
        }

        try
        {
            // Evita gravar as entidades relacionadas (livro/leitor) que vieram com o Include
            var paraGravar = new Emprestimo
            {
                Id = emprestimo.Id,
                LivroId = emprestimo.LivroId,
                LeitorId = emprestimo.LeitorId,
                DataEmprestimo = emprestimo.DataEmprestimo,
                DataPrevistaDevolucao = emprestimo.DataPrevistaDevolucao,
                DataDevolucao = emprestimo.DataDevolucao
            };

            await _emprestimoRepository.UpdateAsync(paraGravar);
            TempData["Sucesso"] = "Empréstimo atualizado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao editar empréstimo {Id}", id);
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao gravar as alterações.");
            return View(emprestimo);
        }
    }

    // POST: Emprestimos/Devolver/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Devolver(int id, string? returnUrl)
    {
        try
        {
            var ok = await _emprestimoRepository.RegistarDevolucaoAsync(id);
            if (ok)
            {
                TempData["Sucesso"] = "Devolução registada com sucesso.";
            }
            else
            {
                TempData["Erro"] = "O empréstimo não existe ou já foi devolvido.";
            }
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao registar devolução {Id}", id);
            TempData["Erro"] = "Ocorreu um erro ao registar a devolução.";
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return View("RegistoNaoEncontrado");
        }

        var emprestimo = await _emprestimoRepository.GetByIdWithDetailsAsync(id.Value);
        if (emprestimo == null)
        {
            return View("RegistoNaoEncontrado");
        }

        return View(emprestimo);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var emprestimo = await _emprestimoRepository.GetByIdAsync(id);
        if (emprestimo == null)
        {
            return View("RegistoNaoEncontrado");
        }

        try
        {
            await _emprestimoRepository.DeleteAsync(emprestimo);
            TempData["Sucesso"] = "Empréstimo eliminado com sucesso.";
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao eliminar empréstimo {Id}", id);
            TempData["Erro"] = "Não foi possível eliminar o empréstimo.";
        }

        return RedirectToAction(nameof(Index));
    }

    // ---------- Métodos auxiliares ----------

    private void ValidarDatas(Emprestimo emprestimo)
    {
        if (emprestimo.DataEmprestimo.Date > DateTime.Today)
        {
            ModelState.AddModelError(nameof(emprestimo.DataEmprestimo), "A data do empréstimo não pode ser futura.");
        }

        if (emprestimo.DataPrevistaDevolucao.Date <= emprestimo.DataEmprestimo.Date)
        {
            ModelState.AddModelError(nameof(emprestimo.DataPrevistaDevolucao),
                "A devolução prevista tem de ser posterior à data do empréstimo.");
        }

        if ((emprestimo.DataPrevistaDevolucao - emprestimo.DataEmprestimo).TotalDays > 60)
        {
            ModelState.AddModelError(nameof(emprestimo.DataPrevistaDevolucao),
                "O prazo máximo de um empréstimo é de 60 dias.");
        }
    }

    private async Task ValidarRegrasNegocioAsync(int livroId, int leitorId)
    {
        var livro = await _livroRepository.GetByIdWithDetailsAsync(livroId);
        if (livro == null)
        {
            ModelState.AddModelError("LivroId", "O livro selecionado não existe.");
        }
        else if (livro.ExemplaresDisponiveis <= 0)
        {
            ModelState.AddModelError("LivroId", $"Não há exemplares disponíveis de \"{livro.Titulo}\".");
        }

        if (!await _leitorRepository.ExistAsync(leitorId))
        {
            ModelState.AddModelError("LeitorId", "O leitor selecionado não existe.");
            return;
        }

        if (await _leitorRepository.CountEmprestimosAtivosAsync(leitorId) >= MaxEmprestimosAtivosPorLeitor)
        {
            ModelState.AddModelError("LeitorId",
                $"Este leitor já atingiu o limite de {MaxEmprestimosAtivosPorLeitor} empréstimos ativos.");
        }

        if (await _emprestimoRepository.LeitorTemLivroAtivoAsync(leitorId, livroId))
        {
            ModelState.AddModelError("LivroId", "Este leitor já tem um exemplar deste livro emprestado.");
        }
    }

    private async Task CarregarCombosAsync(Emprestimo emprestimo)
    {
        ViewBag.Livros = new SelectList(
            await _livroRepository.GetComboLivrosDisponiveisAsync(emprestimo.LivroId), "Value", "Text", emprestimo.LivroId);
        ViewBag.Leitores = new SelectList(
            _leitorRepository.GetComboLeitores(), "Value", "Text", emprestimo.LeitorId);
    }
}
