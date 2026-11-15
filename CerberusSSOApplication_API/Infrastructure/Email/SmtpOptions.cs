namespace Infrastructure.Email;

public class SmtpOptions
{
    public const string SectionName = "Smtp";

    public required string Server { get; set; }
    public required int Port { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string FromName { get; set; }
}
