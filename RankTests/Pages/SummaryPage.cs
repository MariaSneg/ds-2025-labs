using System.Globalization;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace RankTests.Pages;
public class SummaryPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public SummaryPage( IWebDriver driver )
    {
        _driver = driver;
        _wait = new WebDriverWait( driver, TimeSpan.FromSeconds( 10 ) );
    }

    public bool TryGetRankAndSimilarity( out double rank, out int similarity )
    {
        ///try
        //{
            _wait.Until(driver => driver.FindElement( By.Id( "rank"  ) ).Text);

            var rankText = _driver.FindElement( By.Id( "rank" ) ).Text;

            _wait.Until( driver => driver.FindElement( By.Id( "similarity" ) ).Text );
            var similarityText = _driver.FindElement( By.Id( "similarity" ) ).Text;

            rank = double.Parse( rankText.Split( ":" )[ 1 ].Trim(), CultureInfo.InvariantCulture );
            similarity = int.Parse( similarityText.Split( ":" )[ 1 ].Trim() );

            return true;
        //}
        //catch
        //{
        //    rank = 0;
        //    similarity = 0;
        //    return false;
        //}
    }

    public void Refresh()
    {
        _driver.Navigate().Refresh();
    }

    public string Url => _driver.Url;
}