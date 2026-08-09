using MMORPG.Shared.Enums;

namespace MMORPG.Shared.DTOs;

public class EquipmentItemDto
{
    public string ItemId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public EquipmentSlot Slot { get; set; }
    public int RequiredLevel { get; set; }
    public int BonusStr { get; set; }
    public int BonusAgi { get; set; }
    public int BonusInt { get; set; }
    public int BonusVit { get; set; }
    public int AttackPower { get; set; }
    public int DefensePower { get; set; }
    public string SpriteResourcePath { get; set; } = string.Empty;
}
