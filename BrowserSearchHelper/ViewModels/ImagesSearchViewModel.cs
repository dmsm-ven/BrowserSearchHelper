using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ContentManagerHelper.ImageSearcher;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

public partial class ImagesSearchViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ImageSearcherBase> searchProviders = new();

    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveNextCommand))]
    [NotifyCanExecuteChangedFor(nameof(LoadRawDataCommand))]
    [ObservableProperty]
    private ImageSearcherBase selectedProvider;

    [ObservableProperty]
    private ProgressStatus progressStatus = new (0, 100);

    [ObservableProperty]
    string savedImagesStatus;

    public ImagesSearchViewModel(IEnumerable<ImageSearcherBase> providers)
    {
        foreach (var provider in SearchProviders)
        {
            provider.OnProgress += (o, e) => ProgressStatus = new ProgressStatus(e.Item1, e.Item2);
            provider.OnImageSave += (e) => SavedImagesStatus = e.Message;
        }

        SelectedProvider = providers.FirstOrDefault();
    }

    private bool CanLoadRawData => SelectedProvider != null && !SelectedProvider.IsStarted;
    
    [RelayCommand(CanExecute = nameof(CanLoadRawData))]
    private async Task LoadRawData() 
    {
        SelectedProvider.RawData = Clipboard.GetText();
    }

    private bool CanStart => SelectedProvider != null && SelectedProvider.CanStart && !SelectedProvider.IsStarted;

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task Start()
    {
        await SelectedProvider.SearchImages();
    }

    private bool CanMoveNext => SelectedProvider != null && SelectedProvider.IsStarted && !SelectedProvider.IsWorking;
    [RelayCommand(CanExecute = nameof(CanMoveNext))] 
    private async Task MoveNext() 
    {
        SavedImagesStatus = String.Empty;
        SelectedProvider.MoveNext();
    } 
}
