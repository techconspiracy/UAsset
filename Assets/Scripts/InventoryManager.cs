// InventoryManager.cs
// CREATE THIS SIXTH - Requires ItemSystem.cs, PlayerStats.cs
// Manages player inventory and equipment

using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxInventorySlots = 30;
    
    private List<Item> items = new List<Item>();
    private PlayerStats playerStats;
    
    public event System.Action OnInventoryChanged;
    
    void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }
    
    public bool AddItem(Item item)
    {
        if (items.Count >= maxInventorySlots)
        {
            //Debug.Log("Inventory is full!");
            return false;
        }
        
        items.Add(item);
        OnInventoryChanged?.Invoke();
        //Debug.Log($"Added {item.itemName} to inventory");
        return true;
    }
    
    public void RemoveItem(Item item)
    {
        items.Remove(item);
        OnInventoryChanged?.Invoke();
    }
    
    public void EquipItem(Item item)
    {
        if (item is Weapon weapon)
        {
            Weapon currentWeapon = playerStats.GetEquippedWeapon();
            if (currentWeapon != null)
            {
                // Current weapon stays in inventory
            }
            
            playerStats.EquipWeapon(weapon);
            //Debug.Log($"Equipped weapon: {weapon.itemName}");
        }
        else if (item is Armor armor)
        {
            Armor currentArmor = playerStats.GetEquippedArmor(armor.armorType);
            if (currentArmor != null)
            {
                // Current armor stays in inventory
            }
            
            playerStats.EquipArmor(armor);
            //Debug.Log($"Equipped armor: {armor.itemName}");
        }
        
        OnInventoryChanged?.Invoke();
    }
    
    public void UseItem(Item item)
    {
        if (item.itemType == ItemType.Consumable)
        {
            // Apply consumable effects here
            RemoveItem(item);
        }
    }
    
    public void DropItem(Item item)
    {
        RemoveItem(item);
        
        GameObject lootObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lootObj.transform.position = transform.position + transform.forward * 2f;
        lootObj.transform.localScale = Vector3.one * 0.5f;
        
        Renderer renderer = lootObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = item.GetRarityColor();
        }
        
        LootDrop lootDrop = lootObj.AddComponent<LootDrop>();
        lootDrop.SetItem(item);
        
        //Debug.Log($"Dropped {item.itemName}");
    }
    
    public List<Item> GetAllItems()
    {
        return new List<Item>(items);
    }
    
    public int GetItemCount()
    {
        return items.Count;
    }
    
    public int GetMaxSlots()
    {
        return maxInventorySlots;
    }
}