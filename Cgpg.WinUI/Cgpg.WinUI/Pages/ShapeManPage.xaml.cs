using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.UI;

namespace Cgpg.WinUI.Pages;

internal sealed class RectangleCreator
{
    public Matrix Coord { get; set; }

    public Rectangle Object { get; set; } = new Rectangle
    {
        Width = 1.0,
        Height = 1.0,
        Fill = new SolidColorBrush { Color = Color.FromArgb(0xff, 0, 0xff, 0) }
    };

    public RectangleCreator(double initX, double initY)
    {
        Coord = new Matrix { M11 = 1.0, M12 = 0.0, M21 = 0.0, M22 = 1.0, OffsetX = initX, OffsetY = initY };
        Object.RenderTransform = new MatrixTransform { Matrix = Coord };
    }

    public void Move(double x, double y)
    {
        Coord = new Matrix {  };
    }
}

public sealed partial class ShapeManPage : Page
{
    public string ManStartPos
    {
        get { return (string)GetValue(ManStartPosProperty); }
        set { SetValue(ManStartPosProperty, value); }
    }

    public string ManDeltaPos
    {
        get { return (string)GetValue(ManDeltaPosProperty); }
        set { SetValue(ManDeltaPosProperty, value); }
    }

    public string ManEndPos
    {
        get { return (string)GetValue(ManEndPosProperty); }
        set { SetValue(ManEndPosProperty, value); }
    }

    public static readonly DependencyProperty ManStartPosProperty =
        DependencyProperty.Register(nameof(ManStartPos), typeof(string), typeof(ShapeManPage), new PropertyMetadata(default));

    public static readonly DependencyProperty ManDeltaPosProperty =
        DependencyProperty.Register(nameof(ManDeltaPos), typeof(string), typeof(ShapeManPage), new PropertyMetadata(default));

    public static readonly DependencyProperty ManEndPosProperty =
        DependencyProperty.Register(nameof(ManEndPos), typeof(string), typeof(ShapeManPage), new PropertyMetadata(default));

    private RectangleCreator _creator;

    public ShapeManPage()
    {
        InitializeComponent();
    }

    private void CanvasManStarted(object sender, ManipulationStartedRoutedEventArgs args)
    {
        ManStartPos = $"({args.Position.X}, {args.Position.Y})";
        _creator = new RectangleCreator(args.Position.X, args.Position.Y);
        RootCanvas.Children.Add(_creator.Object);
    }

    private void CanvasManDelta(object sender, ManipulationDeltaRoutedEventArgs args)
    {
        ManDeltaPos = $"({args.Position.X}, {args.Position.Y})";
        _creator.MoveDelta(args.Position.X, args.Position.Y);
    }

    private void CanvasManCompleted(object sender, ManipulationCompletedRoutedEventArgs args)
    {
        ManEndPos = $"({args.Position.X}, {args.Position.Y})";
    }
}
