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

        // Build Stylized 3D Red Fire Dragon Avatar (No black boxes!)
        _bossMesh = new MeshInstance3D();
        _bossMesh.Mesh = new BoxMesh { Size = new Vector3(3.5f, 4.5f, 3.5f) };
        _bossMesh.Position = new Vector3(0f, 2.25f, 0f);

        StandardMaterial3D bossMat = new StandardMaterial3D 
        { 
            AlbedoColor = new Color(0.95f, 0.15f, 0.05f),
            EmissionEnabled = true,
            Emission = new Color(0.8f, 0.2f, 0.0f),
            EmissionEnergyMultiplier = 0.6f
        };
        _bossMesh.MaterialOverride = bossMat;
        AddChild(_bossMesh);

        // Dragon Horns
        MeshInstance3D hornLeft = new MeshInstance3D
        {
            Mesh = new PrismMesh { Size = new Vector3(0.8f, 1.8f, 0.8f) },
            Position = new Vector3(-1.2f, 4.8f, 0.5f)
        };
        hornLeft.RotationDegrees = new Vector3(20f, 0f, -25f);
        hornLeft.MaterialOverride = new StandardMaterial3D { AlbedoColor = new Color(0.1f, 0.1f, 0.1f) };
        AddChild(hornLeft);

        MeshInstance3D hornRight = new MeshInstance3D
        {
            Mesh = new PrismMesh { Size = new Vector3(0.8f, 1.8f, 0.8f) },
            Position = new Vector3(1.2f, 4.8f, 0.5f)
        };
        hornRight.RotationDegrees = new Vector3(20f, 0f, 25f);
        hornRight.MaterialOverride = new StandardMaterial3D { AlbedoColor = new Color(0.1f, 0.1f, 0.1f) };
        AddChild(hornRight);

        // Translucent Danger Ground Circle (Telegraph)
        _dangerCircleMesh = new MeshInstance3D();
        _dangerCircleMesh.Mesh = new QuadMesh { Size = new Vector2(10f, 10f) };
        _dangerCircleMesh.RotationDegrees = new Vector3(-90f, 0f, 0f);
        _dangerCircleMesh.Position = new Vector3(0f, 0.05f, 0f);

        StandardMaterial3D dangerMat = new StandardMaterial3D
        {
            AlbedoColor = new Color(1f, 0f, 0f, 0.45f),
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha
        };
        _dangerCircleMesh.MaterialOverride = dangerMat;
        AddChild(_dangerCircleMesh);

        // Overhead Boss Health Bar & Name
        Label3D bossTag = new Label3D();
        bossTag.Text = "🔥 Inferno Dragon Ignis [100,000 / 100,000 HP]";
        bossTag.Position = new Vector3(0f, 6.2f, 0f);
        bossTag.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
        bossTag.Modulate = new Color(1f, 0.25f, 0.1f);
        bossTag.FontSize = 28;
        AddChild(bossTag);
    }
}
