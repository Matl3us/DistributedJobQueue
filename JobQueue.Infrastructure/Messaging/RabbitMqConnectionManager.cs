using JobQueue.Infrastructure.Configuration;
using JobQueue.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace JobQueue.Infrastructure.Messaging;

public class RabbitMqConnectionManager(IOptions<RabbitMqOptions> options, ILogger<RabbitMqConnectionManager> logger)
    : IRabbitMqConnectionManager, IAsyncDisposable
{
    private IConnection? _connection;

    public async Task InitializeAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = options.Value.HostName,
            UserName = options.Value.Username,
            Password = options.Value.Password,
            Port = options.Value.Port
        };

        _connection = await factory.CreateConnectionAsync();
        _connection.ConnectionShutdownAsync += OnConnectionShutdownAsync;
    }

    public async Task<IChannel> CreateChannelAsync()
    {
        if (_connection is null || !_connection.IsOpen) throw new InvalidOperationException("Connection is not open");
        return await _connection.CreateChannelAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
    }

    private Task OnConnectionShutdownAsync(object sender, ShutdownEventArgs args)
    {
        logger.LogWarning("RabbitMQ connection was lost: {ReplyText} with code: {ReplyCode}", args.ReplyText, args.ReplyCode);
        return Task.CompletedTask;
    }
}