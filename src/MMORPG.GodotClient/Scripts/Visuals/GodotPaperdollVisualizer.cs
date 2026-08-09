using Godot;
using System.Collections.Generic;
using MMORPG.Shared.Enums;
using MMORPG.Shared.Registry;

public partial class GodotPaperdollVisualizer : Node3D
{
    public static GodotPaperdollVisualizer Instance { get; private set; } = null!;

    // 10 Modular Paperdoll Sprite3D Layers ordered by Z-Depth
    private readonly Dictionary<string, Sprite3D> _layers = new();

    // Equipment State Storage: Slot -> ItemId (Default: Completely Naked)
    private readonly Dictionary<EquipmentSlot, string> _equippedItems = new()
    {
        [EquipmentSlot.Head] = "None",
        [EquipmentSlot.Chest] = "None",
        [EquipmentSlot.Legs] = "None",
        [EquipmentSlot.Boots] = "None",
        [EquipmentSlot.MainHand] = "None",
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

    // Preloaded Equipment Animation Textures: Slot -> Direction -> FrameIndex -> Texture
    private readonly Dictionary<EquipmentSlot, Dictionary<string, Texture2D[]>> _equipWalkTextures = new();
    private readonly Dictionary<EquipmentSlot, Dictionary<string, Texture2D>> _equipIdleTextures = new();

    private static readonly string[] Directions = new string[] { "south", "south-east", "east", "north-east", "north", "north-west", "west", "south-west" };

    public override void _Ready()
    {
        Instance = this;

        // Initialize 10 Modular Layers with PixelSize offsets for clean Z-sorting
        // Native 64x64px LPC Engine: All layers share exact same center pivot (Position Y = 1.30f)
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
        foreach (string d in Directions)
        {
            Texture2D idleTex = GD.Load<Texture2D>($"res://Assets/Textures/BaseBody/Idle/{d}.png");
            if (idleTex != null) _baseIdleTextures[d] = idleTex;

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
        sprite.Position = new Vector3(0f, 1.30f, 0f);
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
                UpdateAllFrameTextures();
            }
        }
        else
        {
            _currentWalkFrame = 0;
            _frameTimer = 0.0f;
            UpdateAllFrameTextures();
        }
    }

    public void SetMovingState(bool isMoving)
    {
        if (_isMoving == isMoving) return;
        _isMoving = isMoving;
        _currentWalkFrame = 0;
        _frameTimer = 0.0f;
        UpdateAllFrameTextures();
    }

    public void EquipItem(EquipmentSlot slot, string itemId)
    {
        _equippedItems[slot] = itemId;
        PreloadEquippedItemTextures(slot, itemId);
        RefreshAllEquipmentLayers();
        GD.Print($"[Animated Paperdoll Engine] Equipped '{itemId}' into '{slot}' slot!");
    }

    public void UnequipItem(EquipmentSlot slot)
    {
        _equippedItems[slot] = "None";
        _equipIdleTextures.Remove(slot);
        _equipWalkTextures.Remove(slot);
        RefreshAllEquipmentLayers();
        GD.Print($"[Animated Paperdoll Engine] Unequipped item from '{slot}' slot!");
    }

    public void UpdateDirection(string direction)
    {
        if (_currentDirection == direction) return;
        _currentDirection = direction;
        UpdateAllFrameTextures();
    }

    private void PreloadEquippedItemTextures(EquipmentSlot slot, string itemId)
    {
        if (itemId == "None" || string.IsNullOrWhiteSpace(itemId))
        {
            _equipIdleTextures.Remove(slot);
            _equipWalkTextures.Remove(slot);
            return;
        }

        string slotName = slot.ToString();
        if (slot == EquipmentSlot.Chest) slotName = "Armor";
        if (slot == EquipmentSlot.MainHand) slotName = "Weapons";

        var idleMap = new Dictionary<string, Texture2D>();
        var walkMap = new Dictionary<string, Texture2D[]>();

        foreach (string d in Directions)
        {
            Texture2D idleTex = GD.Load<Texture2D>($"res://Assets/Textures/Paperdoll/{slotName}/{itemId}/Idle/{d}.png");
            if (idleTex != null) idleMap[d] = idleTex;

            Texture2D[] walkFrames = new Texture2D[TotalWalkFrames];
            for (int f = 0; f < TotalWalkFrames; f++)
            {
                Texture2D frameTex = GD.Load<Texture2D>($"res://Assets/Textures/Paperdoll/{slotName}/{itemId}/Walking/{d}/frame_00{f}.png");
                if (frameTex != null) walkFrames[f] = frameTex;
            }
            walkMap[d] = walkFrames;
        }

        _equipIdleTextures[slot] = idleMap;
        _equipWalkTextures[slot] = walkMap;
    }

    private void UpdateAllFrameTextures()
    {
        // 1. Update Base Body Frame
        if (_layers.TryGetValue("BaseBody", out var baseSprite))
        {
            baseSprite.Visible = true;
            if (_isMoving && _baseWalkTextures.TryGetValue(_currentDirection, out var bodyWalk) && bodyWalk[_currentWalkFrame] != null)
            {
                baseSprite.Texture = bodyWalk[_currentWalkFrame];
            }
            else if (_baseIdleTextures.TryGetValue(_currentDirection, out var bodyIdle) && bodyIdle != null)
            {
                baseSprite.Texture = bodyIdle;
            }
        }

        // 2. Update Equipment Frames in 1-to-1 Lockstep Frame Sync
        foreach (var (slot, itemId) in _equippedItems)
        {
            string layerKey = slot.ToString();
            if (!_layers.TryGetValue(layerKey, out var layerSprite)) continue;

            if (itemId == "None" || string.IsNullOrWhiteSpace(itemId))
            {
                layerSprite.Visible = false;
                continue;
            }

            if (_isMoving && _equipWalkTextures.TryGetValue(slot, out var walkMap) && walkMap.TryGetValue(_currentDirection, out var frames) && frames[_currentWalkFrame] != null)
            {
                layerSprite.Texture = frames[_currentWalkFrame];
                layerSprite.Visible = true;
            }
            else if (_equipIdleTextures.TryGetValue(slot, out var idleMap) && idleMap.TryGetValue(_currentDirection, out var idleTex) && idleTex != null)
            {
                layerSprite.Texture = idleTex;
                layerSprite.Visible = true;
            }
            else
            {
                layerSprite.Visible = false;
            }
        }
    }

    private void RefreshAllEquipmentLayers()
    {
        foreach (var (slot, itemId) in _equippedItems)
        {
            if (itemId != "None" && !_equipWalkTextures.ContainsKey(slot))
            {
                PreloadEquippedItemTextures(slot, itemId);
            }
        }

        UpdateAllFrameTextures();
    }
}
