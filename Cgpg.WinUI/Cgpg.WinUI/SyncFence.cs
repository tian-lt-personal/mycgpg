using System.Threading;

namespace Cgpg.WinUI;

internal sealed class SyncFence
{
    public void Signal()
    {
        lock (this)
        {
            Monitor.Pulse(this);
        }
    }

    public void Wait()
    {
        lock (this)
        {
            Monitor.Wait(this);
        }
    }
}
