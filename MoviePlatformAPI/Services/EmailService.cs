using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MoviePlatformAPI.Services.Contracts;

namespace MoviePlatformAPI.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var smtpServer = _config["EmailSettings:SmtpServer"] ?? throw new InvalidOperationException("SmtpServer is missing in config");
        var smtpPortString = _config["EmailSettings:SmtpPort"];
        var smtpPort = string.IsNullOrEmpty(smtpPortString) ? 587 : int.Parse(smtpPortString);
        var senderEmail = _config["EmailSettings:SenderEmail"] ?? throw new InvalidOperationException("SenderEmail is missing in config");
        var senderPassword = _config["EmailSettings:SenderPassword"] ?? throw new InvalidOperationException("SenderPassword is missing in config");
        var senderName = _config["EmailSettings:SenderName"] ?? "MoviePlatform";

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(senderName, senderEmail));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = body };
        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        
        await smtp.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(senderEmail, senderPassword);
        
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}