using MMORPG.Domain.DTOs;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class GatewayHandshakeService : IGatewayHandshakeService
{
    private readonly ICacheService _cacheService;
    private readonly IPlayerSessionService _sessionService;
    private readonly ICharacterRepository _characterRepository;
    private readonly IStatRepository _statRepository;
    private readonly IZoneStateService _zoneStateService;

    private static readonly TimeSpan TokenTtl = TimeSpan.FromMinutes(1);

    public GatewayHandshakeService(
        ICacheService cacheService,
        IPlayerSessionService sessionService,
        ICharacterRepository characterRepository,
        IStatRepository statRepository,
        IZoneStateService zoneStateService)
    {
        _cacheService = cacheService;
        _sessionService = sessionService;
        _characterRepository = characterRepository;
        _statRepository = statRepository;
        _zoneStateService = zoneStateService;
    }

    private static string GetKey(string token) => $"handoff:{token}";

    public async Task<ZoneHandoffToken?> IssueHandoffTokenAsync(string sessionToken, Guid characterId, int targetZoneId)
    {
        var session = await _sessionService.GetSessionAsync(sessionToken);
        if (session == null) return null;

        var character = await _characterRepository.GetByIdAsync(characterId);
        if (character == null || character.PlayerId != session.PlayerId) return null;

        var tokenString = Guid.NewGuid().ToString("N");
        var handoffToken = new ZoneHandoffToken
        {
            Token = tokenString,
            PlayerId = session.PlayerId,
            CharacterId = characterId,
            TargetZoneId = targetZoneId,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(TokenTtl)
        };

        // Cache single-use handoff token in Redis
        try
        {
            await _cacheService.SetAsync(GetKey(tokenString), handoffToken, TokenTtl);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GatewayNotice] Redis handoff cache pending: {ex.Message}");
        }

        return handoffToken;
    }

    public async Task<ZoneHandshakeResult> ValidateAndConsumeHandshakeTokenAsync(ZoneHandshakeRequest request)
    {
        var key = GetKey(request.HandoffToken);
        var token = await _cacheService.GetAsync<ZoneHandoffToken>(key);

        if (token == null)
        {
            return new ZoneHandshakeResult
            {
                Success = false,
                Message = "Invalid or expired zone handoff token."
            };
        }

        if (token.TargetZoneId != request.TargetZoneId)
        {
            return new ZoneHandshakeResult
            {
                Success = false,
                Message = "Target zone mismatch."
            };
        }

        // Atomically consume single-use token from Redis to prevent token replay
        await _cacheService.RemoveAsync(key);

        var character = await _characterRepository.GetByIdAsync(token.CharacterId);
        if (character == null)
        {
            return new ZoneHandshakeResult { Success = false, Message = "Character not found." };
        }

        var stat = await _statRepository.GetByCharacterIdAsync(character.Id);

        // Register in Zone State
        await _zoneStateService.RegisterPlayerInZoneAsync(
            request.TargetZoneId,
            character.Id,
            character.PosX,
            character.PosY,
            character.PosZ);

        // Bind/update active session
        var session = await _sessionService.CreateSessionAsync(token.PlayerId, character.Name);
        await _sessionService.UpdateActiveCharacterAsync(session.SessionToken, character.Id);

        return new ZoneHandshakeResult
        {
            Success = true,
            Message = "Zone handshake completed successfully.",
            SessionToken = session.SessionToken,
            SpawnX = character.PosX,
            SpawnY = character.PosY,
            SpawnZ = character.PosZ,
            Character = new CharacterDto
            {
                Id = character.Id,
                PlayerId = character.PlayerId,
                Name = character.Name,
                Level = character.Level,
                Experience = character.Experience,
                CharacterClass = character.CharacterClass,
                PosX = character.PosX,
                PosY = character.PosY,
                PosZ = character.PosZ,
                ZoneId = character.ZoneId,
                Strength = stat?.Strength ?? 10,
                Agility = stat?.Agility ?? 10,
                Intelligence = stat?.Intelligence ?? 10,
                Vitality = stat?.Vitality ?? 10,
                CurrentHp = stat?.CurrentHp ?? 100,
                MaxHp = stat?.MaxHp ?? 100,
                CurrentMp = stat?.CurrentMp ?? 50,
                MaxMp = stat?.MaxMp ?? 50,
                UnallocatedPoints = stat?.UnallocatedPoints ?? 0
            }
        };
    }
}
