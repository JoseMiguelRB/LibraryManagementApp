using Biblioteca.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Data.Repositories;

public class EmprestimoRepository : GenericRepository<Emprestimo>, IEmprestimoRepository
{
    public EmprestimoRepository(DataContext context) : base(context)
    {
    }

    public async Task<List<Emprestimo>> GetAllWithDetailsAsync(string? estado, string? pesquisa)
    {
        var hoje = DateTime.Today;

        var query = _context.Emprestimos
            .Include(e => e.Livro)
            .Include(e => e.Leitor)
            .AsNoTracking()
            .AsQueryable();

        query = estado switch
        {
            "ativos" => query.Where(e => e.DataDevolucao == null),
            "atraso" => query.Where(e => e.DataDevolucao == null && e.DataPrevistaDevolucao < hoje),
            "devolvidos" => query.Where(e => e.DataDevolucao != null),
            _ => query
        };

        if (!string.IsNullOrWhiteSpace(pesquisa))
        {
            query = query.Where(e => e.Livro!.Titulo.Contains(pesquisa) || e.Leitor!.Nome.Contains(pesquisa));
        }

        return await query
            .OrderBy(e => e.DataDevolucao != null)          // ativos primeiro
            .ThenByDescending(e => e.DataEmprestimo)
            .ToListAsync();
    }

    public async Task<Emprestimo?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Emprestimos
            .Include(e => e.Livro).ThenInclude(l => l!.Autor)
            .Include(e => e.Leitor)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<bool> LeitorTemLivroAtivoAsync(int leitorId, int livroId, int idIgnorar = 0)
    {
        return await _context.Emprestimos.AnyAsync(e =>
            e.LeitorId == leitorId &&
            e.LivroId == livroId &&
            e.DataDevolucao == null &&
            e.Id != idIgnorar);
    }

    public async Task<List<Emprestimo>> GetEmAtrasoAsync()
    {
        var hoje = DateTime.Today;

        return await _context.Emprestimos
            .Include(e => e.Livro)
            .Include(e => e.Leitor)
            .Where(e => e.DataDevolucao == null && e.DataPrevistaDevolucao < hoje)
            .OrderBy(e => e.DataPrevistaDevolucao)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> RegistarDevolucaoAsync(int id)
    {
        var emprestimo = await _context.Emprestimos.FindAsync(id);

        if (emprestimo == null || emprestimo.DataDevolucao != null)
        {
            return false;
        }

        emprestimo.DataDevolucao = DateTime.Today;
        return await SaveAllAsync();
    }
}
