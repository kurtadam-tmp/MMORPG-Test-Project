using System.Collections.Generic;

namespace MMORPG.Shared.DTOs;

public class GuildResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Guild { get; set; }
    public IEnumerable<object>? Members { get; set; }
}
