using Biblioteca.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Biblioteca.Data.Repositories;

public interface ILivroRepository : IGenericRepository<Livro>
{
    Task<List<Livro>> GetAllWithDetailsAsync(string? pesquisa, int? categoriaId);

    Task<Livro?> GetByIdWithDetailsAsync(int id);

    Task<bool> IsbnExistsAsync(string isbn, int idIgnorar = 0);

    Task<int> CountEmprestimosAtivosAsync(int livroId);

    Task<bool> HasEmprestimosAsync(int livroId);

    Task<IEnumerable<SelectListItem>> GetComboLivrosDisponiveisAsync(int? incluirLivroId = null);
}
