using System.Collections.Generic;

namespace Cgpg.WinUI;

internal static class GlobalConfigs
{
    public static Dictionary<string, string> SelectableImageMap = new Dictionary<string, string>()
    {
        { "Albion Basin (1920x1080)", "ms-appx:///Assets/pexels-matthew-montrone-1920x1080.jpg" },
        { "Lenna (512x512)", "ms-appx:///Assets/lenna-512x512.png" },
    };
}
