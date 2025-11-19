using BrowserSearchHelper.ViewModels;
using ContentManagerHelper.ImageSearcher;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Configuration.Provider;
using System.Windows;

namespace BrowserSearchHelper;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IHost AppHost { get; private set; }

    public App()
    {

        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<ImageSearcherBase>(x => new GoogleImagesSearcher());
                services.AddSingleton<ImageSearcherBase>(x => new YandexImagesSearcher());
                services.AddSingleton<MainWindowViewModel>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        MainWindow = new MainWindow();
        MainWindow.DataContext = AppHost.Services.GetRequiredService<MainWindowViewModel>();
        MainWindow.Closed += (o, ev) => Shutdown();
        MainWindow.Show();

    }
}

