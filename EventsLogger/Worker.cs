using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace EventsLogger;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConnection _connection;

    public class RankingMessage
    {
        public string Id { get; set; }
        public double Rank { get; set; }
    }

    public class SimilarityMessage
    {
        public string Id { get; set; }
        public int Similarity { get; set; }
    }

    public Worker( ILogger<Worker> logger )
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
            _logger.LogInformation( "����������� � RabbitMQ �����������" );
        }
        catch ( Exception ex )
        {
            _logger.LogError( ex, "������ ����������� � RabbitMQ" );
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

            await DeclareTopologyAsync( channel, stoppingToken );
            var consumer = new AsyncEventingBasicConsumer( channel );
            consumer.ReceivedAsync += async ( _, eventArgs ) => await ConsumeMessageAsync( eventArgs, channel );

            await channel.BasicConsumeAsync(
                queue: "events",
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

    private async Task ConsumeMessageAsync( BasicDeliverEventArgs eventArgs, IChannel channel )
    {
        _logger.LogInformation( "Consuming message..." );

        

        string key = Encoding.UTF8.GetString( eventArgs.Body.ToArray() ).Trim( '\"' );

        if ( eventArgs.RoutingKey.EndsWith( "rank.calculate" ) )
        {
            RankingMessage message = JsonSerializer.Deserialize<RankingMessage>( Encoding.UTF8.GetString( eventArgs.Body.ToArray() ) );
            _logger.LogInformation( "Rank calculate" );
            _logger.LogInformation( "Id: {id}, Rank: {rank}", message.Id, message.Rank );
        }
        else if ( eventArgs.RoutingKey.EndsWith( "similarity.calculate" ) )
        {
            SimilarityMessage message = JsonSerializer.Deserialize<SimilarityMessage>( Encoding.UTF8.GetString( eventArgs.Body.ToArray() ) );
            _logger.LogInformation( "Similarity calculate" );
            _logger.LogInformation( "Id: {id}, Similarity: {similarity}", message.Id, message.Similarity );
        }

        await channel.BasicAckAsync( eventArgs.DeliveryTag, false );
    }

    private async Task DeclareTopologyAsync( IChannel channel, CancellationToken ct )
    {
        await channel.ExchangeDeclareAsync(
            exchange: "events_logger",
            type: ExchangeType.Topic,
            cancellationToken: ct
        );
        await channel.QueueDeclareAsync(
            queue: "events",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: ct
        );
        await channel.QueueBindAsync(
            queue: "events",
            exchange: "events_logger",
            routingKey: "valuator.events_logger.#",
            cancellationToken: ct );
    }
}