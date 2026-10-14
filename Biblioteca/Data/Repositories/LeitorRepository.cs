using Biblioteca.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Data.Repositories;

public class LeitorRepository : GenericRepository<Leitor>, ILeitorRepository
{
    public LeitorRepository(DataContext context) : base(context)
    {
    }

    public async Task<List<Leitor>> GetAllWithEmprestimosAsync(string? pesquisa)
    {
        var query = _context.Leitores
            .Include(l => l.Emprestimos)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(pesquisa))
        {
            query = query.Where(l => l.Nome.Contains(pesquisa) || l.Email.Contains(pesquisa));
        }

        return await query.OrderBy(l => l.Nome).ToListAsync();
    }

    public async Task<Leitor?> GetByIdWithEmprestimosAsync(int id)
    {
        return await _context.Leitores
            .Include(l => l.Emprestimos).ThenInclude(e => e.Livro).ThenInclude(lv => lv!.Autor)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<bool> EmailExistsAsync(string email, int idIgnorar = 0)
    {
        return await _context.Leitores.AnyAsync(l => l.Email == email && l.Id != idIgnorar);
    }

    public async Task<bool> HasEmprestimosAsync(int leitorId)
    {
        return await _context.Emprestimos.AnyAsync(e => e.LeitorId == leitorId);
    }

    public async Task<int> CountEmprestimosAtivosAsync(int leitorId)
    {
        return await _context.Emprestimos.CountAsync(e => e.LeitorId == leitorId && e.DataDevolucao == null);
    }

    public IEnumerable<SelectListItem> GetComboLeitores()
    {
        return _context.Leitores
            .OrderBy(l => l.Nome)
            .Select(l => new SelectListItem { Text = l.Nome + " (" + l.Email + ")", Value = l.Id.ToString() })
            .ToList();
    }
}
