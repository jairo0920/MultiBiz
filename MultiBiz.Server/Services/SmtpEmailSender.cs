using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace MultiBiz.Server;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _cfg;
    public SmtpEmailSender(IConfiguration cfg) => _cfg = cfg;

    public async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        var host = _cfg["Email:Smtp:Host"];
        var port = int.TryParse(_cfg["Email:Smtp:Port"], out var p) ? p : 587;
        var user = _cfg["Email:Smtp:User"];
        var pass = _cfg["Email:Smtp:Pass"];
        var from = _cfg["Email:From"] ?? user;

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
            return; // sin configuración, no enviar (en DEBUG ya devolvemos el token)

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            Credentials = string.IsNullOrWhiteSpace(user) ? CredentialCache.DefaultNetworkCredentials : new NetworkCredential(user, pass)
        };

        var msg = new MailMessage(from!, toEmail, subject, htmlBody) { IsBodyHtml = true };
        await client.SendMailAsync(msg);
    }
}
