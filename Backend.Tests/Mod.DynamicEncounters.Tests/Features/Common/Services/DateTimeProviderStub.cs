using Mod.DynamicEncounters.Common.Interfaces;

namespace Mod.DynamicEncounters.Tests.Features.Common.Services;

public class DateTimeProviderStub : IDateTimeProvider
{
    private DateTime _dateTime;

    public DateTime UtcNow() => _dateTime;

    public void SetDateTime(DateTime dateTime)
    {
        _dateTime = dateTime;
    }

    public void AddTime(TimeSpan timeSpan)
    {
        _dateTime += timeSpan;
    }
}