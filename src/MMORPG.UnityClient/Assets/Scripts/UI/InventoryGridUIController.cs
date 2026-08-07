using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class InventoryItemSlot
{
    public string ItemId;
    public string ItemName;
    public int Quantity;
    public string IconColorHex;
    public int BaseDamage;
    public int BaseArmor;
    public string Rarity = "Rare";
}

public class InventoryGridUIController : MonoBehaviour
{
    public static InventoryGridUIController Instance { get; private set; }

    [Header("Inventory UI References")]
    public Transform SlotContainer;
    public GameObject ItemSlotPrefab;
    public Text GoldAmountText;

    private readonly List<InventoryItemSlot> _items = new List<InventoryItemSlot>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Populate rich default starter inventory
        _items.Add(new InventoryItemSlot { ItemId = "item_sword_01", ItemName = "Iron Sword +5", Quantity = 1, IconColorHex = "#ffd700", BaseDamage = 45, BaseArmor = 0, Rarity = "Rare" });
        _items.Add(new InventoryItemSlot { ItemId = "item_armor_chest_01", ItemName = "Recruit Leather Chestpiece +3", Quantity = 1, IconColorHex = "#6366f1", BaseDamage = 0, BaseArmor = 35, Rarity = "Uncommon" });
        _items.Add(new InventoryItemSlot { ItemId = "item_potion_hp", ItemName = "Health Potion (L)", Quantity = 15, IconColorHex = "#ff5252", BaseDamage = 0, BaseArmor = 0, Rarity = "Common" });
        _items.Add(new InventoryItemSlot { ItemId = "item_potion_mp", ItemName = "Mana Elixir", Quantity = 10, IconColorHex = "#00f2fe", BaseDamage = 0, BaseArmor = 0, Rarity = "Common" });
        _items.Add(new InventoryItemSlot { ItemId = "item_greatsword_godslayer", ItemName = "Godslayer Greatsword", Quantity = 1, IconColorHex = "#ff9100", BaseDamage = 350, BaseArmor = 35, Rarity = "Legendary" });

        RefreshInventoryUI();
    }

    public void RefreshInventoryUI()
    {
        if (SlotContainer == null) return;

        // Clear existing slots
        foreach (Transform child in SlotContainer)
        {
            Destroy(child.gameObject);
        }

        // Render current inventory items
        foreach (var item in _items)
        {
            if (ItemSlotPrefab != null)
            {
                GameObject slotObj = Instantiate(ItemSlotPrefab, SlotContainer);
                Text nameTxt = slotObj.GetComponentInChildren<Text>();
                if (nameTxt != null)
                {
                    nameTxt.text = $"{item.ItemName} (x{item.Quantity})";
                }

                Button slotBtn = slotObj.GetComponent<Button>();
                if (slotBtn != null)
                {
                    var capturedItem = item;
                    slotBtn.onClick.AddListener(() => OnClickItemSlot(capturedItem));
                }
            }
        }
    }

    private void OnClickItemSlot(InventoryItemSlot slot)
    {
        if (slot.ItemId.StartsWith("item_sword") || slot.ItemId.StartsWith("item_armor") || slot.ItemId.StartsWith("item_greatsword"))
        {
            ItemEnhancementUIController.Instance?.OpenEnhancementWindow(slot);
        }
        else
        {
            HUDUIController.Instance?.AppendChatMessage("INVENTORY", $"Kullanıldı: {slot.ItemName}");
        }
    }

    public void UpdateGold(long goldAmount)
    {
        if (GoldAmountText != null)
        {
            GoldAmountText.text = $"{goldAmount:N0} Gold";
        }
    }
}
