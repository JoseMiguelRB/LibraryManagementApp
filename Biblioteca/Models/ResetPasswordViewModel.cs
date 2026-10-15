using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models;

public class ResetPasswordViewModel
{
    [Required(ErrorMessage = "O email é obrigatório.")]
    [EmailAddress(ErrorMessage = "Introduza um email válido.")]
    [Display(Name = "Email")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Indique a nova palavra-passe.")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "A palavra-passe deve ter pelo menos 6 caracteres.")]
    [Display(Name = "Nova palavra-passe")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme a nova palavra-passe.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "As palavras-passe não coincidem.")]
    [Display(Name = "Confirmar palavra-passe")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;
}
