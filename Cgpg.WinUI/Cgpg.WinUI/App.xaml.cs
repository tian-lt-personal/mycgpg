namespace Cgpg.WinUI;

using Microsoft.UI.Xaml;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        this.RequestedTheme = ApplicationTheme.Light;
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        m_window = new MainWindow();
        m_window.Activate();
    }

    private Window m_window;
}
