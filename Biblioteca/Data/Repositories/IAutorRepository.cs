using Biblioteca.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Biblioteca.Data.Repositories;

public interface IAutorRepository : IGenericRepository<Autor>
{
    Task<List<Autor>> GetAllWithLivrosAsync(string? pesquisa);

    Task<Autor?> GetByIdWithLivrosAsync(int id);

    Task<bool> HasLivrosAsync(int id);

    IEnumerable<SelectListItem> GetComboAutores();
}
