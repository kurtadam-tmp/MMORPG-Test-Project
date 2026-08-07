using System.Collections.Concurrent;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class MailboxService : IMailboxService
{
    private readonly ConcurrentDictionary<Guid, MailMessage> _mails = new();

    public MailMessage SendMail(Guid senderId, string senderName, Guid recipientId, string subject, string body, long gold, Guid? itemId, bool isCod, long codAmount)
    {
        var mail = new MailMessage
        {
            SenderId = senderId,
            SenderName = senderName,
            RecipientId = recipientId,
            Subject = subject,
            Body = body,
            GoldAttached = gold,
            ItemAttachedId = itemId,
            IsCashOnDelivery = isCod,
            CODAmount = codAmount
        };

        _mails[mail.MailId] = mail;
        Console.WriteLine($"[MailboxService] Mail '{mail.MailId}' sent from '{senderName}' to '{recipientId}' (COD: {isCod}, COD Amount: {codAmount} Gold).");
        return mail;
    }

    public IEnumerable<MailMessage> GetMailbox(Guid recipientId)
    {
        return _mails.Values.Where(m => m.RecipientId == recipientId).OrderByDescending(m => m.SentAt);
    }

    public bool ClaimMail(Guid mailId, Guid recipientId, out long goldClaimed, out Guid? itemClaimed)
    {
        goldClaimed = 0;
        itemClaimed = null;

        if (_mails.TryGetValue(mailId, out var mail) && mail.RecipientId == recipientId && !mail.IsClaimed)
        {
            mail.IsClaimed = true;
            goldClaimed = mail.GoldAttached;
            itemClaimed = mail.ItemAttachedId;
            Console.WriteLine($"[MailboxService SUCCESS] Character '{recipientId}' claimed Mail '{mailId}' (+{goldClaimed} Gold).");
            return true;
        }

        return false;
    }
}
