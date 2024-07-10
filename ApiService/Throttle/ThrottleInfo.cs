using System;
using System.Threading;

namespace TestAssignment.ApiService.Throttle;

public class ThrottleInfo
{
    public int CountRequests { get; set; } = 0;

    public DateTime FirstRequestTime { get; set; }

    public DateTime? BannedUntil { get; set; } = null;

    public SemaphoreSlim Semaphore { get; init; } = new SemaphoreSlim(1, 1);

    public void Ban(DateTime banUntil)
    {
        BannedUntil = banUntil;
        CountRequests = 0;
    }

    public bool IsBanned(DateTime now)
    {
        return BannedUntil != null && BannedUntil > now;
    }

    public void Reset(DateTime now)
    {
        FirstRequestTime = now;
        CountRequests = 0;
    }

    public bool IsThrottleIntervalExceeded(DateTime now, TimeSpan throttleInterval)
    {
        if (now - FirstRequestTime <= throttleInterval)
        {
            return false;
        }

        Reset(now);
        CountRequests++;

        return true;
    }
}