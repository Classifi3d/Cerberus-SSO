namespace Infrastructure.Outbox;

public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = string.Empty; 
    public byte[] Payload { get; set; } = Array.Empty<byte>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;    
    public DateTime? ProcessedAt { get; init; } = null;
}
