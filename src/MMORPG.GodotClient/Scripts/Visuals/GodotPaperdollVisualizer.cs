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

    // Footstep Vector Offsets for each of the 6 walk frames (X, Y, Z displacement)
    private static readonly Vector3[] FootStepOffsets = new Vector3[]
    {
        new Vector3(0.00f, 0.00f, 0.00f),   // Frame 0: Resting Stand
        new Vector3(-0.03f, 0.03f, 0.03f),  // Frame 1: Left Foot Forward Step
        new Vector3(-0.05f, 0.06f, 0.05f),  // Frame 2: Left Foot Peak Lift Step
        new Vector3(0.00f, 0.01f, 0.00f),   // Frame 3: Mid-stride Neutral
        new Vector3(0.03f, 0.03f, -0.03f),  // Frame 4: Right Foot Forward Step
        new Vector3(0.05f, 0.06f, -0.05f)   // Frame 5: Right Foot Peak Lift Step
    };

    // Preloaded Base Body Animation Textures: Direction -> FrameIndex -> Texture
    private readonly Dictionary<string, Texture2D[]> _baseWalkTextures = new();
    private readonly Dictionary<string, Texture2D> _baseIdleTextures = new();

    // Isolated Equipment Textures Cache: Slot_ItemId_Direction -> Texture2D
    private readonly Dictionary<string, Texture2D> _isolatedEquipCache = new();

    public override void _Ready()
    {
        // Extract open-source LPC paperdoll equipment layers on startup
        LpcPaperdollEngine.ExtractLpcEquipmentSheets();

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
                SyncEquipmentAnimationFrames();
            }
        }
        else
        {
            _currentWalkFrame = 0;
            _frameTimer = 0.0f;
            UpdateBaseBodyFrame();
            SyncEquipmentAnimationFrames();
        }
    }

    public void SetMovingState(bool isMoving)
    {
        if (_isMoving == isMoving) return;
        _isMoving = isMoving;
        _currentWalkFrame = 0;
        _frameTimer = 0.0f;
        UpdateBaseBodyFrame();
        SyncEquipmentAnimationFrames();
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

        baseSprite.Visible = true;

        if (_isMoving && _baseWalkTextures.TryGetValue(_currentDirection, out var frames) && frames[_currentWalkFrame] != null)
        {
            baseSprite.Texture = frames[_currentWalkFrame];
        }
        else if (_baseIdleTextures.TryGetValue(_currentDirection, out var idleTex) && idleTex != null)
        {
            baseSprite.Texture = idleTex;
        }
    }

    private void SyncEquipmentAnimationFrames()
    {
        Vector3 stepOffset = _isMoving ? FootStepOffsets[_currentWalkFrame] : Vector3.Zero;

        foreach (var (slot, itemId) in _equippedItems)
        {
            string layerKey = slot.ToString();
            if (!_layers.TryGetValue(layerKey, out var layerSprite) || !layerSprite.Visible) continue;

            if (slot == EquipmentSlot.Boots || slot == EquipmentSlot.Legs)
            {
                layerSprite.Position = new Vector3(stepOffset.X, 1.3f + stepOffset.Y, stepOffset.Z);
            }
            else
            {
                float torsoBobY = _isMoving ? Mathf.Abs(Mathf.Sin(_currentWalkFrame * 1.05f)) * 0.04f : 0f;
                layerSprite.Position = new Vector3(0f, 1.3f + torsoBobY, 0f);
            }
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

            // 1. Try loading extracted LPC transparent layer first
            string lpcPath = $"res://Assets/Textures/Paperdoll/LPC/{slot}/{itemId}/{_currentDirection}.png";
            Texture2D lpcTex = GD.Load<Texture2D>(lpcPath);

            if (lpcTex != null)
            {
                layerSprite.Texture = lpcTex;
                layerSprite.Visible = true;
                continue;
            }

            // 2. Fallback to registry path
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

            layerSprite.Visible = false;
        }

        SyncEquipmentAnimationFrames();
    }
}
