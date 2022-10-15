namespace Cgpg.WinUI.Pages;

using Cgpg.WinUI.ViewModels;
using Microsoft.UI.Xaml.Controls;

internal sealed partial class BlurAlgoBoxPage : Page
{
    public BlurAlgoBoxPage()
    {
        ViewModel = new BlurAlgoBoxViewModel(DispatcherQueue);
        InitializeComponent();
    }

    public BlurAlgoBoxViewModel ViewModel { get; }
}
