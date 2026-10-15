using Biblioteca.Data.Entities;
using Biblioteca.Models;
using Microsoft.AspNetCore.Identity;

namespace Biblioteca.Helpers;

public interface IUserHelper
{
    Task<User?> GetUserByEmailAsync(string email);

    Task<IdentityResult> AddUserAsync(User user, string password);

    Task<SignInResult> LoginAsync(LoginViewModel model);

    Task LogoutAsync();

    Task<IdentityResult> UpdateUserAsync(User user);

    Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword);

    Task<string> GeneratePasswordResetTokenAsync(User user);

    Task<IdentityResult> ResetPasswordAsync(User user, string token, string password);
}
