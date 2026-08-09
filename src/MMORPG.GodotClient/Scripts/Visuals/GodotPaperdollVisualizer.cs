using Godot;
using System.Collections.Generic;
using MMORPG.Shared.Enums;
using MMORPG.Shared.Registry;

public partial class GodotPaperdollVisualizer : Node3D
{
    // 10 Modular Paperdoll Sprite3D Layers ordered by Z-Depth
    private readonly Dictionary<string, Sprite3D> _layers = new();

    // Equipment State Storage: Slot -> ItemId
    private readonly Dictionary<EquipmentSlot, string> _equippedItems = new()
    {
        [EquipmentSlot.Head] = "None",
        [EquipmentSlot.Chest] = "None",
        [EquipmentSlot.Legs] = "None",
        [EquipmentSlot.Boots] = "None",
        [EquipmentSlot.MainHand] = "IronSword",
        [EquipmentSlot.OffHand] = "None"
    };

    private string _currentDirection = "south";

    public override void _Ready()
    {
        // Initialize 10 Modular Layers with PixelSize offsets for clean Z-sorting
        CreateLayerNode("Shadow", 0.0210f);
        CreateLayerNode("BaseBody", 0.0220f);
        CreateLayerNode("Boots", 0.0222f);
        CreateLayerNode("Legs", 0.0224f);
        CreateLayerNode("Chest", 0.0226f);
        CreateLayerNode("Cape", 0.0228f);
        CreateLayerNode("Head", 0.0230f);
        CreateLayerNode("MainHand", 0.0232f);
        CreateLayerNode("OffHand", 0.0234f);

        // Load Base Body Texture
        Texture2D baseTex = GD.Load<Texture2D>("res://Assets/Textures/Warrior/south.png");
        if (_layers.TryGetValue("BaseBody", out var baseSprite) && baseTex != null)
        {
            baseSprite.Texture = baseTex;
        }

        RefreshAllEquipmentLayers();
    }

    private Sprite3D CreateLayerNode(string layerKey, float pixelSize)
    {
        Sprite3D sprite = new Sprite3D();
        sprite.Name = $"PaperdollLayer_{layerKey}";
        sprite.PixelSize = pixelSize;
        sprite.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
        sprite.Position = new Vector3(0f, 1.3f, 0f);
        AddChild(sprite);
        _layers[layerKey] = sprite;
        return sprite;
    }

    public void EquipItem(EquipmentSlot slot, string itemId)
    {
        _equippedItems[slot] = itemId;
        RefreshAllEquipmentLayers();
        GD.Print($"[Modular Paperdoll System] Equipped '{itemId}' into '{slot}' slot!");
    }

    public void UnequipItem(EquipmentSlot slot)
    {
        _equippedItems[slot] = "None";
        RefreshAllEquipmentLayers();
        GD.Print($"[Modular Paperdoll System] Unequipped item from '{slot}' slot!");
    }

    public void UpdateDirection(string direction)
    {
        if (_currentDirection == direction) return;
        _currentDirection = direction;
        RefreshAllEquipmentLayers();
    }

    private void RefreshAllEquipmentLayers()
    {
        // 1. Update Base Body Sprite for active direction
        Texture2D baseTex = GD.Load<Texture2D>($"res://Assets/Textures/Warrior/{_currentDirection}.png");
        if (_layers.TryGetValue("BaseBody", out var baseSprite) && baseTex != null)
        {
            baseSprite.Texture = baseTex;
        }

        // 2. Refresh Modular Layers matching equipped items
        foreach (var (slot, itemId) in _equippedItems)
        {
            string layerKey = slot.ToString();
            if (!_layers.TryGetValue(layerKey, out var layerSprite)) continue;

            if (itemId == "None" || string.IsNullOrWhiteSpace(itemId))
            {
                layerSprite.Visible = false;
                continue;
            }

            PaperdollLayerInfo? info = PaperdollRegistry.GetLayerInfo(itemId);
            if (info != null && !string.IsNullOrWhiteSpace(info.TextureResourcePattern))
            {
                string resPath = info.TextureResourcePattern.Replace("{dir}", _currentDirection);
                Texture2D equipTex = GD.Load<Texture2D>(resPath);
                if (equipTex != null)
                {
                    layerSprite.Texture = equipTex;
                    layerSprite.Visible = true;
                    continue;
                }
            }

            // Fallback: Keep layer visible if valid equipment
            layerSprite.Visible = true;
        }
    }
}
