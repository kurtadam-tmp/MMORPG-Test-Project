using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Godot;

public partial class MMORPGGodotClient : Node
{
    public static MMORPGGodotClient Instance { get; private set; } = null!;

    [Export] public string ServerIp = "127.0.0.1";
    [Export] public int ServerPort = 7777;
    [Export] public string CharacterId = "hero-godot-001";
    [Export] public string HandoffToken = "dev_godot_handoff_token";

    private UdpClient _udpClient = null!;
    private IPEndPoint _serverEndPoint = null!;
    private bool _isConnected = false;

    public override void _Ready()
    {
        Instance = this;
        ConnectToDedicatedServer();
    }

    private void ConnectToDedicatedServer()
    {
        try
        {
            GD.Print($"[MMORPGGodotClient] Connecting UDP socket to {ServerIp}:{ServerPort}...");
            _serverEndPoint = new IPEndPoint(IPAddress.Parse(ServerIp), ServerPort);
            _udpClient = new UdpClient();
            _udpClient.Connect(_serverEndPoint);
            _isConnected = true;

            SendHandshakePacket();
            GD.Print("[MMORPGGodotClient] Connected & Zone Handshake Packet Sent!");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[MMORPGGodotClient] Connection Error: {ex.Message}");
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_isConnected) return;

        float moveX = 0f;
        float moveZ = 0f;

        if (Input.IsActionPressed("ui_right") || Input.IsKeyPressed(Key.D)) moveX += 1f;
        if (Input.IsActionPressed("ui_left") || Input.IsKeyPressed(Key.A)) moveX -= 1f;
        if (Input.IsActionPressed("ui_down") || Input.IsKeyPressed(Key.S)) moveZ += 1f;
        if (Input.IsActionPressed("ui_up") || Input.IsKeyPressed(Key.W)) moveZ -= 1f;

        if (moveX != 0 || moveZ != 0)
        {
            SendMovementPacket(moveX, moveZ);
        }
    }

    public void SendChatMessage(string message)
    {
        if (_isConnected && !string.IsNullOrWhiteSpace(message))
        {
            string payload = $"CHAT|{CharacterId}|{message}";
            byte[] bytes = Encoding.UTF8.GetBytes(payload);
            _udpClient?.Send(bytes, bytes.Length);
            GD.Print($"[MMORPGGodotClient] Chat Sent: {message}");
        }
    }

    public void SendUseSkill(int skillSlot)
    {
        if (_isConnected)
        {
            string payload = $"SKILL|{CharacterId}|{skillSlot}";
            byte[] bytes = Encoding.UTF8.GetBytes(payload);
            _udpClient?.Send(bytes, bytes.Length);
            GD.Print($"[MMORPGGodotClient] Skill #{skillSlot} Packet Sent!");
        }
    }

    private void SendHandshakePacket()
    {
        string payload = $"HANDSHAKE|{CharacterId}|{HandoffToken}";
        byte[] bytes = Encoding.UTF8.GetBytes(payload);
        _udpClient?.Send(bytes, bytes.Length);
    }

    private void SendMovementPacket(float moveX, float moveZ)
    {
        string payload = $"MOVE|{CharacterId}|{moveX:F2}|{moveZ:F2}";
        byte[] bytes = Encoding.UTF8.GetBytes(payload);
        _udpClient?.Send(bytes, bytes.Length);
    }

    public override void _ExitTree()
    {
        _udpClient?.Close();
    }
}
