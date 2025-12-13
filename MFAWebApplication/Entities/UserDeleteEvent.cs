namespace MFAWebApplication.Entities;

public class UserDeletedEvent
{
    public string Id { get; set; } = default!;
    public ulong ConcurrencyIndex { get; set; }
}
