using MMORPG.Domain.DTOs;
using MMORPG.Domain.Entities;
using MMORPG.Domain.Enums;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class CharacterService : ICharacterService
{
    private readonly ICharacterRepository _characterRepository;
    private readonly IStatRepository _statRepository;
    private readonly IPlayerSessionService _sessionService;
    private const int MaxCharactersPerAccount = 4;

    public CharacterService(
        ICharacterRepository characterRepository,
        IStatRepository statRepository,
        IPlayerSessionService sessionService)
    {
        _characterRepository = characterRepository;
        _statRepository = statRepository;
        _sessionService = sessionService;
    }

    public async Task<CharacterResult> CreateCharacterAsync(CreateCharacterRequest request)
    {
        // 1. Validate Session
        var session = await _sessionService.GetSessionAsync(request.SessionToken);
        if (session == null)
        {
            return new CharacterResult { Success = false, Message = "Invalid or expired session token." };
        }

        // 2. Validate Character Name
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 3 || request.Name.Length > 16)
        {
            return new CharacterResult { Success = false, Message = "Character name must be between 3 and 16 characters." };
        }

        // 3. Validate Account Character Limit
        var existingCharacters = await _characterRepository.GetByPlayerIdAsync(session.PlayerId);
        if (existingCharacters.Count() >= MaxCharactersPerAccount)
        {
            return new CharacterResult { Success = false, Message = $"Account reached maximum character limit ({MaxCharactersPerAccount})." };
        }

        // 4. Calculate Default Class Stats
        var (defaultStat, spawnZoneId) = GetDefaultStatsForClass(request.CharacterClass);

        var character = new Character
        {
            PlayerId = session.PlayerId,
            Name = request.Name,
            Level = 1,
            Experience = 0,
            CharacterClass = request.CharacterClass,
            PosX = 0.0f,
            PosY = 0.0f,
            PosZ = 0.0f,
            ZoneId = spawnZoneId,
            CreatedAt = DateTime.UtcNow
        };

        // 5. Atomically Create Character and Stat entries
        var characterId = await _characterRepository.CreateWithStatsAsync(character, defaultStat);
        character.Id = characterId;
        defaultStat.CharacterId = characterId;

        var characterDto = MapToDto(character, defaultStat);

        return new CharacterResult
        {
            Success = true,
            Message = "Character created successfully.",
            Character = characterDto
        };
    }

    public async Task<CharacterResult> GetPlayerCharactersAsync(string sessionToken)
    {
        var session = await _sessionService.GetSessionAsync(sessionToken);
        if (session == null)
        {
            return new CharacterResult { Success = false, Message = "Invalid or expired session token." };
        }

        var characters = await _characterRepository.GetByPlayerIdAsync(session.PlayerId);
        var dtos = new List<CharacterDto>();

        foreach (var c in characters)
        {
            var stat = await _statRepository.GetByCharacterIdAsync(c.Id);
            dtos.Add(MapToDto(c, stat));
        }

        return new CharacterResult
        {
            Success = true,
            Message = "Characters retrieved successfully.",
            Characters = dtos
        };
    }

    public async Task<CharacterResult> SelectCharacterAsync(string sessionToken, Guid characterId)
    {
        var session = await _sessionService.GetSessionAsync(sessionToken);
        if (session == null)
        {
            return new CharacterResult { Success = false, Message = "Invalid or expired session token." };
        }

        var character = await _characterRepository.GetByIdAsync(characterId);
        if (character == null || character.PlayerId != session.PlayerId)
        {
            return new CharacterResult { Success = false, Message = "Character not found or does not belong to this account." };
        }

        // Update active character in Redis session
        await _sessionService.UpdateActiveCharacterAsync(sessionToken, characterId);

        var stat = await _statRepository.GetByCharacterIdAsync(character.Id);

        return new CharacterResult
        {
            Success = true,
            Message = $"Character '{character.Name}' selected.",
            Character = MapToDto(character, stat)
        };
    }

    private static (Stat Stat, int SpawnZoneId) GetDefaultStatsForClass(CharacterClass characterClass)
    {
        return characterClass switch
        {
            CharacterClass.Warrior => (new Stat
            {
                Strength = 15, Agility = 10, Intelligence = 5, Vitality = 15,
                CurrentHp = 150, MaxHp = 150, CurrentMp = 30, MaxMp = 30
            }, 1),

            CharacterClass.Mage => (new Stat
            {
                Strength = 5, Agility = 8, Intelligence = 18, Vitality = 9,
                CurrentHp = 90, MaxHp = 90, CurrentMp = 120, MaxMp = 120
            }, 1),

            CharacterClass.Rogue => (new Stat
            {
                Strength = 10, Agility = 16, Intelligence = 8, Vitality = 11,
                CurrentHp = 110, MaxHp = 110, CurrentMp = 60, MaxMp = 60
            }, 1),

            CharacterClass.Cleric => (new Stat
            {
                Strength = 10, Agility = 8, Intelligence = 14, Vitality = 13,
                CurrentHp = 130, MaxHp = 130, CurrentMp = 100, MaxMp = 100
            }, 1),

            _ => (new Stat(), 1)
        };
    }

    private static CharacterDto MapToDto(Character c, Stat? s)
    {
        return new CharacterDto
        {
            Id = c.Id,
            PlayerId = c.PlayerId,
            Name = c.Name,
            Level = c.Level,
            Experience = c.Experience,
            CharacterClass = c.CharacterClass,
            PosX = c.PosX,
            PosY = c.PosY,
            PosZ = c.PosZ,
            ZoneId = c.ZoneId,
            CreatedAt = c.CreatedAt,

            Strength = s?.Strength ?? 10,
            Agility = s?.Agility ?? 10,
            Intelligence = s?.Intelligence ?? 10,
            Vitality = s?.Vitality ?? 10,
            CurrentHp = s?.CurrentHp ?? 100,
            MaxHp = s?.MaxHp ?? 100,
            CurrentMp = s?.CurrentMp ?? 50,
            MaxMp = s?.MaxMp ?? 50,
            UnallocatedPoints = s?.UnallocatedPoints ?? 0
        };
    }
}
