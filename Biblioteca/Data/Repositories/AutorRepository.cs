using Biblioteca.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Data.Repositories;

public class AutorRepository : GenericRepository<Autor>, IAutorRepository
{
    public AutorRepository(DataContext context) : base(context)
    {
    }

    public async Task<List<Autor>> GetAllWithLivrosAsync(string? pesquisa)
    {
        var query = _context.Autores
            .Include(a => a.Livros)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(pesquisa))
        {
            query = query.Where(a => a.Nome.Contains(pesquisa) ||
                                     (a.Nacionalidade != null && a.Nacionalidade.Contains(pesquisa)));
        }

        return await query.OrderBy(a => a.Nome).ToListAsync();
    }

    public async Task<Autor?> GetByIdWithLivrosAsync(int id)
    {
        return await _context.Autores
            .Include(a => a.Livros).ThenInclude(l => l.Categoria)
            .Include(a => a.Livros).ThenInclude(l => l.Emprestimos)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<bool> HasLivrosAsync(int id)
    {
        return await _context.Livros.AnyAsync(l => l.AutorId == id);
    }

    public IEnumerable<SelectListItem> GetComboAutores()
    {
        return _context.Autores
            .OrderBy(a => a.Nome)
            .Select(a => new SelectListItem { Text = a.Nome, Value = a.Id.ToString() })
            .ToList();
    }
}
