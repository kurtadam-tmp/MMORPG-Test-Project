using Godot;
using System.Collections.Generic;

public partial class MMORPGMasterGodotUI : CanvasLayer
{
    public static MMORPGMasterGodotUI Instance { get; private set; } = null!;

    private Panel _vitalsPanel = null!;
    private Panel _minimapPanel = null!;
    private Panel _chatPanel = null!;
    private Panel _hotbarPanel = null!;
    private Panel _inventoryPanel = null!;
    private Panel _statsPanel = null!;
    private Panel _anvilPanel = null!;
    private RichTextLabel _chatLabel = null!;

    private int _str = 25, _agi = 18, _int = 15, _vit = 30;
    private int _unallocatedPoints = 5;
    private int _enhanceLevel = 5;

    public override void _Ready()
    {
        Instance = this;
        BuildGodotUIHierarchy();
    }

    private void BuildGodotUIHierarchy()
    {
        // 1. Top-Left Player Vitals Panel
        _vitalsPanel = new Panel { Size = new Vector2(300, 110), Position = new Vector2(20, 20) };
        AddChild(_vitalsPanel);

        Label titleLabel = new Label 
        { 
            Text = "Thorin (Lvl 60 Warrior)", 
            Position = new Vector2(15, 10),
            Modulate = new Color(0f, 0.95f, 1f)
        };
        _vitalsPanel.AddChild(titleLabel);

        ProgressBar hpBar = new ProgressBar 
        { 
            Value = 85, 
            Size = new Vector2(270, 22), 
            Position = new Vector2(15, 40),
            Modulate = new Color(0.9f, 0.2f, 0.2f)
        };
        _vitalsPanel.AddChild(hpBar);

        ProgressBar mpBar = new ProgressBar 
        { 
            Value = 90, 
            Size = new Vector2(270, 22), 
            Position = new Vector2(15, 70),
            Modulate = new Color(0.1f, 0.6f, 1.0f)
        };
        _vitalsPanel.AddChild(mpBar);

        // 2. Top-Right Minimap Box
        _minimapPanel = new Panel { Size = new Vector2(220, 180), Position = new Vector2(1920 - 240, 20) };
        AddChild(_minimapPanel);

        Label mapTitle = new Label { Text = "🗺️ Whisperwood Glen", Position = new Vector2(15, 15), Modulate = Color.FromHtml("#ffd700") };
        _minimapPanel.AddChild(mapTitle);

        Label mapCoords = new Label { Text = "Zone #1 - [X: 12.4 | Z: 45.8]", Position = new Vector2(15, 45) };
        _minimapPanel.AddChild(mapCoords);

        // 3. Bottom-Left Chat Box
        _chatPanel = new Panel { Size = new Vector2(450, 240), Position = new Vector2(20, 1080 - 260) };
        AddChild(_chatPanel);

        _chatLabel = new RichTextLabel 
        { 
            Size = new Vector2(430, 170), 
            Position = new Vector2(10, 10), 
            BbcodeEnabled = true,
            Text = "[color=#00f2fe][GLOBAL] [Thorin][/color]: Whisperwood Glen bölgesine hoş geldiniz!\n[color=#ffd700][ZONE] [System][/color]: World Boss 'Inferno Dragon Ignis' belirdi!"
        };
        _chatPanel.AddChild(_chatLabel);

        LineEdit chatInput = new LineEdit { Size = new Vector2(330, 35), Position = new Vector2(10, 190), PlaceholderText = "Mesajınızı yazın..." };
        _chatPanel.AddChild(chatInput);

        Button sendBtn = new Button { Text = "Gönder", Size = new Vector2(90, 35), Position = new Vector2(350, 190) };
        sendBtn.Pressed += () =>
        {
            if (!string.IsNullOrWhiteSpace(chatInput.Text))
            {
                _chatLabel.Text += $"\n[color=#00f2fe][GLOBAL] [YOU][/color]: {chatInput.Text}";
                MMORPGGodotClient.Instance?.SendChatMessage(chatInput.Text);
                chatInput.Text = "";
            }
        };
        _chatPanel.AddChild(sendBtn);

        // 4. Bottom-Center Action Hotbar
        _hotbarPanel = new Panel { Size = new Vector2(320, 70), Position = new Vector2(1920 / 2 - 160, 1080 - 90) };
        AddChild(_hotbarPanel);

        string[] skills = new string[] { "[1] Slash", "[2] Fire", "[3] Heal", "[4] Shield" };
        for (int i = 0; i < 4; i++)
        {
            int slotIdx = i + 1;
            Button skillBtn = new Button { Text = skills[i], Size = new Vector2(70, 50), Position = new Vector2(10 + (i * 75), 10) };
            skillBtn.Pressed += () =>
            {
                MMORPGGodotClient.Instance?.SendUseSkill(slotIdx);
                _chatLabel.Text += $"\n[color=#ffd700][ACTION][/color] Skill #{slotIdx} kullanıldı!";
            };
            _hotbarPanel.AddChild(skillBtn);
        }

        // 5. Inventory Panel (I) - Paperdoll Integration
        _inventoryPanel = new Panel { Size = new Vector2(400, 360), Position = new Vector2(1920 - 440, 220), Visible = false };
        AddChild(_inventoryPanel);

        Label invTitle = new Label { Text = "🎒 Envanter (Inventory) - [Kısayol: I]", Position = new Vector2(15, 15), Modulate = Color.FromHtml("#00f2fe") };
        _inventoryPanel.AddChild(invTitle);

        string[] items = new string[] { "Iron Sword +5", "Leather Chest +3", "Iron Helmet +2", "Health Potion (x15)" };
        string[] itemSlots = new string[] { "mainhand", "chest", "head", "none" };

        for (int i = 0; i < items.Length; i++)
        {
            Label itemLbl = new Label { Text = items[i], Position = new Vector2(20, 50 + (i * 45)) };
            _inventoryPanel.AddChild(itemLbl);

            Button equipBtn = new Button { Text = "Kuşan", Size = new Vector2(60, 30), Position = new Vector2(230, 45 + (i * 45)) };
            int capturedIdx = i;
            equipBtn.Pressed += () =>
            {
                string slot = itemSlots[capturedIdx];
                string itemName = items[capturedIdx];
                _chatLabel.Text += $"\n[color=#00e676][EQUIP][/color] Kuşanıldı: {itemName}";
                if (slot != "none")
                {
                    GodotPlayerVisualizer.Instance?.EquipPaperdollItem(slot, itemName);
                    MMORPGGodotClient.Instance?.SendEquipItem(slot, itemName);
                }
            };
            _inventoryPanel.AddChild(equipBtn);

            Button enhanceBtn = new Button { Text = "Bas", Size = new Vector2(50, 30), Position = new Vector2(300, 45 + (i * 45)) };
            enhanceBtn.Pressed += () => _anvilPanel.Visible = true;
            _inventoryPanel.AddChild(enhanceBtn);
        }

        // 6. Character Stats Panel (C)
        _statsPanel = new Panel { Size = new Vector2(400, 380), Position = new Vector2(1920 - 860, 220), Visible = false };
        AddChild(_statsPanel);

        Label statsTitle = new Label { Text = "👤 Ekipman Paperdoll & Statlar - [Kısayol: C]", Position = new Vector2(15, 15), Modulate = Color.FromHtml("#ffd700") };
        _statsPanel.AddChild(statsTitle);

        Label pointsLbl = new Label { Text = $"Boş Stat Puanı: {_unallocatedPoints}", Position = new Vector2(20, 45), Modulate = Color.FromHtml("#ffee55") };
        _statsPanel.AddChild(pointsLbl);

        Label strLbl = new Label { Text = $"STR (Güç): {_str}", Position = new Vector2(20, 80) };
        _statsPanel.AddChild(strLbl);
        Button strBtn = new Button { Text = "+", Size = new Vector2(30, 30), Position = new Vector2(220, 75) };
        strBtn.Pressed += () => { if (_unallocatedPoints > 0) { _str++; _unallocatedPoints--; pointsLbl.Text = $"Boş Stat Puanı: {_unallocatedPoints}"; strLbl.Text = $"STR (Güç): {_str}"; } };
        _statsPanel.AddChild(strBtn);

        // 7. Demirci Örsü Panel (E)
        _anvilPanel = new Panel { Size = new Vector2(400, 300), Position = new Vector2(1920 / 2 - 200, 1080 / 2 - 150), Visible = false };
        AddChild(_anvilPanel);

        Label anvilTitle = new Label { Text = "🔨 Demirci Örsü (+9 İtem Basma) - [Kısayol: E]", Position = new Vector2(15, 15), Modulate = Color.FromHtml("#ff9100") };
        _anvilPanel.AddChild(anvilTitle);

        Label itemText = new Label { Text = "Seçilen Eşya: Iron Sword +5", Position = new Vector2(20, 50) };
        _anvilPanel.AddChild(itemText);

        Label statusLbl = new Label { Text = "Hazır. 'Yükselt' butonuna basın.", Position = new Vector2(20, 90) };
        _anvilPanel.AddChild(statusLbl);

        Button forgeBtn = new Button { Text = "🔨 ÖRSÜ VUR (YÜKSELT)", Size = new Vector2(220, 45), Position = new Vector2(90, 180) };
        forgeBtn.Pressed += () =>
        {
            _enhanceLevel++;
            statusLbl.Text = $"✨ TEBRİKLER! Eşya +{_enhanceLevel} oldu!";
            _chatLabel.Text += $"\n[color=#ffd700]✨ TEBRİKLER! Iron Sword +{_enhanceLevel} basıldı![/color]";
        };
        _anvilPanel.AddChild(forgeBtn);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            if (keyEvent.Keycode == Key.I) _inventoryPanel.Visible = !_inventoryPanel.Visible;
            if (keyEvent.Keycode == Key.C) _statsPanel.Visible = !_statsPanel.Visible;
            if (keyEvent.Keycode == Key.E) _anvilPanel.Visible = !_anvilPanel.Visible;
            if (keyEvent.Keycode == Key.M) _minimapPanel.Visible = !_minimapPanel.Visible;
            if (keyEvent.Keycode == Key.Escape)
            {
                _inventoryPanel.Visible = false;
                _statsPanel.Visible = false;
                _anvilPanel.Visible = false;
            }
        }
    }
}
