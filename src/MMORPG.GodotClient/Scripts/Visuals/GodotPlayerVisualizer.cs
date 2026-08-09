using Godot;

public partial class GodotPlayerVisualizer : Node3D
{
    [Export] public float MoveSpeed = 6.0f;
    [Export] public string CharacterClass = "Warrior";

    private Camera3D _camera = null!;
    private MeshInstance3D _heroMesh = null!;
    private MeshInstance3D _auraMesh = null!;

    public override void _Ready()
    {
        // 1. Setup 2.5D Isometric 45-Degree Camera3D
        _camera = GetViewport().GetCamera3D();
        if (_camera == null)
        {
            _camera = new Camera3D();
            _camera.Name = "Godot2.5DCamera";
            GetParent()?.AddChild(_camera);
        }

        _camera.Projection = Camera3D.ProjectionType.Orthogonal;
        _camera.Size = 12.0f;
        _camera.RotationDegrees = new Vector3(-45f, 45f, 0f);

        // 2. Build Hero Visual Mesh (Stylized Cylinder/Box Avatar)
        _heroMesh = new MeshInstance3D();
        _heroMesh.Mesh = new CylinderMesh { TopRadius = 0.5f, BottomRadius = 0.5f, Height = 1.8f };
        AddChild(_heroMesh);

        ApplyClassColor(CharacterClass);
        CreateFeetAuraCircle();
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
        _auraMesh.Mesh = new QuadMesh { Size = new Vector2(1.8f, 1.8f) };
        _auraMesh.RotationDegrees = new Vector3(-90f, 0f, 0f);
        _auraMesh.Position = new Vector3(0f, 0.05f, 0f);

        StandardMaterial3D mat = new StandardMaterial3D 
        { 
            AlbedoColor = new Color(1f, 1f, 1f, 0.3f),
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha
        };
        _auraMesh.MaterialOverride = mat;
        AddChild(_auraMesh);
    }

    public override void _Process(double delta)
    {
        // Smooth 2.5D Isometric Camera Follow
        if (_camera != null)
        {
            Vector3 targetPos = GlobalPosition + new Vector3(-10f, 14f, 10f);
            _camera.GlobalPosition = _camera.GlobalPosition.Lerp(targetPos, (float)delta * 5.0f);
        }
    }
}
