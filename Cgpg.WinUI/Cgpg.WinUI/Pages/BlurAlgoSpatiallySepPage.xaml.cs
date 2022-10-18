namespace Cgpg.WinUI.Pages;

using Cgpg.WinUI.ViewModels;
using Microsoft.UI.Xaml.Controls;

internal sealed partial class BlurAlgoSpatiallySepPage : Page
{
    public BlurAlgoSpatiallySepPage()
    {
        ViewModel = new BlurAlgoSpatiallySepViewModel(DispatcherQueue);
        InitializeComponent();
    }

    public BlurAlgoSpatiallySepViewModel ViewModel { get; }
}
