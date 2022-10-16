using Microsoft.UI.Xaml;

namespace Cgpg.WinUI;

internal static class XBindUtils
{
    public static Visibility VisibleIfIntsAreNotEqual(int lhs, int rhs)
    {
        return lhs != rhs ? Visibility.Visible : Visibility.Collapsed;
    }
}
