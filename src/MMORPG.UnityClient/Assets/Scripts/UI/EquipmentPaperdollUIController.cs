using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class EquippedSlotData
{
    public string SlotName;
    public InventoryItemSlot EquippedItem;
    public Text SlotLabelText;
    public Image SlotIconImage;
}

public class EquipmentPaperdollUIController : MonoBehaviour
{
    public static EquipmentPaperdollUIController Instance { get; private set; }

    [Header("Paperdoll Window UI References")]
    public GameObject PaperdollPanel;
    public EquippedSlotData MainHandSlot;
    public EquippedSlotData OffHandSlot;
    public EquippedSlotData HeadSlot;
    public EquippedSlotData ChestSlot;
    public EquippedSlotData LegsSlot;
    public EquippedSlotData BootsSlot;

    [Header("Character Stats UI")]
    public Text StrengthText;
    public Text AgilityText;
    public Text IntelligenceText;
    public Text VitalityText;
    public Text UnallocatedPointsText;

    [Header("Calculated Vitals Summary")]
    public Text TotalAttackPowerText;
    public Text TotalSpellPowerText;
    public Text TotalArmorText;
    public Text TotalCritChanceText;

    // Character Base Stats & Points
    private int _baseStrength = 25;
    private int _baseAgility = 18;
    private int _baseIntelligence = 15;
    private int _baseVitality = 30;
    private int _unallocatedPoints = 5;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateStatsDisplay();
    }

    public void TogglePaperdollWindow()
    {
        if (PaperdollPanel != null)
        {
            PaperdollPanel.SetActive(!PaperdollPanel.activeSelf);
        }
    }

    public void EquipItem(InventoryItemSlot item, string targetSlot)
    {
        EquippedSlotData slot = targetSlot switch
        {
            "MainHand" => MainHandSlot,
            "OffHand" => OffHandSlot,
            "Head" => HeadSlot,
            "Chest" => ChestSlot,
            "Legs" => LegsSlot,
            "Boots" => BootsSlot,
            _ => MainHandSlot
        };

        if (slot != null)
        {
            slot.EquippedItem = item;
            if (slot.SlotLabelText != null) slot.SlotLabelText.text = item.ItemName;
            HUDUIController.Instance?.AppendChatMessage("EQUIPMENT", $"<color=cyan>Kuşanıldı: {item.ItemName} ({targetSlot})</color>");
        }

        UpdateStatsDisplay();
    }

    public void OnClickAllocateStat(string statName)
    {
        if (_unallocatedPoints <= 0)
        {
            HUDUIController.Instance?.AppendChatMessage("SYSTEM", "Dağıtılacak Stat Puanınız kalmadı!");
            return;
        }

        switch (statName.ToLowerInvariant())
        {
            case "strength": _baseStrength++; break;
            case "agility": _baseAgility++; break;
            case "intelligence": _baseIntelligence++; break;
            case "vitality": _baseVitality++; break;
        }

        _unallocatedPoints--;
        UpdateStatsDisplay();
        HUDUIController.Instance?.AppendChatMessage("SYSTEM", $"Stat Artırıldı: {statName.ToUpper()}!");
    }

    private void UpdateStatsDisplay()
    {
        // Calculate Gear Bonus Stats
        int gearDamage = (MainHandSlot?.EquippedItem?.BaseDamage ?? 0) + (OffHandSlot?.EquippedItem?.BaseDamage ?? 0);
        int gearArmor = (HeadSlot?.EquippedItem?.BaseArmor ?? 0) + (ChestSlot?.EquippedItem?.BaseArmor ?? 0) + (LegsSlot?.EquippedItem?.BaseArmor ?? 0) + (BootsSlot?.EquippedItem?.BaseArmor ?? 0);

        int totalAttackPower = 100 + (_baseStrength * 4) + gearDamage;
        int totalSpellPower = 80 + (_baseIntelligence * 5);
        int totalArmor = (_baseVitality * 2) + gearArmor;
        float totalCrit = 5.0f + (_baseAgility * 0.4f);

        if (StrengthText != null) StrengthText.text = $"STR: {_baseStrength}";
        if (AgilityText != null) AgilityText.text = $"AGI: {_baseAgility}";
        if (IntelligenceText != null) IntelligenceText.text = $"INT: {_baseIntelligence}";
        if (VitalityText != null) VitalityText.text = $"VIT: {_baseVitality}";
        if (UnallocatedPointsText != null) UnallocatedPointsText.text = $"Boş Puan: <color=yellow>{_unallocatedPoints}</color>";

        if (TotalAttackPowerText != null) TotalAttackPowerText.text = $"Atak Gücü: {totalAttackPower}";
        if (TotalSpellPowerText != null) TotalSpellPowerText.text = $"Büyü Gücü: {totalSpellPower}";
        if (TotalArmorText != null) TotalArmorText.text = $"Zırh (Armor): {totalArmor}";
        if (TotalCritChanceText != null) TotalCritChanceText.text = $"Kritik Şansı: %{totalCrit:F1}";
    }
}
