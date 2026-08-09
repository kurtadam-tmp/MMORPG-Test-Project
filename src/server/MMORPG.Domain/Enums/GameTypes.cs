namespace MMORPG.Domain.Enums;

public enum CharacterRace
{
    Human = 0,   // Balanced (+5% EXP Gain)
    Elf = 1,     // Agile (+10% Move Speed & Spell Power)
    Dwarf = 2,   // Stout (+15% Max HP & Armor)
    Orc = 3,     // Fierce (+15% Attack Power & Lifesteal)
    Undead = 4   // Shadow (+20% Dark Magic Resistance)
}

public enum MonsterType
{
    Normal = 0,
    Elite = 1,
    MiniBoss = 2,
    WorldBoss = 3,
    RaidBoss = 4
}

public enum MonsterSpecies
{
    Beast = 0,
    Undead = 1,
    Demon = 2,
    Dragon = 3,
    Elemental = 4,
    Humanoid = 5,
    Insectoid = 6,
    Giant = 7
}

public enum ItemRarity
{
    Poor = 0,
    Common = 1,
    Uncommon = 2,
    Rare = 3,
    Epic = 4,
    Legendary = 5,
    Mythic = 6,
    Divine = 7
}

public enum ItemType
{
    Weapon = 0,
    Armor = 1,
    Consumable = 2,
    Material = 3,
    MonsterCard = 4,
    QuestItem = 5,
    Mount = 6,
    Pet = 7
}

public enum WeaponType
{
    None = 0,
    OneHandedSword = 1,
    TwoHandedGreatsword = 2,
    Dagger = 3,
    Bow = 4,
    Staff = 5,
    Wand = 6,
    Shield = 7,
    Crossbow = 8
}

public enum ArmorType
{
    None = 0,
    Cloth = 1,
    Leather = 2,
    Mail = 3,
    Plate = 4
}

public enum EquipmentSlot
{
    Head = 0,
    Chest = 1,
    Legs = 2,
    Gloves = 3,
    Boots = 4,
    MainHand = 5,
    OffHand = 6,
    Ring1 = 7,
    Ring2 = 8,
    Amulet = 9,
    MountSlot = 10,
    PetSlot = 11
}

public enum ElementalAttribute
{
    Neutral = 0,
    Fire = 1,
    Water = 2,
    Wind = 3,
    Earth = 4,
    Holy = 5,
    Shadow = 6
}
