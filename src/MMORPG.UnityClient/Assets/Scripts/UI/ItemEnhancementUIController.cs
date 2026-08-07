using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ItemEnhancementUIController : MonoBehaviour
{
    public static ItemEnhancementUIController Instance { get; private set; }

    [Header("Anvil Window UI References")]
    public GameObject EnhancementPanel;
    public Text SelectedItemNameText;
    public Text CurrentLevelText;
    public Text SuccessRateText;
    public Toggle UseProtectionScrollToggle;
    public Text ResultMessageText;
    public Image AnvilIconImage;
    public GameObject SparkleVFXPrefab;

    [Header("Item Tooltip UI")]
    public GameObject TooltipPanel;
    public Text TooltipTitleText;
    public Text TooltipStatsText;

    private InventoryItemSlot _selectedSlot;
    private int _currentEnhancementLevel = 0;

    private static readonly float[] SuccessRates = new float[] { 1.00f, 1.00f, 1.00f, 0.70f, 0.50f, 0.35f, 0.20f, 0.10f, 0.04f };

    private void Awake()
    {
        Instance = this;
    }

    public void OpenEnhancementWindow(InventoryItemSlot slot)
    {
        _selectedSlot = slot;
        _currentEnhancementLevel = ParseEnhancementLevel(slot.ItemName);

        if (EnhancementPanel != null) EnhancementPanel.SetActive(true);
        if (SelectedItemNameText != null) SelectedItemNameText.text = slot.ItemName;
        if (CurrentLevelText != null) CurrentLevelText.text = $"+{_currentEnhancementLevel} ➜ +{_currentEnhancementLevel + 1}";
        if (ResultMessageText != null) ResultMessageText.text = "Hazır. 'Yükselt' butonuna basın.";

        UpdateSuccessRateDisplay();
    }

    public void CloseEnhancementWindow()
    {
        if (EnhancementPanel != null) EnhancementPanel.SetActive(false);
    }

    private int ParseEnhancementLevel(string name)
    {
        int plusIdx = name.IndexOf('+');
        if (plusIdx != -1 && int.TryParse(name.Substring(plusIdx + 1), out int lvl))
        {
            return lvl;
        }
        return 0;
    }

    private void UpdateSuccessRateDisplay()
    {
        if (_currentEnhancementLevel >= 9)
        {
            if (SuccessRateText != null) SuccessRateText.text = "Maksimum Seviye (+9)!";
            return;
        }

        float chance = _currentEnhancementLevel < SuccessRates.Length ? SuccessRates[_currentEnhancementLevel] * 100f : 4f;
        if (SuccessRateText != null)
        {
            SuccessRateText.text = $"Başarı Şansı: <color=yellow>%{chance:F0}</color>";
        }
    }

    public void OnClickEnhanceItem()
    {
        if (_selectedSlot == null || _currentEnhancementLevel >= 9) return;

        StartCoroutine(ExecuteAnvilForgingAnimation());
    }

    private IEnumerator ExecuteAnvilForgingAnimation()
    {
        if (ResultMessageText != null) ResultMessageText.text = "🔨 Örs Üzerinde Dövülüyor...";
        
        // Shake Anvil Effect
        if (AnvilIconImage != null)
        {
            Vector3 origPos = AnvilIconImage.transform.localPosition;
            for (int i = 0; i < 6; i++)
            {
                AnvilIconImage.transform.localPosition = origPos + new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0);
                yield return new WaitForSeconds(0.08f);
            }
            AnvilIconImage.transform.localPosition = origPos;
        }

        yield return new WaitForSeconds(0.5f);

        // Calculate Enhancement Roll
        float roll = Random.Range(0f, 1f);
        float chance = _currentEnhancementLevel < SuccessRates.Length ? SuccessRates[_currentEnhancementLevel] : 0.04f;
        bool useScroll = UseProtectionScrollToggle != null && UseProtectionScrollToggle.isOn;

        if (roll <= chance)
        {
            // SUCCESS!
            _currentEnhancementLevel++;
            string baseName = _selectedSlot.ItemName.Split('+')[0].Trim();
            _selectedSlot.ItemName = $"{baseName} +{_currentEnhancementLevel}";

            if (ResultMessageText != null)
            {
                ResultMessageText.text = $"<color=green>✨ TEBRİKLER! Eşya +{_currentEnhancementLevel} seviyesine yükseltildi!</color>";
            }

            VFXManager.Instance?.SpawnVFX("VFX_EnhanceSuccess", Vector3.zero, Quaternion.identity);
            HUDUIController.Instance?.AppendChatMessage("ENHANCEMENT", $"<color=gold>✨ TEBRİKLER! {_selectedSlot.ItemName} dövüldü!</color>");
        }
        else
        {
            // FAILURE!
            if (useScroll)
            {
                if (ResultMessageText != null)
                {
                    ResultMessageText.text = "<color=yellow>⚠️ Yükseltme Başarısız! Kutsal Parşömen eşyayı korudu.</color>";
                }
            }
            else if (_currentEnhancementLevel >= 6)
            {
                // ITEM DESTROYED (SHATTER)
                string destroyedName = _selectedSlot.ItemName;
                if (ResultMessageText != null)
                {
                    ResultMessageText.text = "<color=red>💥 KRİTİK BAŞARISIZLIK! Eşya parçalanarak toz oldu!</color>";
                }
                HUDUIController.Instance?.AppendChatMessage("ENHANCEMENT", $"<color=red>💥 CRITICAL FAIL! {destroyedName} shattered into dust!</color>");
                _selectedSlot = null;
                CloseEnhancementWindow();
            }
            else
            {
                // DEGRADE -1
                _currentEnhancementLevel = Mathf.Max(0, _currentEnhancementLevel - 1);
                string baseName = _selectedSlot.ItemName.Split('+')[0].Trim();
                _selectedSlot.ItemName = _currentEnhancementLevel > 0 ? $"{baseName} +{_currentEnhancementLevel}" : baseName;

                if (ResultMessageText != null)
                {
                    ResultMessageText.text = $"<color=orange>⚠️ Yükseltme Başarısız! Eşya +{_currentEnhancementLevel} seviyesine geriledi.</color>";
                }
            }
        }

        if (_selectedSlot != null)
        {
            if (SelectedItemNameText != null) SelectedItemNameText.text = _selectedSlot.ItemName;
            if (CurrentLevelText != null) CurrentLevelText.text = $"+{_currentEnhancementLevel} ➜ +{_currentEnhancementLevel + 1}";
            UpdateSuccessRateDisplay();
        }

        InventoryGridUIController.Instance?.RefreshInventoryUI();
    }

    public void ShowTooltip(string name, string rarity, int level, int damage, int armor)
    {
        if (TooltipPanel != null) TooltipPanel.SetActive(true);
        if (TooltipTitleText != null) TooltipTitleText.text = $"<b>{name}</b> ({rarity})";
        if (TooltipStatsText != null) TooltipStatsText.text = $"Gerekli Lvl: {level}\nAtak: +{damage}\nArmor: +{armor}";
    }

    public void HideTooltip()
    {
        if (TooltipPanel != null) TooltipPanel.SetActive(false);
    }
}
