using Godot;

public partial class GodotPlayerVisualizer : Node3D
{
    [Export] public float MoveSpeed = 8.0f;
    [Export] public string CharacterClass = "Warrior";

    private Camera3D _camera = null!;
    private Sprite3D _heroSprite = null!;
    private MeshInstance3D _auraMesh = null!;

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
        _camera.GlobalPosition = GlobalPosition + new Vector3(0f, 12f, 14f);
        _camera.LookAt(GlobalPosition, Vector3.Up);

        // 2. Build Hero Sprite3D (Hand-painted Warrior Avatar)
        _heroSprite = new Sprite3D();
        _heroSprite.Texture = GD.Load<Texture2D>("res://Assets/Textures/hero_warrior.jpg");
        _heroSprite.PixelSize = 0.008f;
        _heroSprite.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
        _heroSprite.Position = new Vector3(0f, 1.2f, 0f);
        AddChild(_heroSprite);

        CreateFeetAuraCircle();
        CreateOverheadNameTag();
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
        nameTag.Position = new Vector3(0f, 2.6f, 0f);
        nameTag.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
        nameTag.Modulate = new Color(0f, 0.95f, 1f);
        nameTag.FontSize = 26;
        AddChild(nameTag);
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
        }

        if (_camera != null)
        {
            Vector3 targetPos = GlobalPosition + new Vector3(0f, 12f, 14f);
            _camera.GlobalPosition = _camera.GlobalPosition.Lerp(targetPos, (float)delta * 8.0f);
            _camera.LookAt(GlobalPosition, Vector3.Up);
        }
    }
}
