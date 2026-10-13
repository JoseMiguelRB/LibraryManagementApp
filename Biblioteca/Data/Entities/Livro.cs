using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biblioteca.Data.Entities;

public class Livro : IEntity
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O campo {0} é obrigatório.")]
    [StringLength(150, ErrorMessage = "O {0} não pode ter mais de {1} caracteres.")]
    [Display(Name = "Título")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "O campo {0} é obrigatório.")]
    [RegularExpression(@"^(\d{10}|\d{13})$", ErrorMessage = "O ISBN deve ter 10 ou 13 dígitos (sem hífenes).")]
    public string ISBN { get; set; } = string.Empty;

    [Range(1450, 2100, ErrorMessage = "O {0} deve estar entre {1} e {2}.")]
    [Display(Name = "Ano de publicação")]
    public int AnoPublicacao { get; set; }

    [Range(1, 1000, ErrorMessage = "O número de {0} deve estar entre {1} e {2}.")]
    [Display(Name = "Exemplares")]
    public int NumeroExemplares { get; set; } = 1;

    // FK -> Autor (N:1)
    [Required(ErrorMessage = "Selecione um autor.")]
    [Display(Name = "Autor")]
    public int? AutorId { get; set; }
    public Autor? Autor { get; set; }

    // FK -> Categoria (N:1)
    [Required(ErrorMessage = "Selecione uma categoria.")]
    [Display(Name = "Categoria")]
    public int? CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    // Livro <-> Leitor (N:N) através de Emprestimo
    public ICollection<Emprestimo> Emprestimos { get; set; } = new List<Emprestimo>();

    // Calculado (não é guardado na BD). Requer que os Emprestimos tenham sido carregados (Include).
    [NotMapped]
    [Display(Name = "Disponíveis")]
    public int ExemplaresDisponiveis => NumeroExemplares - Emprestimos.Count(e => e.DataDevolucao == null);
}