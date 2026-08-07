namespace MMORPG.Shared.Enums;

public enum PacketOpCode : byte
{
    PingPong = 0x00,
    HandshakeRequest = 0x01,
    HandshakeResponse = 0x02,
    MovementInput = 0x03,
    MovementValidation = 0x04,
    CastSkill = 0x05,
    CombatResult = 0x06,
    ChatMessage = 0x07
}
