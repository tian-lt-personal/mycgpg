namespace MyCgpg;

public static class ImageLowFilter
{
    public static bool BoxBlur(
        int sampleRadius,
        int imgWidth,
        int imgHeight,
        byte[] imgData,
        Stream dstDataSink,
        Func<bool> shouldContinue)
    {
        var kernelWidth = sampleRadius * 2 + 1;
        var kernelWidthSquare = kernelWidth * kernelWidth;

        int b;
        int g;
        int r;
        var color = new byte[4];
        color[3] = 0xff;
        for (int y = 0; y < imgHeight; ++y)
        {
            for (int x = 0; x < imgWidth; ++x)
            {
                b = 0; g = 0; r = 0;
                for (int u = 0; u < kernelWidth; ++u)
                {
                    for (int v = 0; v < kernelWidth; ++v)
                    {
                        var index =
                            Math.Clamp(x - sampleRadius + u, 0, imgWidth - 1) * 4 +
                            Math.Clamp(y - sampleRadius + v, 0, imgHeight - 1) * imgWidth * 4;
                        b += imgData[index];
                        g += imgData[index + 1];
                        r += imgData[index + 2];
                    }
                }
                b /= kernelWidthSquare;
                g /= kernelWidthSquare;
                r /= kernelWidthSquare;

                color[0] = (byte)b;
                color[1] = (byte)g;
                color[2] = (byte)r;
                dstDataSink.Write(color);
            }
            if (y % 20 == 0 && !shouldContinue())
                return false;
        }
        return true;
    }
}
