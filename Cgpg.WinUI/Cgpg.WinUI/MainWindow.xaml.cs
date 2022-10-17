namespace Cgpg.WinUI;

using System;
using System.Collections.Generic;
using Cgpg.WinUI.Pages;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using WinRT.Interop;

public sealed partial class MainWindow : Window
{
    private readonly AppWindow appWindow_;
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
        appWindow_ = GetCurrentAppWindow(this);
        CustomizeTitleBar();
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

    private void CustomizeTitleBar()
    {
        appWindow_.Title = "CG playground";
        appWindow_.TitleBar.ExtendsContentIntoTitleBar = true;
        appWindow_.TitleBar.ButtonForegroundColor = Colors.Black;
        appWindow_.TitleBar.ButtonHoverForegroundColor = Colors.Black;
        appWindow_.TitleBar.ButtonPressedForegroundColor = Colors.Black;
        appWindow_.TitleBar.ButtonBackgroundColor = Colors.Transparent;
        appWindow_.TitleBar.ButtonHoverBackgroundColor = Colors.LightGray;
        appWindow_.TitleBar.ButtonPressedBackgroundColor = Colors.DimGray;
        AppIcon.Source = new BitmapImage(new Uri("ms-appx:///Assets/appicon-16x16.png"));
    }

    private static AppWindow GetCurrentAppWindow(object target)
    {
        var hwnd = WindowNative.GetWindowHandle(target);
        var wndId = Win32Interop.GetWindowIdFromWindow(hwnd);
        return AppWindow.GetFromWindowId(wndId);
    }
}
