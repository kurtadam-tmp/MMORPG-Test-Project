using System.Collections.Concurrent;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class MonsterCardService : IMonsterCardService
{
    private readonly ConcurrentDictionary<Guid, SlottedCardItem> _slottedGear = new();
    private static readonly Random _rng = new();

    private static readonly Dictionary<string, MonsterCardDefinition> CardDatabase = new()
    {
        {
            "poring_card", new MonsterCardDefinition
            {
                CardId = "poring_card",
                MonsterName = "Poring",
                TargetSlot = CardSlotType.Garment,
                BaseDropRatePercentage = 0.05, // 0.05%
                CardEffectDescription = "+1 LUK & +10% Auto-Loot Drop Rate",
                BonusStatAmount = 1,
                UniqueSkillEffect = "Auto-Loot Boost"
            }
        },
        {
            "peco_peco_card", new MonsterCardDefinition
            {
                CardId = "peco_peco_card",
                MonsterName = "Peco Peco",
                TargetSlot = CardSlotType.Armor,
                BaseDropRatePercentage = 0.02,
                CardEffectDescription = "+10% Max HP Bonus",
                BonusStatAmount = 10,
                UniqueSkillEffect = "Max HP Multiplier"
            }
        },
        {
            "baphomet_card", new MonsterCardDefinition
            {
                CardId = "baphomet_card",
                MonsterName = "Baphomet World Boss",
                TargetSlot = CardSlotType.Weapon,
                BaseDropRatePercentage = 0.005, // 0.005% Ultra Rare World Boss Card
                CardEffectDescription = "Splash Damage: Basic attacks hit all surrounding enemies in 3m radius!",
                BonusStatAmount = 50,
                UniqueSkillEffect = "Area Splash Attack"
            }
        },
        {
            "golden_thief_bug_card", new MonsterCardDefinition
            {
                CardId = "golden_thief_bug_card",
                MonsterName = "Golden Thief Bug",
                TargetSlot = CardSlotType.Headgear,
                BaseDropRatePercentage = 0.001,
                CardEffectDescription = "Immunity to all targeted magical spells!",
                BonusStatAmount = 100,
                UniqueSkillEffect = "Magic Immunity"
            }
        }
    };

    public bool RollMonsterCardDrop(string monsterName, float dropMultiplier, out MonsterCardDefinition droppedCard)
    {
        droppedCard = null!;
        var cardTemplate = CardDatabase.Values.FirstOrDefault(c => c.MonsterName.Equals(monsterName, StringComparison.OrdinalIgnoreCase));
        if (cardTemplate == null)
        {
            // Generic monster card generation if not explicitly in database
            cardTemplate = new MonsterCardDefinition
            {
                CardId = $"{monsterName.ToLower().Replace(" ", "_")}_card",
                MonsterName = monsterName,
                TargetSlot = CardSlotType.Weapon,
                BaseDropRatePercentage = 0.01,
                CardEffectDescription = $"+15 Damage against {monsterName} species",
                BonusStatAmount = 15,
                UniqueSkillEffect = "Species Damage Bonus"
            };
        }

        double roll = _rng.NextDouble() * 100.0;
        double effectiveDropChance = cardTemplate.BaseDropRatePercentage * Math.Max(1.0f, dropMultiplier);

        if (roll <= effectiveDropChance)
        {
            droppedCard = cardTemplate;
            Console.WriteLine($"[RAGNAROK CARD DROP!] Legendary Ultra-Rare '{cardTemplate.MonsterName} Card' dropped! Drop Chance: {effectiveDropChance:F4}%!");
            return true;
        }

        return false;
    }

    public bool CompoundCardToEquipment(Guid itemId, string equipmentName, string cardId, out string resultMessage)
    {
        resultMessage = string.Empty;
        var card = CardDatabase.Values.FirstOrDefault(c => c.CardId.Equals(cardId, StringComparison.OrdinalIgnoreCase));
        if (card == null)
        {
            resultMessage = "Geçersiz Canavar Kartı ID'si.";
            return false;
        }

        var gear = _slottedGear.GetOrAdd(itemId, id => new SlottedCardItem
        {
            EquipmentItemId = id,
            EquipmentName = equipmentName,
            InsertedCards = new List<MonsterCardDefinition>()
        });

        lock (gear)
        {
            if (gear.InsertedCards.Count >= 4) // Max 4 Cards per gear
            {
                resultMessage = "Bu ekipmandaki tüm Kart Yuvaları (Card Sockets) dolu!";
                return false;
            }

            gear.InsertedCards.Add(card);
            resultMessage = $"EFSANEVİ KART İŞLEME BAŞARILI! '{card.MonsterName} Card' [{card.CardEffectDescription}] '{equipmentName}' ekipmanına takıldı! ({gear.InsertedCards.Count}/4 Kart)";
            Console.WriteLine($"[RAGNAROK CARD COMPOUND] Slotted '{card.MonsterName} Card' into '{equipmentName}' ({itemId})!");
            return true;
        }
    }

    public List<MonsterCardDefinition> GetCardsInEquipment(Guid itemId)
    {
        if (_slottedGear.TryGetValue(itemId, out var gear))
        {
            return gear.InsertedCards;
        }
        return new List<MonsterCardDefinition>();
    }
}
