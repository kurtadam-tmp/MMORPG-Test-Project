using System;
using System.Collections.Generic;
using UnityEngine;

public class MMORPGMasterUIManager : MonoBehaviour
{
    public static MMORPGMasterUIManager Instance { get; private set; }

    [Header("Window Toggle States")]
    public bool ShowInventory = false;
    public bool ShowCharacterStats = false;
    public bool ShowEnhancementAnvil = false;
    public bool ShowMinimap = true;

    // Chat State
    private string _chatInputText = "";
    private Vector2 _chatScrollPos = Vector2.zero;
    private readonly List<string> _chatMessages = new List<string>
    {
        "<color=#00f2fe>[GLOBAL] [Thorin]</b>: Whisperwood Glen bölgesine hoş geldiniz!</color>",
        "<color=#ffd700>[ZONE] [System]</b>: World Boss 'Inferno Dragon Ignis' [X: 12, Z: 12] konumunda belirdi!</color>"
    };

    // Inventory & Enhancement State
    private InventoryItemSlot _selectedEnhanceItem;
    private int _enhanceLevel = 5;
    private bool _useProtectionScroll = true;
    private string _forgeStatusMessage = "Hazır. 'Yükselt' butonuna basın.";

    // Character Stat State
    private int _str = 25, _agi = 18, _int = 15, _vit = 30;
    private int _unallocatedPoints = 5;

    // Target Context Menu State
    private bool _showContextMenu = false;
    private Vector2 _contextMenuPos;
    private string _targetName = "Forest Goblin";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // Global Keyboard Hotkeys
        if (Input.GetKeyDown(KeyCode.I)) ShowInventory = !ShowInventory;
        if (Input.GetKeyDown(KeyCode.C)) ShowCharacterStats = !ShowCharacterStats;
        if (Input.GetKeyDown(KeyCode.E)) ShowEnhancementAnvil = !ShowEnhancementAnvil;
        if (Input.GetKeyDown(KeyCode.M)) ShowMinimap = !ShowMinimap;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowInventory = false;
            ShowCharacterStats = false;
            ShowEnhancementAnvil = false;
            _showContextMenu = false;
        }

        // Right-Click Context Menu Trigger
        if (Input.GetMouseButtonDown(1))
        {
            _showContextMenu = true;
            _contextMenuPos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            _showContextMenu = false;
        }
    }

    private void OnGUI()
    {
        // Set Custom Dark Glassmorphic GUI Skin Style
        GUI.skin.box.normal.background = Texture2D.whiteTexture;
        GUI.skin.window.normal.background = Texture2D.whiteTexture;

        // 1. Top-Left Player Vitals Box
        DrawPlayerVitalsHUD();

        // 2. Top-Right Minimap Box
        if (ShowMinimap) DrawMinimapGUI();

        // 3. Bottom-Left Chat Box
        DrawChatGUI();

        // 4. Bottom-Center Action Hotbar
        DrawHotbarGUI();

        // 5. Inventory Window (I key)
        if (ShowInventory)
        {
            GUI.Window(101, new Rect(Screen.width - 420, 140, 400, 360), DrawInventoryWindow, "🎒 Envanter (Inventory) - [Kısayol: I]");
        }

        // 6. Character Stats & Paperdoll Window (C key)
        if (ShowCharacterStats)
        {
            GUI.Window(102, new Rect(Screen.width - 840, 140, 400, 380), DrawCharacterStatsWindow, "👤 Ekipman & Statlar - [Kısayol: C]");
        }

        // 7. Demirci Örsü (+9 Basma Window) (E key)
        if (ShowEnhancementAnvil)
        {
            GUI.Window(103, new Rect(Screen.width / 2 - 200, Screen.height / 2 - 200, 400, 320), DrawEnhancementWindow, "🔨 Demirci Örsü (+9 İtem Basma) - [Kısayol: E]");
        }

        // 8. Target Context Menu (Right-Click)
        if (_showContextMenu)
        {
            GUI.Window(104, new Rect(_contextMenuPos.x, _contextMenuPos.y, 180, 180), DrawContextMenuWindow, $"🎯 {_targetName}");
        }
    }

    private void DrawPlayerVitalsHUD()
    {
        GUI.Box(new Rect(20, 20, 280, 100), "");
        GUI.color = Color.cyan;
        GUI.Label(new Rect(30, 25, 260, 25), "<b>Thorin (Lvl 60 Warrior)</b>");
        GUI.color = Color.white;

        // HP Bar
        GUI.color = new Color(0.1f, 0.1f, 0.1f);
        GUI.Box(new Rect(30, 50, 260, 20), "");
        GUI.color = new Color(0.9f, 0.2f, 0.2f);
        GUI.Box(new Rect(30, 50, 260 * 0.85f, 20), "");
        GUI.color = Color.white;
        GUI.Label(new Rect(35, 50, 250, 20), "<b>HP: 8,500 / 10,000</b>");

        // MP Bar
        GUI.color = new Color(0.1f, 0.1f, 0.1f);
        GUI.Box(new Rect(30, 75, 260, 20), "");
        GUI.color = new Color(0.1f, 0.6f, 1.0f);
        GUI.Box(new Rect(30, 75, 260 * 0.90f, 20), "");
        GUI.color = Color.white;
        GUI.Label(new Rect(35, 75, 250, 20), "<b>MP: 2,700 / 3,000</b>");
    }

    private void DrawMinimapGUI()
    {
        GUI.Box(new Rect(Screen.width - 220, 20, 200, 180), "");
        GUI.color = Color.gold;
        GUI.Label(new Rect(Screen.width - 210, 25, 180, 25), "<b>🗺️ Whisperwood Glen</b>");
        GUI.color = Color.white;
        GUI.Label(new Rect(Screen.width - 210, 50, 180, 25), "Zone #1 - [PvP Open]");

        // Radar Box
        GUI.Box(new Rect(Screen.width - 200, 75, 160, 90), "🛰️ MINIMAP");
        GUI.Label(new Rect(Screen.width - 200, 145, 160, 25), "X: 12.4 | Z: 45.8");
    }

    private void DrawChatGUI()
    {
        GUI.Box(new Rect(20, Screen.height - 240, 420, 220), "");
        _chatScrollPos = GUI.BeginScrollView(new Rect(25, Screen.height - 235, 410, 160), _chatScrollPos, new Rect(0, 0, 390, _chatMessages.Count * 25));
        for (int i = 0; i < _chatMessages.Count; i++)
        {
            GUI.Label(new Rect(0, i * 25, 390, 25), _chatMessages[i]);
        }
        GUI.EndScrollView();

        // Chat Input Box
        _chatInputText = GUI.TextField(new Rect(25, Screen.height - 65, 310, 25), _chatInputText);
        if (GUI.Button(new Rect(340, Screen.height - 65, 95, 25), "Gönder") || (Event.current.isKey && Event.current.keyCode == KeyCode.Return && !string.IsNullOrEmpty(_chatInputText)))
        {
            if (!string.IsNullOrWhiteSpace(_chatInputText))
            {
                _chatMessages.Add($"<color=#00f2fe>[GLOBAL] [YOU]: {_chatInputText}</color>");
                _chatInputText = "";
            }
        }
    }

    private void DrawHotbarGUI()
    {
        float startX = Screen.width / 2 - 130;
        float startY = Screen.height - 70;
        GUI.Box(new Rect(startX - 10, startY - 10, 280, 60), "");

        string[] skills = new string[] { "[1] Slash", "[2] Fireball", "[3] Heal", "[4] Shield" };
        for (int i = 0; i < 4; i++)
        {
            if (GUI.Button(new Rect(startX + (i * 65), startY, 60, 40), skills[i]))
            {
                MMORPGNativeClient.Instance?.SendUseSkill(i + 1);
                _chatMessages.Add($"<color=#ffd700>[ACTION] Skill #{i + 1} ({skills[i]}) kullanıldı!</color>");
            }
        }
    }

    private void DrawInventoryWindow(int windowID)
    {
        GUI.Label(new Rect(20, 30, 360, 25), "<b>Altın Miktarı:</b> <color=gold>1,250,000 Gold</color>");

        string[] items = new string[] { "Iron Sword +5", "Recruit Leather Chest +3", "Health Potion (x15)", "Mana Elixir (x10)", "Godslayer Greatsword" };
        for (int i = 0; i < items.Length; i++)
        {
            GUI.Box(new Rect(20, 60 + (i * 50), 240, 40), items[i]);
            if (GUI.Button(new Rect(270, 65 + (i * 50), 50, 30), "Kuşan"))
            {
                _chatMessages.Add($"<color=#00e676>[EQUIP] Kuşanıldı: {items[i]}</color>");
            }
            if (GUI.Button(new Rect(325, 65 + (i * 50), 55, 30), "Bas"))
            {
                ShowEnhancementAnvil = true;
                _forgeStatusMessage = $"Seçilen Eşya: {items[i]}";
            }
        }
    }

    private void DrawCharacterStatsWindow(int windowID)
    {
        GUI.Label(new Rect(20, 30, 360, 25), $"<b>Dağıtılacak Stat Puanı:</b> <color=yellow>{_unallocatedPoints}</color>");

        GUI.Label(new Rect(20, 65, 200, 25), $"STR (Güç): {_str}");
        if (GUI.Button(new Rect(230, 65, 30, 25), "+") && _unallocatedPoints > 0) { _str++; _unallocatedPoints--; }

        GUI.Label(new Rect(20, 95, 200, 25), $"AGI (Çeviklik): {_agi}");
        if (GUI.Button(new Rect(230, 95, 30, 25), "+") && _unallocatedPoints > 0) { _agi++; _unallocatedPoints--; }

        GUI.Label(new Rect(20, 125, 200, 25), $"INT (Zeka): {_int}");
        if (GUI.Button(new Rect(230, 125, 30, 25), "+") && _unallocatedPoints > 0) { _int++; _unallocatedPoints--; }

        GUI.Label(new Rect(20, 155, 200, 25), $"VIT (Canlılık): {_vit}");
        if (GUI.Button(new Rect(230, 155, 30, 25), "+") && _unallocatedPoints > 0) { _vit++; _unallocatedPoints--; }

        GUI.Box(new Rect(20, 200, 360, 150), "");
        GUI.Label(new Rect(30, 210, 340, 25), $"⚔️ Fiziksel Atak Gücü: {100 + (_str * 4)}");
        GUI.Label(new Rect(30, 235, 340, 25), $"✨ Büyü Gücü: {80 + (_int * 5)}");
        GUI.Label(new Rect(30, 260, 340, 25), $"🛡️ Zırh (Armor): {50 + (_vit * 2)}");
        GUI.Label(new Rect(30, 285, 340, 25), $"🎯 Kritik Şansı: %{5.0f + (_agi * 0.4f):F1}");
    }

    private void DrawEnhancementWindow(int windowID)
    {
        GUI.Label(new Rect(20, 30, 360, 25), "<b>Seçilen Eşya:</b> Iron Sword +5");
        GUI.Label(new Rect(20, 60, 360, 25), $"<b>Geliştirme:</b> +{_enhanceLevel} ➜ +{_enhanceLevel + 1}");
        GUI.Label(new Rect(20, 90, 360, 25), "<b>Başarı Şansı:</b> <color=yellow>%35</color>");

        _useProtectionScroll = GUI.Toggle(new Rect(20, 125, 360, 25), _useProtectionScroll, " Kutsal Koruma Parşömeni Kullan (Yanmayı Önler)");

        GUI.Label(new Rect(20, 160, 360, 40), _forgeStatusMessage);

        if (GUI.Button(new Rect(100, 220, 200, 45), "🔨 ÖRSÜ VUR (YÜKSELT)"))
        {
            float roll = UnityEngine.Random.Range(0f, 1f);
            if (roll <= 0.35f)
            {
                _enhanceLevel++;
                _forgeStatusMessage = $"<color=green>✨ TEBRİKLER! Eşya +{_enhanceLevel} oldu!</color>";
                _chatMessages.Add($"<color=gold>✨ TEBRİKLER! Iron Sword +{_enhanceLevel} basıldı!</color>");
            }
            else
            {
                _forgeStatusMessage = _useProtectionScroll 
                    ? "<color=yellow>⚠️ Yükseltme Başarısız! Kutsal Parşömen korudu.</color>"
                    : "<color=red>💥 KRİTİK BAŞARISIZLIK! Eşya geriledi.</color>";
            }
        }
    }

    private void DrawContextMenuWindow(int windowID)
    {
        if (GUI.Button(new Rect(10, 30, 160, 25), "⚔️ 1v1 Düello İste")) _chatMessages.Add($"<color=red>⚔️ {_targetName} oyuncusuna Düello teklif edildi!</color>");
        if (GUI.Button(new Rect(10, 60, 160, 25), "🤝 Ticaret Teklif Et")) _chatMessages.Add($"<color=cyan>🤝 {_targetName} oyuncusuna Ticaret İsteği gönderildi.</color>");
        if (GUI.Button(new Rect(10, 90, 160, 25), "🛡️ Partiye Davet Et")) _chatMessages.Add($"<color=green>🛡️ {_targetName} oyuncusu Partiye Davet Edildi.</color>");
        if (GUI.Button(new Rect(10, 120, 160, 25), "🔍 Ekipman İncele")) _chatMessages.Add($"<color=gold>🔍 {_targetName} Ekipmanı inceleniyor.</color>");
        if (GUI.Button(new Rect(10, 150, 160, 25), "✉️ Fısılda")) _chatMessages.Add($"<color=pink>✉️ /w {_targetName} yazarak fısıldayabilirsiniz.</color>");
    }

    public void ToggleInventory() => ShowInventory = !ShowInventory;
    public void ToggleCharacterStats() => ShowCharacterStats = !ShowCharacterStats;
    public void ToggleEnhancementAnvil() => ShowEnhancementAnvil = !ShowEnhancementAnvil;
    public void ToggleMinimap() => ShowMinimap = !ShowMinimap;
    public void CloseAllModalWindows()
    {
        ShowInventory = false;
        ShowCharacterStats = false;
        ShowEnhancementAnvil = false;
        _showContextMenu = false;
    }
}
