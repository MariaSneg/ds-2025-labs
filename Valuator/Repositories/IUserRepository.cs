using Valuator.Models;

namespace Valuator.Repositories;

public interface IUserRepository
{
    Task AddUser( User user );
    Task<User?> GetUser( string username );
    Task<bool> UserExists( string username );
}
