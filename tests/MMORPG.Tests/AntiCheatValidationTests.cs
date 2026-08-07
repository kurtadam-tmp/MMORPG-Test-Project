using System;
using System.Numerics;
using System.Threading.Tasks;
using MMORPG.Domain.Interfaces;
using MMORPG.Domain.Models;
using MMORPG.Infrastructure.Services;
using Xunit;

namespace MMORPG.Tests;

public class AntiCheatValidationTests
{
    private readonly AntiCheatValidationService _antiCheatService;

    public AntiCheatValidationTests()
    {
        var mockSessionService = new MockPlayerSessionService();
        _antiCheatService = new AntiCheatValidationService(mockSessionService);
    }

    [Fact]
    public void ValidateMovement_ValidNormalSpeed_ReturnsTrue()
    {
        var charId = Guid.NewGuid();
        var oldPos = new Vector3(0, 0, 0);
        var newPos = new Vector3(5, 0, 0); // 5 m/s

        bool result = _antiCheatService.ValidateMovement(charId, oldPos, newPos, deltaTime: 1.0f);
        Assert.True(result);
    }

    [Fact]
    public void ValidateMovement_SpeedHack_DetectsViolationAndReturnsFalse()
    {
        var charId = Guid.NewGuid();
        var oldPos = new Vector3(0, 0, 0);
        var newPos = new Vector3(50, 0, 0); // 50 m/s > Max 16.5 m/s

        bool result = _antiCheatService.ValidateMovement(charId, oldPos, newPos, deltaTime: 1.0f);
        Assert.False(result);
        Assert.Equal(1, _antiCheatService.GetViolationCount(charId));
    }

    [Fact]
    public void ValidateMovement_TeleportHack_DetectsDiscontinuityAndReturnsFalse()
    {
        var charId = Guid.NewGuid();
        var oldPos = new Vector3(0, 0, 0);
        var newPos = new Vector3(120, 0, 0); // 120m jump

        bool result = _antiCheatService.ValidateMovement(charId, oldPos, newPos, deltaTime: 1.0f, isTeleportSpell: false);
        Assert.False(result);
        Assert.Equal(1, _antiCheatService.GetViolationCount(charId));
    }

    [Fact]
    public async Task RecordViolationAsync_ThreeViolations_TriggersKick()
    {
        var charId = Guid.NewGuid();

        await _antiCheatService.RecordViolationAsync(charId, "TEST_1", "Details 1");
        await _antiCheatService.RecordViolationAsync(charId, "TEST_2", "Details 2");
        bool kickTriggered = await _antiCheatService.RecordViolationAsync(charId, "TEST_3", "Details 3");

        Assert.True(kickTriggered);
        Assert.Equal(3, _antiCheatService.GetViolationCount(charId));
    }
}

public class MockPlayerSessionService : IPlayerSessionService
{
    public Task<PlayerSession> CreateSessionAsync(Guid playerId, string username) => Task.FromResult(new PlayerSession());
    public Task<PlayerSession?> GetSessionAsync(string sessionToken) => Task.FromResult<PlayerSession?>(null);
    public Task<bool> UpdateActiveCharacterAsync(string sessionToken, Guid characterId) => Task.FromResult(true);
    public Task<bool> RevokeSessionAsync(string sessionToken) => Task.FromResult(true);
}
