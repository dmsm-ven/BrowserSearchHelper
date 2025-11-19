using ContentManagerHelper.ImageSearcher;
using OpenQA.Selenium;

public class YandexImagesSearcher : ImageSearcherBase
{
    public override string Name => "Yandex";

    protected override void MoveToInitialLocation()
    {
        driver.Value.Navigate().GoToUrl("https://yandex.ru/images/");
    }

    protected override void FindNext(string phrase)
    {
        var input = driver.Value.FindElement(By.XPath("//input[@type='text']"));
        input.Clear();
        input.SendKeys(phrase + Keys.Enter);
    }
}