namespace Mod.DynamicEncounters.Threads;

public interface IThreadManager
{
    void ReportHeartbeat(ThreadId threadId);
}