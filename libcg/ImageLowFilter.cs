namespace MyCgpg;

using System.Diagnostics;

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

    /*
     Use a simple gaussian function to generate kernel
                  x^2 + y^2
              - --------------
     f(x) = e     4 sigma^2
     */
    public static void Generate2DGaussianKernel(
        int kernelWidth,
        double sigma,
        out float[] kernel)
    {
        Trace.Assert(kernelWidth %2 != 0);
        kernel = new float[kernelWidth * kernelWidth];

        if (kernelWidth == 1)
        {
            kernel[0] = 1.0f;
            return;
        }

        var r = (kernelWidth - 1) / 2;
        float kernelSum = 0.0f;
        for(int v = -r; v <= r; ++v)
        {
            for(int u = -r; u <= r; ++u)
            {
                var idx = (v + r) * kernelWidth + (u + r);
                var weight = (float)Math.Exp(-(u*u + v*v)/(4 * sigma * sigma));
                kernelSum += weight;
                kernel[idx] = weight;
            }
        }

        // normalize the kernel
        for(int i = 0; i < kernelWidth * kernelWidth; ++i)
        {
            kernel[i] /= kernelSum;
        }
    }

    public static bool ConvolutionFilter(
        int imgWidth,
        int imgHeight,
        byte[] imgData,
        int kernelWidth,
        float[] kernel,
        Stream dstDataSink,
        Func<bool> shouldContinue)
    {
        Trace.Assert(kernelWidth % 2 != 0);

        float b;
        float g;
        float r;
        var color = new byte[4] { 0x00, 0x00, 0x00, 0xff };
        var pivot = (kernelWidth - 1) / 2;
        for (int y = 0; y < imgHeight; ++y)
        {
            for (int x = 0; x < imgWidth; ++x)
            {
                b = 0.0f; g = 0.0f; r = 0.0f;
                for (int u = 0; u < kernelWidth; ++u)
                {
                    for (int v = 0; v < kernelWidth; ++v)
                    {
                        var imgIndex =
                            Math.Clamp(x - pivot + u, 0, imgWidth - 1) * 4 +
                            Math.Clamp(y - pivot + v, 0, imgHeight - 1) * imgWidth * 4;
                        var weight = kernel[v * kernelWidth + u];
                        b += imgData[imgIndex] * weight;
                        g += imgData[imgIndex + 1] * weight;
                        r += imgData[imgIndex + 2] * weight;
                    }
                }

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
