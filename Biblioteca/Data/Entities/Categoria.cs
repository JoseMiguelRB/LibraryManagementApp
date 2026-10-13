using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Data.Entities;

public class Categoria : IEntity
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O campo {0} é obrigatório.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "O {0} deve ter entre {2} e {1} caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "A {0} não pode ter mais de {1} caracteres.")]
    [Display(Name = "Descrição")]
    public string? Descricao { get; set; }

    // 1 Categoria -> N Livros
    public ICollection<Livro> Livros { get; set; } = new List<Livro>();
}