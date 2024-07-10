using System;

namespace TestAssignment.ApiService.Providers;

public class TimeProvider : ITimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}