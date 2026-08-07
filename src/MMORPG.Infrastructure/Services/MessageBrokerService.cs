using System.Text.Json;
using MMORPG.Domain.DTOs;
using MMORPG.Domain.Enums;
using MMORPG.Domain.Interfaces;
using MMORPG.Infrastructure.Cache;
using StackExchange.Redis;

namespace MMORPG.Infrastructure.Services;

public class MessageBrokerService : IMessageBrokerService
{
    private readonly IRedisConnectionFactory _redisFactory;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public MessageBrokerService(IRedisConnectionFactory redisFactory)
    {
        _redisFactory = redisFactory;
    }

    private static string GetChannelName(ChatMessageDto dto)
    {
        return dto.Channel switch
        {
            ChatChannel.Global => "chat:global",
            ChatChannel.Zone => $"chat:zone:{dto.TargetZoneId ?? 1}",
            ChatChannel.Guild => $"chat:guild:{dto.TargetGuildId ?? Guid.Empty}",
            ChatChannel.System => "system:announcements",
            _ => "chat:global"
        };
    }

    public async Task PublishChatMessageAsync(ChatMessageDto message)
    {
        var channelName = GetChannelName(message);
        var json = JsonSerializer.Serialize(message, _jsonOptions);

        Console.WriteLine($"[ChatBroadcast] Channel '{channelName}' - Sender: '{message.SenderName}': \"{message.MessageText}\"");

        try
        {
            var sub = _redisFactory.GetConnection().GetSubscriber();
            await sub.PublishAsync(RedisChannel.Literal(channelName), json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatBroadcast Notice] Redis PubSub connection pending: {ex.Message}");
        }
    }

    public async Task PublishSystemAnnouncementAsync(string announcementText)
    {
        var dto = new ChatMessageDto
        {
            Channel = ChatChannel.System,
            SenderCharacterId = Guid.Empty,
            SenderName = "[SYSTEM]",
            MessageText = announcementText,
            Timestamp = DateTime.UtcNow
        };

        await PublishChatMessageAsync(dto);
    }

    public async Task SubscribeToChannelAsync(string channelName, Action<string, ChatMessageDto> onMessageReceived)
    {
        var sub = _redisFactory.GetConnection().GetSubscriber();
        await sub.SubscribeAsync(RedisChannel.Literal(channelName), (ch, msg) =>
        {
            if (msg.HasValue)
            {
                try
                {
                    var dto = JsonSerializer.Deserialize<ChatMessageDto>(msg.ToString(), _jsonOptions);
                    if (dto != null)
                    {
                        onMessageReceived?.Invoke(ch.ToString(), dto);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PubSub Error] Failed to deserialize message from {ch}: {ex.Message}");
                }
            }
        });
    }
}
