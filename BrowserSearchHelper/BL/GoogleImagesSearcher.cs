using ContentManagerHelper.ImageSearcher;
using OpenQA.Selenium;

public class GoogleImagesSearcher : ImageSearcherBase
{
    public override string Name => "Google";

    protected override void MoveToInitialLocation()
    {
        driver.Value.Navigate().GoToUrl("https://www.google.com/search?q=%D0%BA%D0%B0%D1%80%D1%82%D0%B8%D0%BD%D0%BA%D0%B8&source=lnms&tbm=isch&sa=X");
        //driver.Value.FindElement(By.XPath("//input")).SendKeys("Картинки" + Keys.Enter);
        //driver.Value.FindElement(By.XPath("//a[text()='Картинки']")).Click();
    }

    protected override void FindNext(string phrase)
    {
        var input = driver.Value.FindElement(By.XPath("//textarea"));
        input.Clear();
        input.SendKeys(phrase + Keys.Enter);
    }
}