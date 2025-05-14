using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Valuator.Services;

namespace Valuator;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();

		var connectionString = builder.Configuration.GetValue<string>( "RedisConnections:MAIN" );

		builder.Services.AddSingleton<IConnectionMultiplexer>( options =>
            ConnectionMultiplexer.Connect(  connectionString  ) );

        var redis = ConnectionMultiplexer.Connect( connectionString );

        builder.Services.AddDataProtection()
            .PersistKeysToStackExchangeRedis( redis, "DataProtection-Keys" )
            .SetApplicationName( "Valuator" );

        builder.Services.AddMvc( options => options.Filters.Add( new AutoValidateAntiforgeryTokenAttribute() ) );

        builder.Services.AddScoped<IShardManager, ShardManager>();
        builder.Services.AddSingleton<IRabbitmqService, RabbitMQService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();

        app.Run();
    }
}
