using System.Threading.Channels;
using ProtoKeyAPI.Services;

var builder = WebApplication.CreateBuilder( args );

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var commandChannel = Channel.CreateUnbounded<PersistedCommand>();
builder.Services.AddSingleton( commandChannel );
builder.Services.AddSingleton<StorageService>();
builder.Services.AddSingleton<PersistenceService>();
builder.Services.AddHostedService( provider => provider.GetRequiredService<PersistenceService>() );
builder.Services.AddHostedService( provider => provider.GetRequiredService<StorageService>() );

var app = builder.Build();

// Configure the HTTP request pipeline.
if ( app.Environment.IsDevelopment() )
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
