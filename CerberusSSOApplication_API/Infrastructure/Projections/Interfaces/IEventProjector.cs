namespace Application.Projections.Interfaces;
public interface IEventProjector
{
    string EventType { get; }
    Task ProjectAsync(byte[] payload, CancellationToken cancellationToken);
}
