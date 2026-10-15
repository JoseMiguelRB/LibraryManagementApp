using Biblioteca.Data.Entities;

namespace Biblioteca.Models;

public class DashboardViewModel
{
    public int TotalLivros { get; set; }
    public int TotalAutores { get; set; }
    public int TotalLeitores { get; set; }
    public int EmprestimosAtivos { get; set; }
    public List<Emprestimo> EmprestimosEmAtraso { get; set; } = new();
}
