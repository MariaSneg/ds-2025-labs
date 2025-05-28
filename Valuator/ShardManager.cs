using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using StackExchange.Redis;
using Valuator.Models;

namespace Valuator;

public class ShardManager : IShardManager
{
    private Dictionary<string, string> _shardConectionStringDictionary = new Dictionary<string, string>();
    private ILogger<ShardManager> _logger;
    private IDatabase _database;
    private IDatabase _mainDatabase;


    public ShardManager( ILogger<ShardManager> logger, IConfiguration configuration )
    {
        _logger = logger;
		_shardConectionStringDictionary.Add( "MAIN", configuration[ "RedisConnections:MAIN" ] ?? "redis_main:6379" );
		_shardConectionStringDictionary.Add( "RU", configuration[ "RedisConnections:RU" ] ?? "redis_ru:6379" );
		_shardConectionStringDictionary.Add( "EU", configuration[ "RedisConnections:EU" ] ?? "redis_eu:6379" );
		_shardConectionStringDictionary.Add( "ASIA", configuration[ "RedisConnections:ASIA" ] ?? "redis_asia:6379" );

        SetMainShard();
	}

	private string GetEnvironmentVariable( string name )
	{
		var value = Environment.GetEnvironmentVariable( name );
		if ( string.IsNullOrEmpty( value ) )
		{
			throw new InvalidOperationException( $"Environment variable {name} is not set" );
		}
		return value;
	}

	public void SetRegionShard( string key )
    {
		_logger.LogInformation( _shardConectionStringDictionary[ "MAIN" ] );
		var mainShard = ConnectionMultiplexer.Connect( _shardConectionStringDictionary[ "MAIN" ] ).GetDatabase();
        var region = mainShard.StringGet( key ).ToString();

        _logger.LogInformation( $"LOOKUP {key}, {region}" );

		_database = ConnectionMultiplexer.Connect( _shardConectionStringDictionary[ region ] ).GetDatabase();
    }

    public void SetMainShard(  )
    {
        _logger.LogInformation( _shardConectionStringDictionary[ "MAIN" ] );
        _mainDatabase = ConnectionMultiplexer.Connect( _shardConectionStringDictionary[ "MAIN" ] ).GetDatabase();
    }

    public void SetRank( string key, double value )
    {
		_database.StringSet( "RANK-" + key, value );
    }

    public void SetSimilarity( string key, int value )
    {
		_database.StringSet( "SIMILARITY-" + key, value );
    }

    public void SetText( string key, string value )
    {
		_database.StringSet( "TEXT-" + key, value );
    }

    public void SetAuthor( string key, string value )
    {
        _database.StringSet( "AUTHOR-" + key, value );
    }

    public void SetToMain( string key, string value )
    {
		_logger.LogInformation( _shardConectionStringDictionary[ "MAIN" ] );
		var mainShard = ConnectionMultiplexer.Connect( _shardConectionStringDictionary[ "MAIN" ] ).GetDatabase();
        mainShard.StringSet( key, value );
    }

    public RedisValue Get( string key, string prefix )
    {
        return _database.StringGet( prefix + key );
    }

    public int CheckSimilarity( string text )
    {
        foreach ( var region in _shardConectionStringDictionary.Keys )
        {
            if ( region == "MAIN" )
                continue;
            var connectionString = _shardConectionStringDictionary[ region ];

			var shard = ConnectionMultiplexer.Connect( connectionString ).GetDatabase();
			var parts = connectionString.Split( ',' );
			var hostAndPort = parts[ 0 ];

			var server = ConnectionMultiplexer.Connect( connectionString ).GetServer( hostAndPort );
            var keys = server.Keys( pattern: "TEXT-*" );

            foreach ( var key in keys )
            {
                var storedText = shard.StringGet( key );
                if ( storedText == text )
                {
                    return 1;
                }
            }
        }
        return 0;
    }

    public RedisValue GetAuthor( string id )
    {
        return Get( id, "AUTHOR-" );
    }

    public async Task<User?> GetUser( string username )
    {
        var db = _mainDatabase;
        var userId = await db.StringGetAsync( "USER-USERNAME-" + username );

        var hash = await db.HashGetAllAsync( "USER-" + userId );

        if ( hash.Length == 0 )
            return null;
        _logger.LogInformation( "GET USER" );

        return new User
        {
            Id = userId.ToString(),
            Username = hash.FirstOrDefault( x => x.Name == "username" ).Value.ToString(),
            Password = hash.FirstOrDefault( x => x.Name == "password" ).Value.ToString()
        };
    }


    public async Task AddUser( User user )
    {
        _logger.LogInformation( "ADD USER 1 {id}, {username}, {pwd}", user.Id, user.Username, user.Password );
        var entries = new HashEntry[]
        {
        new("id", user.Id),
        new("username", user.Username),
        new("password", user.Password)
        };

        var db = _mainDatabase;

        var transaction = db.CreateTransaction();

        // Основной ключ с данными пользователя
        await db.HashSetAsync( $"USER-{user.Id}", entries );

        // Индекс для поиска по username
        await db.StringSetAsync( $"USER-USERNAME-{user.Username}", user.Id );

        var committed = transaction.Execute();

        if(!committed )
        {
            new Exception( "transaction is not committed" );
        }

        _logger.LogInformation( "ADD USER 2 {id}, {username}, {pwd}", user.Id, user.Username, user.Password );
    }

    public async Task<bool> UserExists( string username )
    {
        var db = _mainDatabase;

        var exists = await db.KeyExistsAsync( $"USER-USERNAME-{username}" );

        if ( !exists )
        {
            return false;
        }

        return true;
    }

}
