using MMORPG.Domain.Enums;

namespace MMORPG.Domain.Interfaces;

public class DynamicItemData
{
    public string ItemId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ItemType Type { get; set; }
    public ItemRarity Rarity { get; set; }
    public int RequiredLevel { get; set; } = 1;
    public int BaseDamage { get; set; }
    public int BaseArmor { get; set; }
    public int PriceGold { get; set; } = 100;
}

public class DynamicMonsterData
{
    public string MonsterId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public MonsterType Type { get; set; }
    public MonsterSpecies Species { get; set; }
    public int Level { get; set; } = 1;
    public int MaxHp { get; set; } = 500;
    public int AttackPower { get; set; } = 50;
    public long ExpYield { get; set; } = 100;
}

public class DynamicNpcData
{
    public string NpcId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = "Merchant";
    public string DialogueText { get; set; } = "Hoş geldiniz!";
    public List<string> ShopItemIds { get; set; } = new();
}

public class DynamicMapData
{
    public int ZoneId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RecommendedLevelMin { get; set; } = 1;
    public int RecommendedLevelMax { get; set; } = 10;
    public string DefaultWeather { get; set; } = "Clear";
    public List<string> SpawnedMonsterIds { get; set; } = new();
    public string ZoneBossMonsterId { get; set; } = string.Empty;
}

public class DynamicClassDefinition
{
    public string ClassId { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string PrimaryRole { get; set; } = "Tank"; // Tank, DPS, Healer
    public string PrimaryAttribute { get; set; } = "Strength"; // Strength, Agility, Intelligence, Vitality
    public string ResourceType { get; set; } = "Mana"; // Mana, Rage, Energy
    public int BaseHp { get; set; } = 500;
    public int BaseMana { get; set; } = 200;
    public List<string> SkillIds { get; set; } = new();
}

public class DynamicSkillDefinition
{
    public string SkillId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string RequiredClassId { get; set; } = string.Empty;
    public int RequiredLevel { get; set; } = 1;
    public float CastTimeSeconds { get; set; } = 0.0f; // 0.0s = Instant
    public float CooldownSeconds { get; set; } = 3.0f;
    public int ResourceCost { get; set; } = 25;
    public float DamageMultiplier { get; set; } = 1.5f;
    public string TargetType { get; set; } = "SingleEnemy"; // SingleEnemy, AoE, Self, Ally
}

public interface IGameDataEditorService
{
    List<DynamicItemData> GetAllItems();
    bool SaveItem(DynamicItemData item);
    bool DeleteItem(string itemId);

    List<DynamicMonsterData> GetAllMonsters();
    bool SaveMonster(DynamicMonsterData monster);
    bool DeleteMonster(string monsterId);

    List<DynamicNpcData> GetAllNpcs();
    bool SaveNpc(DynamicNpcData npc);
    bool DeleteNpc(string npcId);

    List<DynamicMapData> GetAllMaps();
    bool SaveMap(DynamicMapData map);
    bool DeleteMap(int zoneId);

    List<DynamicClassDefinition> GetAllClasses();
    bool SaveClass(DynamicClassDefinition classDef);

    List<DynamicSkillDefinition> GetAllSkills();
    bool SaveSkill(DynamicSkillDefinition skillDef);
}
