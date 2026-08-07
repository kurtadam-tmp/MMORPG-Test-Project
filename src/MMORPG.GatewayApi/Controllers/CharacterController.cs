using Microsoft.AspNetCore.Mvc;
using MMORPG.Domain.DTOs;
using MMORPG.Domain.Interfaces;

namespace MMORPG.GatewayApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CharacterController : ControllerBase
{
    private readonly ICharacterService _characterService;
    private readonly IGatewayHandshakeService _handshakeService;
    private readonly IPlayerSessionService _sessionService;

    public CharacterController(
        ICharacterService characterService,
        IGatewayHandshakeService handshakeService,
        IPlayerSessionService sessionService)
    {
        _characterService = characterService;
        _handshakeService = handshakeService;
        _sessionService = sessionService;
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetCharacters([FromQuery] string sessionToken)
    {
        var result = await _characterService.GetPlayerCharactersAsync(sessionToken);
        if (!result.Success)
        {
            return Unauthorized(result);
        }
        return Ok(result);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateCharacter([FromBody] CreateCharacterRequest request)
    {
        var result = await _characterService.CreateCharacterAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("select")]
    public async Task<IActionResult> SelectCharacter([FromBody] SelectCharacterRequest request)
    {
        var result = await _characterService.SelectCharacterAsync(request.SessionToken, request.CharacterId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        // Issue single-use Zone Handoff Token for Dedicated Zone Server (Zone #1)
        var handoffToken = await _handshakeService.IssueHandoffTokenAsync(request.SessionToken, request.CharacterId, request.TargetZoneId);

        return Ok(new
        {
            Success = true,
            Message = "Character selected. Connect to Dedicated Zone Server.",
            HandoffToken = handoffToken
        });
    }
}

public class SelectCharacterRequest
{
    public string SessionToken { get; set; } = string.Empty;
    public Guid CharacterId { get; set; }
    public int TargetZoneId { get; set; } = 1;
}
