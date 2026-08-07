using System.Collections.Generic;

namespace MMORPG.Shared.DTOs;

public class QuestResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Quest { get; set; }
    public IEnumerable<object>? ActiveQuests { get; set; }
}
