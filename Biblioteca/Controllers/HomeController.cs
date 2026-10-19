using Biblioteca.Data.Repositories;
using Biblioteca.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Controllers;

public class HomeController : Controller
{
    private readonly ILivroRepository _livroRepository;
    private readonly IAutorRepository _autorRepository;
    private readonly ILeitorRepository _leitorRepository;
    private readonly IEmprestimoRepository _emprestimoRepository;

    public HomeController(
        ILivroRepository livroRepository,
        IAutorRepository autorRepository,
        ILeitorRepository leitorRepository,
        IEmprestimoRepository emprestimoRepository)
    {
        _livroRepository = livroRepository;
        _autorRepository = autorRepository;
        _leitorRepository = leitorRepository;
        _emprestimoRepository = emprestimoRepository;
    }

    public async Task<IActionResult> Index()
    {
        // Visitantes veem a página de apresentação; utilizadores autenticados veem o painel
        if (User.Identity?.IsAuthenticated != true)
        {
            return View("Welcome");
        }

        var model = new DashboardViewModel
        {
            TotalLivros = await _livroRepository.GetAll().CountAsync(),
            TotalAutores = await _autorRepository.GetAll().CountAsync(),
            TotalLeitores = await _leitorRepository.GetAll().CountAsync(),
            EmprestimosAtivos = await _emprestimoRepository.GetAll().CountAsync(e => e.DataDevolucao == null),
            EmprestimosEmAtraso = await _emprestimoRepository.GetEmAtrasoAsync()
        };

        return View(model);
    }
}