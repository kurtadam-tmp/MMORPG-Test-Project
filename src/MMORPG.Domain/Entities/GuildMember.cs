using MMORPG.Shared.Enums;

namespace MMORPG.Domain.Entities;

public class GuildMember
{
    public Guid GuildId { get; set; }
    public Guid CharacterId { get; set; }
    public GuildRank Rank { get; set; } = GuildRank.Member;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
