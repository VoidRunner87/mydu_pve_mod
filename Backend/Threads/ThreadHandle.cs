using System.Threading;
using System.Threading.Tasks;

namespace Mod.DynamicEncounters.Threads;

public abstract class ThreadHandle(
    ThreadId threadId,
    IThreadManager threadManager,
    CancellationToken cancellationToken
) : IThreadHandle
{
    public ThreadId ThreadId { get; } = threadId;
    public IThreadManager ThreadManager { get; } = threadManager;
    public CancellationToken CancellationToken { get; } = cancellationToken;

    public void ReportHeartbeat()
    {
        ThreadManager.ReportHeartbeat(ThreadId);
    }

    public virtual Task Tick()
    {
        return Task.CompletedTask;
    }
}