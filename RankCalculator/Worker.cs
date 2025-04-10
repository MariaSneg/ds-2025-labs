using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System.Text;

namespace RankCalculator;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConnectionMultiplexer _redis;
    private readonly IConnection _connection;
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
            _logger.LogInformation( "Connection to RabbitMQ successful");
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
            IChannel channel = await _connection.CreateChannelAsync();

            await DeclareTopologyAsync( channel , stoppingToken);
            var consumer = new AsyncEventingBasicConsumer( channel );
            consumer.ReceivedAsync += async ( _, eventArgs ) => await ConsumeMessageAsync( eventArgs, channel );

            await channel.BasicConsumeAsync(
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

        await Task.Delay( 1000, stoppingToken );
    }

    private async Task ConsumeMessageAsync( BasicDeliverEventArgs eventArgs, IChannel channel )
    {
        _logger.LogInformation( "Consuming message..." );

        string key = Encoding.UTF8.GetString( eventArgs.Body.ToArray() ).Trim( '\"' );
        var db = _redis.GetDatabase();

        string textKey = "TEXT-" + key;

        string text = Convert.ToString( db.StringGet( textKey ) );

        var rank = CalculateRank( text! );

        string rankKey = "RANK-" + key;

        await db.StringSetAsync( rankKey, rank );

        await channel.BasicAckAsync( eventArgs.DeliveryTag, false );

        _logger.LogInformation( "key: {key} text: {text}", key, text );

        _logger.LogInformation( "Message processed. Rank: {rank}", rank );
    }

    private static double CalculateRank( string text )
    {
        if ( string.IsNullOrEmpty( text ) )
            return 0;

        int totalChars = text.Length;
        int nonAlphabeticCount = text.Count( c => !char.IsLetter( c ) );

        double ratio = ( double )nonAlphabeticCount / totalChars;
        return Math.Round( ratio, 3 );
    }

    private async Task DeclareTopologyAsync( IChannel channel, CancellationToken ct )
    {
        await channel.ExchangeDeclareAsync(
            exchange: "valuator",
            type: ExchangeType.Direct,
            cancellationToken: ct
        );
        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: ct
        );
        await channel.QueueBindAsync(
            queue: QueueName,
            exchange: "valuator",
            routingKey: "rank",
            cancellationToken: ct );
    }
}