using MMORPG.Domain.Enums;

namespace MMORPG.Domain.DTOs;

public class CreateCharacterRequest
{
    public string SessionToken { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public CharacterClass CharacterClass { get; set; }
}
