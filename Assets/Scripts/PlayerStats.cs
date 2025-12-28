// PlayerStats.cs - UPDATED FOR RIG SYSTEM
// Now uses ProceduralCharacterRig for proper armor/weapon attachment

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
    
    [Header("Stat Scaling Per Level")]
    [SerializeField] private float healthPerLevel = 10f;
    [SerializeField] private float manaPerLevel = 5f;
    [SerializeField] private float damagePerLevel = 2f;
    [SerializeField] private float armorPerLevel = 1f;
    [SerializeField] private float critPerLevel = 0.5f;
    
    [Header("Current Stats")]
    private float currentHealth;
    private float currentMana;
    
    [Header("Equipment")]
    private Weapon equippedWeapon;
    private Dictionary<ArmorType, Armor> equippedArmor = new Dictionary<ArmorType, Armor>();
    
    [Header("Character Rig")]
    [SerializeField] private ProceduralCharacterRig characterRig;
    
    [Header("UI References")]
    [SerializeField] private Image healthBar;
    [SerializeField] private Image manaBar;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI manaText;
    
    // Events
    public event System.Action OnDeath;
    public event System.Action<float, bool> OnTakeDamage;
    
    void Awake()
    {
        // Try to find character rig
        if (characterRig == null)
        {
            characterRig = GetComponent<ProceduralCharacterRig>();
        }
        
        // If no rig exists, generate one
        if (characterRig == null)
        {
            characterRig = gameObject.AddComponent<ProceduralCharacterRig>();
            characterRig.GenerateCompleteRig();
            Debug.Log("✅ Auto-generated character rig for player");
        }
    }
    
    void Start()
    {
        currentHealth = GetMaxHealth();
        currentMana = GetMaxMana();
        UpdateUI();
        
        // TEMPORARY TEST CODE - Can remove this later
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
        float totalHealth = baseMaxHealth + (level * healthPerLevel);
        
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
        return baseMaxMana + (level * manaPerLevel);
    }
    
    public float GetTotalDamage()
    {
        float totalDamage = baseDamage + (level * damagePerLevel);
        
        if (equippedWeapon != null && equippedWeapon.stats != null)
        {
            totalDamage += equippedWeapon.stats.damage;
        }
        
        return totalDamage;
    }
    
    public float GetTotalArmor()
    {
        float totalArmor = baseArmor + (level * armorPerLevel);
        
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
        float totalCrit = baseCritChance + (level * critPerLevel);
        
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
        // Unequip old weapon model if exists
        if (equippedWeapon != null && equippedWeapon.weaponModel != null)
        {
            equippedWeapon.weaponModel.SetActive(false);
        }
        
        equippedWeapon = weapon;
        Debug.Log($"Equipped weapon: {weapon.itemName} (+{weapon.stats.damage} damage)");
        
        // Attach weapon model to right hand using rig
        if (weapon.weaponModel != null && characterRig != null)
        {
            Transform weaponBone = characterRig.GetWeaponBone();
            if (weaponBone != null)
            {
                weapon.weaponModel.transform.SetParent(weaponBone);
                weapon.weaponModel.transform.localPosition = Vector3.zero;
                weapon.weaponModel.transform.localRotation = Quaternion.identity;
                weapon.weaponModel.SetActive(true);
                Debug.Log($"✅ Attached weapon to {weaponBone.name}");
            }
            else
            {
                Debug.LogWarning("⚠️ Weapon bone not found in character rig!");
            }
        }
        
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
        if (armor == null) return;
        
        // Unequip old armor model if exists
        if (equippedArmor.ContainsKey(armor.armorType))
        {
            Armor oldArmor = equippedArmor[armor.armorType];
            if (oldArmor != null && oldArmor.armorModel != null)
            {
                oldArmor.armorModel.SetActive(false);
            }
        }
        
        // Store new armor
        equippedArmor[armor.armorType] = armor;
        
        // Update health if armor provides health bonus
        float newMaxHealth = GetMaxHealth();
        if (currentHealth < newMaxHealth)
        {
            currentHealth = Mathf.Min(currentHealth + armor.stats.health, newMaxHealth);
        }
        
        Debug.Log($"Equipped armor: {armor.itemName} (+{armor.stats.armor} armor)");
        
        // Attach armor model using rig system
        AttachArmorModel(armor);
        
        UpdateUI();
    }
    
    void AttachArmorModel(Armor armor)
    {
        if (armor.armorModel == null)
        {
            Debug.LogWarning($"⚠️ Armor {armor.itemName} has no model!");
            return;
        }
        
        if (characterRig == null)
        {
            Debug.LogError("❌ Character rig is missing! Cannot attach armor.");
            return;
        }
        
        // Get the correct bone for this armor type
        Transform targetBone = characterRig.GetArmorBone(armor.armorType);
        
        if (targetBone != null)
        {
            // Attach armor to bone
            armor.armorModel.transform.SetParent(targetBone);
            armor.armorModel.transform.localPosition = GetArmorOffset(armor.armorType);
            armor.armorModel.transform.localRotation = Quaternion.identity;
            armor.armorModel.SetActive(true);
            
            Debug.Log($"✅ Attached {armor.armorType} to {targetBone.name}");
        }
        else
        {
            Debug.LogWarning($"⚠️ Could not find bone for {armor.armorType}");
        }
    }
    
    Vector3 GetArmorOffset(ArmorType type)
    {
        // Fine-tune positioning for each armor type
        return type switch
        {
            ArmorType.Helmet => new Vector3(0, 0.1f, 0),
            ArmorType.Chestplate => Vector3.zero,
            ArmorType.Leggings => new Vector3(0, -0.1f, 0),
            ArmorType.Boots => new Vector3(0, -0.05f, 0.05f),
            ArmorType.Gloves => Vector3.zero,
            ArmorType.Shield => new Vector3(-0.2f, 0, 0.1f),
            _ => Vector3.zero
        };
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
            levelText.text = $"Level {level}";
        }
        
        if (healthText != null)
        {
            healthText.text = $"HP: {Mathf.Ceil(currentHealth)} / {GetMaxHealth()}";
        }
        
        if (manaText != null)
        {
            manaText.text = $"MP: {Mathf.Ceil(currentMana)} / {GetMaxMana()}";
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
    
    public void OnLevelUp(int newLevel)
    {
        int levelsGained = newLevel - level;
        level = newLevel;
        
        baseMaxHealth += healthPerLevel * levelsGained;
        baseMaxMana += manaPerLevel * levelsGained;
        baseDamage += damagePerLevel * levelsGained;
        baseArmor += armorPerLevel * levelsGained;
        baseCritChance += critPerLevel * levelsGained;
        
        currentHealth = GetMaxHealth();
        currentMana = GetMaxMana();
        
        UpdateUI();
        
        Debug.Log($"🎉 LEVEL UP to {level}!");
        Debug.Log($"  Health: {GetMaxHealth():F0} (+{healthPerLevel * levelsGained:F0})");
        Debug.Log($"  Damage: {GetTotalDamage():F0} (+{damagePerLevel * levelsGained:F0})");
        Debug.Log($"  Armor: {GetTotalArmor():F0} (+{armorPerLevel * levelsGained:F0})");
        Debug.Log($"  Crit Chance: {GetCritChance():F1}% (+{critPerLevel * levelsGained:F1}%)");
    }
}