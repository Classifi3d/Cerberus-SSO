using Application.Email.Contracts;

namespace Application.Email.Abstraction;

public interface IEmailEventProducer
{
    Task QueueEmailEventAsync(EmailMessageEvent emailEvent, CancellationToken cancellationToken = default);
}
