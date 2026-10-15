namespace Biblioteca.Helpers;

public interface IMailHelper
{
    Task<bool> SendEmailAsync(string to, string subject, string body);
}
