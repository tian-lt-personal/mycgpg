namespace Cgpg.WinUI.Pages;

using Microsoft.UI.Xaml.Controls;
using System;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        InitializeComponent();
    }

#if DEBUG
    public Uri HomeLink { get; } = new Uri("about:blank");
#else
    public Uri HomeLink { get; } = new Uri("https://tian-lt-personal.github.io");
#endif
}
