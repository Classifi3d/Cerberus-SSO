namespace Application.Email.Abstraction;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string toEmail, string toName, CancellationToken cancellationToken);

    //Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetLink, CancellationToken cancellationToken);
}
