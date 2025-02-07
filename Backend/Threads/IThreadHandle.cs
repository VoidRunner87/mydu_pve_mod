using System.Threading;
using System.Threading.Tasks;

namespace Mod.DynamicEncounters.Threads;

public interface IThreadHandle
{
    ThreadId ThreadId { get; }
    IThreadManager ThreadManager { get; }
    CancellationToken CancellationToken { get; }

    void ReportHeartbeat();

    Task Tick();
}