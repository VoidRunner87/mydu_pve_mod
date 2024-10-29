namespace Mod.DynamicEncounters.Threads;

public interface IThreadManager
{
    void ReportHeartbeat(ThreadId threadId);

    void CancelAllThreads();
    void Pause();
    void Resume();
}