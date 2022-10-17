using System.Threading;

namespace Cgpg.WinUI;

internal sealed class DiscardCondition
{
    private long topId_ = 0;

    public long Tag() => Interlocked.Increment(ref topId_);

    public bool CheckTag(long tag) => tag == Interlocked.Read(ref topId_);
}
