using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Data.Entities;

public class Autor : IEntity
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O campo {0} é obrigatório.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O {0} deve ter entre {2} e {1} caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "A {0} não pode ter mais de {1} caracteres.")]
    public string? Nacionalidade { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Data de nascimento")]
    public DateTime? DataNascimento { get; set; }

    // 1 Autor -> N Livros
    public ICollection<Livro> Livros { get; set; } = new List<Livro>();
}
