using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

// I am using a package Nito.AsyncEx to use the asynchronous implementation 
// of ReaderWriterLock so as not to block threads.

// When clearing a dictionary, no thread should read data from it.
using Nito.AsyncEx;

using TestAssignment.ApiService.ApiRequest;

namespace TestAssignment.ApiService.Throttle;

// I assume that the start of the time interval limiting
// the number of requests from a single IP address may be different for each IP address.

// Since a ITimeProvider is used, it is quite difficult 
// to implement a timer for periodic cleaning of the dictionary.  
// Therefore, I assume that the CanAccessAsync function is called quite often
public class ThrottleService 
{
    const int TIME_BETWEEN_CLEANUPS_IN_SECONDS = 300; // 5 munites
    private AsyncReaderWriterLock _readerWriterLock = new();
    private ConcurrentDictionary<string, ThrottleInfo> _ipThrottleInfos = new(); 
    private readonly ThrottleSettings _throttleSettings;
    private readonly ITimeProvider _timeProvider;
    private DateTime _lastCleaningTime;

    public ThrottleService(ThrottleSettings throttleSettings, ITimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _throttleSettings = throttleSettings;
        _lastCleaningTime = _timeProvider.UtcNow;
    }

    public async Task<bool> CanAccessAsync(BaseRequest request)
    {
        if (_timeProvider.UtcNow < _throttleSettings.IntervalRootUtc)
        {
            return true;
        }

        await CleanAsync();

        bool hasAccess = false;

        using IDisposable readerLock = await _readerWriterLock.ReaderLockAsync();

        ThrottleInfo throttleInfo = _ipThrottleInfos.GetOrAdd(request.IpAddress, _ => new ThrottleInfo()
        {
            FirstRequestTime = _timeProvider.UtcNow,
        });

        await throttleInfo.Semaphore.WaitAsync();

        try
        {
            hasAccess = ProcessRequest(throttleInfo);
        }
        finally
        {
            throttleInfo.Semaphore.Release();
        }

        return hasAccess;
    }

    public async Task CleanAsync()
    {
        if (_timeProvider.UtcNow -  _lastCleaningTime < TimeSpan.FromSeconds(TIME_BETWEEN_CLEANUPS_IN_SECONDS))
        {
            return;
        }

        using IDisposable writerLock = await _readerWriterLock.WriterLockAsync();

        List<string> keysForRemove = _ipThrottleInfos
            .Where(x => x.Value.BannedUntil is null || x.Value.BannedUntil <= _timeProvider.UtcNow)
            .Where(x => x.Value.FirstRequestTime + _throttleSettings.ThrottleInterval <= _timeProvider.UtcNow)
            .Select(x => x.Key).ToList();

        foreach (string key in keysForRemove)
        {
            _ipThrottleInfos.TryRemove(key, out _);
        }

        _lastCleaningTime = _timeProvider.UtcNow;
    }

    private bool ProcessRequest(ThrottleInfo throttleInfo)
    {
        DateTime now = _timeProvider.UtcNow;

        if (throttleInfo.IsBanned(now))
        {
            return false;
        }

        if (throttleInfo.IsThrottleIntervalExceeded(now, _throttleSettings.ThrottleInterval))
        {
            return true;
        }

        if (throttleInfo.CountRequests < _throttleSettings.MaxRequestsPerIp)
        {
            throttleInfo.CountRequests++;
            return true;
        }

        throttleInfo.Ban(now + _throttleSettings.BanTimeOut);
        
        return false;
    }

}