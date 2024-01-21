using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyCgpg;
using MyCgpg.Model;
using System.Collections.Generic;

namespace Cgpg.WinUI.Pages;

public sealed partial class LineBreakGreedyPage : Page
{
    private readonly List<(Position<float>, CanvasTextLayout)> _textBlocks = new List<(Position<float>, CanvasTextLayout)>();

    public static readonly DependencyProperty PageHeightProperty =
        DependencyProperty.Register(
            nameof(PageHeight),
            typeof(int),
            typeof(LineBreakGreedyPage),
            new PropertyMetadata(600));
    public static readonly DependencyProperty LineWidthProperty =
        DependencyProperty.Register(
            nameof(LineWidth),
            typeof(int),
            typeof(LineBreakGreedyPage),
            new PropertyMetadata(800));
    public static readonly DependencyProperty LineHeightProperty =
        DependencyProperty.Register(
            nameof(LineHeight),
            typeof(int),
            typeof(LineBreakGreedyPage),
            new PropertyMetadata(22));
    public static readonly DependencyProperty WsWidthProperty =
        DependencyProperty.Register(
            nameof(WsWidth),
            typeof(int),
            typeof(LineBreakGreedyPage),
            new PropertyMetadata(8));

    public int PageHeight
    {
        get => (int)GetValue(PageHeightProperty);
        set => SetValue(PageHeightProperty, value);
    }
    public int LineWidth
    {
        get => (int)GetValue(LineWidthProperty);
        set
        {
            SetValue(LineWidthProperty, value);
            UpdateTypeset();
        }
    }
    public int LineHeight
    {
        get => (int)GetValue(LineHeightProperty);
        set
        {
            SetValue(LineHeightProperty, value);
            UpdateTypeset();
        }
    }
    public int WsWidth
    {
        get => (int)GetValue(WsWidthProperty);
        set
        {
            SetValue(WsWidthProperty, value);
            UpdateTypeset();
        }
    }

    public LineBreakGreedyPage()
    {
        InitializeComponent();
    }

    public void OnInputTextChanged(object sender, TextChangedEventArgs args)
    {
        UpdateTypeset();
    }

    private void UpdateTypeset()
    {
        _textBlocks.Clear();
        try
        {
            foreach (var (pos, word) in GreedyLb.MoveCursor(
                NaiveWb.MoveWord(InputTextBox.Text),
                LineWidth,
                LineHeight,
                GetWordExtent))
            {
                _textBlocks.Add((pos, GetTextLayout(word)));
            }
        }
        catch (InsufficientLineWidthError)
        {
            _textBlocks.Clear();
        }
        TypesetCanvas.Invalidate();
    }

    private Extent<float> GetWordExtent(string word)
    {
        var text = GetTextLayout(word);
        if (text != null)
        {
            return new Extent<float> { W = (float)text.LayoutBounds.Width, H = (float)text.LayoutBounds.Height };
        }
        return new Extent<float> { W = WsWidth, H = LineHeight };
    }

    private CanvasTextLayout GetTextLayout(string word)
    {
        if (word != " ")
        {
            var fmt = new CanvasTextFormat { FontFamily = "Arial.ttf", FontSize = 16, FontWeight = FontWeights.Normal };
            var layout = new CanvasTextLayout(TypesetCanvas.Device, word, fmt, float.PositiveInfinity, float.PositiveInfinity);
            return layout;
        }
        return null;
    }

    private void TypesetCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        var ds = args.DrawingSession;
        ds.Clear(Colors.White);
        foreach (var (pos, text) in _textBlocks)
        {
            if (text == null) continue;
            ds.DrawTextLayout(text, new System.Numerics.Vector2 { X = pos.X, Y = pos.Y }, Colors.Black);
        }
    }
}
