using Biblioteca.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Data;

public class DataContext : IdentityDbContext<User>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<Autor> Autores => Set<Autor>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Livro> Livros => Set<Livro>();
    public DbSet<Leitor> Leitores => Set<Leitor>();
    public DbSet<Emprestimo> Emprestimos => Set<Emprestimo>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // obrigatório para as tabelas do Identity

        // Índices únicos
        builder.Entity<Livro>().HasIndex(l => l.ISBN).IsUnique();
        builder.Entity<Categoria>().HasIndex(c => c.Nome).IsUnique();
        builder.Entity<Leitor>().HasIndex(l => l.Email).IsUnique();

        // Autor 1 -> N Livros
        builder.Entity<Livro>()
            .HasOne(l => l.Autor)
            .WithMany(a => a.Livros)
            .HasForeignKey(l => l.AutorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Categoria 1 -> N Livros
        builder.Entity<Livro>()
            .HasOne(l => l.Categoria)
            .WithMany(c => c.Livros)
            .HasForeignKey(l => l.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Livro N <-> N Leitor (via Emprestimo)
        builder.Entity<Emprestimo>()
            .HasOne(e => e.Livro)
            .WithMany(l => l.Emprestimos)
            .HasForeignKey(e => e.LivroId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Emprestimo>()
            .HasOne(e => e.Leitor)
            .WithMany(l => l.Emprestimos)
            .HasForeignKey(e => e.LeitorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
