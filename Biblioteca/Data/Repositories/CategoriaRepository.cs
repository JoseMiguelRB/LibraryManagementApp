using Biblioteca.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Data.Repositories;

public class CategoriaRepository : GenericRepository<Categoria>, ICategoriaRepository
{
    public CategoriaRepository(DataContext context) : base(context)
    {
    }

    public async Task<List<Categoria>> GetAllWithLivrosAsync()
    {
        return await _context.Categorias
            .Include(c => c.Livros)
            .AsNoTracking()
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }

    public async Task<Categoria?> GetByIdWithLivrosAsync(int id)
    {
        return await _context.Categorias
            .Include(c => c.Livros).ThenInclude(l => l.Autor)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> NomeExistsAsync(string nome, int idIgnorar = 0)
    {
        return await _context.Categorias.AnyAsync(c => c.Nome == nome && c.Id != idIgnorar);
    }

    public async Task<bool> HasLivrosAsync(int id)
    {
        return await _context.Livros.AnyAsync(l => l.CategoriaId == id);
    }

    public IEnumerable<SelectListItem> GetComboCategorias()
    {
        return _context.Categorias
            .OrderBy(c => c.Nome)
            .Select(c => new SelectListItem { Text = c.Nome, Value = c.Id.ToString() })
            .ToList();
    }
}
