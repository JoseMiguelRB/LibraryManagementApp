using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models;

public class RecoverPasswordViewModel
{
    [Required(ErrorMessage = "O email é obrigatório.")]
    [EmailAddress(ErrorMessage = "Introduza um email válido.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
}
