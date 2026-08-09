using Godot;
using System.Collections.Generic;

public partial class GodotPaperdollVisualizer : Node3D
{
    // Modular Paperdoll Layer Node References
    private Sprite3D _baseBodySprite = null!;
    private Sprite3D _headEquipmentSprite = null!;
    private Sprite3D _chestEquipmentSprite = null!;
    private Sprite3D _mainHandEquipmentSprite = null!;
    private Sprite3D _offHandEquipmentSprite = null!;

    // Current Equipped Item Identifiers
    public string EquippedHead { get; private set; } = "None";
    public string EquippedChest { get; private set; } = "None";
    public string EquippedMainHand { get; private set; } = "IronSword";
    public string EquippedOffHand { get; private set; } = "None";

    private string _currentDirection = "south";

    public override void _Ready()
    {
        // 1. Base Body Layer (Naked Character Base Sprite)
        _baseBodySprite = CreatePaperdollLayerNode("BaseBodySprite", 0.022f);
        _baseBodySprite.Texture = GD.Load<Texture2D>("res://Assets/Textures/Warrior/south.png");

        // 2. Chest Armor Overlay Layer
        _chestEquipmentSprite = CreatePaperdollLayerNode("ChestEquipmentSprite", 0.0225f);

        // 3. Head / Helmet Overlay Layer
        _headEquipmentSprite = CreatePaperdollLayerNode("HeadEquipmentSprite", 0.0230f);

        // 4. MainHand Weapon Overlay Layer
        _mainHandEquipmentSprite = CreatePaperdollLayerNode("MainHandEquipmentSprite", 0.0235f);

        // 5. OffHand Shield Overlay Layer
        _offHandEquipmentSprite = CreatePaperdollLayerNode("OffHandEquipmentSprite", 0.0240f);

        RefreshEquipmentVisuals();
    }

    private Sprite3D CreatePaperdollLayerNode(string nodeName, float pixelSize)
    {
        Sprite3D sprite = new Sprite3D();
        sprite.Name = nodeName;
        sprite.PixelSize = pixelSize;
        sprite.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
        sprite.Position = new Vector3(0f, 1.3f, 0f);
        AddChild(sprite);
        return sprite;
    }

    public void EquipItem(string slot, string itemId)
    {
        switch (slot.ToLowerInvariant())
        {
            case "head": EquippedHead = itemId; break;
            case "chest": EquippedChest = itemId; break;
            case "mainhand": EquippedMainHand = itemId; break;
            case "offhand": EquippedOffHand = itemId; break;
        }

        RefreshEquipmentVisuals();
        GD.Print($"[Paperdoll] Equipped {itemId} in {slot} slot!");
    }

    public void UnequipItem(string slot)
    {
        switch (slot.ToLowerInvariant())
        {
            case "head": EquippedHead = "None"; break;
            case "chest": EquippedChest = "None"; break;
            case "mainhand": EquippedMainHand = "None"; break;
            case "offhand": EquippedOffHand = "None"; break;
        }

        RefreshEquipmentVisuals();
        GD.Print($"[Paperdoll] Unequipped item from {slot} slot!");
    }

    public void UpdateDirection(string direction)
    {
        if (_currentDirection == direction) return;
        _currentDirection = direction;
        RefreshEquipmentVisuals();
    }

    private void RefreshEquipmentVisuals()
    {
        // 1. Update Base Body Sprite for current direction
        Texture2D baseTex = GD.Load<Texture2D>($"res://Assets/Textures/Warrior/{_currentDirection}.png");
        if (baseTex != null) _baseBodySprite.Texture = baseTex;

        // 2. Head Slot Overlay
        _headEquipmentSprite.Visible = EquippedHead != "None";

        // 3. Chest Slot Overlay
        _chestEquipmentSprite.Visible = EquippedChest != "None";

        // 4. MainHand Slot Overlay
        _mainHandEquipmentSprite.Visible = EquippedMainHand != "None";

        // 5. OffHand Slot Overlay
        _offHandEquipmentSprite.Visible = EquippedOffHand != "None";
    }
}
