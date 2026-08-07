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
        // Populate default starter inventory
        _items.Add(new InventoryItemSlot { ItemId = "item_sword_01", ItemName = "Iron Sword +5", Quantity = 1, IconColorHex = "#ffd700" });
        _items.Add(new InventoryItemSlot { ItemId = "item_potion_hp", ItemName = "Health Potion (L)", Quantity = 15, IconColorHex = "#ff5252" });
        _items.Add(new InventoryItemSlot { ItemId = "item_potion_mp", ItemName = "Mana Elixir", Quantity = 10, IconColorHex = "#00f2fe" });

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
            }
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
