using MMORPG.Shared.DTOs;
using MMORPG.Shared.Enums;

namespace MMORPG.Domain.Interfaces;

public interface IGuildService
{
    Task<GuildResult> CreateGuildAsync(CreateGuildRequest request);
    Task<GuildResult> JoinGuildAsync(GuildOperationRequest request);
    Task<GuildResult> LeaveGuildAsync(GuildOperationRequest request);
    Task<GuildResult> PromoteMemberAsync(GuildOperationRequest request, GuildRank newRank);
    Task<GuildResult> DepositVaultGoldAsync(GuildOperationRequest request);
    Task<GuildResult> GetGuildDetailsAsync(Guid guildId);
}
