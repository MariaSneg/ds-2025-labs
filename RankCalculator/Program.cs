using StackExchange.Redis;

namespace RankCalculator;

public class Program
{
    public static void Main( string[] args )
    {
        var builder = WebApplication.CreateBuilder( args );

        builder.Services.AddCors( options =>
        {
            options.AddPolicy( "AllowLocalhost", policy =>
            {
                policy.WithOrigins( "http://localhost:8080" )
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials(); // Important for SignalR
            } );
        } );

        builder.Services.AddSignalR();

        builder.Services.AddHostedService<Worker>();

        builder.Services.AddSingleton<IConnectionMultiplexer>( options =>
            ConnectionMultiplexer.Connect( ( "redis:6379" ) ) );

        var app = builder.Build();

        app.UseCors( "AllowLocalhost" );

        // Настраиваем маршрут для SignalR хаба
        app.MapHub<RankHub>( "/rank" );

        app.Run();
    }
}