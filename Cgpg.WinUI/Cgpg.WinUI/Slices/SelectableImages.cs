namespace Cgpg.WinUI.Slices;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

internal sealed class SelectableImages : ReadOnlyCollection<string>
{
    public SelectableImages() : base(GlobalConfigs
        .SelectableImageMap
        .Select(x => x.Key).ToList())
    {}
}
