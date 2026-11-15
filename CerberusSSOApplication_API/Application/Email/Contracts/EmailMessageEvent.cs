namespace Application.Email.Contracts;

public class EmailMessageEvent
{
    public string RecipientEmail { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public Dictionary<string, string> TemplatePayload { get; set; } = new();
    public int RetryCount { get; set; }
}
