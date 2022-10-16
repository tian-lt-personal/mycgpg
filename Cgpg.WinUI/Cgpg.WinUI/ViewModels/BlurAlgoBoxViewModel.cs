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
using System.Threading;
using System.Collections.Generic;
using System.IO;

internal class BlurAlgoBoxViewModel : DependencyObject
{
    public BlurAlgoBoxViewModel(DispatcherQueue uiQueue)
    {
        SelectableImages = GlobalConfigs
            .SelectableImageMap
            .Select(x => x.Key)
            .ToArray();
        uiQueue_ = uiQueue;
        uiQueue_.TryEnqueue(async () => await ReloadSourceImage(GetSelectableImageUri(0)));
    }

    private readonly DispatcherQueue uiQueue_;
    private readonly object mtxImgMetadata_ = new object();
    private long topImgProcId_ = 0;
    private int sampleRadius_;
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
                _ = Task.Run(() => vm.ProcessImage());
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

    public static readonly DependencyProperty IsProcessingProperty =
        DependencyProperty.Register(
            nameof(IsProcessing),
            typeof(bool),
            typeof(BlurAlgoBoxViewModel),
            new PropertyMetadata(false));

    public string[] SelectableImages { get; }

    public int SampleRadius
    {
        get { return (int)GetValue(SampleRadiusProperty); }
        set
        {
            lock (mtxImgMetadata_)
            {
                sampleRadius_ = value;
            }
            SetValue(SampleRadiusProperty, value);
        }
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

    public bool IsProcessing
    {
        get { return (bool)GetValue(IsProcessingProperty); }
        set { SetValue(IsProcessingProperty, value); }
    }

    private string GetSelectableImageUri(int index)
        => GlobalConfigs.SelectableImageMap[SelectableImages[index]];

    private async Task ReloadSourceImage(string fileUri)
    {
        var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(fileUri));
        using var stream = await file.OpenReadAsync();
        var decoder = await BitmapDecoder.CreateAsync(stream);

        var width = (int)decoder.PixelWidth;
        var height = (int)decoder.PixelHeight;
        var data = (await decoder.GetPixelDataAsync()).DetachPixelData();
        lock (mtxImgMetadata_)
        {
            srcImgWidth_ = width;
            srcImgHeight_ = height;
            srcImgData_ = data;
        }
        _ = Task.Run(() => ProcessImage());
    }

    private void ProcessImage()
    {
        var cts = new CancellationTokenSource();
        var tid = Task.CurrentId.Value;

        int sampleRadius;
        int width;
        int height;
        byte[] data;
        lock (mtxImgMetadata_)
        {
            sampleRadius = sampleRadius_;
            width = srcImgWidth_;
            height = srcImgHeight_;
            data = srcImgData_;
        }

        var mtxGetBitmapSink = new object();
        WriteableBitmap bitmap = null;
        Stream sink = null;
        if (!uiQueue_.TryEnqueue(() =>
        {
            lock (mtxGetBitmapSink)
            {
                lock (mtxImgMetadata_)
                {
                    bitmap = new WriteableBitmap(srcImgWidth_, srcImgHeight_);
                    sink = bitmap.PixelBuffer.AsStream();
                }
                Monitor.Pulse(mtxGetBitmapSink);
            }
        }))
        {
            return;
        }

        lock (mtxGetBitmapSink)
        {
            Monitor.Wait(mtxGetBitmapSink);
        }

        uiQueue_.TryEnqueue(() => IsProcessing = true);

        var currProcId = Interlocked.Increment(ref topImgProcId_);

        var succ = ProcessImage(
            sampleRadius,
            width,
            height,
            data,
            sink,
            () => Interlocked.Read(ref topImgProcId_) == currProcId);

        if (succ)
        {
            var mtxSetDisplayImage = new object();
            uiQueue_.TryEnqueue(() => IsProcessing = false);
            if (uiQueue_.TryEnqueue(() =>
            {
                DisplayImage = bitmap;
                lock (mtxSetDisplayImage)
                {
                    Monitor.Pulse(mtxSetDisplayImage);
                }
            }))
            {
                lock (mtxSetDisplayImage)
                {
                    Monitor.Wait(mtxSetDisplayImage);
                }
            }
        }
    }

    private static unsafe bool ProcessImage(
        int radius,
        int width,
        int height,
        byte[] data,
        Stream sink,
        Func<bool> shouldCont)
    {
        var kernelWidth = radius * 2 + 1;
        var kernelWidthSquare = kernelWidth * kernelWidth;

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

            if (y % 20 == 0 &&
                !shouldCont())
            {
                return false;
            }
        }

        return true;
    }
}
