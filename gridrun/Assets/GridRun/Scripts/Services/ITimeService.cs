using System;

namespace GridRun.Services
{
    public interface ITimeService
    {
        DateTime UtcNow { get; }
    }
}
