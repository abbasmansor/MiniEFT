namespace MiniEft.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
