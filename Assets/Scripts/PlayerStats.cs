// PlayerStats.cs - COMPATIBLE VERSION
// Works with OR WITHOUT ProceduralCharacterRig
// No breaking changes to existing functionality

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
    
    [Header("UI References")]
    [SerializeField] private Image healthBar;
    [SerializeField] private Image manaBar;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI manaText;
    
    // Events
    public event System.Action OnDeath;
    public event System.Action<float, bool> OnTakeDamage;
    
    // Optional: Reference to procedural rig (if exists)
    private ProceduralCharacterRig characterRig;
    
    void Awake()
    {
        // Try to find character rig (optional)
        characterRig = GetComponent<ProceduralCharacterRig>();
        
        // If makeItems wants us to have a rig and we don't have one yet
        makeItems itemGen = FindObjectOfType<makeItems>();
        if (itemGen != null)
        {
            // Check via reflection if useProceduralRigs is true
            var field = typeof(makeItems).GetField("useProceduralRigs", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                bool useRigs = (bool)field.GetValue(itemGen);
                if (useRigs && characterRig == null)
                {
                    // Add rig if system is enabled
                    itemGen.AddProceduralRigToExistingCharacter(gameObject, false);
                    characterRig = GetComponent<ProceduralCharacterRig>();
                }
            }
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
        
        // Attach weapon model
        if (weapon.weaponModel != null)
        {
            Transform weaponBone = GetWeaponAttachPoint();
            
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
                Debug.LogWarning("⚠️ No weapon attach point found!");
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
        
        // Attach armor model
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
        
        Transform targetBone = GetArmorAttachPoint(armor.armorType);
        
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
    
    // COMPATIBILITY: Get weapon attach point (works with or without rig)
    Transform GetWeaponAttachPoint()
    {
        // Try procedural rig first
        if (characterRig != null)
        {
            Transform weaponPoint = characterRig.weaponAttachPoint;
            if (weaponPoint != null) return weaponPoint;
            
            Transform rightHand = characterRig.GetWeaponBone();
            if (rightHand != null) return rightHand;
        }
        
        // Fall back to searching for bones in children
        Transform rightHand = FindBone("RightHand");
        if (rightHand != null)
        {
            // Look for weapon attach point child
            Transform weaponPoint = rightHand.Find("WeaponAttachPoint");
            if (weaponPoint != null) return weaponPoint;
            return rightHand;
        }
        
        // Last resort: use this transform
        return transform;
    }
    
    // COMPATIBILITY: Get armor attach point (works with or without rig)
    Transform GetArmorAttachPoint(ArmorType armorType)
    {
        // Try procedural rig first
        if (characterRig != null)
        {
            Transform bone = characterRig.GetArmorBone(armorType);
            if (bone != null) return bone;
        }
        
        // Fall back to searching for bones by name
        string boneName = armorType switch
        {
            ArmorType.Helmet => "Head",
            ArmorType.Chestplate => "Chest",
            ArmorType.Leggings => "Pelvis",
            ArmorType.Gloves => "RightHand",
            ArmorType.Boots => "RightFoot",
            ArmorType.Shield => "LeftLowerArm",
            _ => "Chest"
        };
        
        Transform bone = FindBone(boneName);
        if (bone != null) return bone;
        
        // Last resort: use this transform
        return transform;
    }
    
    // Helper to find bones (for non-rig characters)
    Transform FindBone(string boneName)
    {
        Transform[] bones = GetComponentsInChildren<Transform>();
        foreach (Transform bone in bones)
        {
            if (bone.name.Contains(boneName))
            {
                return bone;
            }
        }
        return null;
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