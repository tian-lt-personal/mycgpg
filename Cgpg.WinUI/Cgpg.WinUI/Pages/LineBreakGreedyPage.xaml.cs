using Microsoft.UI.Xaml.Controls;
using MyCgpg;
using MyCgpg.Model;

namespace Cgpg.WinUI.Pages;

public sealed partial class LineBreakGreedyPage : Page
{

    public int LineWidth = 300;
    public int LineHeight = 20;

    public LineBreakGreedyPage()
    {
        InitializeComponent();
    }

    public void OnInputTextChanged(object sender, TextChangedEventArgs args)
    {
        foreach (var (pos, word) in GreedyLb.MoveCursor(
            NaiveWb.MoveWord(InputTextBox.Text),
            LineWidth,
            LineHeight,
            GetWordExtent))
        {
        }
    }


    private Extent<int> GetWordExtent(string word)
    {
        return new Extent<int> { W = 8, H = 16 };
    }
}
