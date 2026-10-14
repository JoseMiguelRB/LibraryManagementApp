using Biblioteca.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Data.Repositories;

public class LivroRepository : GenericRepository<Livro>, ILivroRepository
{
    public LivroRepository(DataContext context) : base(context)
    {
    }

    public async Task<List<Livro>> GetAllWithDetailsAsync(string? pesquisa, int? categoriaId)
    {
        var query = _context.Livros
            .Include(l => l.Autor)
            .Include(l => l.Categoria)
            .Include(l => l.Emprestimos)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(pesquisa))
        {
            query = query.Where(l => l.Titulo.Contains(pesquisa) ||
                                     l.ISBN.Contains(pesquisa) ||
                                     l.Autor!.Nome.Contains(pesquisa));
        }

        if (categoriaId.HasValue)
        {
            query = query.Where(l => l.CategoriaId == categoriaId);
        }

        return await query.OrderBy(l => l.Titulo).ToListAsync();
    }

    public async Task<Livro?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Livros
            .Include(l => l.Autor)
            .Include(l => l.Categoria)
            .Include(l => l.Emprestimos).ThenInclude(e => e.Leitor)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<bool> IsbnExistsAsync(string isbn, int idIgnorar = 0)
    {
        return await _context.Livros.AnyAsync(l => l.ISBN == isbn && l.Id != idIgnorar);
    }

    public async Task<int> CountEmprestimosAtivosAsync(int livroId)
    {
        return await _context.Emprestimos.CountAsync(e => e.LivroId == livroId && e.DataDevolucao == null);
    }

    public async Task<bool> HasEmprestimosAsync(int livroId)
    {
        return await _context.Emprestimos.AnyAsync(e => e.LivroId == livroId);
    }

    public async Task<IEnumerable<SelectListItem>> GetComboLivrosDisponiveisAsync(int? incluirLivroId = null)
    {
        // Só mostra livros com exemplares disponíveis (ou o livro indicado, útil na edição)
        return await _context.Livros
            .Where(l => l.Id == incluirLivroId ||
                        l.NumeroExemplares > l.Emprestimos.Count(e => e.DataDevolucao == null))
            .OrderBy(l => l.Titulo)
            .Select(l => new SelectListItem
            {
                Text = l.Titulo + " (" + l.Autor!.Nome + ")",
                Value = l.Id.ToString()
            })
            .ToListAsync();
    }
}
