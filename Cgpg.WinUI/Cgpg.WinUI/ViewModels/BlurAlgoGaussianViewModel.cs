namespace Cgpg.WinUI.ViewModels;

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using MyCgpg;
using Windows.Graphics.Imaging;
using Windows.Graphics.Printing;
using Windows.Storage;

internal sealed class BlurAlgoGaussianViewModel : DependencyObject
{
    public BlurAlgoGaussianViewModel(DispatcherQueue uiQueue)
    {
        SelectableImages = GlobalConfigs
            .SelectableImageMap
            .Select(x => x.Key)
            .ToArray();
        uiQueue_ = uiQueue;
        uiQueue_.TryEnqueue(async () => await ReloadSourceImage(GetSelectableImageUri(0)));
    }

    private sealed class ImageMetadata
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int SampleRadius { get; set; } = 0;
        public float Sigma { get; set; } = initialSigma;
        public byte[] Data { get; set; }
    }

    private const float initialSigma = 8.0f;
    private readonly DispatcherQueue uiQueue_;
    private readonly ImageMetadata srcImage_ = new ImageMetadata();
    private readonly DiscardCondition discardProc_ = new DiscardCondition();

    public static readonly DependencyProperty KernelTextProperty =
        DependencyProperty.Register(
            nameof(KernelText),
            typeof(string),
            typeof(BlurAlgoGaussianViewModel),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty SigmaProperty =
        DependencyProperty.Register(
            nameof(Sigma),
            typeof(float),
            typeof(BlurAlgoGaussianViewModel),
            new PropertyMetadata(initialSigma, (d, e) =>
            {
                if (e.NewValue == null) return;
                var vm = (BlurAlgoGaussianViewModel)d;
                _ = Task.Run(() => vm.ProcessImage());
            }));

    public static readonly DependencyProperty SampleRadiusProperty =
        DependencyProperty.Register(
            nameof(SampleRadius),
            typeof(int),
            typeof(BlurAlgoGaussianViewModel),
            new PropertyMetadata(0, (d, e) =>
            {
                if (e.NewValue == null) return;
                var vm = (BlurAlgoGaussianViewModel)d;
                _ = Task.Run(() => vm.ProcessImage());
            }));

    public static readonly DependencyProperty SelectedSourceImageIndexProperty =
        DependencyProperty.Register(
            nameof(SelectedSourceImageIndex),
            typeof(int),
            typeof(BlurAlgoGaussianViewModel),
            new PropertyMetadata(0, async (d, e) =>
            {
                if (e.NewValue == null) return;
                var vm = (BlurAlgoGaussianViewModel)d;
                var uri = vm.GetSelectableImageUri((int)e.NewValue);
                await vm.ReloadSourceImage(uri);
            }));

    public static readonly DependencyProperty IsProcessingProperty =
        DependencyProperty.Register(
            nameof(IsProcessing),
            typeof(bool),
            typeof(BlurAlgoGaussianViewModel),
            new PropertyMetadata(false));

    public static readonly DependencyProperty DisplayImageProperty =
        DependencyProperty.Register(
            nameof(DisplayImage),
            typeof(WriteableBitmap),
            typeof(BlurAlgoGaussianViewModel),
            new PropertyMetadata(null));


    public string[] SelectableImages { get; }

    public int SelectedSourceImageIndex
    {
        get { return (int)GetValue(SelectedSourceImageIndexProperty); }
        set { SetValue(SelectedSourceImageIndexProperty, value); }
    }

    public string KernelText
    {
        get { return (string)GetValue(KernelTextProperty); }
        set { SetValue(KernelTextProperty, value); }
    }

    public float Sigma
    {
        get { return (float)GetValue(SigmaProperty); }
        set
        {
            lock (srcImage_)
            {
                srcImage_.Sigma = value;
            }
            SetValue(SigmaProperty, value);
        }
    }

    public int SampleRadius
    {
        get { return (int)GetValue(SampleRadiusProperty); }
        set
        {
            lock (srcImage_)
            {
                srcImage_.SampleRadius = value;
            }
            SetValue(SampleRadiusProperty, value);
        }
    }

    public bool IsProcessing
    {
        get { return (bool)GetValue(IsProcessingProperty); }
        set { SetValue(IsProcessingProperty, value); }
    }

    public WriteableBitmap DisplayImage
    {
        get { return (WriteableBitmap)GetValue(DisplayImageProperty); }
        set { SetValue(DisplayImageProperty, value); }
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

        lock (srcImage_)
        {
            srcImage_.Width = width;
            srcImage_.Height = height;
            srcImage_.Data = data;
        }
        _ = Task.Run(() => ProcessImage());
    }

    private void ProcessImage()
    {
        int width;
        int height;
        float sigma;
        int sampleRadius;
        byte[] data;
        lock (srcImage_)
        {
            width = srcImage_.Width;
            height = srcImage_.Height;
            sampleRadius = srcImage_.SampleRadius;
            sigma = srcImage_.Sigma;
            data = srcImage_.Data;
        }

        var fenceGetBitmapSink = new SyncFence();
        WriteableBitmap bitmap = null;
        Stream sink = null;
        if (!uiQueue_.TryEnqueue(() =>
        {
            bitmap = new WriteableBitmap(width, height);
            sink = bitmap.PixelBuffer.AsStream();
            fenceGetBitmapSink.Signal();
        }))
        {
            return;
        }
        fenceGetBitmapSink.Wait();

        uiQueue_.TryEnqueue(() => IsProcessing = true);
        var kernelWidth = sampleRadius * 2 + 1;
        ImageLowFilter.Generate2DGaussianKernel(
            kernelWidth,
            sigma,
            out var kernel);

        var tag = discardProc_.Tag();
        if (!ImageLowFilter.ConvolutionFilter(
            width,
            height,
            data,
            kernelWidth,
            kernel,
            sink,
            () => discardProc_.CheckTag(tag)))
        {
            return;
        }

        var fenceSetImage = new SyncFence();
        uiQueue_.TryEnqueue(() => IsProcessing = false);
        if (uiQueue_.TryEnqueue(() =>
        {
            KernelText = FormatKernel(kernel, kernelWidth);
            DisplayImage = bitmap;
            fenceSetImage.Signal();
        }))
        {
            fenceSetImage.Wait();
        }
    }

    private static string FormatKernel(float[] kernel, int width)
    {
        var builder = new StringBuilder();
        for(int i = 0; i < width * width; ++i)
        {
            if (i > 0 && i % width == 0)
            {
                builder.AppendLine();
            }
            builder.Append($"{kernel[i]}, ");
        }
        return builder.ToString();
    }
}
