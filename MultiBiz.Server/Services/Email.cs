using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace MultiBiz.Server.Services;
public interface IEmailSender
{
    Task SendAsync(string to, string subject, string htmlBody);
}

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _cfg;
    public SmtpEmailSender(IConfiguration cfg) => _cfg = cfg;

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        var host = _cfg["Email:SmtpHost"]!;
        var port = int.Parse(_cfg["Email:SmtpPort"]!);
        var user = _cfg["Email:User"]!;
        var pass = _cfg["Email:Password"]!;
        var fromName = _cfg["Email:FromName"] ?? "MultiBiz";
        using var smtp = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(user, pass),
            EnableSsl = true
        };
        var msg = new MailMessage(new MailAddress(user, fromName), new MailAddress(to))
        {
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        await smtp.SendMailAsync(msg);
    }
}
