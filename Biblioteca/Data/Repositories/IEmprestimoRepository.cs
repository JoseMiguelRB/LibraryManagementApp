using Biblioteca.Data.Entities;

namespace Biblioteca.Data.Repositories;

public interface IEmprestimoRepository : IGenericRepository<Emprestimo>
{
    Task<List<Emprestimo>> GetAllWithDetailsAsync(string? estado, string? pesquisa);

    Task<Emprestimo?> GetByIdWithDetailsAsync(int id);

    Task<bool> LeitorTemLivroAtivoAsync(int leitorId, int livroId, int idIgnorar = 0);

    Task<List<Emprestimo>> GetEmAtrasoAsync();

    Task<bool> RegistarDevolucaoAsync(int id);
}
