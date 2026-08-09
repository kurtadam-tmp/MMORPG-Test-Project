using Godot;
using System.Collections.Generic;

public partial class GodotPlayerVisualizer : Node3D
{
    [Export] public float MoveSpeed = 8.0f;
    [Export] public string CharacterClass = "Warrior";

    private Camera3D _camera = null!;
    private Sprite3D _heroSprite = null!;
    private MeshInstance3D _auraMesh = null!;

    private Dictionary<string, Texture2D> _directionalTextures = new();
    private string _currentDirection = "south";

    private bool _isRightClickDragging = false;
    private float _cameraYawOffset = 0.0f; // Range: -15 to +15 degrees

    public override void _Ready()
    {
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

        // 2. Load 8-Directional PixelLab Transparent PNG Textures
        LoadDirectionalTextures();

        // 3. Build Hero Sprite3D
        _heroSprite = new Sprite3D();
        _heroSprite.Texture = _directionalTextures.GetValueOrDefault("south");
        _heroSprite.PixelSize = 0.022f;
        _heroSprite.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
        _heroSprite.Position = new Vector3(0f, 1.3f, 0f);
        AddChild(_heroSprite);

        CreateFeetAuraCircle();
        CreateOverheadNameTag();
    }

    private void LoadDirectionalTextures()
    {
        string baseDir = "res://Assets/Textures/Warrior/";
        string[] dirs = new string[] { "south", "south-east", "east", "north-east", "north", "north-west", "west", "south-west" };

        foreach (string d in dirs)
        {
            Texture2D tex = GD.Load<Texture2D>($"{baseDir}{d}.png");
            if (tex != null) _directionalTextures[d] = tex;
        }
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
        // Mouse Right-Click Drag for Camera Yaw Rotation (-15° to +15°)
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

        if (moveDir != Vector3.Zero)
        {
            moveDir = moveDir.Normalized();
            GlobalPosition += moveDir * MoveSpeed * (float)delta;
            UpdateSpriteDirection(moveDir);
        }

        UpdateCameraTransform();
    }

    private void UpdateCameraTransform()
    {
        if (_camera == null) return;

        // Apply ±15 degree Yaw Offset around Player
        float yawRad = Mathf.DegToRad(_cameraYawOffset);
        Vector3 baseOffset = new Vector3(0f, 12f, 14f);
        Vector3 rotatedOffset = baseOffset.Rotated(Vector3.Up, yawRad);

        Vector3 targetPos = GlobalPosition + rotatedOffset;
        _camera.GlobalPosition = _camera.GlobalPosition.Lerp(targetPos, 0.2f);
        _camera.LookAt(GlobalPosition, Vector3.Up);
    }

    private void UpdateSpriteDirection(Vector3 moveDir)
    {
        string newDir = "south";

        if (moveDir.Z > 0.3f && Mathf.Abs(moveDir.X) < 0.3f) newDir = "south";
        else if (moveDir.Z < -0.3f && Mathf.Abs(moveDir.X) < 0.3f) newDir = "north";
        else if (moveDir.X > 0.3f && Mathf.Abs(moveDir.Z) < 0.3f) newDir = "east";
        else if (moveDir.X < -0.3f && Mathf.Abs(moveDir.Z) < 0.3f) newDir = "west";
        else if (moveDir.Z > 0f && moveDir.X > 0f) newDir = "south-east";
        else if (moveDir.Z > 0f && moveDir.X < 0f) newDir = "south-west";
        else if (moveDir.Z < 0f && moveDir.X > 0f) newDir = "north-east";
        else if (moveDir.Z < 0f && moveDir.X < 0f) newDir = "north-west";

        if (newDir != _currentDirection && _directionalTextures.ContainsKey(newDir))
        {
            _currentDirection = newDir;
            _heroSprite.Texture = _directionalTextures[newDir];
        }
    }
}
