using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class MMORPGNativeClient : MonoBehaviour
{
    public static MMORPGNativeClient Instance { get; private set; }

    [Header("Dedicated Zone Server Connection")]
    public string ServerIp = "127.0.0.1";
    public int ServerPort = 7777;

    [Header("Character Credentials")]
    public string CharacterId = "hero-thorin-001";
    public string HandoffToken = "dev_handoff_token";

    private UdpClient _udpClient;
    private IPEndPoint _serverEndPoint;
    private bool _isConnected = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        try
        {
            Debug.Log($"[MMORPGNativeClient] Connecting UDP socket to {ServerIp}:{ServerPort}...");
            _serverEndPoint = new IPEndPoint(IPAddress.Parse(ServerIp), ServerPort);
            _udpClient = new UdpClient();
            _udpClient.Connect(_serverEndPoint);
            _isConnected = true;

            SendHandshakePacket();
            Debug.Log("[MMORPGNativeClient] Connected & Zone Handshake Packet Sent!");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[MMORPGNativeClient] Connection Error: {ex.Message}");
        }
    }

    private void FixedUpdate()
    {
        if (!_isConnected) return;

        // Read WASD Keyboard Input at 30 Hz
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

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
            Debug.Log($"[MMORPGNativeClient] Chat Sent: {message}");
        }
    }

    public void SendUseSkill(int skillSlot)
    {
        if (_isConnected)
        {
            string payload = $"SKILL|{CharacterId}|{skillSlot}";
            byte[] bytes = Encoding.UTF8.GetBytes(payload);
            _udpClient?.Send(bytes, bytes.Length);
            Debug.Log($"[MMORPGNativeClient] Skill #{skillSlot} Packet Sent!");
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

    private void OnDestroy()
    {
        _udpClient?.Close();
    }
}
