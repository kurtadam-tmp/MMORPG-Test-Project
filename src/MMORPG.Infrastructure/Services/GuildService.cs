using MMORPG.Domain.Entities;
using MMORPG.Domain.Interfaces;
using MMORPG.Shared.DTOs;
using MMORPG.Shared.Enums;

namespace MMORPG.Infrastructure.Services;

public class GuildService : IGuildService
{
    private readonly IGuildRepository _guildRepository;
    private readonly IPlayerSessionService _sessionService;

    public GuildService(
        IGuildRepository guildRepository,
        IPlayerSessionService sessionService)
    {
        _guildRepository = guildRepository;
        _sessionService = sessionService;
    }

    public async Task<GuildResult> CreateGuildAsync(CreateGuildRequest request)
    {
        var session = await _sessionService.GetSessionAsync(request.SessionToken);
        if (session == null || session.ActiveCharacterId != request.LeaderCharacterId)
        {
            return new GuildResult { Success = false, Message = "Unauthorized session token." };
        }

        if (string.IsNullOrWhiteSpace(request.GuildName) || request.GuildName.Length < 3)
        {
            return new GuildResult { Success = false, Message = "Guild name must be at least 3 characters long." };
        }

        var existingMember = await _guildRepository.GetMemberAsync(request.LeaderCharacterId);
        if (existingMember != null)
        {
            return new GuildResult { Success = false, Message = "Character is already a member of a guild." };
        }

        var existingGuild = await _guildRepository.GetByNameAsync(request.GuildName);
        if (existingGuild != null)
        {
            return new GuildResult { Success = false, Message = "A guild with this name already exists." };
        }

        var guild = new Guild
        {
            Name = request.GuildName,
            LeaderCharacterId = request.LeaderCharacterId,
            VaultGold = 0,
            CreatedAt = DateTime.UtcNow
        };

        var leaderMember = new GuildMember
        {
            CharacterId = request.LeaderCharacterId,
            Rank = GuildRank.Leader,
            JoinedAt = DateTime.UtcNow
        };

        await _guildRepository.CreateGuildAtomicAsync(guild, leaderMember);

        return new GuildResult
        {
            Success = true,
            Message = $"Guild '{guild.Name}' successfully created!",
            Guild = guild
        };
    }

    public async Task<GuildResult> JoinGuildAsync(GuildOperationRequest request)
    {
        var session = await _sessionService.GetSessionAsync(request.SessionToken);
        if (session == null || session.ActiveCharacterId != request.ActorCharacterId)
        {
            return new GuildResult { Success = false, Message = "Unauthorized session token." };
        }

        var existingMember = await _guildRepository.GetMemberAsync(request.ActorCharacterId);
        if (existingMember != null)
        {
            return new GuildResult { Success = false, Message = "Character is already a member of a guild." };
        }

        var newMember = new GuildMember
        {
            GuildId = request.GuildId,
            CharacterId = request.ActorCharacterId,
            Rank = GuildRank.Member,
            JoinedAt = DateTime.UtcNow
        };

        await _guildRepository.AddMemberAsync(newMember);

        return new GuildResult { Success = true, Message = "Joined guild successfully." };
    }

    public async Task<GuildResult> LeaveGuildAsync(GuildOperationRequest request)
    {
        var session = await _sessionService.GetSessionAsync(request.SessionToken);
        if (session == null || session.ActiveCharacterId != request.ActorCharacterId)
        {
            return new GuildResult { Success = false, Message = "Unauthorized session token." };
        }

        var member = await _guildRepository.GetMemberAsync(request.ActorCharacterId);
        if (member == null || member.GuildId != request.GuildId)
        {
            return new GuildResult { Success = false, Message = "Character is not a member of this guild." };
        }

        if (member.Rank == GuildRank.Leader)
        {
            return new GuildResult { Success = false, Message = "Guild leader cannot leave the guild without transferring leadership." };
        }

        await _guildRepository.RemoveMemberAsync(request.GuildId, request.ActorCharacterId);
        return new GuildResult { Success = true, Message = "Left guild successfully." };
    }

    public async Task<GuildResult> PromoteMemberAsync(GuildOperationRequest request, GuildRank newRank)
    {
        var session = await _sessionService.GetSessionAsync(request.SessionToken);
        if (session == null || session.ActiveCharacterId != request.ActorCharacterId)
        {
            return new GuildResult { Success = false, Message = "Unauthorized session token." };
        }

        var actorMember = await _guildRepository.GetMemberAsync(request.ActorCharacterId);
        if (actorMember == null || actorMember.GuildId != request.GuildId || actorMember.Rank != GuildRank.Leader)
        {
            return new GuildResult { Success = false, Message = "Only the Guild Leader can promote or demote members." };
        }

        if (!request.TargetCharacterId.HasValue)
        {
            return new GuildResult { Success = false, Message = "Target character ID required." };
        }

        await _guildRepository.UpdateMemberRankAsync(request.GuildId, request.TargetCharacterId.Value, newRank);
        return new GuildResult { Success = true, Message = $"Member rank updated to {newRank}." };
    }

    public async Task<GuildResult> DepositVaultGoldAsync(GuildOperationRequest request)
    {
        var session = await _sessionService.GetSessionAsync(request.SessionToken);
        if (session == null || session.ActiveCharacterId != request.ActorCharacterId)
        {
            return new GuildResult { Success = false, Message = "Unauthorized session token." };
        }

        if (request.GoldAmount <= 0)
        {
            return new GuildResult { Success = false, Message = "Gold amount must be greater than zero." };
        }

        var member = await _guildRepository.GetMemberAsync(request.ActorCharacterId);
        if (member == null || member.GuildId != request.GuildId)
        {
            return new GuildResult { Success = false, Message = "Character is not a member of this guild." };
        }

        var deposited = await _guildRepository.DepositVaultGoldAtomicAsync(request.GuildId, request.ActorCharacterId, request.GoldAmount);
        if (!deposited)
        {
            return new GuildResult { Success = false, Message = "Failed to deposit gold. Insufficient character gold." };
        }

        return new GuildResult { Success = true, Message = $"Successfully deposited {request.GoldAmount} gold into the guild vault." };
    }

    public async Task<GuildResult> GetGuildDetailsAsync(Guid guildId)
    {
        var guild = await _guildRepository.GetByIdAsync(guildId);
        if (guild == null)
        {
            return new GuildResult { Success = false, Message = "Guild not found." };
        }

        var members = await _guildRepository.GetGuildMembersAsync(guildId);
        return new GuildResult
        {
            Success = true,
            Message = "Guild details retrieved.",
            Guild = guild,
            Members = members
        };
    }
}
