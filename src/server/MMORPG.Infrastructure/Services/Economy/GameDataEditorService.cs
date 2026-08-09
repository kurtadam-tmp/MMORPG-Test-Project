using System.Collections.Concurrent;
using MMORPG.Domain.Enums;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services.Economy;

public class GameDataEditorService : IGameDataEditorService
{
    private readonly ConcurrentDictionary<string, DynamicItemData> _items = new();
    private readonly ConcurrentDictionary<string, DynamicMonsterData> _monsters = new();
    private readonly ConcurrentDictionary<string, DynamicNpcData> _npcs = new();
    private readonly ConcurrentDictionary<int, DynamicMapData> _maps = new();
    private readonly ConcurrentDictionary<string, DynamicClassDefinition> _classes = new();
    private readonly ConcurrentDictionary<string, DynamicSkillDefinition> _skills = new();

    public GameDataEditorService()
    {
        SeedStarterGameDatabase();
    }

    private void SeedStarterGameDatabase()
    {
        // -------------------------------------------------------------
        // 1. STARTER CLASSES & SKILLS SEED DATA
        // -------------------------------------------------------------
        SaveClass(new DynamicClassDefinition
        {
            ClassId = "class_warrior",
            ClassName = "Warrior (Savaşçı)",
            PrimaryRole = "Tank / Melee DPS",
            PrimaryAttribute = "Strength",
            ResourceType = "Rage",
            BaseHp = 800,
            BaseMana = 100,
            SkillIds = new List<string> { "skill_warrior_whirlwind", "skill_warrior_charge", "skill_warrior_taunt", "skill_warrior_shieldwall" }
        });

        SaveClass(new DynamicClassDefinition
        {
            ClassId = "class_mage",
            ClassName = "Mage (Büyücü)",
            PrimaryRole = "Ranged Magic DPS",
            PrimaryAttribute = "Intelligence",
            ResourceType = "Mana",
            BaseHp = 450,
            BaseMana = 750,
            SkillIds = new List<string> { "skill_mage_fireball", "skill_mage_frostnova", "skill_mage_blink", "skill_mage_arcane_explosion" }
        });

        SaveClass(new DynamicClassDefinition
        {
            ClassId = "class_rogue",
            ClassName = "Rogue (Suikastçı)",
            PrimaryRole = "Agile Melee Burst",
            PrimaryAttribute = "Agility",
            ResourceType = "Energy",
            BaseHp = 550,
            BaseMana = 200,
            SkillIds = new List<string> { "skill_rogue_backstab", "skill_rogue_shadowstep", "skill_rogue_poison_dagger", "skill_rogue_stealth" }
        });

        SaveClass(new DynamicClassDefinition
        {
            ClassId = "class_priest",
            ClassName = "Priest (Rahip)",
            PrimaryRole = "Healer / Holy Support",
            PrimaryAttribute = "Intelligence",
            ResourceType = "Mana",
            BaseHp = 500,
            BaseMana = 800,
            SkillIds = new List<string> { "skill_priest_flashheal", "skill_priest_shield", "skill_priest_holynova", "skill_priest_smite" }
        });

        SaveClass(new DynamicClassDefinition
        {
            ClassId = "class_paladin",
            ClassName = "Paladin (Kutsal Şövalye)",
            PrimaryRole = "Hybrid Tank / Healer",
            PrimaryAttribute = "Strength",
            ResourceType = "Mana",
            BaseHp = 750,
            BaseMana = 400,
            SkillIds = new List<string> { "skill_paladin_holystrike", "skill_paladin_divineshield", "skill_paladin_layonhands", "skill_paladin_consecration" }
        });

        SaveClass(new DynamicClassDefinition
        {
            ClassId = "class_necromancer",
            ClassName = "Necromancer (Ölüm Büyücüsü)",
            PrimaryRole = "Summoner / Dark DPS",
            PrimaryAttribute = "Intelligence",
            ResourceType = "Mana",
            BaseHp = 480,
            BaseMana = 700,
            SkillIds = new List<string> { "skill_necro_summonskeleton", "skill_necro_poisonnova", "skill_necro_lifedrain", "skill_necro_armyofthedead" }
        });

        // -------------------------------------------------------------
        // 2. SKILL DEFINITIONS SEED DATA
        // -------------------------------------------------------------
        SaveSkill(new DynamicSkillDefinition { SkillId = "skill_warrior_whirlwind", Name = "Whirlwind (Döner Kılıç)", RequiredClassId = "class_warrior", RequiredLevel = 1, CastTimeSeconds = 0.0f, CooldownSeconds = 4.0f, ResourceCost = 25, DamageMultiplier = 2.2f, TargetType = "AoE" });
        SaveSkill(new DynamicSkillDefinition { SkillId = "skill_warrior_charge", Name = "Battle Charge (Hücum)", RequiredClassId = "class_warrior", RequiredLevel = 5, CastTimeSeconds = 0.0f, CooldownSeconds = 8.0f, ResourceCost = 15, DamageMultiplier = 1.4f, TargetType = "SingleEnemy" });
        SaveSkill(new DynamicSkillDefinition { SkillId = "skill_warrior_shieldwall", Name = "Shield Wall (Kalkan Duvarı)", RequiredClassId = "class_warrior", RequiredLevel = 20, CastTimeSeconds = 0.0f, CooldownSeconds = 45.0f, ResourceCost = 30, DamageMultiplier = 0.0f, TargetType = "Self" });

        SaveSkill(new DynamicSkillDefinition { SkillId = "skill_mage_fireball", Name = "Infernal Fireball (Alev Topu)", RequiredClassId = "class_mage", RequiredLevel = 1, CastTimeSeconds = 1.8f, CooldownSeconds = 1.5f, ResourceCost = 45, DamageMultiplier = 3.0f, TargetType = "SingleEnemy" });
        SaveSkill(new DynamicSkillDefinition { SkillId = "skill_mage_frostnova", Name = "Frost Nova (Buz Halkası)", RequiredClassId = "class_mage", RequiredLevel = 10, CastTimeSeconds = 0.0f, CooldownSeconds = 12.0f, ResourceCost = 60, DamageMultiplier = 1.8f, TargetType = "AoE" });
        SaveSkill(new DynamicSkillDefinition { SkillId = "skill_mage_blink", Name = "Arcane Blink (Işınlanma)", RequiredClassId = "class_mage", RequiredLevel = 15, CastTimeSeconds = 0.0f, CooldownSeconds = 15.0f, ResourceCost = 30, DamageMultiplier = 0.0f, TargetType = "Self" });

        SaveSkill(new DynamicSkillDefinition { SkillId = "skill_priest_flashheal", Name = "Flash Heal (Hızlı Şifa)", RequiredClassId = "class_priest", RequiredLevel = 1, CastTimeSeconds = 1.2f, CooldownSeconds = 0.5f, ResourceCost = 50, DamageMultiplier = -2.5f, TargetType = "Ally" });

        // -------------------------------------------------------------
        // 3. MAPS & ZONES SEED DATA
        // -------------------------------------------------------------
        SaveMap(new DynamicMapData { ZoneId = 1, Name = "Whisperwood Glen (Fısıltı Ormanı - Başlangıç)", Description = "Yeşil huzurlu başlangıç vadisi. Acemi macera arayanların ilk durağı.", RecommendedLevelMin = 1, RecommendedLevelMax = 15, DefaultWeather = "Sunny", SpawnedMonsterIds = new List<string> { "mob_poring", "mob_forest_wolf", "mob_shadow_goblin", "mob_boar_charger" }, ZoneBossMonsterId = "mob_broodmother_spider" });
        SaveMap(new DynamicMapData { ZoneId = 2, Name = "Shadowfen Swamps (Gölge Bataklıkları)", Description = "Zehirli sisler ve tehlikeli örümceklerle kaplı bataklık bölgesi.", RecommendedLevelMin = 15, RecommendedLevelMax = 35, DefaultWeather = "Rainy", SpawnedMonsterIds = new List<string> { "mob_swamp_corruptor", "mob_frost_troll", "mob_desert_bandit" }, ZoneBossMonsterId = "mob_greshnok_crusher" });
        SaveMap(new DynamicMapData { ZoneId = 3, Name = "Inferno Volcano Caldera (Alev Yanardağı)", Description = "Ejderhaların ve lav elementallerinin hüküm sürdüğü tehlikeli kanyon.", RecommendedLevelMin = 35, RecommendedLevelMax = 60, DefaultWeather = "VolcanoAsh", SpawnedMonsterIds = new List<string> { "mob_lava_elemental", "mob_flame_drake", "mob_nether_fiend" }, ZoneBossMonsterId = "mob_ignis_dragon" });
        SaveMap(new DynamicMapData { ZoneId = 99, Name = "Ironforge Fortress (Demir Kalesi - Klan Şatosu)", Description = "Haftalık Klan Şatosu Kuşatması ve PvP Krallık Arenası bölgesi.", RecommendedLevelMin = 50, RecommendedLevelMax = 60, DefaultWeather = "Thunderstorm", SpawnedMonsterIds = new List<string> { "mob_undead_knight" }, ZoneBossMonsterId = "mob_malakor_boss" });

        // -------------------------------------------------------------
        // 4. MONSTERS & BOSSES SEED DATA
        // -------------------------------------------------------------
        SaveMonster(new DynamicMonsterData { MonsterId = "mob_poring", Name = "Poring", Type = MonsterType.Normal, Species = MonsterSpecies.Beast, Level = 1, MaxHp = 80, AttackPower = 8, ExpYield = 20 });
        SaveMonster(new DynamicMonsterData { MonsterId = "mob_forest_wolf", Name = "Forest Wolf", Type = MonsterType.Normal, Species = MonsterSpecies.Beast, Level = 3, MaxHp = 160, AttackPower = 18, ExpYield = 45 });
        SaveMonster(new DynamicMonsterData { MonsterId = "mob_shadow_goblin", Name = "Shadow Goblin Scout", Type = MonsterType.Normal, Species = MonsterSpecies.Humanoid, Level = 5, MaxHp = 240, AttackPower = 25, ExpYield = 70 });
        SaveMonster(new DynamicMonsterData { MonsterId = "mob_boar_charger", Name = "Wild Boar Charger", Type = MonsterType.Normal, Species = MonsterSpecies.Beast, Level = 8, MaxHp = 420, AttackPower = 38, ExpYield = 110 });

        SaveMonster(new DynamicMonsterData { MonsterId = "mob_swamp_corruptor", Name = "Swamp Corruptor", Type = MonsterType.Elite, Species = MonsterSpecies.Undead, Level = 20, MaxHp = 3500, AttackPower = 180, ExpYield = 850 });
        SaveMonster(new DynamicMonsterData { MonsterId = "mob_frost_troll", Name = "Frost Troll Warrior", Type = MonsterType.Elite, Species = MonsterSpecies.Giant, Level = 25, MaxHp = 5200, AttackPower = 260, ExpYield = 1400 });
        SaveMonster(new DynamicMonsterData { MonsterId = "mob_broodmother_spider", Name = "Broodmother Spider", Type = MonsterType.MiniBoss, Species = MonsterSpecies.Insectoid, Level = 15, MaxHp = 18000, AttackPower = 420, ExpYield = 4500 });

        SaveMonster(new DynamicMonsterData { MonsterId = "mob_greshnok_crusher", Name = "Greshnok the Crusher", Type = MonsterType.MiniBoss, Species = MonsterSpecies.Giant, Level = 35, MaxHp = 75000, AttackPower = 850, ExpYield = 15000 });
        SaveMonster(new DynamicMonsterData { MonsterId = "mob_flame_drake", Name = "Inferno Flame Drake", Type = MonsterType.Elite, Species = MonsterSpecies.Dragon, Level = 48, MaxHp = 22000, AttackPower = 720, ExpYield = 6500 });
        SaveMonster(new DynamicMonsterData { MonsterId = "mob_ignis_dragon", Name = "Inferno Dragon Ignis", Type = MonsterType.WorldBoss, Species = MonsterSpecies.Dragon, Level = 60, MaxHp = 1500000, AttackPower = 2400, ExpYield = 100000 });
        SaveMonster(new DynamicMonsterData { MonsterId = "mob_malakor_boss", Name = "Arch-Demon Malakor", Type = MonsterType.RaidBoss, Species = MonsterSpecies.Demon, Level = 60, MaxHp = 3000000, AttackPower = 3500, ExpYield = 250000 });

        // -------------------------------------------------------------
        // 5. ITEMS & EQUIPMENT SEED DATA
        // -------------------------------------------------------------
        SaveItem(new DynamicItemData { ItemId = "item_sword_01", Name = "Novice Iron Sword", Type = ItemType.Weapon, Rarity = ItemRarity.Common, RequiredLevel = 1, BaseDamage = 15, BaseArmor = 0, PriceGold = 50 });
        SaveItem(new DynamicItemData { ItemId = "item_staff_01", Name = "Apprentice Oak Staff", Type = ItemType.Weapon, Rarity = ItemRarity.Common, RequiredLevel = 1, BaseDamage = 22, BaseArmor = 0, PriceGold = 50 });
        SaveItem(new DynamicItemData { ItemId = "item_dagger_01", Name = "Rusty Iron Dagger", Type = ItemType.Weapon, Rarity = ItemRarity.Common, RequiredLevel = 1, BaseDamage = 12, BaseArmor = 0, PriceGold = 45 });
        SaveItem(new DynamicItemData { ItemId = "item_armor_chest_01", Name = "Recruit Leather Chestpiece", Type = ItemType.Armor, Rarity = ItemRarity.Common, RequiredLevel = 1, BaseDamage = 0, BaseArmor = 18, PriceGold = 80 });

        SaveItem(new DynamicItemData { ItemId = "item_sword_dragonslayer", Name = "Dragon Slayer Greatsword", Type = ItemType.Weapon, Rarity = ItemRarity.Epic, RequiredLevel = 50, BaseDamage = 180, BaseArmor = 15, PriceGold = 15000 });
        SaveItem(new DynamicItemData { ItemId = "item_staff_archmage", Name = "Archmage Arcane Staff", Type = ItemType.Weapon, Rarity = ItemRarity.Epic, RequiredLevel = 50, BaseDamage = 210, BaseArmor = 5, PriceGold = 18000 });
        SaveItem(new DynamicItemData { ItemId = "item_greatsword_godslayer", Name = "Godslayer Greatsword", Type = ItemType.Weapon, Rarity = ItemRarity.Legendary, RequiredLevel = 60, BaseDamage = 350, BaseArmor = 35, PriceGold = 75000 });

        // -------------------------------------------------------------
        // 6. NPC & SHOP SEED DATA
        // -------------------------------------------------------------
        SaveNpc(new DynamicNpcData { NpcId = "npc_grom", Name = "Blacksmith Grom", Role = "Blacksmith", DialogueText = "Silahını ve zırhını dövmeye mi geldin, savaşçı?", ShopItemIds = new List<string> { "item_sword_01", "item_dagger_01", "item_armor_chest_01" } });
        SaveNpc(new DynamicNpcData { NpcId = "npc_elena", Name = "Alchemist Elena", Role = "Potion Merchant", DialogueText = "Şifalı iksirler ve büyülü bileşenler burada!", ShopItemIds = new List<string> { "item_potion_hp", "item_potion_mp", "item_elixir_power" } });
        SaveNpc(new DynamicNpcData { NpcId = "npc_valerius", Name = "Grandmaster Valerius", Role = "Skill Trainer", DialogueText = "Sınıfının gizli tekniklerini öğrenmek için hazır mısın?", ShopItemIds = new List<string>() });
    }

    public List<DynamicItemData> GetAllItems() => _items.Values.ToList();

    public bool SaveItem(DynamicItemData item)
    {
        if (string.IsNullOrEmpty(item.ItemId)) item.ItemId = $"item_{Guid.NewGuid().ToString("N")[..8]}";
        _items[item.ItemId] = item;
        return true;
    }

    public bool DeleteItem(string itemId) => _items.TryRemove(itemId, out _);

    public List<DynamicMonsterData> GetAllMonsters() => _monsters.Values.ToList();

    public bool SaveMonster(DynamicMonsterData monster)
    {
        if (string.IsNullOrEmpty(monster.MonsterId)) monster.MonsterId = $"mob_{Guid.NewGuid().ToString("N")[..8]}";
        _monsters[monster.MonsterId] = monster;
        return true;
    }

    public bool DeleteMonster(string monsterId) => _monsters.TryRemove(monsterId, out _);

    public List<DynamicNpcData> GetAllNpcs() => _npcs.Values.ToList();

    public bool SaveNpc(DynamicNpcData npc)
    {
        if (string.IsNullOrEmpty(npc.NpcId)) npc.NpcId = $"npc_{Guid.NewGuid().ToString("N")[..8]}";
        _npcs[npc.NpcId] = npc;
        return true;
    }

    public bool DeleteNpc(string npcId) => _npcs.TryRemove(npcId, out _);

    public List<DynamicMapData> GetAllMaps() => _maps.Values.ToList();

    public bool SaveMap(DynamicMapData map)
    {
        _maps[map.ZoneId] = map;
        return true;
    }

    public bool DeleteMap(int zoneId) => _maps.TryRemove(zoneId, out _);

    public List<DynamicClassDefinition> GetAllClasses() => _classes.Values.ToList();

    public bool SaveClass(DynamicClassDefinition classDef)
    {
        if (string.IsNullOrEmpty(classDef.ClassId)) classDef.ClassId = $"class_{Guid.NewGuid().ToString("N")[..8]}";
        _classes[classDef.ClassId] = classDef;
        return true;
    }

    public List<DynamicSkillDefinition> GetAllSkills() => _skills.Values.ToList();

    public bool SaveSkill(DynamicSkillDefinition skillDef)
    {
        if (string.IsNullOrEmpty(skillDef.SkillId)) skillDef.SkillId = $"skill_{Guid.NewGuid().ToString("N")[..8]}";
        _skills[skillDef.SkillId] = skillDef;
        return true;
    }
}
