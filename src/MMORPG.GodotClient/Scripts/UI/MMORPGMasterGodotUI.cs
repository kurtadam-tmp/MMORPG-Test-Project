using Godot;
using MMORPG.Shared.Enums;

public partial class MMORPGMasterGodotUI : CanvasLayer
{
    public static MMORPGMasterGodotUI Instance { get; private set; } = null!;

    private Panel _inventoryPanel = null!;
    private Panel _statsPanel = null!;
    private Panel _anvilPanel = null!;
    private RichTextLabel _chatLabel = null!;

    public override void _Ready()
    {
        Instance = this;
        BuildMasterUI();
        GD.Print("[MMORPG UI] Master Game Overlay UI loaded!");
    }

    private void BuildMasterUI()
    {
        // 1. Top Left Player Status Card
        Panel statusCard = new Panel { Size = new Vector2(320, 100), Position = new Vector2(20, 20) };
        AddChild(statusCard);

        Label nameLbl = new Label { Text = "⚔️ Hero Warrior - Lv. 50", Position = new Vector2(15, 10), Modulate = Color.FromHtml("#ffd700") };
        statusCard.AddChild(nameLbl);

        ProgressBar hpBar = new ProgressBar { Size = new Vector2(290, 20), Position = new Vector2(15, 40), Value = 100, ShowPercentage = true };
        hpBar.Modulate = Color.FromHtml("#ff4444");
        statusCard.AddChild(hpBar);

        ProgressBar mpBar = new ProgressBar { Size = new Vector2(290, 20), Position = new Vector2(15, 68), Value = 100, ShowPercentage = true };
        mpBar.Modulate = Color.FromHtml("#00b0ff");
        statusCard.AddChild(mpBar);

        // 2. Bottom Left Action Log / Chat Panel
        Panel chatPanel = new Panel { Size = new Vector2(450, 180), Position = new Vector2(20, 1080 - 200) };
        AddChild(chatPanel);

        _chatLabel = new RichTextLabel { Size = new Vector2(430, 160), Position = new Vector2(10, 10), BbcodeEnabled = true };
        _chatLabel.Text = "[color=#00e676][SYSTEM][/color] Tree of Savior 2.5D Engine Başlatıldı!\n[color=#ffeb3b][CONTROL][/color] WASD: Yürüme | I: Envanter | C: Statlar | U: Demirci";
        chatPanel.AddChild(_chatLabel);

        // 3. Inventory Panel (I)
        _inventoryPanel = new Panel { Size = new Vector2(380, 280), Position = new Vector2(1920 - 420, 20), Visible = false };
        AddChild(_inventoryPanel);

        Label invTitle = new Label { Text = "🎒 Envanter (Inventory) - [Kısayol: I]", Position = new Vector2(15, 15), Modulate = Color.FromHtml("#00f2fe") };
        _inventoryPanel.AddChild(invTitle);

        string[] items = new string[] { "ScaleMailChest" };
        EquipmentSlot[] itemSlots = new EquipmentSlot[] { EquipmentSlot.Chest };

        for (int i = 0; i < items.Length; i++)
        {
            Label itemLbl = new Label { Text = items[i], Position = new Vector2(20, 60 + (i * 45)) };
            _inventoryPanel.AddChild(itemLbl);

            Button equipBtn = new Button { Text = "Kuşan", Size = new Vector2(80, 32), Position = new Vector2(240, 55 + (i * 45)) };
            int capturedIdx = i;
            equipBtn.Pressed += () =>
            {
                EquipmentSlot slot = itemSlots[capturedIdx];
                string itemName = items[capturedIdx];
                _chatLabel.Text += $"\n[color=#00e676][EQUIP][/color] Kuşanıldı: {itemName}";
                GodotPlayerController.Instance?.EquipItem(slot, itemName);
            };
            _inventoryPanel.AddChild(equipBtn);
        }

        // 4. Stats Panel (C)
        _statsPanel = new Panel { Size = new Vector2(360, 240), Position = new Vector2(1920 - 420, 320), Visible = false };
        AddChild(_statsPanel);
        Label statsTitle = new Label { Text = "👤 Modüler Paperdoll Statlar - [Kısayol: C]", Position = new Vector2(15, 15), Modulate = Color.FromHtml("#ffd700") };
        _statsPanel.AddChild(statsTitle);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            if (keyEvent.Keycode == Key.I)
            {
                _inventoryPanel.Visible = !_inventoryPanel.Visible;
            }
            else if (keyEvent.Keycode == Key.C)
            {
                _statsPanel.Visible = !_statsPanel.Visible;
            }
        }
    }
}
