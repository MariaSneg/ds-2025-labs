using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using static System.Net.Mime.MediaTypeNames;

namespace RankCalculator;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConnectionMultiplexer _redis;
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private const string QueueName = "calculate";

    public Worker( ILogger<Worker> logger, IConnectionMultiplexer redis )
    {
        _logger = logger;
        _redis = redis;

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
            _logger.LogInformation( "Connection to RabbitMQ successful" );
        }
        catch ( Exception ex )
        {
            _logger.LogError( ex, "Error conecting to RabbitMQ" );
            throw;
        }
    }

    protected override async Task ExecuteAsync( CancellationToken stoppingToken )
    {
        _logger.LogInformation( "Worker started at: {time}", DateTimeOffset.Now );

        try
        {
            await RunConsumerAsync( stoppingToken );
        }
        catch ( Exception ex )
        {
            _logger.LogError( ex, "Error occurred while processing messages" );
        }

        _logger.LogInformation( "Worker stopped at: {time}", DateTimeOffset.Now );
    }

    private async Task RunConsumerAsync( CancellationToken stoppingToken )
    {
        try
        {
            await DeclareTopologyAsync(stoppingToken);
            var consumer = new AsyncEventingBasicConsumer( _channel );
            consumer.ReceivedAsync += async ( _, eventArgs ) => await ConsumeMessageAsync( eventArgs, stoppingToken );

            await _channel.BasicConsumeAsync(
                queue: QueueName,
                autoAck: false,
                consumer: consumer
            );
        }
        catch ( Exception e )
        {
            _logger.LogError( e.Message );
        };
        _logger.LogInformation( "Consumer started and waiting for messages..." );

        // Keep the consumer running until cancellation is requested
        await Task.Delay( 1000, stoppingToken );
    }

    private async Task ConsumeMessageAsync( BasicDeliverEventArgs eventArgs, CancellationToken stoppingToken )
    {
        _logger.LogInformation( "Consuming message..." );

        string key = Encoding.UTF8.GetString( eventArgs.Body.ToArray() ).Trim( '\"' );
        var db = _redis.GetDatabase();

        string textKey = "TEXT-" + key;

        string text = Convert.ToString( db.StringGet( textKey ) );

        var rank = CalculateRank( text! );

        string rankKey = "RANK-" + key;

        await db.StringSetAsync( rankKey, rank );

        await _channel.BasicAckAsync( eventArgs.DeliveryTag, false );

        SendMessage(key, rank, stoppingToken);

        _logger.LogInformation( "key: {key} text: {text}", key, text );

        _logger.LogInformation( "Message processed. Rank: {rank}", rank );
    }

    private static double CalculateRank( string text )
    {
        if ( string.IsNullOrEmpty( text ) )
            return 0;

        int totalChars = text.Length;
        int nonAlphabeticCount = text.Count( c => !char.IsLetter( c ) );

        return ( double )nonAlphabeticCount / totalChars;
    }

    private async Task DeclareTopologyAsync( CancellationToken ct )
    {
        await _channel.ExchangeDeclareAsync(
            exchange: "valuator",
            type: ExchangeType.Direct,
            cancellationToken: ct
        );
        await _channel.QueueDeclareAsync(
            queue: "calculate",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: ct
        );
        await _channel.QueueBindAsync(
            queue: "calculate",
            exchange: "valuator",
            routingKey: "rank",
            cancellationToken: ct );
    }

    public void SendMessage( string id, double rank, CancellationToken stoppingToken )
    {
        _logger.LogInformation( "key: {key} rank: {rank}", id, rank );
        var message = new { Id = id, Rank = rank };
        string serializedMessage = JsonSerializer.Serialize( message );
        Task.Factory.StartNew( () => ProduceAsync( stoppingToken, serializedMessage ), stoppingToken );
    }

    private async Task ProduceAsync( CancellationToken ct, string jsonMessage )
    {
        byte[] messageData = Encoding.UTF8.GetBytes( jsonMessage );

        await _channel.BasicPublishAsync(
            exchange: "events_logger",
            routingKey: "valuator.events_logger.rank.calculate",
            mandatory: false,
            body: messageData,
            cancellationToken: ct
        );

        await Task.Delay( TimeSpan.FromSeconds( 1 ), ct );
    }
}