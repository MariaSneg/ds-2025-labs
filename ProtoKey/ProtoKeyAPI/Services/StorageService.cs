using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Channels;

namespace ProtoKeyAPI.Services;

public class StorageService : BackgroundService
{
    private readonly Dictionary<string, int> store = new();
    private readonly BlockingCollection<Command> queue = new();
    private readonly Channel<PersistedCommand> _commandChannel;

    private static readonly Regex keyRegex = new( "^[a-zA-Z0-9_.-]{1,1000}$" );

    public StorageService( Channel<PersistedCommand> commandChannel )
    {
        _commandChannel = commandChannel;
    }

    public Task<int> Get( string key )
    {
        ValidateKey( key );
        var tcs = new TaskCompletionSource<int>();
        queue.Add( new Command.Get( key, tcs ) );
        return tcs.Task;
    }

    public async Task<bool> Set( string key, int value )
    {
        ValidateKey( key );
        ValidateValue( value );
        var tcs = new TaskCompletionSource<bool>();
        queue.Add( new Command.Set( key, value, tcs ) );

        await _commandChannel.Writer.WriteAsync( new PersistedCommand( key, value ) );

        return await tcs.Task;
    }

    public Task<List<string>> Keys( string? prefix )
    {
        if ( !string.IsNullOrEmpty( prefix ) )
            ValidateKey( prefix );
        var tcs = new TaskCompletionSource<List<string>>();
        queue.Add( new Command.Keys( prefix, tcs ) );
        return tcs.Task;
    }

    private static void ValidateKey( string key )
    {
        if ( !keyRegex.IsMatch( key ) )
            throw new ArgumentException( "Invalid key" );
    }

    private static void ValidateValue( int value )
    {
        if ( value < int.MinValue || value > int.MaxValue )
            throw new ArgumentException( "Invalid value" );
    }

    public override async Task StartAsync( CancellationToken cancellationToken )
    {
        await LoadFromDisk();
        await base.StartAsync( cancellationToken );
    }


    protected override Task ExecuteAsync( CancellationToken stoppingToken )
    {
        return Task.Run( () =>
        {
            foreach ( var cmd in queue.GetConsumingEnumerable( stoppingToken ) )
            {
                switch ( cmd )
                {
                    case Command.Set set:
                        store[ set.Key ] = set.Value;
                        set.Tcs.SetResult( true );
                        break;
                    case Command.Get get:
                        get.Tcs.SetResult( store.TryGetValue( get.Key, out var value ) ? value : 0 );
                        break;
                    case Command.Keys keys:
                        var result = string.IsNullOrEmpty( keys.Prefix )
                            ? store.Keys.ToList()
                            : store.Keys.Where( k => k.StartsWith( keys.Prefix ) ).ToList();
                        keys.Tcs.SetResult( result );
                        break;
                }
            }
        }, stoppingToken );
    }

    public async Task LoadFromDisk()
    {
        if ( !File.Exists( "ProtoKey.data" ) )
            return;

        var lines = await File.ReadAllLinesAsync( "ProtoKey.data" );
        foreach ( var line in lines )
        {
            var cmd = JsonSerializer.Deserialize<Command.Set>( line );
            if ( cmd != null )
            {
                store[ cmd.Key ] = cmd.Value;
            }
        }
    }

    private abstract record Command
    {
        public record Set( string Key, int Value, TaskCompletionSource<bool> Tcs ) : Command;
        public record Get( string Key, TaskCompletionSource<int> Tcs ) : Command;
        public record Keys( string? Prefix, TaskCompletionSource<List<string>> Tcs ) : Command;
    }
}

public record PersistedCommand( string Key, int Value );

