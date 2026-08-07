namespace MMORPG.Domain.Interfaces;

public enum CardSlotType
{
    Weapon = 0,
    Headgear = 1,
    Armor = 2,
    Garment = 3,
    Footwear = 4,
    Accessory = 5
}

public class MonsterCardDefinition
{
    public string CardId { get; set; } = string.Empty;
    public string MonsterName { get; set; } = string.Empty;
    public CardSlotType TargetSlot { get; set; }
    public double BaseDropRatePercentage { get; set; } = 0.01;
    public string CardEffectDescription { get; set; } = string.Empty;
    public int BonusStatAmount { get; set; }
    public string UniqueSkillEffect { get; set; } = string.Empty;
}

public class SlottedCardItem
{
    public Guid EquipmentItemId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public List<MonsterCardDefinition> InsertedCards { get; set; } = new();
}

public interface IMonsterCardService
{
    bool RollMonsterCardDrop(string monsterName, float dropMultiplier, out MonsterCardDefinition droppedCard);
    bool CompoundCardToEquipment(Guid itemId, string equipmentName, string cardId, out string resultMessage);
    List<MonsterCardDefinition> GetCardsInEquipment(Guid itemId);
}
