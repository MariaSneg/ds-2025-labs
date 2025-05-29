using System.Text.Json;
using System.Threading.Channels;

namespace ProtoKeyAPI.Services;

public class PersistenceService : BackgroundService
{
    private readonly Channel<PersistedCommand> _commandChannel;
    private readonly string _filePath = "ProtoKey.data";
    private readonly List<PersistedCommand> _commands = new();
    private readonly Timer _timer;

    public PersistenceService( Channel<PersistedCommand> commandChannel )
    {
        _commandChannel = commandChannel;
        _timer = new Timer( WriteToDisk, null, 1000, 1000 );
    }

    protected override async Task ExecuteAsync( CancellationToken stoppingToken )
    {
        await foreach ( var command in _commandChannel.Reader.ReadAllAsync( stoppingToken ) )
        {
            lock ( _commands )
            {
                _commands.Add( command );
            }
        }
    }

    private void WriteToDisk( object? state )
    {
        List<PersistedCommand> toWrite;
        lock ( _commands )
        {
            if ( _commands.Count == 0 )
                return;
            toWrite = new( _commands );
            _commands.Clear();
        }

        using var stream = new StreamWriter( _filePath, append: true );
        foreach ( var cmd in toWrite )
        {
            var line = JsonSerializer.Serialize( cmd );
            stream.WriteLine( line );
        }
    }
}

