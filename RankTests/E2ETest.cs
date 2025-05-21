using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using RankTests.Pages;

namespace RankTests;
public class E2ETest : IDisposable
{
    private readonly IWebDriver _driver;
    private readonly string _baseUrl = "http://localhost:8080";

    public E2ETest()
    {
        var options = new ChromeOptions();
        _driver = new ChromeDriver( options );
        _driver.Manage().Window.Maximize();
    }

    [Fact]
    public void FullScenario_SubmitTextAndVerifySummary()
    {
        var indexPage = new IndexPage( _driver, _baseUrl );
        var summaryPage = new SummaryPage( _driver );

        indexPage.Navigate();
        indexPage.SubmitText( "abc123" );

        Thread.Sleep( 2000 ); 
        Assert.Contains( "/summary?id=", summaryPage.Url );

        //Thread.Sleep( 2000 );
        bool success = summaryPage.TryGetRankAndSimilarity( out double rank, out int similarity );

        Assert.True( success, "Результаты Rank и Similarity не получены." );
        Assert.Equal( similarity, 1 );
        Assert.Equal( rank, 0.5 );
    }

    public void Dispose()
    {
        _driver.Quit();
        _driver.Dispose();
    }
}
