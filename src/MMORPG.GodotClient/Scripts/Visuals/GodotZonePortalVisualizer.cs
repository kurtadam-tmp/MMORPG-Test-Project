using Godot;

public partial class GodotZonePortalVisualizer : Node3D
{
    [Export] public string DestinationName = "Shadowfen Swamps";
    [Export] public int TargetZoneId = 2;
    [Export] public int RequiredLevel = 10;

    private Sprite3D _portalSprite = null!;

    public override void _Ready()
    {
        GlobalPosition = new Vector3(0f, 0f, -8f);

        // Build Portal Sprite3D with PixelLab Transparent PNG
        _portalSprite = new Sprite3D();
        _portalSprite.Texture = GD.Load<Texture2D>("res://Assets/Textures/pixellab_portal.png");
        _portalSprite.PixelSize = 0.025f;
        _portalSprite.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
        _portalSprite.Position = new Vector3(0f, 2.2f, 0f);
        AddChild(_portalSprite);

        // Overhead Portal Label
        Label3D portalTag = new Label3D();
        portalTag.Text = "🌀 Zone Portal: Shadowfen Swamps (Lvl 10+)";
        portalTag.Position = new Vector3(0f, 4.5f, 0f);
        portalTag.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
        portalTag.Modulate = new Color(0f, 0.9f, 1f);
        portalTag.FontSize = 26;
        AddChild(portalTag);
    }
}
