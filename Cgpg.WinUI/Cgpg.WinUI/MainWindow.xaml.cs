namespace Cgpg.WinUI;

using Cgpg.WinUI.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

public sealed partial class MainWindow : Window
{
    private readonly IDictionary<string, Action<MainWindow>> navigatorActions_
        = new Dictionary<string, Action<MainWindow>>()
        {
            { "tagHome", wnd => wnd.MainFrame.Navigate(typeof(HomePage)) },
            { "tagNotFound", wnd => wnd.MainFrame.Navigate(typeof(NotFoundPage)) },

            { "tagBlurAlgoBox", wnd => wnd.MainFrame.Navigate(typeof(BlurAlgoBoxPage)) },
            { "tagBlurAlgoGaussian", wnd => wnd.MainFrame.Navigate(typeof(BlurAlgoGaussianPage)) },
            { "tagBlurAlgoSeparable", wnd => wnd.MainFrame.Navigate(typeof(BlurAlgoSpatiallySepPage)) },
            { "tagBlurAlgoCustom", wnd => wnd.MainFrame.Navigate(typeof(BlurAlgoCustomPage)) },
        };

    public MainWindow()
    {
        InitializeComponent();
        Navigator.SelectedItem = Navigator.MenuItems[0];
    }

    private void Navigator_SelectionChanged(
        NavigationView sender,
        NavigationViewSelectionChangedEventArgs args)
    {
        var item = (NavigationViewItem)args.SelectedItem;
        if (item.Tag != null &&
            navigatorActions_.TryGetValue((string)item.Tag, out var redirect))
        {
            redirect(this);
        }
        else
        {
            navigatorActions_["tagNotFound"](this);
        }
    }
}
