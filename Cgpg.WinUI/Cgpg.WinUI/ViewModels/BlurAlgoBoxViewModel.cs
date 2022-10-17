namespace Cgpg.WinUI.ViewModels;

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using MyCgpg;
using Windows.Graphics.Imaging;
using Windows.Storage;

internal sealed class BlurAlgoBoxViewModel : DependencyObject
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
    private readonly DiscardCondition discardProc_ = new DiscardCondition();
    private int sampleRadius_;
    private int srcImgWidth_;
    private int srcImgHeight_;
    private byte[] srcImgData_;

    public static readonly DependencyProperty SampleRadiusProperty =
        DependencyProperty.Register(
            nameof(SampleRadius),
            typeof(int),
            typeof(BlurAlgoBoxViewModel),
            new PropertyMetadata(0, (d, e) =>
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

        var tag = discardProc_.Tag();
        if (!ImageLowFilter.BoxBlur(
            sampleRadius,
            width,
            height,
            data,
            sink,
            () => discardProc_.CheckTag(tag)))
        {
            return;
        }

        var fenceSetImage = new SyncFence();
        uiQueue_.TryEnqueue(() => IsProcessing = false);
        if (uiQueue_.TryEnqueue(() =>
        {
            DisplayImage = bitmap;
            fenceSetImage.Signal();
        }))
        {
            fenceSetImage.Wait();
        }
    }
}
