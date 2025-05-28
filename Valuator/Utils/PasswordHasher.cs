using System.Security.Cryptography;
using System.Text;

namespace Valuator.Utils;

public class PasswordHasher : IPasswordHasher
{
    public string Hash( string password )
    {
        using ( SHA256 sha256Hash = SHA256.Create() )
        {
            // Получаем байты из входной строки
            byte[] bytes = sha256Hash.ComputeHash( Encoding.UTF8.GetBytes( password ) );

            // Преобразуем байты в шестнадцатеричную строку
            StringBuilder builder = new StringBuilder();
            foreach ( byte b in bytes )
            {
                builder.Append( b.ToString( "x2" ) ); // "x2" означает два символа в нижнем регистре
            }

            return builder.ToString();
        }
    }

    public bool Verify( string password, string hashedPassword )
    {
        return hashedPassword == Hash(password);
    }
}

