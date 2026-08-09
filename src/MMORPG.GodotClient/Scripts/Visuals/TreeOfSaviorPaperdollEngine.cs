using Godot;
using System.Collections.Generic;
using MMORPG.Shared.Enums;
using MMORPG.Shared.Registry;

public partial class TreeOfSaviorPaperdollEngine : Node2D
{
    public static TreeOfSaviorPaperdollEngine Instance { get; private set; } = null!;

    // 2D Skeleton Nodes
    private Skeleton2D _skeleton = null!;
    private Bone2D _hipsBone = null!;
    private Bone2D _chestBone = null!;
    private Bone2D _headBone = null!;
    private Bone2D _legLeftBone = null!;
    private Bone2D _legRightBone = null!;
    private Bone2D _armLeftBone = null!;
    private Bone2D _armRightBone = null!;

    // 10 Attachment Layer Sprites (Key -> Sprite2D)
    private readonly Dictionary<string, Sprite2D> _attachmentSprites = new();

    // Equipment State: Slot -> ItemId (Default: Completely Naked)
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
    private float _walkTimer = 0.0f;
    private const float WalkSpeed = 9.0f;

    // Default Layer Draw Orders for South (Front Facing) vs North (Back Facing)
    private static readonly Dictionary<string, int> SouthDrawOrders = new()
    {
        ["Shadow"]   = 0,
        ["Cape"]     = 1,
        ["BaseBody"] = 2,
        ["Legs"]     = 3,
        ["Boots"]    = 4,
        ["Chest"]    = 5,
        ["Hair"]     = 6,
        ["Headwear"] = 7,
        ["OffHand"]  = 8,
        ["MainHand"] = 9
    };

    private static readonly Dictionary<string, int> NorthDrawOrders = new()
    {
        ["Shadow"]   = 0,
        ["MainHand"] = 1,
        ["OffHand"]  = 2,
        ["Chest"]    = 3,
        ["Boots"]    = 4,
        ["Legs"]     = 5,
        ["BaseBody"] = 6,
        ["Headwear"] = 7,
        ["Hair"]     = 8,
        ["Cape"]     = 9
    };

    public override void _Ready()
    {
        Instance = this;
        Build2DSkeletonTree();
        BuildAttachmentSprites();
        RefreshAllEquipmentLayers();
        GD.Print("[Tree of Savior Engine] 2D Skeletal Paperdoll Engine initialized successfully!");
    }

    private void Build2DSkeletonTree()
    {
        _skeleton = new Skeleton2D { Name = "ToSSkeleton2D" };
        AddChild(_skeleton);

        _hipsBone = CreateBone("HipsBone", new Vector2(0, -32));
        _skeleton.AddChild(_hipsBone);

        _chestBone = CreateBone("ChestBone", new Vector2(0, -20));
        _hipsBone.AddChild(_chestBone);

        _headBone = CreateBone("HeadBone", new Vector2(0, -24));
        _chestBone.AddChild(_headBone);

        _legLeftBone = CreateBone("LegLeftBone", new Vector2(-10, 16));
        _legRightBone = CreateBone("LegRightBone", new Vector2(10, 16));
        _hipsBone.AddChild(_legLeftBone);
        _hipsBone.AddChild(_legRightBone);

        _armLeftBone = CreateBone("ArmLeftBone", new Vector2(-18, -10));
        _armRightBone = CreateBone("ArmRightBone", new Vector2(18, -10));
        _chestBone.AddChild(_armLeftBone);
        _chestBone.AddChild(_armRightBone);
    }

    private Bone2D CreateBone(string name, Vector2 localPos)
    {
        Bone2D bone = new Bone2D { Name = name, Position = localPos };
        return bone;
    }

    private void BuildAttachmentSprites()
    {
        var layers = new[] { "Shadow", "Cape", "BaseBody", "Legs", "Boots", "Chest", "Hair", "Headwear", "OffHand", "MainHand" };

        Shader rawShader = GD.Load<Shader>("res://Assets/Shaders/TreeOfSaviorShader.gdshader");
        ShaderMaterial shaderMaterial = null!;
        if (rawShader != null)
        {
            shaderMaterial = new ShaderMaterial { Shader = rawShader };
        }

        foreach (string layerKey in layers)
        {
            Sprite2D sprite = new Sprite2D
            {
                Name = $"Attachment_{layerKey}",
                Material = shaderMaterial
            };

            // Attach sprites to corresponding bones
            switch (layerKey)
            {
                case "Headwear":
                case "Hair":
                    _headBone.AddChild(sprite);
                    break;
                case "Chest":
                case "Cape":
                    _chestBone.AddChild(sprite);
                    break;
                case "MainHand":
                    _armRightBone.AddChild(sprite);
                    break;
                case "OffHand":
                    _armLeftBone.AddChild(sprite);
                    break;
                default:
                    _hipsBone.AddChild(sprite);
                    break;
            }

            _attachmentSprites[layerKey] = sprite;
        }

        UpdateDrawOrders();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_isMoving)
        {
            _walkTimer += (float)delta * WalkSpeed;

            // 2D Skeletal Procedural Walk Animation (Sine Wave Gait)
            float legSwingLeft = Mathf.Sin(_walkTimer) * 0.35f;
            float legSwingRight = Mathf.Sin(_walkTimer + Mathf.Pi) * 0.35f;
            float armSwingLeft = Mathf.Sin(_walkTimer + Mathf.Pi) * 0.25f;
            float armSwingRight = Mathf.Sin(_walkTimer) * 0.25f;
            float hipsBobY = Mathf.Abs(Mathf.Sin(_walkTimer * 2f)) * 3f;

            _legLeftBone.Rotation = legSwingLeft;
            _legRightBone.Rotation = legSwingRight;
            _armLeftBone.Rotation = armSwingLeft;
            _armRightBone.Rotation = armSwingRight;
            _hipsBone.Position = new Vector2(0, -32 + hipsBobY);
        }
        else
        {
            _walkTimer = 0.0f;
            _legLeftBone.Rotation = Mathf.Lerp(_legLeftBone.Rotation, 0f, 0.2f);
            _legRightBone.Rotation = Mathf.Lerp(_legRightBone.Rotation, 0f, 0.2f);
            _armLeftBone.Rotation = Mathf.Lerp(_armLeftBone.Rotation, 0f, 0.2f);
            _armRightBone.Rotation = Mathf.Lerp(_armRightBone.Rotation, 0f, 0.2f);
            _hipsBone.Position = _hipsBone.Position.Lerp(new Vector2(0, -32), 0.2f);
        }
    }

    public void SetMovingState(bool isMoving)
    {
        _isMoving = isMoving;
    }

    public void EquipItem(EquipmentSlot slot, string itemId)
    {
        _equippedItems[slot] = itemId;
        RefreshAllEquipmentLayers();
        GD.Print($"[Tree of Savior Engine] Equipped '{itemId}' into '{slot}' slot!");
    }

    public void UnequipItem(EquipmentSlot slot)
    {
        _equippedItems[slot] = "None";
        RefreshAllEquipmentLayers();
        GD.Print($"[Tree of Savior Engine] Unequipped item from '{slot}' slot!");
    }

    public void UpdateDirection(string direction)
    {
        if (_currentDirection == direction) return;
        _currentDirection = direction;
        UpdateDrawOrders();
        RefreshAllEquipmentLayers();
    }

    private void UpdateDrawOrders()
    {
        bool isFacingNorth = _currentDirection.Contains("north");
        var orders = isFacingNorth ? NorthDrawOrders : SouthDrawOrders;

        foreach (var (layerKey, sprite) in _attachmentSprites)
        {
            if (orders.TryGetValue(layerKey, out int zOrder))
            {
                sprite.ZIndex = zOrder;
            }
        }
    }

    private void RefreshAllEquipmentLayers()
    {
        // Load Base Body for Current Direction
        if (_attachmentSprites.TryGetValue("BaseBody", out var bodySprite))
        {
            Texture2D bodyTex = GD.Load<Texture2D>($"res://Assets/Textures/BaseBody/Idle/{_currentDirection}.png");
            if (bodyTex != null)
            {
                bodySprite.Texture = bodyTex;
                bodySprite.Visible = true;
            }
        }

        // Refresh Equipment Overlay Attachments
        foreach (var (slot, itemId) in _equippedItems)
        {
            string layerKey = slot.ToString();
            if (!_attachmentSprites.TryGetValue(layerKey, out var layerSprite)) continue;

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

            layerSprite.Visible = false;
        }
    }
}
