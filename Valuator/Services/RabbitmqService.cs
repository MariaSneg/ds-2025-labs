using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Valuator.Services;

public class RabbitMQService : IRabbitmqService
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly ILogger<RabbitMQService> _logger;

    public RabbitMQService( IConfiguration configuration, ILogger<RabbitMQService> logger )
    {
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = "rabbitmq",
            UserName = "admin",
            Password = "123",
            Port = 5672,
        };

        try
        {
            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
            _logger.LogInformation( "Подключение к RabbitMQ установлено" );
        }
        catch ( Exception ex )
        {
            _logger.LogError( ex, "Ошибка подключения к RabbitMQ" );
            throw;
        }
    }

    public void SendMessage( string id, CancellationTokenSource cts )
    {
        var message = JsonSerializer.Serialize( id );
        Task.Factory.StartNew( () => ProduceAsync( cts.Token, message ), cts.Token );
    }

    private async Task ProduceAsync( CancellationToken ct, string textId )
    {
        byte[] messageData = Encoding.UTF8.GetBytes( textId );

        await _channel.BasicPublishAsync(
        exchange: "valuator",
        routingKey: "rank",
        mandatory: false,
        body: messageData,
        cancellationToken: ct
        );

        await Task.Delay( TimeSpan.FromSeconds( 1 ), ct );
    }

    /// <summary>
    ///  Определяет топологию: producer -> exchange -> queue -> consumer.
    ///  В нашем случае соответствие 1:1 между exchange и queue, а routing key не используется.
    /// </summary>
    
}