namespace Cgpg.WinUI.ViewModels;

using Cgpg.WinUI.ComDef;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;
using WinRT;

internal class BlurAlgoBoxViewModel : DependencyObject
{
    public BlurAlgoBoxViewModel(DispatcherQueue uiQueue)
    {
        SelectableImages = GlobalConfigs
            .SelectableImageMap
            .Select(x => x.Key)
            .ToArray();
        uiQueue.TryEnqueue(async () => await ReloadSourceImage(GetSelectableImageUri(0)));
    }

    private int srcImgWidth_;
    private int srcImgHeight_;
    private byte[] srcImgData_;

    public static readonly DependencyProperty SampleRadiusProperty =
        DependencyProperty.Register(
            nameof(SampleRadius),
            typeof(int),
            typeof(BlurAlgoBoxViewModel),
            new PropertyMetadata(1, (d, e) =>
            {
                if (e.NewValue == null) return;

                var vm = (BlurAlgoBoxViewModel)d;
                vm.DisplayImage = ProcessImage(vm.SampleRadius, vm.srcImgWidth_, vm.srcImgHeight_, vm.srcImgData_);
            }));

    public static readonly DependencyProperty DisplayImageProperty =
        DependencyProperty.Register(
            nameof(DisplayImage),
            typeof(WriteableBitmap),
            typeof(BlurAlgoBoxViewModel),
            new PropertyMetadata(null));

    public static readonly DependencyProperty SelectedSourceImageIndexProperty =
        DependencyProperty.Register(
            nameof(SelectedSourceImageIndex),
            typeof(int),
            typeof(BlurAlgoBoxViewModel),
            new PropertyMetadata(0, async (d, e) =>
            {
                if (e.NewValue == null) return;

                var vm = (BlurAlgoBoxViewModel)d;
                var uri = vm.GetSelectableImageUri((int)e.NewValue);
                await vm.ReloadSourceImage(uri);
            }));

    public string[] SelectableImages { get; }

    public int SampleRadius
    {
        get { return (int)GetValue(SampleRadiusProperty); }
        set { SetValue(SampleRadiusProperty, value); }
    }

    public WriteableBitmap DisplayImage
    {
        get { return (WriteableBitmap)GetValue(DisplayImageProperty); }
        set { SetValue(DisplayImageProperty, value); }
    }

    public int SelectedSourceImageIndex
    {
        get { return (int)GetValue(SelectedSourceImageIndexProperty); }
        set { SetValue(SelectedSourceImageIndexProperty, value); }
    }

    private string GetSelectableImageUri(int index)
        => GlobalConfigs.SelectableImageMap[SelectableImages[index]];

    private async Task ReloadSourceImage(string fileUri)
    {
        var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(fileUri));
        using var stream = await file.OpenReadAsync();
        var decoder = await BitmapDecoder.CreateAsync(stream);
        srcImgWidth_ = (int)decoder.PixelWidth;
        srcImgHeight_ = (int)decoder.PixelHeight;
        srcImgData_ = (await decoder.GetPixelDataAsync()).DetachPixelData();

        DisplayImage = ProcessImage(SampleRadius, srcImgWidth_, srcImgHeight_, srcImgData_);
    }

    private static unsafe WriteableBitmap ProcessImage(int radius, int width, int height, byte[] data)
    {
        var bitmap = new WriteableBitmap(width, height);
        var kernelWidth = radius * 2 + 1;
        var kernelWidthSquare = kernelWidth * kernelWidth;
        using var sink = bitmap.PixelBuffer.AsStream();

        int b = 0;
        int g = 0;
        int r = 0;
        var color = new byte[4];
        color[3] = 0xff;
        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                b = 0; g = 0; r = 0;

                for (int u = 0; u < kernelWidth; ++u)
                {
                    for (int v = 0; v < kernelWidth; ++v)
                    {
                        var index =
                            Math.Clamp(x - radius + u, 0, width - 1) * 4 +
                            Math.Clamp(y - radius + v, 0, height - 1) * width * 4;
                        b += data[index];
                        g += data[index + 1];
                        r += data[index + 2];
                    }
                }

                b /= kernelWidthSquare;
                g /= kernelWidthSquare;
                r /= kernelWidthSquare;

                color[0] = (byte)b;
                color[1] = (byte)g;
                color[2] = (byte)r;
                sink.Write(color);
            }
        }

        return bitmap;
    }
}
