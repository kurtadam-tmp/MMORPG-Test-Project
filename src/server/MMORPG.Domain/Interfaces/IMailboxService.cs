namespace MMORPG.Domain.Interfaces;

public class MailMessage
{
    public Guid MailId { get; set; } = Guid.NewGuid();
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public Guid RecipientId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public long GoldAttached { get; set; }
    public Guid? ItemAttachedId { get; set; }
    public bool IsCashOnDelivery { get; set; }
    public long CODAmount { get; set; }
    public bool IsClaimed { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}

public interface IMailboxService
{
    MailMessage SendMail(Guid senderId, string senderName, Guid recipientId, string subject, string body, long gold, Guid? itemId, bool isCod, long codAmount);
    IEnumerable<MailMessage> GetMailbox(Guid recipientId);
    bool ClaimMail(Guid mailId, Guid recipientId, out long goldClaimed, out Guid? itemClaimed);
}
