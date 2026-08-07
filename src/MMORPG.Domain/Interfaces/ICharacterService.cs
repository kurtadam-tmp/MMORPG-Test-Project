using MMORPG.Domain.DTOs;

namespace MMORPG.Domain.Interfaces;

public interface ICharacterService
{
    Task<CharacterResult> CreateCharacterAsync(CreateCharacterRequest request);
    Task<CharacterResult> GetPlayerCharactersAsync(string sessionToken);
    Task<CharacterResult> SelectCharacterAsync(string sessionToken, Guid characterId);
}
