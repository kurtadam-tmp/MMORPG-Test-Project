using System.Collections.Concurrent;
using MMORPG.Domain.DTOs;
using MMORPG.Domain.Interfaces;
using MMORPG.Domain.Models;
using MMORPG.Shared.DTOs;
using MMORPG.Shared.Enums;

namespace MMORPG.Infrastructure.Services;

public class DungeonPartyService : IDungeonPartyService
{
    private readonly IPlayerSessionService _sessionService;
    private readonly ICharacterRepository _characterRepository;
    private readonly IStatRepository _statRepository;
    private readonly IGatewayHandshakeService _handshakeService;

    private readonly ConcurrentDictionary<Guid, PartyGroup> _activeParties = new();
    private readonly ConcurrentDictionary<Guid, DungeonInstance> _dungeonInstances = new();

    public DungeonPartyService(
        IPlayerSessionService sessionService,
        ICharacterRepository characterRepository,
        IStatRepository statRepository,
        IGatewayHandshakeService handshakeService)
    {
        _sessionService = sessionService;
        _characterRepository = characterRepository;
        _statRepository = statRepository;
        _handshakeService = handshakeService;
    }

    public async Task<PartyResult> CreatePartyAsync(CreatePartyRequest request)
    {
        var session = await _sessionService.GetSessionAsync(request.SessionToken);
        if (session == null || session.ActiveCharacterId != request.LeaderCharacterId)
        {
            return new PartyResult { Success = false, Message = "Unauthorized session token." };
        }

        var character = await _characterRepository.GetByIdAsync(request.LeaderCharacterId);
        var stats = await _statRepository.GetByCharacterIdAsync(request.LeaderCharacterId);

        if (character == null || stats == null)
        {
            return new PartyResult { Success = false, Message = "Character data not found." };
        }

        var party = new PartyGroup
        {
            PartyId = Guid.NewGuid(),
            LeaderCharacterId = request.LeaderCharacterId
        };

        var leaderMember = new PartyMember
        {
            CharacterId = character.Id,
            CharacterName = character.Name,
            Level = character.Level,
            Class = (Shared.Enums.CharacterClass)character.CharacterClass,
            CurrentHp = stats.CurrentHp,
            MaxHp = stats.MaxHp,
            IsLeader = true
        };

        party.Members.Add(leaderMember);
        _activeParties.TryAdd(party.PartyId, party);

        return new PartyResult
        {
            Success = true,
            Message = "Party created successfully.",
            Party = party
        };
    }

    public async Task<PartyResult> InviteMemberAsync(PartyOperationRequest request)
    {
        var session = await _sessionService.GetSessionAsync(request.SessionToken);
        if (session == null || session.ActiveCharacterId != request.ActorCharacterId)
        {
            return new PartyResult { Success = false, Message = "Unauthorized session token." };
        }

        if (!_activeParties.TryGetValue(request.PartyId, out var party))
        {
            return new PartyResult { Success = false, Message = "Party not found." };
        }

        if (party.LeaderCharacterId != request.ActorCharacterId)
        {
            return new PartyResult { Success = false, Message = "Only the party leader can invite members." };
        }

        if (party.Members.Count >= 5)
        {
            return new PartyResult { Success = false, Message = "Party is full (Max 5 members)." };
        }

        if (!request.TargetCharacterId.HasValue)
        {
            return new PartyResult { Success = false, Message = "Target character ID required." };
        }

        var targetChar = await _characterRepository.GetByIdAsync(request.TargetCharacterId.Value);
        var targetStats = await _statRepository.GetByCharacterIdAsync(request.TargetCharacterId.Value);

        if (targetChar == null || targetStats == null)
        {
            return new PartyResult { Success = false, Message = "Target character data not found." };
        }

        var member = new PartyMember
        {
            CharacterId = targetChar.Id,
            CharacterName = targetChar.Name,
            Level = targetChar.Level,
            Class = (Shared.Enums.CharacterClass)targetChar.CharacterClass,
            CurrentHp = targetStats.CurrentHp,
            MaxHp = targetStats.MaxHp,
            IsLeader = false
        };

        party.Members.Add(member);

        return new PartyResult
        {
            Success = true,
            Message = $"Player '{targetChar.Name}' added to the party.",
            Party = party
        };
    }

    public async Task<PartyResult> LeavePartyAsync(PartyOperationRequest request)
    {
        var session = await _sessionService.GetSessionAsync(request.SessionToken);
        if (session == null || session.ActiveCharacterId != request.ActorCharacterId)
        {
            return new PartyResult { Success = false, Message = "Unauthorized session token." };
        }

        if (!_activeParties.TryGetValue(request.PartyId, out var party))
        {
            return new PartyResult { Success = false, Message = "Party not found." };
        }

        party.Members.RemoveAll(m => m.CharacterId == request.ActorCharacterId);

        if (party.Members.Count == 0)
        {
            _activeParties.TryRemove(request.PartyId, out _);
        }
        else if (party.LeaderCharacterId == request.ActorCharacterId)
        {
            // Re-assign leadership to next member
            party.LeaderCharacterId = party.Members[0].CharacterId;
            party.Members[0].IsLeader = true;
        }

        return new PartyResult { Success = true, Message = "Left party successfully." };
    }

    public async Task<PartyResult> EnterDungeonInstanceAsync(EnterDungeonRequest request)
    {
        var session = await _sessionService.GetSessionAsync(request.SessionToken);
        if (session == null || session.ActiveCharacterId != request.LeaderCharacterId)
        {
            return new PartyResult { Success = false, Message = "Unauthorized session token." };
        }

        if (!_activeParties.TryGetValue(request.PartyId, out var party))
        {
            return new PartyResult { Success = false, Message = "Party not found." };
        }

        if (party.LeaderCharacterId != request.LeaderCharacterId)
        {
            return new PartyResult { Success = false, Message = "Only the party leader can start a dungeon instance." };
        }

        // Spawn dynamic Dungeon Instance (Zone #99 for Dungeon)
        var dungeonInstance = new DungeonInstance
        {
            InstanceId = Guid.NewGuid(),
            DungeonTypeId = request.DungeonTypeId,
            PartyId = party.PartyId,
            TargetZoneId = 99,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        party.DungeonInstanceId = dungeonInstance.InstanceId;
        _dungeonInstances.TryAdd(dungeonInstance.InstanceId, dungeonInstance);

        // Generate Single-Use Zone Handoff Tokens for all party members
        var tokens = new List<ZoneHandoffToken>();
        foreach (var member in party.Members)
        {
            var token = await _handshakeService.IssueHandoffTokenAsync(request.SessionToken, member.CharacterId, dungeonInstance.TargetZoneId);
            if (token != null) tokens.Add(token);
        }

        Console.WriteLine($"[DungeonParty] Party '{party.PartyId}' entered Dungeon '{request.DungeonTypeId}' (Zone #{dungeonInstance.TargetZoneId}). Generated {tokens.Count} handoff tokens.");

        return new PartyResult
        {
            Success = true,
            Message = $"Dungeon Instance '{request.DungeonTypeId}' spawned! Redirecting party members.",
            DungeonInstance = dungeonInstance,
            HandoffTokens = tokens
        };
    }

    public Task<PartyGroup?> GetPartyAsync(Guid partyId)
    {
        _activeParties.TryGetValue(partyId, out var party);
        return Task.FromResult(party);
    }
}
