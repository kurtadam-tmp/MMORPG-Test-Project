using MMORPG.Domain.Models;
using MMORPG.Shared.DTOs;

namespace MMORPG.Domain.Interfaces;

public interface IDungeonPartyService
{
    Task<PartyResult> CreatePartyAsync(CreatePartyRequest request);
    Task<PartyResult> InviteMemberAsync(PartyOperationRequest request);
    Task<PartyResult> LeavePartyAsync(PartyOperationRequest request);
    Task<PartyResult> EnterDungeonInstanceAsync(EnterDungeonRequest request);
    Task<PartyGroup?> GetPartyAsync(Guid partyId);
}
