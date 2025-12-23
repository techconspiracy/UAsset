// PlayerStats.cs - UPDATED VERSION
// Fixed to use new ArmorType enum (Helmet, Chestplate, etc.)

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private int level = 1;
    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private float baseMaxMana = 100f;
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float baseArmor = 5f;
    [SerializeField] private float baseCritChance = 5f;
    
    [Header("Current Stats")]
    private float currentHealth;
    private float currentMana;
    
    [Header("Equipment")]
    private Weapon equippedWeapon;
    private Dictionary<ArmorType, Armor> equippedArmor = new Dictionary<ArmorType, Armor>();
    
    [Header("UI References (Assign After Creating UI)")]
    [SerializeField] private Image healthBar;
    [SerializeField] private Image manaBar;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI manaText;
    
    // Events
    public event System.Action OnDeath;
    public event System.Action<float, bool> OnTakeDamage;
    
    void Start()
    {
        currentHealth = GetMaxHealth();
        currentMana = GetMaxMana();
        UpdateUI();
        
        // TEMPORARY TEST CODE - Remove later
        makeItems generator = FindObjectOfType<makeItems>();
        InventoryManager inventory = GetComponent<InventoryManager>();
        
        if (generator != null && inventory != null)
        {
            // Generate some test items
            for (int i = 0; i < 5; i++)
            {
                Weapon weapon = generator.GenerateWeapon(1);
                inventory.AddItem(weapon);
            }
            
            for (int i = 0; i < 3; i++)
            {
                Armor armor = generator.GenerateArmor(1);
                inventory.AddItem(armor);
            }
            
            Debug.Log("Added 8 test items to inventory");
        }
        
        UpdateUI();
    }
    
    public float GetMaxHealth()
    {
        float totalHealth = baseMaxHealth + (level * 10);
        
        foreach (var armor in equippedArmor.Values)
        {
            if (armor != null && armor.stats != null)
            {
                totalHealth += armor.stats.health;
            }
        }
        
        return totalHealth;
    }
    
    public float GetMaxMana()
    {
        return baseMaxMana + (level * 5);
    }
    
    public float GetTotalDamage()
    {
        float totalDamage = baseDamage + (level * 2);
        
        if (equippedWeapon != null && equippedWeapon.stats != null)
        {
            totalDamage += equippedWeapon.stats.damage;
        }
        
        return totalDamage;
    }
    
    public float GetTotalArmor()
    {
        float totalArmor = baseArmor;
        
        foreach (var armor in equippedArmor.Values)
        {
            if (armor != null && armor.stats != null)
            {
                totalArmor += armor.stats.armor;
            }
        }
        
        return totalArmor;
    }
    
    public float GetCritChance()
    {
        float totalCrit = baseCritChance;
        
        if (equippedWeapon != null && equippedWeapon.stats != null)
        {
            totalCrit += equippedWeapon.stats.critChance;
        }
        
        return totalCrit;
    }
    
    public void TakeDamage(float damage, bool isCrit = false)
    {
        float armorReduction = Mathf.Min(GetTotalArmor() / 100f, 0.75f);
        float actualDamage = damage * (1 - armorReduction);
        
        currentHealth -= actualDamage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        OnTakeDamage?.Invoke(actualDamage, isCrit);
        UpdateUI();
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, GetMaxHealth());
        UpdateUI();
    }
    
    public void EquipWeapon(Weapon weapon)
    {
        equippedWeapon = weapon;
        Debug.Log($"Equipped weapon: {weapon.itemName} (+{weapon.stats.damage} damage)");
        
        // Update combat systems
        CombatAnimationController combat = GetComponent<CombatAnimationController>();
        if (combat != null)
        {
            combat.EquipWeapon(weapon);
        }
        
        RangedWeaponHandler ranged = GetComponent<RangedWeaponHandler>();
        if (ranged != null)
        {
            ranged.SetEquippedWeapon(weapon);
        }
        
        UpdateUI();
    }
    
    public void EquipArmor(Armor armor)
    {
        if (armor != null)
        {
            equippedArmor[armor.armorType] = armor;
            
            float newMaxHealth = GetMaxHealth();
            if (currentHealth < newMaxHealth)
            {
                currentHealth = Mathf.Min(currentHealth + armor.stats.health, newMaxHealth);
            }
            
            Debug.Log($"Equipped armor: {armor.itemName} (+{armor.stats.armor} armor)");
            
            // Attach armor model to character
            AttachArmorModel(armor);
            
            UpdateUI();
        }
    }
    
    void AttachArmorModel(Armor armor)
    {
        if (armor.armorModel == null) return;
        
        // Find the appropriate bone to attach to
        Transform targetBone = GetArmorAttachBone(armor.armorType);
        
        if (targetBone != null)
        {
            ProceduralArmorModels armorGen = FindObjectOfType<ProceduralArmorModels>();
            if (armorGen != null)
            {
                armorGen.AttachArmorToBone(armor.armorModel, targetBone, armor.armorType);
                Debug.Log($"Attached {armor.armorType} model to character");
            }
        }
        else
        {
            Debug.LogWarning($"Could not find bone for {armor.armorType}");
        }
    }
    
    Transform GetArmorAttachBone(ArmorType type)
    {
        // Find bones in character rig
        Transform[] bones = GetComponentsInChildren<Transform>();
        
        switch (type)
        {
            case ArmorType.Helmet:
                return FindBone(bones, "Head") ?? transform; // Fallback to root
                
            case ArmorType.Chestplate:
                return FindBone(bones, "Spine") ?? FindBone(bones, "Chest") ?? transform;
                
            case ArmorType.Leggings:
                return FindBone(bones, "Hips") ?? FindBone(bones, "Pelvis") ?? transform;
                
            case ArmorType.Gloves:
                return FindBone(bones, "RightHand") ?? transform;
                
            case ArmorType.Boots:
                return FindBone(bones, "RightFoot") ?? FindBone(bones, "LeftFoot") ?? transform;
                
            case ArmorType.Shield:
                return FindBone(bones, "LeftHand") ?? transform;
                
            default:
                return transform;
        }
    }
    
    Transform FindBone(Transform[] bones, string boneName)
    {
        foreach (Transform bone in bones)
        {
            if (bone.name.Contains(boneName))
            {
                return bone;
            }
        }
        return null;
    }
    
    public Weapon GetEquippedWeapon()
    {
        return equippedWeapon;
    }
    
    public Armor GetEquippedArmor(ArmorType type)
    {
        equippedArmor.TryGetValue(type, out Armor armor);
        return armor;
    }
    
    public Dictionary<ArmorType, Armor> GetAllEquippedArmor()
    {
        return equippedArmor;
    }
    
    void Die()
    {
        Debug.Log("Player Died!");
        OnDeath?.Invoke();
        // Add respawn logic here
    }
    
    void UpdateUI()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = currentHealth / GetMaxHealth();
        }
        
        if (manaBar != null)
        {
            manaBar.fillAmount = currentMana / GetMaxMana();
        }
        
        if (levelText != null)
        {
            levelText.text = $"Player Level : {level}";
        }
        
        if (healthText != null)
        {
            healthText.text = $"Health : {Mathf.Ceil(currentHealth)} / {GetMaxHealth()}";
        }
        
        if (manaText != null)
        {
            manaText.text = $"Mana : {Mathf.Ceil(currentMana)} / {GetMaxMana()}";
        }
    }
    
    public int GetLevel()
    {
        return level;
    }
    
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
    
    // Called by ExperienceManager when leveling up
    public void OnLevelUp(int newLevel)
    {
        level = newLevel;
        
        // Heal to full on level up
        currentHealth = GetMaxHealth();
        currentMana = GetMaxMana();
        
        UpdateUI();
        
        Debug.Log($"Player leveled up to {level}! Stats increased!");
    }
}