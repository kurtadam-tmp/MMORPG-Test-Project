using System.Text.Json;
using MMORPG.Domain.Enums;
using MMORPG.Domain.Models;

namespace MMORPG.Infrastructure.Network;

public static class PacketSerializer
{
    public static byte[] Serialize<T>(PacketOpCode opCode, long sequenceId, T payload)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        writer.Write((byte)opCode);
        writer.Write(sequenceId);

        var json = JsonSerializer.Serialize(payload);
        writer.Write(json);

        return ms.ToArray();
    }

    public static (PacketOpCode OpCode, long SequenceId, T? Payload) Deserialize<T>(byte[] rawData)
    {
        if (rawData.Length < 9)
            throw new ArgumentException("Invalid raw packet length.");

        using var ms = new MemoryStream(rawData);
        using var reader = new BinaryReader(ms);

        var opCode = (PacketOpCode)reader.ReadByte();
        var sequenceId = reader.ReadInt64();
        var json = reader.ReadString();

        var payload = JsonSerializer.Deserialize<T>(json);
        return (opCode, sequenceId, payload);
    }
}
