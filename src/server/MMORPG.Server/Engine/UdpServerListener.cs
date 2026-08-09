using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Server.Engine;

public class UdpServerListener : BackgroundService
{
    private readonly INetworkPacketProcessor _packetProcessor;
    private readonly int _listenPort;

    public UdpServerListener(INetworkPacketProcessor packetProcessor, int listenPort = 7777)
    {
        _packetProcessor = packetProcessor;
        _listenPort = listenPort;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var udpClient = new UdpClient(_listenPort);
        Console.WriteLine($"[UdpServerListener] Listening for incoming UDP packets on port {_listenPort}...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = await udpClient.ReceiveAsync(stoppingToken);
                var responseBytes = await _packetProcessor.ProcessIncomingPacketAsync(result.Buffer);

                if (responseBytes.Length > 0)
                {
                    await udpClient.SendAsync(responseBytes, responseBytes.Length, result.RemoteEndPoint);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UdpServerListener Error] Exception processing UDP packet: {ex.Message}");
            }
        }

        Console.WriteLine("[UdpServerListener] Stopped.");
    }
}
