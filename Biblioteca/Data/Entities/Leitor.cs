using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Data.Entities;

public class Leitor : IEntity
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O campo {0} é obrigatório.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O {0} deve ter entre {2} e {1} caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O campo {0} é obrigatório.")]
    [EmailAddress(ErrorMessage = "Introduza um email válido.")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [RegularExpression(@"^9\d{8}$|^2\d{8}$", ErrorMessage = "Introduza um número de telefone português válido (9 dígitos).")]
    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Data de registo")]
    public DateTime DataRegisto { get; set; } = DateTime.Today;

    // Leitor <-> Livro (N:N) através de Emprestimo
    public ICollection<Emprestimo> Emprestimos { get; set; } = new List<Emprestimo>();
}
