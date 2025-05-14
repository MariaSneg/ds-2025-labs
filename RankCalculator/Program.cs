using StackExchange.Redis;

namespace RankCalculator;

public class Program
{
    public static void Main( string[] args )
    {
        var builder = Host.CreateApplicationBuilder( args );

        builder.Services.AddHostedService<Worker>();

        builder.Services.AddSingleton<IRankCalculator, RankCalculatorService>();

        builder.Services.AddSingleton<IConnectionMultiplexer>( options =>
            ConnectionMultiplexer.Connect( ( "redis:6379" ) ) );

        var host = builder.Build();
        host.Run();
    }
}