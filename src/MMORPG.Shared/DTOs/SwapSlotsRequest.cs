using System;

namespace MMORPG.Shared.DTOs;

public class SwapSlotsRequest
{
    public string SessionToken { get; set; } = string.Empty;
    public Guid CharacterId { get; set; }
    public int FromSlot { get; set; }
    public int ToSlot { get; set; }
}
