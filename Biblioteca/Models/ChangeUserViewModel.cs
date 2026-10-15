using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models;

public class ChangeUserViewModel
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(50)]
    [Display(Name = "Nome")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "O apelido é obrigatório.")]
    [StringLength(50)]
    [Display(Name = "Apelido")]
    public string LastName { get; set; } = string.Empty;

    [RegularExpression(@"^9\d{8}$|^2\d{8}$", ErrorMessage = "Introduza um número de telefone válido (9 dígitos).")]
    [Display(Name = "Telefone")]
    public string? PhoneNumber { get; set; }
}
