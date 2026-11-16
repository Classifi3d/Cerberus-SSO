using Application.Email.Abstraction;
using Application.Email.Contracts;

namespace Infrastructure.Email;

public class EmailService : IEmailService
{
    private readonly IEmailEventProducer _producer;

    public EmailService(IEmailEventProducer producer)
    {
        _producer = producer;
    }

    public Task SendWelcomeEmailAsync(string toEmail, string toName, CancellationToken cancellationToken)
    {
        var emailEvent = new EmailMessageEvent
        {
            RecipientEmail = toEmail,
            RecipientName = toName,
            TemplateName = "WelcomeTemplate",
            TemplatePayload = new Dictionary<string, string>
            {
                ["Name"] = toName,
            }
        };

        return _producer.QueueEmailEventAsync(emailEvent, cancellationToken);
    }
}
