using Biblioteca.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Biblioteca.Data.Repositories;

public interface ILeitorRepository : IGenericRepository<Leitor>
{
    Task<List<Leitor>> GetAllWithEmprestimosAsync(string? pesquisa);

    Task<Leitor?> GetByIdWithEmprestimosAsync(int id);

    Task<bool> EmailExistsAsync(string email, int idIgnorar = 0);

    Task<bool> HasEmprestimosAsync(int leitorId);

    Task<int> CountEmprestimosAtivosAsync(int leitorId);

    IEnumerable<SelectListItem> GetComboLeitores();
}
