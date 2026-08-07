using UnityEngine;

public class UniversalCombatEntity : MonoBehaviour, IDamageable
{
    [Header("Entity Info")]
    public string Name = "Forest Goblin";
    public int Level = 1;
    public int MaxHealth = 150;
    public int CurrentHealth = 150;
    public Color ColorTint = new Color(0.2f, 0.8f, 0.2f);

    [Header("Floating Overhead Bar (3D Quads - No Canvas Overlay)")]
    public Transform HpBarFillTransform;
    public TextMesh NameTitleTextMesh;

    public string EntityName => Name;
    public int CurrentHp => CurrentHealth;
    public int MaxHp => MaxHealth;
    public bool IsDead => CurrentHealth <= 0;

    private Transform _mainCamTransform;

    private void Start()
    {
        if (Camera.main != null)
        {
            _mainCamTransform = Camera.main.transform;
        }

        // Apply Color Tint to Mesh
        Renderer rend = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            rend.material.color = ColorTint;
        }

        CreateCleanOverheadBar3D();
    }

    private void Update()
    {
        // Billboard text towards 2.5D Camera
        if (NameTitleTextMesh != null && _mainCamTransform != null)
        {
            NameTitleTextMesh.transform.rotation = _mainCamTransform.rotation;
        }
    }

    public void TakeDamage(int damage, bool isCritical, Vector3 attackerPosition)
    {
        if (IsDead) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);

        float fillPct = (float)CurrentHealth / MaxHealth;
        if (HpBarFillTransform != null)
        {
            HpBarFillTransform.localScale = new Vector3(fillPct * 1.2f, 0.12f, 1f);
            HpBarFillTransform.localPosition = new Vector3(-0.6f * (1f - fillPct), 2.1f, -0.01f);
        }

        if (NameTitleTextMesh != null)
        {
            NameTitleTextMesh.text = $"[Lvl {Level}] {Name} ({FormatHp(CurrentHealth)}/{FormatHp(MaxHealth)})";
        }

        // Spawn formatted 3D damage text
        Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 2.5f, Random.Range(-0.5f, 0.5f));
        DamageTextManager.Instance?.SpawnDamageText(spawnPos, damage, isCritical);

        // Play Hit VFX Effect
        VFXManager.Instance?.PlaySkillVFX(1, transform.position);

        if (IsDead)
        {
            OnDeath();
        }
    }

    private void OnDeath()
    {
        Debug.Log($"[Combat] Entity '{Name}' has been slain!");
        Destroy(gameObject, 0.5f);
    }

    private void CreateCleanOverheadBar3D()
    {
        // 1. Black Background Bar (1.25m x 0.15m)
        GameObject bgObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bgObj.name = "HP_Background_Quad";
        bgObj.transform.SetParent(transform);
        bgObj.transform.localPosition = new Vector3(0, 2.1f, 0);
        bgObj.transform.localScale = new Vector3(1.25f, 0.15f, 1f);
        if (_mainCamTransform != null) bgObj.transform.rotation = _mainCamTransform.rotation;

        Renderer bgRend = bgObj.GetComponent<Renderer>();
        if (bgRend != null) bgRend.material.color = new Color(0.05f, 0.05f, 0.05f);
        Destroy(bgObj.GetComponent<Collider>());

        // 2. Red HP Fill Bar (1.2m x 0.12m)
        GameObject fillObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        fillObj.name = "HP_Fill_Quad";
        fillObj.transform.SetParent(transform);
        fillObj.transform.localPosition = new Vector3(0, 2.1f, -0.01f);
        fillObj.transform.localScale = new Vector3(1.2f, 0.12f, 1f);
        if (_mainCamTransform != null) fillObj.transform.rotation = _mainCamTransform.rotation;

        Renderer fillRend = fillObj.GetComponent<Renderer>();
        if (fillRend != null) fillRend.material.color = new Color(0.9f, 0.15f, 0.15f);
        Destroy(fillObj.GetComponent<Collider>());
        HpBarFillTransform = fillObj.transform;

        // 3. Name & HP TextMesh
        GameObject textObj = new GameObject("OverheadTextMesh");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = new Vector3(0, 2.4f, 0);
        if (_mainCamTransform != null) textObj.transform.rotation = _mainCamTransform.rotation;

        NameTitleTextMesh = textObj.AddComponent<TextMesh>();
        NameTitleTextMesh.fontSize = 20;
        NameTitleTextMesh.characterSize = 0.12f;
        NameTitleTextMesh.alignment = TextAlignment.Center;
        NameTitleTextMesh.anchor = TextAnchor.MiddleCenter;
        NameTitleTextMesh.color = Color.white;
        NameTitleTextMesh.text = $"[Lvl {Level}] {Name} ({FormatHp(CurrentHealth)}/{FormatHp(MaxHealth)})";
    }

    private string FormatHp(int val)
    {
        if (val >= 1000000) return $"{val / 1000000.0f:F1}M";
        if (val >= 1000) return $"{val / 1000.0f:F1}K";
        return val.ToString();
    }
}
