using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MMORPG.Shared.DTOs;
using MMORPG.Shared.Enums;
using MMORPG.Shared.Network;

namespace MMORPG.Shared.Unity;

public struct Vector3Struct
{
    public float x;
    public float y;
    public float z;

    public Vector3Struct(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

/// <summary>
/// Unity and Pure C# compatible Network Client Adapter.
/// Can be imported directly into Unity C# Projects.
/// </summary>
public class MMORPGNetworkClient
{
    public string ServerIp { get; set; } = "127.0.0.1";
    public int ServerPort { get; set; } = 7777;
    public string SessionToken { get; set; } = "";
    public string CharacterIdString { get; set; } = "";
    public float MovementSendInterval { get; set; } = 0.05f; // 20 Hz send rate

    private UdpClient? _udpClient;
    private IPEndPoint? _serverEndPoint;
    private CancellationTokenSource? _cts;
    private long _sequenceId = 1;
    private DateTime _lastMovementSendTime;

    private readonly ConcurrentQueue<Action> _mainThreadQueue = new();

    public event Action<PacketOpCode, long, string>? OnPacketReceived;

    public void ConnectToServer(string ip, int port)
    {
        try
        {
            ServerIp = ip;
            ServerPort = port;
            _serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            _udpClient = new UdpClient();
            _udpClient.Connect(_serverEndPoint);
            _cts = new CancellationTokenSource();

            Task.Run(() => ReceiveLoopAsync(_cts.Token));

            Console.WriteLine($"[MMORPGNetworkClient] Connected to Dedicated Server at {ip}:{port}");
            SendPing();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MMORPGNetworkClient] Failed to connect: {ex.Message}");
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested && _udpClient != null)
        {
            try
            {
                var result = await _udpClient.ReceiveAsync();
                if (result.Buffer.Length >= 9)
                {
                    var (opCode, seq, jsonPayload) = PacketSerializer.Deserialize<object>(result.Buffer);

                    _mainThreadQueue.Enqueue(() =>
                    {
                        OnPacketReceived?.Invoke(opCode, seq, jsonPayload?.ToString() ?? "");
                    });
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MMORPGNetworkClient] Receive loop exception: {ex.Message}");
            }
        }
    }

    public void Tick()
    {
        while (_mainThreadQueue.TryDequeue(out var action))
        {
            action?.Invoke();
        }
    }

    public void SendPing()
    {
        if (_udpClient == null) return;
        var bytes = PacketSerializer.Serialize(PacketOpCode.PingPong, Interlocked.Increment(ref _sequenceId), new { ClientTime = DateTime.UtcNow });
        _udpClient.Send(bytes, bytes.Length);
    }

    public void SendMovement(Vector3Struct position)
    {
        if (_udpClient == null) return;
        if ((DateTime.UtcNow - _lastMovementSendTime).TotalSeconds < MovementSendInterval) return;

        _lastMovementSendTime = DateTime.UtcNow;

        Guid.TryParse(CharacterIdString, out var charId);

        var req = new MovementInputRequest
        {
            SessionToken = SessionToken,
            CharacterId = charId,
            TargetX = position.x,
            TargetY = position.y,
            TargetZ = position.z,
            ClientTimestampMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            SequenceId = Interlocked.Increment(ref _sequenceId)
        };

        var bytes = PacketSerializer.Serialize(PacketOpCode.MovementInput, req.SequenceId, req);
        _udpClient.Send(bytes, bytes.Length);
    }

    public void CastSkill(string skillId, Vector3Struct targetPosition)
    {
        if (_udpClient == null) return;

        Guid.TryParse(CharacterIdString, out var charId);

        var req = new CastSkillRequest
        {
            SessionToken = SessionToken,
            AttackerCharacterId = charId,
            TargetCharacterId = Guid.Empty,
            SkillId = skillId,
            TargetX = targetPosition.x,
            TargetY = targetPosition.y,
            TargetZ = targetPosition.z
        };

        var bytes = PacketSerializer.Serialize(PacketOpCode.CastSkill, Interlocked.Increment(ref _sequenceId), req);
        _udpClient.Send(bytes, bytes.Length);
    }

    public void Disconnect()
    {
        _cts?.Cancel();
        _udpClient?.Close();
        _udpClient?.Dispose();
    }
}
