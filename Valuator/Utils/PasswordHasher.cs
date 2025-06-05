using System.Security.Cryptography;

namespace Valuator.Utils;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100000;

    public string Hash( string password )
    {
        byte[] salt = RandomNumberGenerator.GetBytes( SaltSize );

        using var pbkdf2 = new Rfc2898DeriveBytes( password, salt, Iterations, HashAlgorithmName.SHA256 );
        byte[] hash = pbkdf2.GetBytes( HashSize );

        return $"{Convert.ToBase64String( salt )}:{Convert.ToBase64String( hash )}";
    }

    public bool Verify( string password, string hashedPassword )
    {
        var parts = hashedPassword.Split( ':' );
        if ( parts.Length != 2 )
            return false;

        byte[] salt = Convert.FromBase64String( parts[ 0 ] );
        byte[] expectedHash = Convert.FromBase64String( parts[ 1 ] );

        using var pbkdf2 = new Rfc2898DeriveBytes( password, salt, Iterations, HashAlgorithmName.SHA256 );
        byte[] actualHash = pbkdf2.GetBytes( HashSize );

        return CryptographicOperations.FixedTimeEquals( expectedHash, actualHash );
    }
}
