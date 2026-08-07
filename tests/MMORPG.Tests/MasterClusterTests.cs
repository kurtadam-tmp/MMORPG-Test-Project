using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MMORPG.Domain.Interfaces;
using MMORPG.Domain.Models;
using MMORPG.Infrastructure.Services;
using Xunit;

namespace MMORPG.Tests;

public class MasterClusterTests
{
    private readonly MasterClusterService _clusterService;

    public MasterClusterTests()
    {
        var mockCacheService = new MockCacheService();
        _clusterService = new MasterClusterService(mockCacheService);
    }

    [Fact]
    public async Task RegisterZoneNodeAsync_ValidNode_RegistersSuccessfully()
    {
        var node = new ZoneServerNode
        {
            ServerId = "zone-node-1",
            IpAddress = "127.0.0.1",
            Port = 7777,
            HostedZoneIds = new List<int> { 1 }
        };

        bool registered = await _clusterService.RegisterZoneNodeAsync(node);
        Assert.True(registered);
    }

    [Fact]
    public async Task GetBestZoneServerAsync_MultipleNodes_SelectsLowestCapacityNode()
    {
        var node1 = new ZoneServerNode
        {
            ServerId = "node-busy",
            IpAddress = "127.0.0.1",
            Port = 7777,
            HostedZoneIds = new List<int> { 1 },
            CurrentPlayerCount = 300,
            MaxPlayerCount = 500
        };

        var node2 = new ZoneServerNode
        {
            ServerId = "node-free",
            IpAddress = "127.0.0.1",
            Port = 7778,
            HostedZoneIds = new List<int> { 1 },
            CurrentPlayerCount = 40,
            MaxPlayerCount = 500
        };

        await _clusterService.RegisterZoneNodeAsync(node1);
        await _clusterService.RegisterZoneNodeAsync(node2);

        var bestNode = await _clusterService.GetBestZoneServerAsync(targetZoneId: 1);

        Assert.NotNull(bestNode);
        Assert.Equal("node-free", bestNode.ServerId);
        Assert.Equal(7778, bestNode.Port);
    }
}

public class MockCacheService : ICacheService
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> _store = new();
    public Task<T?> GetAsync<T>(string key) => Task.FromResult(_store.TryGetValue(key, out var json) ? System.Text.Json.JsonSerializer.Deserialize<T>(json) : default);
    public Task<bool> SetAsync<T>(string key, T value, TimeSpan? timeToLive = null) { _store[key] = System.Text.Json.JsonSerializer.Serialize(value); return Task.FromResult(true); }
    public Task<bool> RemoveAsync(string key) => Task.FromResult(_store.TryRemove(key, out _));
    public Task<bool> KeyExistsAsync(string key) => Task.FromResult(_store.ContainsKey(key));
}
