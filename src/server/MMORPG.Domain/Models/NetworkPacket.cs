using MMORPG.Domain.Enums;

namespace MMORPG.Domain.Models;

public class NetworkPacket
{
    public PacketOpCode OpCode { get; set; }
    public long SequenceId { get; set; }
    public byte[] Payload { get; set; } = Array.Empty<byte>();
}
