using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    [Header("UI References")]
    public Slider HealthSlider;
    public Image FillImage;
    public Text NameText;
    public Text LevelText;

    [Header("Colors")]
    public Color PlayerColor = new Color(0f, 0.95f, 0.4f);
    public Color HostileMobColor = new Color(1f, 0.2f, 0.2f);

    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (_mainCamera != null)
        {
            transform.rotation = _mainCamera.transform.rotation;
        }
    }

    public void Initialize(string entityName, int level, bool isHostile)
    {
        if (NameText != null) NameText.text = entityName;
        if (LevelText != null) LevelText.text = $"Lvl {level}";
        if (FillImage != null) FillImage.color = isHostile ? HostileMobColor : PlayerColor;
    }

    public void UpdateHealth(int currentHp, int maxHp)
    {
        if (HealthSlider != null && maxHp > 0)
        {
            HealthSlider.value = (float)currentHp / maxHp;
        }
    }
}
