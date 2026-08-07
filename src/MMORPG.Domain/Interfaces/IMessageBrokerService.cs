using MMORPG.Domain.DTOs;

namespace MMORPG.Domain.Interfaces;

public interface IMessageBrokerService
{
    Task PublishChatMessageAsync(ChatMessageDto message);
    Task PublishSystemAnnouncementAsync(string announcementText);
    Task SubscribeToChannelAsync(string channelName, Action<string, ChatMessageDto> onMessageReceived);
}
