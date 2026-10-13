using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Biblioteca.Data.Entities;

public class User : IdentityUser
{
    [Required, MaxLength(50)]
    [Display(Name = "Nome")]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    [Display(Name = "Apelido")]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "Nome completo")]
    public string FullName => $"{FirstName} {LastName}";
}
