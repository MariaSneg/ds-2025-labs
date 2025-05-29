using System.Net.Http;
using System.Text;
using System.Text.Json;

class Program
{
    private const string ProtoKeyHost = "https://localhost:7288/api/protokey";

    static int Main( string[] args )
    {
        Run();
        return 0;
    }

    private static void Run()
    {
        Console.WriteLine( "введите команды или 'exit' для выхода." );
        while ( true )
        {
            Console.Write( "ProtoCli > " );
            var input = Console.ReadLine();

            if ( string.IsNullOrWhiteSpace( input ) )
                continue;

            if ( input.Trim().ToLower() == "exit" )
                break;

            var args = ParseArguments( input );
            HandleCommand( args ).Wait();
        }
    }

    private static async Task HandleCommand( string[] args )
    {
        var command = args[ 0 ].ToLower();

        try
        {
            switch ( command )
            {
                case "set":
                    if ( args.Length != 3 )
                    {
                        Console.WriteLine( "Используйте: set <key> <value>" );
                        return;
                    }
                    await Set( args[ 1 ], args[ 2 ] );
                    break;

                case "get":
                    if ( args.Length != 2 )
                    {
                        Console.WriteLine( "Используйте: get <key>" );
                        return;
                    }
                    await Get( args[ 1 ] );
                    break;

                case "keys":
                    if ( args.Length > 2 )
                    {
                        Console.WriteLine( "Используйте: keys <prefix>" );
                        return;
                    }
                    string prefix = args.Length == 2 ? args[ 1 ] : "";
                    await Keys( prefix );
                    break;

                default:
                    ShowHelp();
                    return;
            }
        }
        catch ( Exception ex )
        {
            Console.WriteLine( $"Ошибка: {ex.Message}" );
        }
    }

    private static async Task Set( string key, string value )
    {
        if ( !int.TryParse( value, out int intValue ) )
        {
            Console.WriteLine( "Невалидное значение, должен быть тип int" );
            return;
        }

        var data = new { key, value = intValue };

        using var client = new HttpClient();
        var content = new StringContent( JsonSerializer.Serialize( data ), Encoding.UTF8, "application/json" );
        var response = await client.PostAsync( $"{ProtoKeyHost}/set", content );


        if ( response.IsSuccessStatusCode )
            Console.WriteLine( "OK" );
        else
            Console.WriteLine( $"Ошибка: {await response.Content.ReadAsStringAsync()}" );
    }

    private static async Task Get( string key )
    {
        using var client = new HttpClient();
        var response = await client.GetAsync( $"{ProtoKeyHost}/get/{ key }" );

        if ( response.IsSuccessStatusCode )
            Console.WriteLine( await response.Content.ReadAsStringAsync() );
        else
            Console.WriteLine( $"Ошибка: {await response.Content.ReadAsStringAsync()}" );
    }

    private static async Task Keys( string prefix )
    {
        using var client = new HttpClient();
        var response = await client.GetAsync( $"{ProtoKeyHost}/keys?prefix={ prefix }" );

        if ( response.IsSuccessStatusCode )
        {
            var json = await response.Content.ReadAsStringAsync();
            var keys = JsonSerializer.Deserialize<List<string>>( json );
            Console.WriteLine( string.Join( "\n", keys ) );
        }
        else
        {
            Console.WriteLine( $"Ошибка: {await response.Content.ReadAsStringAsync()}" );
        }
    }

    private static void ShowHelp()
    {
        Console.WriteLine( "Команды:" );
        Console.WriteLine( "  set <key> <value>    Устанавливает значение" );
        Console.WriteLine( "  get <key>            Получает значение по ключу" );
        Console.WriteLine( "  keys <prefix>        Получает список ключей по префиксу" );
    }

    private static string[] ParseArguments( string input )
    {
        var args = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        foreach ( char c in input )
        {
            if ( c == '"' )
            {
                inQuotes = !inQuotes;
                continue;
            }

            if ( char.IsWhiteSpace( c ) && !inQuotes )
            {
                if ( current.Length > 0 )
                {
                    args.Add( current.ToString() );
                    current.Clear();
                }
            }
            else
            {
                current.Append( c );
            }
        }

        if ( current.Length > 0 )
            args.Add( current.ToString() );

        return args.ToArray();
    }
}
