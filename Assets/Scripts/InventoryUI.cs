// InventoryUI.cs - UPGRADED VERSION with Equipment Comparison
// Replace your current InventoryUI.cs with this version

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform itemsParent;
    [SerializeField] private GameObject inventorySlotPrefab;
    
    [Header("Item Details")]
    [SerializeField] private GameObject itemDetailPanel;
    [SerializeField] private Image itemIconPreview;  // NEW: Shows the item icon large
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemTypeText;
    [SerializeField] private TextMeshProUGUI itemLevelText;
    [SerializeField] private TextMeshProUGUI itemStatsText;
    [SerializeField] private Button equipButton;
    [SerializeField] private Button dropButton;
    
    [Header("Comparison Dialog")]
    [SerializeField] private GameObject comparisonDialog;
    [SerializeField] private TextMeshProUGUI comparisonText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    
    private InventoryManager inventoryManager;
    private PlayerStats playerStats;
    private List<InventorySlotUI> slots = new List<InventorySlotUI>();
    private Item selectedItem;
    private Item itemToReplace;
    
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            inventoryManager = player.GetComponent<InventoryManager>();
            playerStats = player.GetComponent<PlayerStats>();
            
            if (inventoryManager != null)
            {
                inventoryManager.OnInventoryChanged += RefreshInventory;
            }
            else
            {
                //Debug.LogError("InventoryUI: Could not find InventoryManager on Player!");
            }
        }
        else
        {
            //Debug.LogError("InventoryUI: Could not find Player! Make sure player is tagged 'Player'");
        }
        
        CreateInventorySlots();
        
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
        
        if (itemDetailPanel != null)
        {
            itemDetailPanel.SetActive(false);
        }
        
        if (comparisonDialog != null)
        {
            comparisonDialog.SetActive(false);
        }
        
        if (equipButton != null)
            equipButton.onClick.AddListener(OnEquipClicked);
        if (dropButton != null)
            dropButton.onClick.AddListener(OnDropClicked);
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmReplace);
        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelReplace);
            
        //Debug.Log("InventoryUI initialized successfully!");
    }
    
    void CreateInventorySlots()
    {
        if (inventoryManager == null || inventorySlotPrefab == null || itemsParent == null) return;
        
        int maxSlots = inventoryManager.GetMaxSlots();
        
        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slotObj = Instantiate(inventorySlotPrefab, itemsParent);
            InventorySlotUI slot = slotObj.GetComponent<InventorySlotUI>();
            
            if (slot != null)
            {
                slot.SetSlotIndex(i);
                slot.OnSlotClicked += OnSlotClicked;
                slots.Add(slot);
            }
        }
    }
    
    public void ToggleInventory()
    {
        if (inventoryPanel == null) return;
        
        bool newState = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(newState);
        
        if (newState)
        {
            RefreshInventory();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (itemDetailPanel != null)
            {
                itemDetailPanel.SetActive(false);
            }
            if (comparisonDialog != null)
            {
                comparisonDialog.SetActive(false);
            }
        }
    }
    
    void RefreshInventory()
    {
        if (inventoryManager == null) return;
        
        List<Item> items = inventoryManager.GetAllItems();
        
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < items.Count)
            {
                slots[i].SetItem(items[i]);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
    }
    
    void OnSlotClicked(int slotIndex)
    {
        List<Item> items = inventoryManager.GetAllItems();
        
        if (slotIndex < items.Count)
        {
            selectedItem = items[slotIndex];
            ShowItemDetails(selectedItem);
        }
    }
    
    void ShowItemDetails(Item item)
    {
        if (item == null || itemDetailPanel == null) return;
        
        itemDetailPanel.SetActive(true);
        
        // Show icon preview - IMPORTANT: Create new sprite instance if needed
        if (itemIconPreview != null)
        {
            if (item.icon != null)
            {
                itemIconPreview.sprite = item.icon;
                itemIconPreview.color = Color.white;
                itemIconPreview.enabled = true;
            }
            else
            {
                // If no icon exists, generate it now
                ProceduralIconGenerator iconGen = FindFirstObjectByType<ProceduralIconGenerator>();
                if (iconGen != null)
                {
                    if (item is Weapon weapon)
                    {
                        weapon.icon = iconGen.GenerateWeaponIcon(weapon);
                        itemIconPreview.sprite = weapon.icon;
                    }
                    else if (item is Armor armor)
                    {
                        armor.icon = iconGen.GenerateArmorIcon(armor);
                        itemIconPreview.sprite = armor.icon;
                    }
                    else if (item is Collectible collectible)
                    {
                        collectible.icon = iconGen.GenerateCollectibleIcon(collectible);
                        itemIconPreview.sprite = collectible.icon;
                    }
                    itemIconPreview.color = Color.white;
                    itemIconPreview.enabled = true;
                }
                else
                {
                    itemIconPreview.enabled = false;
                }
            }
        }
        
        if (itemNameText != null)
        {
            itemNameText.text = item.itemName;
            itemNameText.color = item.GetRarityColor();
        }
        
        if (itemTypeText != null)
        {
            string typeStr = item.itemType.ToString();
            if (item is Weapon weapon)
            {
                typeStr = weapon.weaponType.ToString();
            }
            else if (item is Armor armor)
            {
                typeStr = armor.armorType.ToString() + " Armor";
            }
            itemTypeText.text = typeStr;
        }
        
        if (itemLevelText != null)
        {
            itemLevelText.text = $"Level {item.level} {item.rarity}";
            itemLevelText.color = item.GetRarityColor();
        }
        
        if (itemStatsText != null)
        {
            string stats = "";
            
            if (item.stats.damage > 0)
                stats += $"+{item.stats.damage:F0} Damage\n";
            if (item.stats.armor > 0)
                stats += $"+{item.stats.armor:F0} Armor\n";
            if (item.stats.health > 0)
                stats += $"+{item.stats.health:F0} Health\n";
            if (item.stats.critChance > 0)
                stats += $"+{item.stats.critChance:F1}% Crit Chance\n";
            if (item.stats.attackSpeed > 0 && item is Weapon)
                stats += $"{item.stats.attackSpeed:F2} Attack Speed\n";
            
            itemStatsText.text = stats;
        }
        
        if (equipButton != null)
        {
            equipButton.gameObject.SetActive(item is Weapon || item is Armor);
        }
    }
    
    void OnEquipClicked()
    {
        if (selectedItem == null || inventoryManager == null || playerStats == null) return;
        
        // Check if slot already has equipment
        if (selectedItem is Weapon newWeapon)
        {
            Weapon currentWeapon = playerStats.GetEquippedWeapon();
            if (currentWeapon != null)
            {
                // Show comparison dialog
                ShowComparisonDialog(newWeapon, currentWeapon);
            }
            else
            {
                // No weapon equipped, just equip
                inventoryManager.EquipItem(selectedItem);
            }
        }
        else if (selectedItem is Armor newArmor)
        {
            Armor currentArmor = playerStats.GetEquippedArmor(newArmor.armorType);
            if (currentArmor != null)
            {
                // Show comparison dialog
                ShowComparisonDialog(newArmor, currentArmor);
            }
            else
            {
                // No armor in that slot, just equip
                inventoryManager.EquipItem(selectedItem);
            }
        }
    }
    
    void ShowComparisonDialog(Item newItem, Item currentItem)
    {
        if (comparisonDialog == null || comparisonText == null) return;
        
        itemToReplace = currentItem;
        comparisonDialog.SetActive(true);
        
        // Build comparison text
        string comparison = $"Replace <color=#{ColorUtility.ToHtmlStringRGB(currentItem.GetRarityColor())}>{currentItem.itemName}</color> with <color=#{ColorUtility.ToHtmlStringRGB(newItem.GetRarityColor())}>{newItem.itemName}</color>?\n\n";
        
        comparison += "<b>Current:</b>\n";
        comparison += GetItemStatsString(currentItem);
        comparison += "\n<b>New:</b>\n";
        comparison += GetItemStatsString(newItem);
        comparison += "\n<b>Change:</b>\n";
        comparison += GetStatComparison(newItem, currentItem);
        
        comparisonText.text = comparison;
    }
    
    string GetItemStatsString(Item item)
    {
        string stats = "";
        
        if (item.stats.damage > 0)
            stats += $"  +{item.stats.damage:F0} Damage\n";
        if (item.stats.armor > 0)
            stats += $"  +{item.stats.armor:F0} Armor\n";
        if (item.stats.health > 0)
            stats += $"  +{item.stats.health:F0} Health\n";
        if (item.stats.critChance > 0)
            stats += $"  +{item.stats.critChance:F1}% Crit\n";
        if (item.stats.attackSpeed > 0 && item is Weapon)
            stats += $"  {item.stats.attackSpeed:F2} Attack Speed\n";
        
        return stats;
    }
    
    string GetStatComparison(Item newItem, Item oldItem)
    {
        string comparison = "";
        
        // Damage comparison
        if (newItem.stats.damage > 0 || oldItem.stats.damage > 0)
        {
            float diff = newItem.stats.damage - oldItem.stats.damage;
            comparison += FormatStatDiff("Damage", diff);
        }
        
        // Armor comparison
        if (newItem.stats.armor > 0 || oldItem.stats.armor > 0)
        {
            float diff = newItem.stats.armor - oldItem.stats.armor;
            comparison += FormatStatDiff("Armor", diff);
        }
        
        // Health comparison
        if (newItem.stats.health > 0 || oldItem.stats.health > 0)
        {
            float diff = newItem.stats.health - oldItem.stats.health;
            comparison += FormatStatDiff("Health", diff);
        }
        
        // Crit comparison
        if (newItem.stats.critChance > 0 || oldItem.stats.critChance > 0)
        {
            float diff = newItem.stats.critChance - oldItem.stats.critChance;
            comparison += FormatStatDiff("Crit", diff, true);
        }
        
        // Attack speed comparison (for weapons)
        if (newItem is Weapon && oldItem is Weapon)
        {
            float diff = newItem.stats.attackSpeed - oldItem.stats.attackSpeed;
            comparison += FormatStatDiff("Attack Speed", diff, false, false);
        }
        
        return comparison;
    }
    
    string FormatStatDiff(string statName, float diff, bool isPercentage = false, bool showPlus = true)
    {
        if (Mathf.Abs(diff) < 0.01f) return "";
        
        string color = diff > 0 ? "green" : "red";
        string sign = diff > 0 && showPlus ? "+" : "";
        string suffix = isPercentage ? "%" : "";
        
        return $"  <color={color}>{sign}{diff:F1}{suffix} {statName}</color>\n";
    }
    
    void OnConfirmReplace()
    {
        if (selectedItem != null && inventoryManager != null)
        {
            inventoryManager.EquipItem(selectedItem);
            comparisonDialog.SetActive(false);
            itemToReplace = null;
        }
    }
    
    void OnCancelReplace()
    {
        comparisonDialog.SetActive(false);
        itemToReplace = null;
    }
    
    void OnDropClicked()
    {
        if (selectedItem != null && inventoryManager != null)
        {
            inventoryManager.DropItem(selectedItem);
            if (itemDetailPanel != null)
            {
                itemDetailPanel.SetActive(false);
            }
            selectedItem = null;
        }
    }
    
    void OnDestroy()
    {
        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryChanged -= RefreshInventory;
        }
    }
}