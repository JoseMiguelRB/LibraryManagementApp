using Biblioteca.Data.Entities;
using Biblioteca.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Data;

public class SeedDb
{
    private readonly DataContext _context;
    private readonly IUserHelper _userHelper;

    public SeedDb(DataContext context, IUserHelper userHelper)
    {
        _context = context;
        _userHelper = userHelper;
    }

    public async Task SeedAsync()
    {
        // Aplica as migrations pendentes (cria a BD se não existir)
        await _context.Database.MigrateAsync();

        await CriarAdminAsync();

        if (await _context.Autores.AnyAsync())
        {
            return; // já tem dados
        }

        var saramago = new Autor { Nome = "José Saramago", Nacionalidade = "Portuguesa", DataNascimento = new DateTime(1922, 11, 16) };
        var eca = new Autor { Nome = "Eça de Queirós", Nacionalidade = "Portuguesa", DataNascimento = new DateTime(1845, 11, 25) };
        var orwell = new Autor { Nome = "George Orwell", Nacionalidade = "Britânica", DataNascimento = new DateTime(1903, 6, 25) };
        var tolkien = new Autor { Nome = "J. R. R. Tolkien", Nacionalidade = "Britânica", DataNascimento = new DateTime(1892, 1, 3) };

        var romance = new Categoria { Nome = "Romance", Descricao = "Ficção narrativa em prosa." };
        var classico = new Categoria { Nome = "Clássico", Descricao = "Obras de referência da literatura." };
        var fantasia = new Categoria { Nome = "Fantasia", Descricao = "Mundos imaginários e elementos mágicos." };

        var livros = new List<Livro>
        {
            new() { Titulo = "Ensaio sobre a Cegueira", ISBN = "9789720046710", AnoPublicacao = 1995, NumeroExemplares = 3, Autor = saramago, Categoria = romance },
            new() { Titulo = "Memorial do Convento", ISBN = "9789722100977", AnoPublicacao = 1982, NumeroExemplares = 2, Autor = saramago, Categoria = classico },
            new() { Titulo = "Os Maias", ISBN = "9789723826302", AnoPublicacao = 1888, NumeroExemplares = 4, Autor = eca, Categoria = classico },
            new() { Titulo = "1984", ISBN = "9780451524935", AnoPublicacao = 1949, NumeroExemplares = 2, Autor = orwell, Categoria = romance },
            new() { Titulo = "O Senhor dos Anéis", ISBN = "9789721044159", AnoPublicacao = 1954, NumeroExemplares = 1, Autor = tolkien, Categoria = fantasia }
        };

        var ana = new Leitor { Nome = "Ana Silva", Email = "ana.silva@email.pt", Telefone = "912345678", DataRegisto = DateTime.Today.AddMonths(-6) };
        var rui = new Leitor { Nome = "Rui Costa", Email = "rui.costa@email.pt", Telefone = "936543210", DataRegisto = DateTime.Today.AddMonths(-2) };

        var emprestimos = new List<Emprestimo>
        {
            new() { Livro = livros[0], Leitor = ana, DataEmprestimo = DateTime.Today.AddDays(-5), DataPrevistaDevolucao = DateTime.Today.AddDays(10) },
            new() { Livro = livros[2], Leitor = rui, DataEmprestimo = DateTime.Today.AddDays(-30), DataPrevistaDevolucao = DateTime.Today.AddDays(-15) },
            new() { Livro = livros[3], Leitor = ana, DataEmprestimo = DateTime.Today.AddDays(-40), DataPrevistaDevolucao = DateTime.Today.AddDays(-25), DataDevolucao = DateTime.Today.AddDays(-27) }
        };

        _context.Autores.AddRange(saramago, eca, orwell, tolkien);
        _context.Categorias.AddRange(romance, classico, fantasia);
        _context.Livros.AddRange(livros);
        _context.Leitores.AddRange(ana, rui);
        _context.Emprestimos.AddRange(emprestimos);

        await _context.SaveChangesAsync();
    }

    private async Task CriarAdminAsync()
    {
        const string email = "admin@biblioteca.pt";

        var user = await _userHelper.GetUserByEmailAsync(email);
        if (user != null)
        {
            return;
        }

        user = new User
        {
            FirstName = "Administrador",
            LastName = "Biblioteca",
            Email = email,
            UserName = email,
            EmailConfirmed = true
        };

        var result = await _userHelper.AddUserAsync(user, "Admin123");
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Não foi possível criar o utilizador administrador no seed.");
        }
    }
}
