using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Indique a palavra-passe atual.")]
    [DataType(DataType.Password)]
    [Display(Name = "Palavra-passe atual")]
    public string OldPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Indique a nova palavra-passe.")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "A palavra-passe deve ter pelo menos 6 caracteres.")]
    [Display(Name = "Nova palavra-passe")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme a nova palavra-passe.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "As palavras-passe não coincidem.")]
    [Display(Name = "Confirmar nova palavra-passe")]
    public string Confirm { get; set; } = string.Empty;
}
