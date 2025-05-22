using RankCalculator;

namespace RankTests;

public class RankCalculatorUnitTests
{
    private readonly RankCalculatorService _calculator = new();
    public static TheoryData<string, double> RankTestData => new TheoryData<string, double>
    {
        { "Hello, world!", 3.0 / 13 },   
        { "12345", 1.0 },             
        { "abcDEF", 0.0 },       
        { "", 0.0 },          
        { "     ", 1.0 },         
        { "😊", 1.0 },          
        { "New\nLine", 1.0 / 8 }    
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
