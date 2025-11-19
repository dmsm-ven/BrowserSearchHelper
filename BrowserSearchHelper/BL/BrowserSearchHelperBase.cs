using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace ContentManagerHelper.ImageSearcher;

public abstract class ImageSearcherBase : INotifyPropertyChanged
{
    public event EventHandler<Tuple<int, int>> OnProgress;
    public event Action<SeleniumImageSaveResult> OnImageSave;
    public event PropertyChangedEventHandler PropertyChanged;

    private bool _isWorking;
    public bool IsWorking
    {
        get => _isWorking;
        set { _isWorking = value; RaisePropertyChanged(); }
    }

    private bool _isStarted;
    public bool IsStarted
    {
        get => _isStarted;
        set
        {
            if (_isStarted != value)
            {
                _isStarted = value;
                RaisePropertyChanged();
            }
        }
    }

    private bool _canStart;
    public bool CanStart
    {
        get => _canStart;
        set { _canStart = value; RaisePropertyChanged(); }
    }

    private string _defaultFolder;
    public string DefaultFolder
    {
        get => _defaultFolder;
        set
        {
            _defaultFolder = value;
            RaisePropertyChanged();
        }
    }

    private string _rawData;
    public string RawData
    {
        get => _rawData;
        set
        {
            _rawData = value;
            var clearData = GetClearData(_rawData);
            CanStart = clearData.Any();
            OnProgress?.Invoke(this, Tuple.Create(0, clearData.Count));
        }
    }

    protected int currentIndex;

    public abstract string Name { get; }

    protected abstract void FindNext(string image);
    protected abstract void MoveToInitialLocation();

    protected Lazy<IWebDriver> driver;

    public ImageSearcherBase()
    {
        _defaultFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");


        driver = new Lazy<IWebDriver>(() =>
        {
            if (Directory.Exists(DefaultFolder))
            {
                var chromeOptions = new ChromeOptions();
                chromeOptions.AddUserProfilePreference("download.default_directory", DefaultFolder);
                chromeOptions.AddUserProfilePreference("intl.accept_languages", "ru");
                chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);

                return new ChromeDriver(chromeOptions);
            }
            return new ChromeDriver();
        });
    }

    public async Task SaveFirstOpenedImageAndMoveNext()
    {
        driver.Value.FindElement(By.XPath("//div[@data-ictx]"))?.Click();

        //WaitUntilHasNoElements(By.XPath("//img[@data-noaft]"), TimeSpan.FromSeconds(2));

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            string image = driver.Value.FindElement(By.XPath("//img[@data-noaft]"))?.GetAttribute("src");
            await SaveOpenedImageAndMoveNext(image);
        }
        catch (NoSuchElementException)
        {

        }
    }

    public async Task SaveOpenedImageAndMoveNext(string image)
    {
        string ext = await GetValidExtension(image);

        if (ext != null)
        {
            try
            {
                string localPath = Path.Combine(DefaultFolder, Clipboard.GetText() + ext);

                await new WebClient().DownloadFileTaskAsync(image, localPath);

                OnImageSave?.Invoke(new SeleniumImageSaveResult(true, "Файл загружен"));

                MoveNext();
            }
            catch (Exception ex)
            {
                OnImageSave?.Invoke(new SeleniumImageSaveResult(false, $"Ошибка загрузки файла: {ex.Message}"));
            }
        }
        else
        {
            OnImageSave?.Invoke(new SeleniumImageSaveResult(false, $"Ошибка загрузки файла: Неизвесное расширения ({ext})"));
        }
    }
    private async Task<string> GetValidExtension(string openedImageSource)
    {
        string ext = Path.GetExtension(openedImageSource).ToLower();
        string result = null;
        var validExtensions = new string[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        if (validExtensions.Contains(ext))
        {
            result = ext;
        }
        else
        {
            foreach (var validExt in validExtensions)
            {
                if (openedImageSource.ToLower().Contains(validExt))
                {
                    result = validExt;
                    break;
                }
            }

            if (openedImageSource.StartsWith("http"))
            {
                var client = new HttpClient();
                var response = await client.GetAsync(openedImageSource);
                var remoteExtension = response.Content.Headers.ContentType.ToString();
                if (remoteExtension.Contains("webp"))
                {
                    result = ".webp";
                }
            }
        }

        if (result == ".jpeg")
        {
            result = ".jpg";
        }

        return result;

    }
    public void MoveNext()
    {
        IsWorking = true;
    }
    public async Task SearchImages()
    {
        if (string.IsNullOrWhiteSpace(RawData)) { return; }

        List<ImageSearchData> searchData = GetClearData(RawData);
        if (searchData.Count == 0) { return; }

        int total = searchData.Count();
        currentIndex = 0;

        IsStarted = true;
        IsWorking = true;

        MoveToInitialLocation();

        await Task.Delay(TimeSpan.FromSeconds(5));

        foreach (var data in searchData)
        {
            await SleepIfPaused();

            string clipboardText = !string.IsNullOrWhiteSpace(data.SaveName) ? data.SaveName : data.FindPhrase;
            Clipboard.SetText(clipboardText);
            FindNext(data.FindPhrase);

            currentIndex++;
            OnProgress?.Invoke(this, Tuple.Create(currentIndex, total));
            IsWorking = false;
        }

        IsStarted = false;
    }
    private List<ImageSearchData> GetClearData(string rawData)
    {
        var list = new List<ImageSearchData>();

        if (string.IsNullOrWhiteSpace(RawData)) { return new List<ImageSearchData>(); }

        try
        {
            var result = rawData
            .Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries))
            .Select(data => new ImageSearchData()
            {
                FindPhrase = data[0],
                SaveName = (data.Length > 1 ? data[1] : null)
            })
            .ToList();

            return result;
        }
        catch
        {
            return new List<ImageSearchData>();
        }
    }
    protected virtual async Task SleepIfPaused()
    {
        while (!IsWorking)
        {
            await Task.Delay(TimeSpan.FromSeconds(0.25));
        }
    }

    protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected void WaitUntilHasNoElements(By searchElementBy, TimeSpan waitTime)
    {
        try
        {
            new WebDriverWait(driver.Value, waitTime)
                        .Until(drv => !HasElement(searchElementBy));
        }
        catch (WebDriverTimeoutException)
        {

        }
    }

    private bool HasElement(By searchElementBy)
    {
        try
        {
            return driver.Value.FindElements(searchElementBy).Count > 0;

        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }
}