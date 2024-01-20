namespace Cgpg.WinUI.ViewModels;

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Cgpg.WinUI.Slices;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using MyCgpg;
using Windows.Graphics.Imaging;
using Windows.Storage;

internal sealed class BlurAlgoSpatiallySepViewModel : DependencyObject
{
    public BlurAlgoSpatiallySepViewModel(DispatcherQueue uiQueue)
    {
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
    private readonly DiscardCondition discardProc1_ = new DiscardCondition();
    private readonly DiscardCondition discardProc2_ = new DiscardCondition();

    public static readonly DependencyProperty SigmaProperty =
        DependencyProperty.Register(
            nameof(Sigma),
            typeof(float),
            typeof(BlurAlgoSpatiallySepViewModel),
            new PropertyMetadata(initialSigma, (d, e) =>
            {
                if (e.NewValue == null) return;
                var vm = (BlurAlgoSpatiallySepViewModel)d;
                _ = Task.Run(() => vm.ProcessImage1());
                _ = Task.Run(() => vm.ProcessImage2());
            }));

    public static readonly DependencyProperty SelectedSourceImageIndexProperty =
        DependencyProperty.Register(
            nameof(SelectedSourceImageIndex),
            typeof(int),
            typeof(BlurAlgoSpatiallySepViewModel),
            new PropertyMetadata(0, async (d, e) =>
            {
                if (e.NewValue == null) return;
                var vm = (BlurAlgoSpatiallySepViewModel)d;
                var uri = vm.GetSelectableImageUri((int)e.NewValue);
                await vm.ReloadSourceImage(uri);
            }));

    public static readonly DependencyProperty IsProcessing2Property =
        DependencyProperty.Register(
            nameof(IsProcessing2),
            typeof(bool),
            typeof(BlurAlgoSpatiallySepViewModel),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsProcessing1Property =
        DependencyProperty.Register(
            nameof(IsProcessing1),
            typeof(bool),
            typeof(BlurAlgoSpatiallySepViewModel),
            new PropertyMetadata(false));

    public static readonly DependencyProperty DisplayImage2Property =
        DependencyProperty.Register(
            nameof(DisplayImage2),
            typeof(WriteableBitmap),
            typeof(BlurAlgoSpatiallySepViewModel),
            new PropertyMetadata(null));

    public static readonly DependencyProperty DisplayImage1Property =
        DependencyProperty.Register(
            nameof(DisplayImage1),
            typeof(WriteableBitmap),
            typeof(BlurAlgoSpatiallySepViewModel),
            new PropertyMetadata(null));

    public static readonly DependencyProperty SampleRadiusProperty =
        DependencyProperty.Register(
            nameof(SampleRadius),
            typeof(int),
            typeof(BlurAlgoSpatiallySepViewModel),
            new PropertyMetadata(0, (d, e) =>
            {
                if (e.NewValue == null) return;
                var vm = (BlurAlgoSpatiallySepViewModel)d;
                _ = Task.Run(() => vm.ProcessImage1());
                _ = Task.Run(() => vm.ProcessImage2());
            }));

    public static readonly DependencyProperty KernelText1Property =
        DependencyProperty.Register(
            nameof(KernelText1),
            typeof(string),
            typeof(BlurAlgoSpatiallySepViewModel),
            new PropertyMetadata(string.Empty));


    public static readonly DependencyProperty KernelText2Property =
        DependencyProperty.Register(
            nameof(KernelText2),
            typeof(string),
            typeof(BlurAlgoSpatiallySepViewModel),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty TimeElapsed2Property =
        DependencyProperty.Register(
            nameof(TimeElapsed2),
            typeof(string),
            typeof(BlurAlgoSpatiallySepViewModel),
            new PropertyMetadata("???s, ???fps"));

    public static readonly DependencyProperty TimeElapsed1Property =
        DependencyProperty.Register(
            nameof(TimeElapsed1),
            typeof(string),
            typeof(BlurAlgoSpatiallySepViewModel),
            new PropertyMetadata("???s, ???fps"));

    public SelectableImages SelectableImages { get; } = new SelectableImages();

    public string TimeElapsed1
    {
        get { return (string)GetValue(TimeElapsed1Property); }
        set { SetValue(TimeElapsed1Property, value); }
    }

    public string TimeElapsed2
    {
        get { return (string)GetValue(TimeElapsed2Property); }
        set { SetValue(TimeElapsed2Property, value); }
    }

    public string KernelText1
    {
        get { return (string)GetValue(KernelText1Property); }
        set { SetValue(KernelText1Property, value); }
    }
    
    public string KernelText2
    {
        get { return (string)GetValue(KernelText2Property); }
        set { SetValue(KernelText2Property, value); }
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

    public WriteableBitmap DisplayImage1
    {
        get { return (WriteableBitmap)GetValue(DisplayImage1Property); }
        set { SetValue(DisplayImage1Property, value); }
    }

    public WriteableBitmap DisplayImage2
    {
        get { return (WriteableBitmap)GetValue(DisplayImage2Property); }
        set { SetValue(DisplayImage2Property, value); }
    }

    public bool IsProcessing1
    {
        get { return (bool)GetValue(IsProcessing1Property); }
        set { SetValue(IsProcessing1Property, value); }
    }

    public bool IsProcessing2
    {
        get { return (bool)GetValue(IsProcessing2Property); }
        set { SetValue(IsProcessing2Property, value); }
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

        var width = (int)decoder.PixelWidth;
        var height = (int)decoder.PixelHeight;
        var data = (await decoder.GetPixelDataAsync()).DetachPixelData();

        lock (srcImage_)
        {
            srcImage_.Width = width;
            srcImage_.Height = height;
            srcImage_.Data = data;
        }

        _ = Task.Run(() => ProcessImage1());
        _ = Task.Run(() => ProcessImage2());
    }

    private void ProcessImage1()
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

        var sw = new Stopwatch();
        sw.Start();
        uiQueue_.TryEnqueue(() => IsProcessing1 = true);
        var kernelWidth = sampleRadius * 2 + 1;
        ImageLowpassFilter.Generate2DGaussianKernel(
            kernelWidth,
            sigma,
            out var kernel);

        var tag = discardProc1_.Tag();
        if (!ImageLowpassFilter.ConvolutionBlur(
            width,
            height,
            data,
            kernelWidth,
            kernel,
            sink,
            () => discardProc1_.CheckTag(tag)))
        {
            sw.Stop();
            return;
        }
        sw.Stop();

        var fenceSetImage = new SyncFence();
        uiQueue_.TryEnqueue(() => IsProcessing1 = false);
        if (uiQueue_.TryEnqueue(() =>
        {
            KernelText1 = FormatKernel1(kernel, kernelWidth);
            DisplayImage1 = bitmap;
            TimeElapsed1 = $"{sw.ElapsedMilliseconds/1000.0f}s, {1000.0f/sw.ElapsedMilliseconds}fps";
            fenceSetImage.Signal();
        }))
        {
            fenceSetImage.Wait();
        }
    }

    private void ProcessImage2()
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

        var sw = new Stopwatch();
        sw.Start();
        uiQueue_.TryEnqueue(() => IsProcessing2 = true);
        var kernelWidth = sampleRadius * 2 + 1;
        ImageLowpassFilter.Generate1DGaussianKernel(
            kernelWidth,
            sigma,
            out var kernel);

        var tag = discardProc2_.Tag();
        if (!ImageLowpassFilter.SeparableConvolutionBlur(
            width,
            height,
            data,
            kernelWidth,
            kernel,
            sink,
            () => discardProc2_.CheckTag(tag)))
        {
            sw.Stop();
            return;
        }
        sw.Stop();

        var fenceSetImage = new SyncFence();
        uiQueue_.TryEnqueue(() => IsProcessing2 = false);
        if (uiQueue_.TryEnqueue(() =>
        {
            KernelText2 = FormatKernel2(kernel, kernelWidth);
            DisplayImage2 = bitmap;
            TimeElapsed2 = $"{sw.ElapsedMilliseconds/1000.0f}s, {1000.0f/sw.ElapsedMilliseconds}fps";
            fenceSetImage.Signal();
        }))
        {
            fenceSetImage.Wait();
        }
    }

    private static string FormatKernel1(float[] kernel, int width)
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

    private static string FormatKernel2(float[] kernel, int width)
    {
        var builder = new StringBuilder();
        for(int i = 0; i < width; ++i)
        {
            builder.AppendLine($"{kernel[i]}, ");
        }
        return builder.ToString();
    }
}
