using Godot;

public partial class GodotZonePortalVisualizer : Node3D
{
    [Export] public string DestinationName = "Shadowfen Swamps";
    [Export] public int TargetZoneId = 2;
    [Export] public int RequiredLevel = 10;

    private MeshInstance3D _portalRingMesh = null!;

    public override void _Ready()
    {
        GlobalPosition = new Vector3(0f, 0f, -8f);

        // Build Portal Ring Mesh
        _portalRingMesh = new MeshInstance3D();
        _portalRingMesh.Mesh = new QuadMesh { Size = new Vector2(3f, 3f) };
        _portalRingMesh.RotationDegrees = new Vector3(-90f, 0f, 0f);
        _portalRingMesh.Position = new Vector3(0f, 0.05f, 0f);

        StandardMaterial3D mat = new StandardMaterial3D
        {
            AlbedoColor = new Color(0f, 0.9f, 1f, 0.7f),
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha
        };
        _portalRingMesh.MaterialOverride = mat;
        AddChild(_portalRingMesh);
    }
}
