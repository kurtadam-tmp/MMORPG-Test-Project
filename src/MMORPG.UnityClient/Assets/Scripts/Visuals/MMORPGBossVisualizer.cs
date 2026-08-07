using UnityEngine;

public enum BossPhase
{
    Phase1_Normal = 1,
    Phase2_EnragedLavaShield = 2,
    Phase3_ApocalypseNova = 3
}

public class MMORPGBossVisualizer : MonoBehaviour
{
    [Header("World Boss Settings")]
    public string BossName = "Inferno Dragon Ignis (Raid Boss)";
    public int BossLevel = 60;
    public int MaxHp = 1000000;
    public int CurrentHp = 1000000;
    public Color BossAuraColor = new Color(1f, 0.2f, 0f); // Fire Red

    [Header("Boss Combat Phases")]
    public BossPhase CurrentPhase = BossPhase.Phase1_Normal;
    public GameObject TelegraphCircleAOE;

    [Header("Clean 3D Overhead HP Bar (No Screen Overlay)")]
    public Transform HpBarFillTransform;
    public TextMesh BossTitleTextMesh;

    private Transform _mainCamTransform;
    private Renderer _bossRenderer;

    private void Start()
    {
        if (Camera.main != null)
        {
            _mainCamTransform = Camera.main.transform;
        }

        // Build 2.5D Boss Billboard Mesh Representation (3.0m x 3.0m)
        GameObject bossVisual = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bossVisual.name = "BossMeshVisual";
        bossVisual.transform.SetParent(transform);
        bossVisual.transform.localPosition = new Vector3(0, 2.0f, 0);
        bossVisual.transform.localScale = new Vector3(3.0f, 3.0f, 1f);
        if (_mainCamTransform != null)
        {
            bossVisual.transform.rotation = _mainCamTransform.rotation;
        }

        _bossRenderer = bossVisual.GetComponent<Renderer>();
        if (_bossRenderer != null)
        {
            _bossRenderer.material.color = BossAuraColor;
        }

        // Build Ground Telegraph AOE Danger Circle Indicator
        CreateGroundTelegraphAOE();

        CreateCleanBossBar3D();
    }

    private void CreateGroundTelegraphAOE()
    {
        TelegraphCircleAOE = GameObject.CreatePrimitive(PrimitiveType.Quad);
        TelegraphCircleAOE.name = "BossTelegraphAOECircle";
        TelegraphCircleAOE.transform.SetParent(transform);
        TelegraphCircleAOE.transform.localPosition = new Vector3(0, 0.05f, 0);
        TelegraphCircleAOE.transform.rotation = Quaternion.Euler(90, 0, 0);
        TelegraphCircleAOE.transform.localScale = new Vector3(8.0f, 8.0f, 1f);

        Renderer rend = TelegraphCircleAOE.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = new Color(1.0f, 0.0f, 0.0f, 0.35f); // Danger Translucent Red
        }

        Destroy(TelegraphCircleAOE.GetComponent<Collider>());
        TelegraphCircleAOE.SetActive(false);
    }

    private void Update()
    {
        if (BossTitleTextMesh != null && _mainCamTransform != null)
        {
            BossTitleTextMesh.transform.rotation = _mainCamTransform.rotation;
        }
    }

    public void TakeDamage(int damage)
    {
        CurrentHp = Mathf.Max(0, CurrentHp - damage);

        float fillPct = (float)CurrentHp / MaxHp;
        if (HpBarFillTransform != null)
        {
            HpBarFillTransform.localScale = new Vector3(fillPct * 2.8f, 0.22f, 1f);
            HpBarFillTransform.localPosition = new Vector3(-1.4f * (1f - fillPct), 4.2f, -0.01f);
        }

        // Check Boss Phase Transitions
        CheckPhaseTransitions(fillPct);

        if (BossTitleTextMesh != null)
        {
            BossTitleTextMesh.text = $"[Lvl {BossLevel}] {BossName} <color=orange>[PHASE {(int)CurrentPhase}]</color>\n{FormatHp(CurrentHp)} / {FormatHp(MaxHp)} HP";
        }
    }

    private void CheckPhaseTransitions(float hpPercent)
    {
        if (hpPercent <= 0.20f && CurrentPhase != BossPhase.Phase3_ApocalypseNova)
        {
            CurrentPhase = BossPhase.Phase3_ApocalypseNova;
            if (_bossRenderer != null) _bossRenderer.material.color = new Color(0.9f, 0.0f, 1.0f); // Purple Enraged Nova
            if (TelegraphCircleAOE != null) TelegraphCircleAOE.SetActive(true);
            HUDUIController.Instance?.AppendChatMessage("BOSS ALERT", "🔥 INFERNO DRAGON IGNIS ENTAGED IN PHASE 3 APOCALYPSE NOVA (8m Danger Zone)!");
        }
        else if (hpPercent <= 0.50f && hpPercent > 0.20f && CurrentPhase != BossPhase.Phase2_EnragedLavaShield)
        {
            CurrentPhase = BossPhase.Phase2_EnragedLavaShield;
            if (_bossRenderer != null) _bossRenderer.material.color = new Color(1.0f, 0.4f, 0.0f); // Fiery Orange
            HUDUIController.Instance?.AppendChatMessage("BOSS ALERT", "🛡️ INFERNO DRAGON IGNIS CAST LAVA SHIELD (Phase 2 Started)!");
        }
    }

    private void CreateCleanBossBar3D()
    {
        // 1. Black Background Bar (3.0m x 0.25m)
        GameObject bgObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bgObj.name = "Boss_HP_Background_Quad";
        bgObj.transform.SetParent(transform);
        bgObj.transform.localPosition = new Vector3(0, 4.2f, 0);
        bgObj.transform.localScale = new Vector3(3.0f, 0.25f, 1f);
        if (_mainCamTransform != null) bgObj.transform.rotation = _mainCamTransform.rotation;

        Renderer bgRend = bgObj.GetComponent<Renderer>();
        if (bgRend != null) bgRend.material.color = new Color(0.05f, 0.05f, 0.05f);
        Destroy(bgObj.GetComponent<Collider>());

        // 2. Red HP Fill Bar (2.8m x 0.22m)
        GameObject fillObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        fillObj.name = "Boss_HP_Fill_Quad";
        fillObj.transform.SetParent(transform);
        fillObj.transform.localPosition = new Vector3(0, 4.2f, -0.01f);
        fillObj.transform.localScale = new Vector3(2.8f, 0.22f, 1f);
        if (_mainCamTransform != null) fillObj.transform.rotation = _mainCamTransform.rotation;

        Renderer fillRend = fillObj.GetComponent<Renderer>();
        if (fillRend != null) fillRend.material.color = new Color(0.95f, 0.1f, 0.1f);
        Destroy(fillObj.GetComponent<Collider>());
        HpBarFillTransform = fillObj.transform;

        // 3. Boss Title TextMesh
        GameObject textObj = new GameObject("BossTitleTextMesh");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = new Vector3(0, 4.7f, 0);
        if (_mainCamTransform != null) textObj.transform.rotation = _mainCamTransform.rotation;

        BossTitleTextMesh = textObj.AddComponent<TextMesh>();
        BossTitleTextMesh.fontSize = 24;
        BossTitleTextMesh.characterSize = 0.15f;
        BossTitleTextMesh.alignment = TextAlignment.Center;
        BossTitleTextMesh.anchor = TextAnchor.MiddleCenter;
        BossTitleTextMesh.color = Color.yellow;
        BossTitleTextMesh.text = $"[Lvl {BossLevel}] {BossName} <color=orange>[PHASE 1]</color>\n{FormatHp(CurrentHp)} / {FormatHp(MaxHp)} HP";
    }

    private string FormatHp(int val)
    {
        if (val >= 1000000) return $"{val / 1000000.0f:F1}M";
        if (val >= 1000) return $"{val / 1000.0f:F1}K";
        return val.ToString();
    }
}
