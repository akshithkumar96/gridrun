using System;

namespace GridRun.Services
{
    public sealed class SystemTimeService : ITimeService
    {
        public DateTime UtcNow
        {
            get { return DateTime.UtcNow; }
        }
    }
}
