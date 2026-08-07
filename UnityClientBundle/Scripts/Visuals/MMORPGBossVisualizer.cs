using UnityEngine;

public class MMORPGBossVisualizer : MonoBehaviour
{
    [Header("World Boss Settings")]
    public string BossName = "Inferno Dragon Ignis (Raid Boss)";
    public int BossLevel = 50;
    public int MaxHp = 100000;
    public int CurrentHp = 100000;
    public Color BossAuraColor = new Color(1f, 0.2f, 0f); // Fire Red

    [Header("Clean 3D Overhead HP Bar (No Screen Overlay)")]
    public Transform HpBarFillTransform;
    public TextMesh BossTitleTextMesh;

    private Transform _mainCamTransform;

    private void Start()
    {
        if (Camera.main != null)
        {
            _mainCamTransform = Camera.main.transform;
        }

        // Build 2.5D Boss Billboard Mesh Representation (2.5m x 2.5m)
        GameObject bossVisual = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bossVisual.name = "BossMeshVisual";
        bossVisual.transform.SetParent(transform);
        bossVisual.transform.localPosition = new Vector3(0, 1.75f, 0);
        bossVisual.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
        if (_mainCamTransform != null)
        {
            bossVisual.transform.rotation = _mainCamTransform.rotation;
        }

        Renderer rend = bossVisual.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = BossAuraColor;
        }

        CreateCleanBossBar3D();
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
            HpBarFillTransform.localPosition = new Vector3(-1.4f * (1f - fillPct), 3.8f, -0.01f);
        }

        if (BossTitleTextMesh != null)
        {
            BossTitleTextMesh.text = $"[Lvl {BossLevel}] {BossName}\n{FormatHp(CurrentHp)} / {FormatHp(MaxHp)} HP";
        }
    }

    private void CreateCleanBossBar3D()
    {
        // 1. Black Background Bar (3.0m x 0.25m)
        GameObject bgObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bgObj.name = "Boss_HP_Background_Quad";
        bgObj.transform.SetParent(transform);
        bgObj.transform.localPosition = new Vector3(0, 3.8f, 0);
        bgObj.transform.localScale = new Vector3(3.0f, 0.25f, 1f);
        if (_mainCamTransform != null) bgObj.transform.rotation = _mainCamTransform.rotation;

        Renderer bgRend = bgObj.GetComponent<Renderer>();
        if (bgRend != null) bgRend.material.color = new Color(0.05f, 0.05f, 0.05f);
        Destroy(bgObj.GetComponent<Collider>());

        // 2. Red HP Fill Bar (2.8m x 0.22m)
        GameObject fillObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        fillObj.name = "Boss_HP_Fill_Quad";
        fillObj.transform.SetParent(transform);
        fillObj.transform.localPosition = new Vector3(0, 3.8f, -0.01f);
        fillObj.transform.localScale = new Vector3(2.8f, 0.22f, 1f);
        if (_mainCamTransform != null) fillObj.transform.rotation = _mainCamTransform.rotation;

        Renderer fillRend = fillObj.GetComponent<Renderer>();
        if (fillRend != null) fillRend.material.color = new Color(0.95f, 0.1f, 0.1f);
        Destroy(fillObj.GetComponent<Collider>());
        HpBarFillTransform = fillObj.transform;

        // 3. Boss Title TextMesh
        GameObject textObj = new GameObject("BossTitleTextMesh");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = new Vector3(0, 4.3f, 0);
        if (_mainCamTransform != null) textObj.transform.rotation = _mainCamTransform.rotation;

        BossTitleTextMesh = textObj.AddComponent<TextMesh>();
        BossTitleTextMesh.fontSize = 24;
        BossTitleTextMesh.characterSize = 0.15f;
        BossTitleTextMesh.alignment = TextAnchor.MiddleCenter;
        BossTitleTextMesh.anchor = TextAnchor.MiddleCenter;
        BossTitleTextMesh.color = Color.yellow;
        BossTitleTextMesh.text = $"[Lvl {BossLevel}] {BossName}\n{FormatHp(CurrentHp)} / {FormatHp(MaxHp)} HP";
    }

    private string FormatHp(int val)
    {
        if (val >= 1000000) return $"{val / 1000000.0f:F1}M";
        if (val >= 1000) return $"{val / 1000.0f:F1}K";
        return val.ToString();
    }
}
