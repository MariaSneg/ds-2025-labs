namespace RankCalculator;
public class RankCalculatorService : IRankCalculator
{
    public double CalculateRank( string text )
    {
        if ( string.IsNullOrEmpty( text ) )
            return 0;

        int totalChars = text.Length;
        int nonAlphabeticCount = text.Count( c => !char.IsLetter( c ) );

        return ( double )nonAlphabeticCount / totalChars;
    }
}
