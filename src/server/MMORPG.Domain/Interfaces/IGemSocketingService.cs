namespace MMORPG.Domain.Interfaces;

public class SocketedEquipment
{
    public Guid ItemId { get; set; } = Guid.NewGuid();
    public string ItemName { get; set; } = string.Empty;
    public int MaxSockets { get; set; } = 3;
    public List<string> InsertedGems { get; set; } = new();
    public string ActiveRuneWord { get; set; } = string.Empty;
    public int BonusAttackPower { get; set; }
    public int BonusArmor { get; set; }
    public int BonusAllStats { get; set; }
}

public interface IGemSocketingService
{
    bool PunchSockets(Guid itemId, string itemName, out SocketedEquipment equipment);
    bool InsertGemOrRune(Guid itemId, string gemOrRuneName, out string resultMessage);
    bool RemoveAllGems(Guid itemId, out string resultMessage);
}
