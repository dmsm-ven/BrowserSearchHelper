using CommunityToolkit.Mvvm.ComponentModel;
using ContentManagerHelper.ImageSearcher;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrowserSearchHelper.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private ImagesSearchViewModel imagesSearchViewModel;

    public MainWindowViewModel(IEnumerable<ImageSearcherBase> providers)
    {
        ImagesSearchViewModel = new ImagesSearchViewModel(providers);
    }
}
