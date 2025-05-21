using RankCalculator;

namespace RankTests;

public class RankCalculatorUnitTests
{
    private readonly RankCalculatorService _calculator = new();
    public static TheoryData<string, double> RankTestData => new TheoryData<string, double>
    {
        { "Hello, world!", 3.0 / 13 },   // ',' and '!'
        { "12345", 1.0 },                // All non-letters
        { "abcDEF", 0.0 },               // All letters
        { "", 0.0 },                     // Empty string
        { "  a b  ", 5.0 / 7 },          // 4 spaces, 3 letters
        { "!@#ABC", 3.0 / 6 },           // 3 symbols, 3 letters
        { "     ", 1.0 },                // Only spaces
        { "😊", 1.0 },                   // Emoji
        { "New\nLine", 1.0 / 8 }         // Newline is non-letter
    };
    

    [Theory]
    [MemberData( nameof( RankTestData ) )]
    public void CalculateRank_ReturnsExpected( string input, double expected )
    {
        // Act
        double result = _calculator.CalculateRank( input );

        // Assert
        Assert.Equal( expected, result, precision: 5 );
    }
}
