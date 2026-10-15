using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Biblioteca.Helpers;

public class MailHelper : IMailHelper
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MailHelper> _logger;

    public MailHelper(IConfiguration configuration, ILogger<MailHelper> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        var nameFrom = _configuration["Mail:NameFrom"];
        var from = _configuration["Mail:From"];
        var smtp = _configuration["Mail:Smtp"];
        var port = _configuration["Mail:Port"];
        var password = _configuration["Mail:Password"];

        if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(smtp) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Configuração de email incompleta. Email para {To} não enviado.", to);
            return false;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(nameFrom, from));
            message.To.Add(new MailboxAddress(to, to));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtp, int.Parse(port ?? "587"), SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(from, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email para {To}", to);
            return false;
        }
    }
}
