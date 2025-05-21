using OpenQA.Selenium;

namespace RankTests.Pages;
public class IndexPage
{
    private readonly IWebDriver _driver;
    private readonly string _url;

    public IndexPage( IWebDriver driver, string baseUrl )
    {
        _driver = driver;
        _url = $"{baseUrl}/";
    }

    public void Navigate()
    {
        _driver.Navigate().GoToUrl( _url );
    }

    public void SubmitText( string text )
    {
        var textArea = _driver.FindElement( By.Name( "text" ) );
        textArea.Clear();
        textArea.SendKeys( text );

        var submitButton = _driver.FindElement( By.CssSelector( "input[type='submit']" ) );
        submitButton.Click();
    }
}
