using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Mod.DynamicEncounters.Common.Data;

public class RetryOptions(int retryCount, ILogger? logger = null)
{
    public int RetryCount { get; set; } = retryCount;
    public Func<Exception, bool> ShouldRetryOnException { get; set; } = ex => true;
    public ILogger? Logger { get; set; } = logger;
    public Func<Exception, Task> OnRetryAttempt { get; set; } = ex => Task.CompletedTask;
    
    public static RetryOptions Default(ILogger logger) => new(3, logger);
}

public class RetryOptions<T>(int retryCount, ILogger? logger = null) : RetryOptions(retryCount, logger)
{
    public Func<T, bool> ShouldRetryOnResult { get; set; } = _ => false;
}