﻿using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace KBot.Services;

public class RedisService : IInjectable
{
    private readonly IConnectionMultiplexer _redis;

    public RedisService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public Task SetEpicRefreshDateAsync(DateTime time)
    {
        var db = _redis.GetDatabase();
        return db.StringSetAsync("epic_refresh_date", time.ToString("yyyy-MM-dd HH:mm:ss"));
    }
    public async Task<DateTime> GetEpicRefreshDateAsync()
    {
        var db = _redis.GetDatabase();
        var date = await db.StringGetAsync("epic_refresh_date").ConfigureAwait(false);
        return DateTime.Parse(date.ToString());
    }
    public Task SetDbdRefreshDateAsync(DateTime time)
    {
        var db = _redis.GetDatabase();
        return db.StringSetAsync("dbd_refresh_date", time.ToString("yyyy-MM-dd HH:mm:ss"));
    }
    public Task<DateTime> GetDbdRefreshDateAsync()
    {
        var db = _redis.GetDatabase();
        var date = db.StringGet("dbd_refresh_date");
        return Task.FromResult(DateTime.Parse(date));
    }
}