using Godot;

public partial class GodotPlayerVisualizer : Node3D
{
    [Export] public float MoveSpeed = 8.0f;
    [Export] public string CharacterClass = "Warrior";

    private Camera3D _camera = null!;
    private MeshInstance3D _heroMesh = null!;
    private MeshInstance3D _auraMesh = null!;

    public override void _Ready()
    {
        // 1. Setup 2.5D Isometric 45-Degree Camera3D
        _camera = GetViewport()?.GetCamera3D()!;
        if (_camera == null)
        {
            _camera = new Camera3D();
            _camera.Name = "Godot2.5DCamera";
            GetParent()?.AddChild(_camera);
        }

        _camera.Fov = 60.0f;
        _camera.Current = true; // Make camera active

        // Position camera looking at player
        _camera.GlobalPosition = GlobalPosition + new Vector3(0f, 12f, 14f);
        _camera.LookAt(GlobalPosition, Vector3.Up);

        // 2. Build Hero Visual Mesh (Stylized Cylinder Avatar)
        _heroMesh = new MeshInstance3D();
        _heroMesh.Mesh = new CylinderMesh { TopRadius = 0.6f, BottomRadius = 0.6f, Height = 2.0f };
        _heroMesh.Position = new Vector3(0f, 1.0f, 0f);
        AddChild(_heroMesh);

        ApplyClassColor(CharacterClass);
        CreateFeetAuraCircle();
        CreateOverheadNameTag();
    }

    private void ApplyClassColor(string className)
    {
        Color color = className.ToLowerInvariant() switch
        {
            "warrior" => new Color(0.9f, 0.2f, 0.2f),
            "mage" => new Color(0.1f, 0.6f, 1.0f),
            "rogue" => new Color(0.1f, 0.9f, 0.3f),
            "priest" => new Color(1.0f, 0.9f, 0.5f),
            "paladin" => new Color(1.0f, 0.8f, 0.1f),
            "necromancer" => new Color(0.6f, 0.1f, 0.9f),
            _ => new Color(0.0f, 0.95f, 0.95f)
        };

        StandardMaterial3D mat = new StandardMaterial3D { AlbedoColor = color };
        _heroMesh.MaterialOverride = mat;
    }

    private void CreateFeetAuraCircle()
    {
        _auraMesh = new MeshInstance3D();
        _auraMesh.Mesh = new QuadMesh { Size = new Vector2(2.2f, 2.2f) };
        _auraMesh.RotationDegrees = new Vector3(-90f, 0f, 0f);
        _auraMesh.Position = new Vector3(0f, 0.05f, 0f);

        StandardMaterial3D mat = new StandardMaterial3D 
        { 
            AlbedoColor = new Color(0f, 0.95f, 1f, 0.5f),
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha
        };
        _auraMesh.MaterialOverride = mat;
        AddChild(_auraMesh);
    }

    private void CreateOverheadNameTag()
    {
        Label3D nameTag = new Label3D();
        nameTag.Text = "Thorin [Lvl 60 Warrior]";
        nameTag.Position = new Vector3(0f, 2.5f, 0f);
        nameTag.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
        nameTag.Modulate = new Color(0f, 0.95f, 1f);
        nameTag.FontSize = 24;
        AddChild(nameTag);
    }

    public override void _PhysicsProcess(double delta)
    {
        // Smooth WASD & Arrow Key Movement
        Vector3 moveDir = Vector3.Zero;

        if (Input.IsKeyPressed(Key.W) || Input.IsKeyPressed(Key.Up)) moveDir.Z -= 1f;
        if (Input.IsKeyPressed(Key.S) || Input.IsKeyPressed(Key.Down)) moveDir.Z += 1f;
        if (Input.IsKeyPressed(Key.A) || Input.IsKeyPressed(Key.Left)) moveDir.X -= 1f;
        if (Input.IsKeyPressed(Key.D) || Input.IsKeyPressed(Key.Right)) moveDir.X += 1f;

        if (moveDir != Vector3.Zero)
        {
            moveDir = moveDir.Normalized();
            GlobalPosition += moveDir * MoveSpeed * (float)delta;
        }

        // Smooth Camera Follow
        if (_camera != null)
        {
            Vector3 targetPos = GlobalPosition + new Vector3(0f, 12f, 14f);
            _camera.GlobalPosition = _camera.GlobalPosition.Lerp(targetPos, (float)delta * 8.0f);
            _camera.LookAt(GlobalPosition, Vector3.Up);
        }
    }
}
