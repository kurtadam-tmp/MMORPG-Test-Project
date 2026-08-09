using Godot;

public partial class GodotBossVisualizer : Node3D
{
    [Export] public string BossName = "Inferno Dragon Ignis";
    [Export] public int CurrentHp = 100000;
    [Export] public int MaxHp = 100000;

    private Sprite3D _bossSprite = null!;
    private MeshInstance3D _dangerCircleMesh = null!;

    public override void _Ready()
    {
        GlobalPosition = new Vector3(12f, 0f, -12f);

        // Build Boss Sprite3D (Red Dragon)
        _bossSprite = new Sprite3D();
        _bossSprite.Texture = GD.Load<Texture2D>("res://Assets/Textures/boss_dragon.jpg");
        _bossSprite.PixelSize = 0.015f;
        _bossSprite.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
        _bossSprite.Position = new Vector3(0f, 2.5f, 0f);
        AddChild(_bossSprite);

        // Create Translucent Danger Ground Circle (Telegraph)
        _dangerCircleMesh = new MeshInstance3D();
        _dangerCircleMesh.Mesh = new QuadMesh { Size = new Vector2(9f, 9f) };
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
        bossTag.Position = new Vector3(0f, 5.5f, 0f);
        bossTag.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
        bossTag.Modulate = new Color(1f, 0.2f, 0.2f);
        bossTag.FontSize = 28;
        AddChild(bossTag);
    }
}
