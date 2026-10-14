using Biblioteca.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Biblioteca.Data.Repositories;

public interface ICategoriaRepository : IGenericRepository<Categoria>
{
    Task<List<Categoria>> GetAllWithLivrosAsync();

    Task<Categoria?> GetByIdWithLivrosAsync(int id);

    Task<bool> NomeExistsAsync(string nome, int idIgnorar = 0);

    Task<bool> HasLivrosAsync(int id);

    IEnumerable<SelectListItem> GetComboCategorias();
}
