namespace MMORPG.Domain.DTOs;

public class CharacterResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public CharacterDto? Character { get; set; }
    public IEnumerable<CharacterDto>? Characters { get; set; }
}
