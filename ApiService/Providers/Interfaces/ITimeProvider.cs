using System;

namespace TestAssignment.ApiService;

public interface ITimeProvider
{
    public DateTime UtcNow { get; }
}