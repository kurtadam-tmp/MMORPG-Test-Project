using Godot;

public partial class MMORPGMasterGodotUI : Control
{
    public static MMORPGMasterGodotUI Instance { get; private set; } = null!;

    public bool ShowInventory = false;
    public bool ShowCharacterStats = false;
    public bool ShowEnhancementAnvil = false;
    public bool ShowMinimap = true;

    private int _str = 25, _agi = 18, _int = 15, _vit = 30;
    private int _unallocatedPoints = 5;
    private int _enhanceLevel = 5;

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.I) ShowInventory = !ShowInventory;
            if (keyEvent.Keycode == Key.C) ShowCharacterStats = !ShowCharacterStats;
            if (keyEvent.Keycode == Key.E) ShowEnhancementAnvil = !ShowEnhancementAnvil;
            if (keyEvent.Keycode == Key.M) ShowMinimap = !ShowMinimap;
            if (keyEvent.Keycode == Key.Escape)
            {
                ShowInventory = false;
                ShowCharacterStats = false;
                ShowEnhancementAnvil = false;
            }
        }
    }
}
