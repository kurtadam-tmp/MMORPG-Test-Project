namespace MMORPG.Domain.Interfaces;

public interface INetworkPacketProcessor
{
    Task<byte[]> ProcessIncomingPacketAsync(byte[] rawPacketData);
}
