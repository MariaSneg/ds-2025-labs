using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Valuator.Repositories;
using Valuator.Services;
using Valuator.Utils;

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

        builder.Services.AddSingleton<IShardRouter, ShardRouter>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<ITextService, TextService>();
        builder.Services.AddSingleton<IRabbitmqService, RabbitMQService>();
        builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

        builder.Services.AddAuthentication( CookieAuthenticationDefaults.AuthenticationScheme )
            .AddCookie( options => options.LoginPath = "/Auth" );
        builder.Services.AddAuthorization();


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapRazorPages();

        app.Run();
    }
}
