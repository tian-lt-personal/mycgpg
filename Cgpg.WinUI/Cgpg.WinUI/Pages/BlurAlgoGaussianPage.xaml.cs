namespace Cgpg.WinUI.Pages;

using Cgpg.WinUI.ViewModels;
using Microsoft.UI.Xaml.Controls;

internal sealed partial class BlurAlgoGaussianPage : Page
{
    public BlurAlgoGaussianPage()
    {
        ViewModel = new BlurAlgoGaussianViewModel(DispatcherQueue);
        InitializeComponent();
    }

    public BlurAlgoGaussianViewModel ViewModel { get; }
}
