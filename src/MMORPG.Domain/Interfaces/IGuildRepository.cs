using MMORPG.Domain.Entities;
using MMORPG.Shared.Enums;

namespace MMORPG.Domain.Interfaces;

public interface IGuildRepository
{
    Task<Guild?> GetByIdAsync(Guid guildId);
    Task<Guild?> GetByNameAsync(string name);
    Task<GuildMember?> GetMemberAsync(Guid characterId);
    Task<IEnumerable<GuildMember>> GetGuildMembersAsync(Guid guildId);
    Task<bool> CreateGuildAtomicAsync(Guild guild, GuildMember leaderMember);
    Task<bool> AddMemberAsync(GuildMember member);
    Task<bool> RemoveMemberAsync(Guid guildId, Guid characterId);
    Task<bool> UpdateMemberRankAsync(Guid guildId, Guid characterId, GuildRank newRank);
    Task<bool> DepositVaultGoldAtomicAsync(Guid guildId, Guid characterId, long goldAmount);
}
