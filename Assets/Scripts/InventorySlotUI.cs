// InventorySlotUI.cs
// CREATE THIS TWELFTH - Requires ItemSystem.cs
// Handles individual inventory slot UI
// ATTACH THIS TO YOUR INVENTORY SLOT PREFAB

using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Text itemLevelText;
    
    private Item item;
    private int slotIndex;
    
    public event System.Action<int> OnSlotClicked;
    
    public void SetSlotIndex(int index)
    {
        slotIndex = index;
    }
    
    public void SetItem(Item newItem)
    {
        item = newItem;
        
        if (itemIcon != null)
        {
            // Make sure icon exists, generate if needed
            if (item.icon == null)
            {
                ProceduralIconGenerator iconGen = FindFirstObjectByType<ProceduralIconGenerator>();
                if (iconGen != null)
                {
                    if (item is Weapon weapon)
                    {
                        weapon.icon = iconGen.GenerateWeaponIcon(weapon);
                    }
                    else if (item is Armor armor)
                    {
                        armor.icon = iconGen.GenerateArmorIcon(armor);
                    }
                    else if (item is Collectible collectible)
                    {
                        collectible.icon = iconGen.GenerateCollectibleIcon(collectible);
                    }
                }
            }
            
            // Set the icon
            if (item.icon != null)
            {
                itemIcon.sprite = item.icon;
                itemIcon.color = item.GetRarityColor();
                itemIcon.enabled = true;
            }
            else
            {
                itemIcon.enabled = false;
            }
        }
        
        if (backgroundImage != null)
        {
            Color bgColor = item.GetRarityColor();
            bgColor.a = 0.3f;
            backgroundImage.color = bgColor;
        }
        
        if (itemLevelText != null)
        {
            itemLevelText.text = item.level.ToString();
            itemLevelText.enabled = true;
        }
    }
    
    public void ClearSlot()
    {
        item = null;
        
        if (itemIcon != null)
            itemIcon.enabled = false;
        
        if (backgroundImage != null)
            backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        
        if (itemLevelText != null)
            itemLevelText.enabled = false;
    }
    
    public void OnSlotClick()
    {
        if (item != null)
        {
            OnSlotClicked?.Invoke(slotIndex);
        }
    }
}