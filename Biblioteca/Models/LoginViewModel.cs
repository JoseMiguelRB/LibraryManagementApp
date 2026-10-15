using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "O email é obrigatório.")]
    [EmailAddress(ErrorMessage = "Introduza um email válido.")]
    [Display(Name = "Email")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "A palavra-passe é obrigatória.")]
    [DataType(DataType.Password)]
    [Display(Name = "Palavra-passe")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Lembrar-me")]
    public bool RememberMe { get; set; }
}