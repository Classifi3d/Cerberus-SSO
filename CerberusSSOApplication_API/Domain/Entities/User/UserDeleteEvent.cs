namespace Domain.Entities.User;

public class UserDeleteEvent
{
    public string Id { get; set; } = default!;
    public ulong ConcurrencyIndex { get; set; }
}
