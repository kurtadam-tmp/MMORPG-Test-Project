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
    private bool _isMoving = false;

    // Frame-by-Frame Walk Cycle Animation
    private float _frameTimer = 0.0f;
    private int _currentWalkFrame = 0;
    private const int TotalWalkFrames = 6;
    private const float FrameRate = 0.10f; // 10 FPS walk cycle animation

    // Preloaded Base Body Animation Textures: Direction -> FrameIndex -> Texture
    private readonly Dictionary<string, Texture2D[]> _baseWalkTextures = new();
    private readonly Dictionary<string, Texture2D> _baseIdleTextures = new();

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

        LoadBaseBodyTextures();
        RefreshAllEquipmentLayers();
    }

    private void LoadBaseBodyTextures()
    {
        string[] dirs = new string[] { "south", "south-east", "east", "north-east", "north", "north-west", "west", "south-west" };

        foreach (string d in dirs)
        {
            // Idle Texture
            Texture2D idleTex = GD.Load<Texture2D>($"res://Assets/Textures/BaseBody/Idle/{d}.png");
            if (idleTex != null) _baseIdleTextures[d] = idleTex;

            // 6-Frame Walk Cycle Textures
            Texture2D[] walkFrames = new Texture2D[TotalWalkFrames];
            for (int f = 0; f < TotalWalkFrames; f++)
            {
                Texture2D frameTex = GD.Load<Texture2D>($"res://Assets/Textures/BaseBody/Walking/{d}/frame_00{f}.png");
                if (frameTex != null) walkFrames[f] = frameTex;
            }
            _baseWalkTextures[d] = walkFrames;
        }
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

    public override void _PhysicsProcess(double delta)
    {
        if (_isMoving)
        {
            _frameTimer += (float)delta;
            if (_frameTimer >= FrameRate)
            {
                _frameTimer = 0.0f;
                _currentWalkFrame = (_currentWalkFrame + 1) % TotalWalkFrames;
                UpdateBaseBodyFrame();
            }
        }
        else
        {
            _currentWalkFrame = 0;
            _frameTimer = 0.0f;
            UpdateBaseBodyFrame();
        }
    }

    public void SetMovingState(bool isMoving)
    {
        if (_isMoving == isMoving) return;
        _isMoving = isMoving;
        _currentWalkFrame = 0;
        _frameTimer = 0.0f;
        UpdateBaseBodyFrame();
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

    private void UpdateBaseBodyFrame()
    {
        if (!_layers.TryGetValue("BaseBody", out var baseSprite)) return;

        if (_isMoving && _baseWalkTextures.TryGetValue(_currentDirection, out var frames) && frames[_currentWalkFrame] != null)
        {
            baseSprite.Texture = frames[_currentWalkFrame];
        }
        else if (_baseIdleTextures.TryGetValue(_currentDirection, out var idleTex) && idleTex != null)
        {
            baseSprite.Texture = idleTex;
        }
    }

    private void RefreshAllEquipmentLayers()
    {
        UpdateBaseBodyFrame();

        // Refresh Modular Equipment Overlay Layers
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

            layerSprite.Visible = true;
        }
    }
}
