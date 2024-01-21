using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;
using MyCgpg;
using MyCgpg.Model;
using System.Collections.Generic;

namespace Cgpg.WinUI.Pages;

public sealed partial class LineBreakGreedyPage : Page
{
    private readonly List<(Position<double>, CanvasTextLayout)> _textBlocks = new List<(Position<double>, CanvasTextLayout)>();
    public int LineWidth = 300;
    public int LineHeight = 20;

    public LineBreakGreedyPage()
    {
        InitializeComponent();
    }

    public void OnInputTextChanged(object sender, TextChangedEventArgs args)
    {
        _textBlocks.Clear();
        foreach (var (pos, word) in GreedyLb.MoveCursor(
            NaiveWb.MoveWord(InputTextBox.Text),
            LineWidth,
            LineHeight,
            GetWordExtent))
        {
            _textBlocks.Add((pos, GetTextLayout(word)));
        }
        TypesetCanvas.Invalidate();
    }

    private Extent<double> GetWordExtent(string word)
    {
        var text = GetTextLayout(word);
        return new Extent<double> { W = text.LayoutBounds.Width, H = text.LayoutBounds.Height };
    }

    private CanvasTextLayout GetTextLayout(string word)
    {
        var fmt = new CanvasTextFormat { FontFamily = "Arial.ttf", FontSize = 16, FontWeight = FontWeights.Normal };
        var layout = new CanvasTextLayout(TypesetCanvas.Device, word, fmt, 100, 100);
        return layout;
    }

    private void TypesetCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        var ds = args.DrawingSession;
        ds.Clear(Colors.White);
        foreach (var (pos, text) in _textBlocks)
        {
            ds.DrawTextLayout(text, new System.Numerics.Vector2 { X = (float)pos.X, Y = (float)pos.Y }, Colors.Black);
        }
    }
}
