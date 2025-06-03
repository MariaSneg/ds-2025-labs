using StackExchange.Redis;
using Valuator.Models;

namespace Valuator.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDatabase _db;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository( IShardRouter router, ILogger<UserRepository> logger )
    {
        _db = router.GetMainShard();
        _logger = logger;
    }

    public async Task AddUser( User user )
    {
        _logger.LogInformation( "ADD USER" );
        //var tx = _db.CreateTransaction();

        await _db.HashSetAsync( $"USER-{user.Id}", new HashEntry[]
        {
            new("id", user.Id),
            new("username", user.Username),
            new("password", user.Password),
        } );

        await _db.StringSetAsync( $"USER-USERNAME-{user.Username}", user.Id );

        //var committed = await tx.ExecuteAsync();
        //if ( !committed )
        //    throw new Exception( "Transaction failed during AddUser" );

        _logger.LogInformation( "User {Username} added", user.Username );
    }

    public async Task<User?> GetUser( string username )
    {
        var userId = await _db.StringGetAsync( $"USER-USERNAME-{username}" );
        if ( userId.IsNullOrEmpty )
            return null;

        var hash = await _db.HashGetAllAsync( $"USER-{userId}" );
        if ( hash.Length == 0 )
            return null;

        return new User
        {
            Id = userId,
            Username = hash.FirstOrDefault( h => h.Name == "username" ).Value,
            Password = hash.FirstOrDefault( h => h.Name == "password" ).Value,
        };
    }

    public async Task<bool> UserExists( string username )
    {
        return await _db.KeyExistsAsync( $"USER-USERNAME-{username}" );
    }
}
