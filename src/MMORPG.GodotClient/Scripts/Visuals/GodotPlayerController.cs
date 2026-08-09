using Godot;
using MMORPG.Shared.Enums;

public partial class GodotPlayerController : CharacterBody3D
{
    public static GodotPlayerController Instance { get; private set; } = null!;

    [Export] public float MoveSpeed = 6.0f;

    private TreeOfSaviorPaperdollEngine _paperdoll = null!;
    private Camera3D _camera = null!;
    private Sprite3D _paperdollSprite3D = null!;
    private SubViewport _paperdollSubViewport = null!;

    public override void _Ready()
    {
        Instance = this;

        // Build Viewport 2D to 3D Paperdoll Renderer
        _paperdollSubViewport = new SubViewport
        {
            Name = "PaperdollViewport",
            Size = new Vector2I(256, 256),
            TransparentBg = true,
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always
        };
        AddChild(_paperdollSubViewport);

        _paperdoll = new TreeOfSaviorPaperdollEngine { Name = "ToSPaperdollEngine" };
        _paperdollSubViewport.AddChild(_paperdoll);

        _paperdollSprite3D = new Sprite3D
        {
            Name = "PaperdollSprite3D",
            PixelSize = 0.022f,
            Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
            Position = new Vector3(0f, 1.30f, 0f),
            Texture = _paperdollSubViewport.GetTexture()
        };
        AddChild(_paperdollSprite3D);

        _camera = GetViewport()?.GetCamera3D()!;
        GD.Print("[GodotPlayerController] 8-Directional Player Controller initialized!");
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 inputDir = Vector3.Zero;

        if (Input.IsKeyPressed(Key.W) || Input.IsKeyPressed(Key.Up)) inputDir.Z -= 1f;
        if (Input.IsKeyPressed(Key.S) || Input.IsKeyPressed(Key.Down)) inputDir.Z += 1f;
        if (Input.IsKeyPressed(Key.A) || Input.IsKeyPressed(Key.Left)) inputDir.X -= 1f;
        if (Input.IsKeyPressed(Key.D) || Input.IsKeyPressed(Key.Right)) inputDir.X += 1f;

        bool isMoving = inputDir != Vector3.Zero;
        _paperdoll?.SetMovingState(isMoving);

        if (isMoving)
        {
            inputDir = inputDir.Normalized();
            Velocity = inputDir * MoveSpeed;
            MoveAndSlide();

            Update8WayDirection(inputDir);
        }
        else
        {
            Velocity = Vector3.Zero;
        }

        // Camera Follow
        if (_camera != null)
        {
            Vector3 targetCamPos = GlobalPosition + new Vector3(0f, 8f, 12f);
            _camera.GlobalPosition = _camera.GlobalPosition.Lerp(targetCamPos, 0.15f);
            _camera.LookAt(GlobalPosition, Vector3.Up);
        }
    }

    private void Update8WayDirection(Vector3 moveDir)
    {
        float angleDeg = Mathf.RadToDeg(Mathf.Atan2(moveDir.X, moveDir.Z));
        if (angleDeg < 0) angleDeg += 360f;

        string newDir = "south";

        if (angleDeg >= 337.5f || angleDeg < 22.5f) newDir = "south";
        else if (angleDeg >= 22.5f && angleDeg < 67.5f) newDir = "south-east";
        else if (angleDeg >= 67.5f && angleDeg < 112.5f) newDir = "east";
        else if (angleDeg >= 112.5f && angleDeg < 157.5f) newDir = "north-east";
        else if (angleDeg >= 157.5f && angleDeg < 202.5f) newDir = "north";
        else if (angleDeg >= 202.5f && angleDeg < 247.5f) newDir = "north-west";
        else if (angleDeg >= 247.5f && angleDeg < 292.5f) newDir = "west";
        else if (angleDeg >= 292.5f && angleDeg < 337.5f) newDir = "south-west";

        _paperdoll?.UpdateDirection(newDir);
    }

    public void EquipItem(EquipmentSlot slot, string itemId)
    {
        _paperdoll?.EquipItem(slot, itemId);
    }
}
