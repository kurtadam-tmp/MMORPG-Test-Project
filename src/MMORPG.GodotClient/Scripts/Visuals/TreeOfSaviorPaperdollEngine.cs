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

    // 10 Geometric Placeholder Nodes (Key -> CanvasItem / Polygon2D)
    private readonly Dictionary<string, Node2D> _attachmentNodes = new();

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

    public override void _Ready()
    {
        Instance = this;
        Build2DSkeletonTree();
        BuildGeometricPlaceholders();
        RefreshAllEquipmentLayers();
        GD.Print("[Tree of Savior Engine] Procedural Geometric Placeholder Engine ready!");
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

        _legLeftBone = CreateBone("LegLeftBone", new Vector2(-12, 16));
        _legRightBone = CreateBone("LegRightBone", new Vector2(12, 16));
        _hipsBone.AddChild(_legLeftBone);
        _hipsBone.AddChild(_legRightBone);

        _armLeftBone = CreateBone("ArmLeftBone", new Vector2(-22, -10));
        _armRightBone = CreateBone("ArmRightBone", new Vector2(22, -10));
        _chestBone.AddChild(_armLeftBone);
        _chestBone.AddChild(_armRightBone);
    }

    private Bone2D CreateBone(string name, Vector2 localPos)
    {
        return new Bone2D { Name = name, Position = localPos };
    }

    private void BuildGeometricPlaceholders()
    {
        // 1. Geometric Base Body (Head, Torso, Limbs)
        Polygon2D headPoly = CreateCirclePolygon(new Vector2(0, 0), 16, Color.FromHtml("#ffe0bd"));
        headPoly.Name = "BaseBody_Head";
        _headBone.AddChild(headPoly);

        Polygon2D chestPoly = CreateRectPolygon(new Vector2(-16, -16), new Vector2(32, 32), Color.FromHtml("#ffd8b3"));
        chestPoly.Name = "BaseBody_Chest";
        _chestBone.AddChild(chestPoly);

        Polygon2D legLeftPoly = CreateRectPolygon(new Vector2(-5, 0), new Vector2(10, 24), Color.FromHtml("#ffcc99"));
        _legLeftBone.AddChild(legLeftPoly);

        Polygon2D legRightPoly = CreateRectPolygon(new Vector2(-5, 0), new Vector2(10, 24), Color.FromHtml("#ffcc99"));
        _legRightBone.AddChild(legRightPoly);

        Polygon2D armLeftPoly = CreateRectPolygon(new Vector2(-4, 0), new Vector2(8, 20), Color.FromHtml("#ffcc99"));
        _armLeftBone.AddChild(armLeftPoly);

        Polygon2D armRightPoly = CreateRectPolygon(new Vector2(-4, 0), new Vector2(8, 20), Color.FromHtml("#ffcc99"));
        _armRightBone.AddChild(armRightPoly);

        // 2. Equipment Slots Geometric Overlays
        Node2D chestEquipNode = CreateRectPolygon(new Vector2(-18, -18), new Vector2(36, 36), Color.FromHtml("#4682b4"));
        chestEquipNode.Name = "Equip_Chest";
        chestEquipNode.Visible = false;
        _chestBone.AddChild(chestEquipNode);
        _attachmentNodes["Chest"] = chestEquipNode;

        Node2D headEquipNode = CreateCirclePolygon(new Vector2(0, -4), 19, Color.FromHtml("#708090"));
        headEquipNode.Name = "Equip_Head";
        headEquipNode.Visible = false;
        _headBone.AddChild(headEquipNode);
        _attachmentNodes["Head"] = headEquipNode;

        Node2D mainHandEquipNode = CreateRectPolygon(new Vector2(-3, -25), new Vector2(6, 40), Color.FromHtml("#ffd700"));
        mainHandEquipNode.Name = "Equip_MainHand";
        mainHandEquipNode.Visible = false;
        _armRightBone.AddChild(mainHandEquipNode);
        _attachmentNodes["MainHand"] = mainHandEquipNode;

        Node2D offHandEquipNode = CreateRectPolygon(new Vector2(-12, -12), new Vector2(24, 24), Color.FromHtml("#c0c0c0"));
        offHandEquipNode.Name = "Equip_OffHand";
        offHandEquipNode.Visible = false;
        _armLeftBone.AddChild(offHandEquipNode);
        _attachmentNodes["OffHand"] = offHandEquipNode;

        Node2D bootsEquipNodeLeft = CreateRectPolygon(new Vector2(-6, 16), new Vector2(12, 10), Color.FromHtml("#8b4513"));
        bootsEquipNodeLeft.Name = "Equip_Boots";
        bootsEquipNodeLeft.Visible = false;
        _legLeftBone.AddChild(bootsEquipNodeLeft);
        _attachmentNodes["Boots"] = bootsEquipNodeLeft;
    }

    private Polygon2D CreateRectPolygon(Vector2 topLeft, Vector2 size, Color col)
    {
        return new Polygon2D
        {
            Polygon = new Vector2[]
            {
                topLeft,
                new Vector2(topLeft.X + size.X, topLeft.Y),
                new Vector2(topLeft.X + size.X, topLeft.Y + size.Y),
                new Vector2(topLeft.X, topLeft.Y + size.Y)
            },
            Color = col
        };
    }

    private Polygon2D CreateCirclePolygon(Vector2 center, float radius, Color col)
    {
        const int numSegments = 16;
        Vector2[] points = new Vector2[numSegments];
        for (int i = 0; i < numSegments; i++)
        {
            float angle = i * Mathf.Tau / numSegments;
            points[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        return new Polygon2D
        {
            Polygon = points,
            Color = col
        };
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_isMoving)
        {
            _walkTimer += (float)delta * WalkSpeed;

            // 2D Skeletal Procedural Walk Animation (Sine Wave Gait)
            float legSwingLeft = Mathf.Sin(_walkTimer) * 0.45f;
            float legSwingRight = Mathf.Sin(_walkTimer + Mathf.Pi) * 0.45f;
            float armSwingLeft = Mathf.Sin(_walkTimer + Mathf.Pi) * 0.35f;
            float armSwingRight = Mathf.Sin(_walkTimer) * 0.35f;
            float hipsBobY = Mathf.Abs(Mathf.Sin(_walkTimer * 2f)) * 4f;

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
        GD.Print($"[Geometric Paperdoll] Equipped '{itemId}' into '{slot}' slot!");
    }

    public void UnequipItem(EquipmentSlot slot)
    {
        _equippedItems[slot] = "None";
        RefreshAllEquipmentLayers();
        GD.Print($"[Geometric Paperdoll] Unequipped item from '{slot}' slot!");
    }

    public void UpdateDirection(string direction)
    {
        if (_currentDirection == direction) return;
        _currentDirection = direction;
        RefreshAllEquipmentLayers();
    }

    public void RefreshAllEquipmentLayers()
    {
        foreach (var (slot, itemId) in _equippedItems)
        {
            string layerKey = slot.ToString();
            if (!_attachmentNodes.TryGetValue(layerKey, out var layerNode)) continue;

            if (itemId == "None" || string.IsNullOrWhiteSpace(itemId))
            {
                layerNode.Visible = false;
            }
            else
            {
                layerNode.Visible = true;
            }
        }
    }
}
