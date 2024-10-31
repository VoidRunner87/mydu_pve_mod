using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Common.Data;

namespace Mod.DynamicEncounters.Common;

public static class TaskExtensions
{
    public static async Task<T> WithRetry<T>(this Task<T> task, RetryOptions<T> options)
    {
        int attempt = 0;

        while (true)
        {
            try
            {
                // Await the task
                var result = await task;

                // Check if the result meets retry conditions
                if (!options.ShouldRetryOnResult(result))
                    return result;
            }
            catch (Exception ex) when (options.ShouldRetryOnException(ex) && attempt < options.RetryCount)
            {
                attempt++;

                // Log the exception if a logger is provided
                options.Logger?.LogWarning(ex, "Attempt {Attempt} failed. Retrying...", attempt);

                // Execute the action to potentially fix the error, with error handling
                try
                {
                    await options.OnRetryAttempt(ex);
                }
                catch (Exception onRetryEx)
                {
                    options.Logger?.LogError(onRetryEx, "OnRetryAttempt action failed.");
                }

                continue; // Retry the loop
            }

            // If retries are exhausted, rethrow exception if any, or return last result
            return await task;
        }
    }

    public static async Task WithRetry(this Task task, RetryOptions options)
    {
        var attempt = 0;

        while (true)
        {
            try
            {
                // Await the task
                await task;
                return; // Task completed successfully, no need to retry
            }
            catch (Exception ex) when (options.ShouldRetryOnException(ex) && attempt < options.RetryCount)
            {
                attempt++;

                // Log the exception if a logger is provided
                options.Logger?.LogWarning(ex, "Attempt {Attempt} failed. Retrying...", attempt);

                // Execute the action to potentially fix the error, with error handling
                try
                {
                    await options.OnRetryAttempt(ex);
                }
                catch (Exception onRetryEx)
                {
                    options.Logger?.LogError(onRetryEx, "OnRetryAttempt action failed.");
                }
            }
        }
    }
}
