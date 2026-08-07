using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class MMORPGUnityClientExample : MonoBehaviour
{
    public static MMORPGUnityClientExample Instance { get; private set; }

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
            Debug.Log($"[MMORPGUnityClientExample] Connecting to Dedicated Server at {ServerIp}:{ServerPort}...");
            _serverEndPoint = new IPEndPoint(IPAddress.Parse(ServerIp), ServerPort);
            _udpClient = new UdpClient();
            _udpClient.Connect(_serverEndPoint);
            _isConnected = true;

            SendHandshakePacket();
            Debug.Log("[MMORPGUnityClientExample] Connected & Handshake Sent!");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[MMORPGUnityClientExample] Connection Error: {ex.Message}");
        }
    }

    private void FixedUpdate()
    {
        if (!_isConnected) return;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        if (moveX != 0 || moveZ != 0)
        {
            SendMovementPacket(moveX, moveZ);
        }
    }

    public void SendChatMessage(string msg)
    {
        if (_isConnected && !string.IsNullOrWhiteSpace(msg))
        {
            string payload = $"CHAT|{CharacterId}|{msg}";
            byte[] bytes = Encoding.UTF8.GetBytes(payload);
            _udpClient?.Send(bytes, bytes.Length);
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
