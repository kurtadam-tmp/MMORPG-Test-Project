using System.Text.Json;
using MMORPG.Domain.DTOs;
using MMORPG.Domain.Enums;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Network;

public class NetworkPacketProcessor : INetworkPacketProcessor
{
    private readonly IGatewayHandshakeService _handshakeService;
    private readonly IMovementValidationService _movementService;
    private readonly ICombatEngineService _combatService;
    private readonly IMessageBrokerService _messageBrokerService;

    public NetworkPacketProcessor(
        IGatewayHandshakeService handshakeService,
        IMovementValidationService movementService,
        ICombatEngineService combatService,
        IMessageBrokerService messageBrokerService)
    {
        _handshakeService = handshakeService;
        _movementService = movementService;
        _combatService = combatService;
        _messageBrokerService = messageBrokerService;
    }

    public async Task<byte[]> ProcessIncomingPacketAsync(byte[] rawPacketData)
    {
        if (rawPacketData.Length < 9)
            return PacketSerializer.Serialize(PacketOpCode.PingPong, 0, new { Error = "Invalid header" });

        using var ms = new MemoryStream(rawPacketData);
        using var reader = new BinaryReader(ms);

        var opCode = (PacketOpCode)reader.ReadByte();
        var sequenceId = reader.ReadInt64();
        var jsonPayload = reader.ReadString();

        return opCode switch
        {
            PacketOpCode.HandshakeRequest => await HandleHandshakeAsync(sequenceId, jsonPayload),
            PacketOpCode.MovementInput => await HandleMovementAsync(sequenceId, jsonPayload),
            PacketOpCode.CastSkill => await HandleCombatAsync(sequenceId, jsonPayload),
            PacketOpCode.ChatMessage => await HandleChatAsync(sequenceId, jsonPayload),
            PacketOpCode.PingPong => PacketSerializer.Serialize(PacketOpCode.PingPong, sequenceId, new { Status = "PONG", ServerTime = DateTime.UtcNow }),
            _ => PacketSerializer.Serialize(opCode, sequenceId, new { Error = "Unsupported OpCode" })
        };
    }

    private async Task<byte[]> HandleHandshakeAsync(long sequenceId, string json)
    {
        var req = JsonSerializer.Deserialize<ZoneHandshakeRequest>(json) ?? new ZoneHandshakeRequest();
        var res = await _handshakeService.ValidateAndConsumeHandshakeTokenAsync(req);
        return PacketSerializer.Serialize(PacketOpCode.HandshakeResponse, sequenceId, res);
    }

    private async Task<byte[]> HandleMovementAsync(long sequenceId, string json)
    {
        var req = JsonSerializer.Deserialize<MovementInputRequest>(json) ?? new MovementInputRequest();
        req.SequenceId = sequenceId;
        var res = await _movementService.ValidateAndApplyMovementAsync(req);
        return PacketSerializer.Serialize(PacketOpCode.MovementValidation, sequenceId, res);
    }

    private async Task<byte[]> HandleCombatAsync(long sequenceId, string json)
    {
        var req = JsonSerializer.Deserialize<CastSkillRequest>(json) ?? new CastSkillRequest();
        var res = await _combatService.ExecuteSkillCastAsync(req);
        return PacketSerializer.Serialize(PacketOpCode.CombatResult, sequenceId, res);
    }

    private async Task<byte[]> HandleChatAsync(long sequenceId, string json)
    {
        var chatMsg = JsonSerializer.Deserialize<ChatMessageDto>(json) ?? new ChatMessageDto();
        try
        {
            await _messageBrokerService.PublishChatMessageAsync(chatMsg);
            return PacketSerializer.Serialize(PacketOpCode.ChatMessage, sequenceId, new { Success = true, BroadcastTime = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            return PacketSerializer.Serialize(PacketOpCode.ChatMessage, sequenceId, new { Success = false, Error = ex.Message });
        }
    }
}
