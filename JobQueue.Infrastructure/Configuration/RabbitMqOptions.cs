namespace JobQueue.Infrastructure.Configuration;

public record RabbitMqOptions
{
    public required string HostName { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
    public int Port { get; init; }
}