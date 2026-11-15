using Application.Email.Contracts;

namespace Application.Email.Abstraction;

public interface IEmailSender
{
    Task SendAsync(EmailMessageEvent emailEvent, CancellationToken cancellationToken);
}
