using Godot;

public partial class GodotBossVisualizer : Node3D
{
    [Export] public string BossName = "Inferno Dragon Ignis";
    [Export] public int CurrentHp = 100000;
    [Export] public int MaxHp = 100000;

    private MeshInstance3D _bossMesh = null!;
    private MeshInstance3D _dangerCircleMesh = null!;

    public override void _Ready()
    {
        GlobalPosition = new Vector3(12f, 0f, -12f);

        // Build Boss Mesh (Dragon Avatar)
        _bossMesh = new MeshInstance3D();
        _bossMesh.Mesh = new BoxMesh { Size = new Vector3(3.0f, 4.0f, 3.0f) };
        _bossMesh.Position = new Vector3(0f, 2.0f, 0f);

        StandardMaterial3D mat = new StandardMaterial3D { AlbedoColor = new Color(0.9f, 0.1f, 0.1f) };
        _bossMesh.MaterialOverride = mat;
        AddChild(_bossMesh);

        // Create Translucent Danger Ground Circle (Telegraph)
        _dangerCircleMesh = new MeshInstance3D();
        _dangerCircleMesh.Mesh = new QuadMesh { Size = new Vector2(8f, 8f) };
        _dangerCircleMesh.RotationDegrees = new Vector3(-90f, 0f, 0f);
        _dangerCircleMesh.Position = new Vector3(0f, 0.05f, 0f);

        StandardMaterial3D dangerMat = new StandardMaterial3D
        {
            AlbedoColor = new Color(1f, 0f, 0f, 0.35f),
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha
        };
        _dangerCircleMesh.MaterialOverride = dangerMat;
        AddChild(_dangerCircleMesh);
    }
}
