using System.Collections.Generic;

namespace MMORPG.Shared.DTOs;

public class PartyResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Party { get; set; }
    public object? DungeonInstance { get; set; }
    public IEnumerable<object>? HandoffTokens { get; set; }
}
