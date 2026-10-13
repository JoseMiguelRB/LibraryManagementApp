using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biblioteca.Data.Entities;

// Entidade associativa: implementa a relação N:N entre Livro e Leitor,
// guardando dados próprios (datas do empréstimo).
public class Emprestimo : IEntity
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Selecione um livro.")]
    [Display(Name = "Livro")]
    public int? LivroId { get; set; }
    public Livro? Livro { get; set; }

    [Required(ErrorMessage = "Selecione um leitor.")]
    [Display(Name = "Leitor")]
    public int? LeitorId { get; set; }
    public Leitor? Leitor { get; set; }

    [Required(ErrorMessage = "O campo {0} é obrigatório.")]
    [DataType(DataType.Date)]
    [Display(Name = "Data do empréstimo")]
    public DateTime DataEmprestimo { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "O campo {0} é obrigatório.")]
    [DataType(DataType.Date)]
    [Display(Name = "Devolução prevista")]
    public DateTime DataPrevistaDevolucao { get; set; } = DateTime.Today.AddDays(15);

    [DataType(DataType.Date)]
    [Display(Name = "Data de devolução")]
    public DateTime? DataDevolucao { get; set; }

    [NotMapped]
    public bool Devolvido => DataDevolucao.HasValue;

    [NotMapped]
    public bool EmAtraso => !Devolvido && DataPrevistaDevolucao.Date < DateTime.Today;

    [NotMapped]
    public string Estado => Devolvido ? "Devolvido" : EmAtraso ? "Em atraso" : "Ativo";
}
