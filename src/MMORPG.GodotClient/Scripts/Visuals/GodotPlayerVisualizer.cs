using Godot;
using System.Collections.Generic;
using MMORPG.Shared.Enums;

public partial class GodotPlayerVisualizer : Node3D
{
    public static GodotPlayerVisualizer Instance { get; private set; } = null!;

    [Export] public float MoveSpeed = 8.0f;
    [Export] public string CharacterClass = "Warrior";

    private Camera3D _camera = null!;
    private GodotPaperdollVisualizer _paperdoll = null!;
    private MeshInstance3D _auraMesh = null!;

    private bool _isRightClickDragging = false;
    private float _cameraYawOffset = 0.0f; // Range: -15 to +15 degrees

    // Walking Bobbing Parameters
    private float _walkAnimTimer = 0.0f;
    private const float WalkBobSpeed = 14.0f;
    private const float WalkBobHeight = 0.08f;

    public override void _Ready()
    {
        Instance = this;

        // 1. Setup Camera3D
        _camera = GetViewport()?.GetCamera3D()!;
        if (_camera == null)
        {
            _camera = new Camera3D();
            _camera.Name = "Godot2.5DCamera";
            GetParent()?.AddChild(_camera);
        }

        _camera.Fov = 60.0f;
        _camera.Current = true;
        UpdateCameraTransform();

        // 2. Attach Modular Paperdoll Visualizer
        _paperdoll = new GodotPaperdollVisualizer();
        _paperdoll.Name = "PaperdollSystem";
        AddChild(_paperdoll);

        CreateFeetAuraCircle();
        CreateOverheadNameTag();
    }

    public void EquipPaperdollItem(EquipmentSlot slot, string itemId)
    {
        _paperdoll?.EquipItem(slot, itemId);
    }

    public void UnequipPaperdollItem(EquipmentSlot slot)
    {
        _paperdoll?.UnequipItem(slot);
    }

    private void CreateFeetAuraCircle()
    {
        _auraMesh = new MeshInstance3D();
        _auraMesh.Mesh = new QuadMesh { Size = new Vector2(2.2f, 2.2f) };
        _auraMesh.RotationDegrees = new Vector3(-90f, 0f, 0f);
        _auraMesh.Position = new Vector3(0f, 0.05f, 0f);

        StandardMaterial3D mat = new StandardMaterial3D 
        { 
            AlbedoColor = new Color(0f, 0.95f, 1f, 0.6f),
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha
        };
        _auraMesh.MaterialOverride = mat;
        AddChild(_auraMesh);
    }

    private void CreateOverheadNameTag()
    {
        Label3D nameTag = new Label3D();
        nameTag.Text = "Thorin [Lvl 60 Warrior]";
        nameTag.Position = new Vector3(0f, 2.8f, 0f);
        nameTag.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
        nameTag.Modulate = new Color(0f, 0.95f, 1f);
        nameTag.FontSize = 26;
        AddChild(nameTag);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseBtn && mouseBtn.ButtonIndex == MouseButton.Right)
        {
            _isRightClickDragging = mouseBtn.Pressed;
        }
        else if (@event is InputEventMouseMotion mouseMotion && _isRightClickDragging)
        {
            _cameraYawOffset += mouseMotion.Relative.X * 0.15f;
            _cameraYawOffset = Mathf.Clamp(_cameraYawOffset, -15.0f, 15.0f);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 moveDir = Vector3.Zero;

        if (Input.IsKeyPressed(Key.W) || Input.IsKeyPressed(Key.Up)) moveDir.Z -= 1f;
        if (Input.IsKeyPressed(Key.S) || Input.IsKeyPressed(Key.Down)) moveDir.Z += 1f;
        if (Input.IsKeyPressed(Key.A) || Input.IsKeyPressed(Key.Left)) moveDir.X -= 1f;
        if (Input.IsKeyPressed(Key.D) || Input.IsKeyPressed(Key.Right)) moveDir.X += 1f;

        bool isMoving = moveDir != Vector3.Zero;
        _paperdoll?.SetMovingState(isMoving);

        if (isMoving)
        {
            moveDir = moveDir.Normalized();
            GlobalPosition += moveDir * MoveSpeed * (float)delta;
            UpdateDirection(moveDir);

            // Subtle Procedural Bobbing
            _walkAnimTimer += (float)delta * WalkBobSpeed;
            float bobY = Mathf.Abs(Mathf.Sin(_walkAnimTimer)) * WalkBobHeight;

            if (_paperdoll != null)
            {
                _paperdoll.Position = new Vector3(0f, bobY, 0f);
            }
        }
        else
        {
            _walkAnimTimer = 0.0f;
            if (_paperdoll != null)
            {
                _paperdoll.Position = _paperdoll.Position.Lerp(Vector3.Zero, 0.2f);
            }
        }

        UpdateCameraTransform();
    }

    private void UpdateCameraTransform()
    {
        if (_camera == null) return;

        float yawRad = Mathf.DegToRad(_cameraYawOffset);
        Vector3 baseOffset = new Vector3(0f, 12f, 14f);
        Vector3 rotatedOffset = baseOffset.Rotated(Vector3.Up, yawRad);

        Vector3 targetPos = GlobalPosition + rotatedOffset;
        _camera.GlobalPosition = _camera.GlobalPosition.Lerp(targetPos, 0.2f);
        _camera.LookAt(GlobalPosition, Vector3.Up);
    }

    private void UpdateDirection(Vector3 moveDir)
    {
        if (moveDir.LengthSquared() < 0.01f) return;

        // 360-Degree 8-Sector Direction Calculation (45 degrees per sector)
        float angleDeg = Mathf.RadToDeg(Mathf.Atan2(moveDir.X, moveDir.Z));
        if (angleDeg < 0) angleDeg += 360f;

        string newDir = "south";

        if (angleDeg >= 337.5f || angleDeg < 22.5f) newDir = "south";
        else if (angleDeg >= 22.5f && angleDeg < 67.5f) newDir = "south-east";
        else if (angleDeg >= 67.5f && angleDeg < 112.5f) newDir = "east";
        else if (angleDeg >= 112.5f && angleDeg < 157.5f) newDir = "north-east";
        else if (angleDeg >= 157.5f && angleDeg < 202.5f) newDir = "north";
        else if (angleDeg >= 202.5f && angleDeg < 247.5f) newDir = "north-west";
        else if (angleDeg >= 247.5f && angleDeg < 292.5f) newDir = "west";
        else if (angleDeg >= 292.5f && angleDeg < 337.5f) newDir = "south-west";

        _paperdoll?.UpdateDirection(newDir);
    }
}
