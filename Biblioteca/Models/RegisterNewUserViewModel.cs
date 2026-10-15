using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Models;

public class RegisterNewUserViewModel
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(50)]
    [Display(Name = "Nome")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "O apelido é obrigatório.")]
    [StringLength(50)]
    [Display(Name = "Apelido")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "O email é obrigatório.")]
    [EmailAddress(ErrorMessage = "Introduza um email válido.")]
    [Display(Name = "Email")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "A palavra-passe é obrigatória.")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "A palavra-passe deve ter pelo menos 6 caracteres.")]
    [Display(Name = "Palavra-passe")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme a palavra-passe.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "As palavras-passe não coincidem.")]
    [Display(Name = "Confirmar palavra-passe")]
    public string Confirm { get; set; } = string.Empty;
}
