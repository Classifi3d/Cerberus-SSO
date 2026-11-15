using Application.Email.Abstraction;
using Application.Email.Contracts;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Email;

public class EmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IOptions<SmtpOptions> options, ILogger<EmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessageEvent emailEvent, CancellationToken cancellationToken)
    {
        var from = new MailboxAddress(_options.FromName, _options.Username);
        var to = new MailboxAddress(emailEvent.RecipientName, emailEvent.RecipientEmail);

        var message = new MimeMessage();
        message.From.Add(from);
        message.To.Add(to);
        message.Subject = emailEvent.TemplateName;
        message.Body = new BodyBuilder
        {
            HtmlBody = EmailTemplateRenderer.Render(emailEvent.TemplateName, emailEvent.TemplatePayload)
        }.ToMessageBody();


        using var client = new SmtpClient();

        await client.ConnectAsync(
            _options.Server,
            _options.Port,
            SecureSocketOptions.StartTlsWhenAvailable,
            cancellationToken);

        if (!string.IsNullOrEmpty(_options.Username))
        {
            await client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
        }

        var request = new MailKit.SendRequest(message);
        await client.SendAsync(FormatOptions.Default, request, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

    }
}
