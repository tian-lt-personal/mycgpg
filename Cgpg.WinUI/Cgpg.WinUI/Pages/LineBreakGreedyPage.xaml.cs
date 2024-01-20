using System;
using System.Collections.Generic;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using MyCgpg;
using MyCgpg.Model;

namespace Cgpg.WinUI.Pages;

public sealed partial class LineBreakGreedyPage : Page
{
    private readonly Dictionary<string, Glyphs> _cachedGlyphs = new Dictionary<string, Glyphs>();
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

    private Extent<double> GetWordExtent(string word)
    {
        var glyphs = GetGlyphs(word);
        return new Extent<double> { W = glyphs.ActualWidth, H = glyphs.ActualHeight };
    }

    private Glyphs GetGlyphs(string word)
    {
        if (_cachedGlyphs.TryGetValue(word, out var glyphs))
        {
            return glyphs;
        }
        else
        {
            var newGlyphs = new Glyphs
            {
                FontUri = new Uri("ms-appx:///Assets/Arial.ttf"),
                FontRenderingEmSize = 16.0,
                StyleSimulations = StyleSimulations.None,
                UnicodeString = word,
                Fill = new SolidColorBrush(Colors.Black),
                Opacity = 1,
            };
            _cachedGlyphs.Add(word, newGlyphs);
            return newGlyphs;
        }
    }
}
