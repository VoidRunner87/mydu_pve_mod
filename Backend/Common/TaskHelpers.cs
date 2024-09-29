using System;
using System.Threading.Tasks;

namespace Mod.DynamicEncounters.Common;

public static class TaskHelpers
{
    public static Task<T> OnErrorWithResult<T>(this Task<T> task, Action<AggregateException> errorAction)
    {
        task.ContinueWith(t =>
        {
            if (t.Exception != null)
            {
                // Capture the first exception (or aggregate multiple exceptions if needed)
                errorAction(t.Exception);
            }
        }, TaskContinuationOptions.OnlyOnFaulted);

        return task;
    }

    public static Task OnError(this Task task, Action<AggregateException> errorAction)
    {
        task.ContinueWith(t =>
        {
            if (t.Exception != null)
            {
                // Capture the first exception (or aggregate multiple exceptions if needed)
                errorAction(t.Exception);
            }
        }, TaskContinuationOptions.OnlyOnFaulted);

        return task;
    }
}