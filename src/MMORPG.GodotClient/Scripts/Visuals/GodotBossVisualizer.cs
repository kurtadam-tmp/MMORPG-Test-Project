using Godot;
using System.Collections.Generic;

public partial class GodotBossVisualizer : Node3D
{
    [Export] public string BossName = "Inferno Dragon Ignis";
    [Export] public int CurrentHp = 100000;
    [Export] public int MaxHp = 100000;
    [Export] public float MoveSpeed = 3.5f;

    private Sprite3D _bossSprite = null!;
    private MeshInstance3D _dangerCircleMesh = null!;
    private Node3D? _targetPlayer = null;

    private Dictionary<string, Texture2D> _directionalTextures = new();
    private string _currentDirection = "south";

    public override void _Ready()
    {
        GlobalPosition = new Vector3(12f, 0f, -12f);

        // Load 4-Directional PixelLab Dragon PNGs
        LoadDirectionalTextures();

        // Build Boss Sprite3D with PixelLab 180x180px Transparent Dragon PNG
        _bossSprite = new Sprite3D();
        _bossSprite.Texture = _directionalTextures.GetValueOrDefault("south");
        _bossSprite.PixelSize = 0.035f;
        _bossSprite.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
        _bossSprite.Position = new Vector3(0f, 3.2f, 0f);
        AddChild(_bossSprite);

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
        bossTag.Position = new Vector3(0f, 6.5f, 0f);
        bossTag.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
        bossTag.Modulate = new Color(1f, 0.25f, 0.1f);
        bossTag.FontSize = 28;
        AddChild(bossTag);
    }

    private void LoadDirectionalTextures()
    {
        string baseDir = "res://Assets/Textures/DragonBoss/";
        string[] dirs = new string[] { "south", "east", "north", "west" };

        foreach (string d in dirs)
        {
            Texture2D tex = GD.Load<Texture2D>($"{baseDir}{d}.png");
            if (tex != null) _directionalTextures[d] = tex;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // Locate target player if not cached
        if (_targetPlayer == null || !_targetPlayer.IsInsideTree())
        {
            _targetPlayer = GetParent()?.GetNodeOrNull<GodotPlayerVisualizer>("PlayerCharacter");
        }

        if (_targetPlayer != null)
        {
            Vector3 diff = _targetPlayer.GlobalPosition - GlobalPosition;
            diff.Y = 0; // Keep on ground plane

            float dist = diff.Length();
            if (dist > 3.0f) // Chase player until close combat range
            {
                Vector3 moveDir = diff.Normalized();
                GlobalPosition += moveDir * MoveSpeed * (float)delta;
                UpdateSpriteDirection(moveDir);
            }
        }
    }

    private void UpdateSpriteDirection(Vector3 moveDir)
    {
        string newDir = "south";

        // Map movement vector to 4 cardinal directions (south, north, east, west)
        if (Mathf.Abs(moveDir.Z) >= Mathf.Abs(moveDir.X))
        {
            newDir = moveDir.Z > 0 ? "south" : "north";
        }
        else
        {
            newDir = moveDir.X > 0 ? "east" : "west";
        }

        if (newDir != _currentDirection && _directionalTextures.ContainsKey(newDir))
        {
            _currentDirection = newDir;
            _bossSprite.Texture = _directionalTextures[newDir];
        }
    }
}
