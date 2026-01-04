// makeItems.cs - COMPATIBLE VERSION
// Works with existing systems WITHOUT breaking changes
// Optional procedural rig - falls back to traditional setup if not used

using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public enum ItemType { Weapon, Armor, Consumable, Collectible, Quest }

[System.Serializable]
public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary }

[System.Serializable]
public enum WeaponType { Sword, Axe, Mace, Dagger, Staff, Bow }

[System.Serializable]
public enum ArmorType { 
    Helmet,
    Chestplate,
    Leggings,
    Gloves,
    Boots,
    Shield
}

[System.Serializable]
public class ItemStats
{
    public float damage;
    public float armor;
    public float health;
    public float mana;
    public float critChance;
    public float attackSpeed;
}

[System.Serializable]
public class Item
{
    public string itemName;
    public string itemID;
    public ItemType itemType;
    public ItemRarity rarity;
    public ItemStats stats;
    public int level;
    public Sprite icon;
    public GameObject worldModel;
    
    public Color GetRarityColor()
    {
        return rarity switch
        {
            ItemRarity.Common => Color.white,
            ItemRarity.Uncommon => Color.green,
            ItemRarity.Rare => Color.blue,
            ItemRarity.Epic => new Color(0.64f, 0.21f, 0.93f),
            ItemRarity.Legendary => new Color(1f, 0.5f, 0f),
            _ => Color.white
        };
    }
}

[System.Serializable]
public class Weapon : Item
{
    public WeaponType weaponType;
    public float range;
    public GameObject weaponModel;
    
    public Weapon()
    {
        itemType = ItemType.Weapon;
    }
}

[System.Serializable]
public class Armor : Item
{
    public ArmorType armorType;
    public GameObject armorModel;
    
    public Armor()
    {
        itemType = ItemType.Armor;
    }
}

[System.Serializable]
public class Collectible : Item
{
    public string description;
    public int collectibleID;
    public Sprite collectibleIcon;
    
    public Collectible()
    {
        itemType = ItemType.Collectible;
    }
}

public class makeItems : MonoBehaviour
{
    [Header("Name Generation")]
    public string[] prefixes = { "Ancient", "Blessed", "Cursed", "Dark", "Divine", 
                                  "Elder", "Fallen", "Grim", "Holy", "Infernal" };
    public string[] suffixes = { "of Power", "of Doom", "of Light", "of Shadows", 
                                 "of the Bear", "of the Phoenix", "of Valor", "of Wisdom" };
    
    [Header("Weapon Base Names")]
    public string[] swordNames = { "Blade", "Sword", "Saber", "Longsword", "Claymore" };
    public string[] axeNames = { "Axe", "Hatchet", "Cleaver", "Waraxe", "Battleaxe" };
    public string[] maceNames = { "Mace", "Hammer", "Cudgel", "Maul", "Warhammer" };
    public string[] daggerNames = { "Dagger", "Knife", "Shiv", "Dirk", "Stiletto" };
    public string[] staffNames = { "Staff", "Rod", "Wand", "Stave", "Scepter" };
    
    [Header("Optional: Procedural Character System")]
    [SerializeField] private bool useProceduralRigs = false;
    [SerializeField] private Color playerSkinColor = new Color(1f, 0.8f, 0.6f);
    [SerializeField] private Color enemySkinColor = new Color(0.5f, 0.7f, 0.5f);
    
    public Weapon GenerateWeapon(int playerLevel)
    {
        Weapon weapon = new Weapon();
        weapon.itemID = System.Guid.NewGuid().ToString();
        weapon.level = playerLevel + UnityEngine.Random.Range(-2, 3);
        weapon.level = Mathf.Max(1, weapon.level);
        
        // Determine rarity
        float rarityRoll = UnityEngine.Random.value;
        if (rarityRoll < 0.5f) weapon.rarity = ItemRarity.Common;
        else if (rarityRoll < 0.75f) weapon.rarity = ItemRarity.Uncommon;
        else if (rarityRoll < 0.90f) weapon.rarity = ItemRarity.Rare;
        else if (rarityRoll < 0.97f) weapon.rarity = ItemRarity.Epic;
        else weapon.rarity = ItemRarity.Legendary;
        
        // Generate weapon type
        weapon.weaponType = (WeaponType)UnityEngine.Random.Range(0, 6);
        
        // Generate name
        string baseName = weapon.weaponType switch
        {
            WeaponType.Sword => swordNames[UnityEngine.Random.Range(0, swordNames.Length)],
            WeaponType.Axe => axeNames[UnityEngine.Random.Range(0, axeNames.Length)],
            WeaponType.Mace => maceNames[UnityEngine.Random.Range(0, maceNames.Length)],
            WeaponType.Dagger => daggerNames[UnityEngine.Random.Range(0, daggerNames.Length)],
            WeaponType.Staff => staffNames[UnityEngine.Random.Range(0, staffNames.Length)],
            _ => "Weapon"
        };
        
        if (weapon.rarity >= ItemRarity.Rare)
        {
            weapon.itemName = $"{prefixes[UnityEngine.Random.Range(0, prefixes.Length)]} {baseName} {suffixes[UnityEngine.Random.Range(0, suffixes.Length)]}";
        }
        else
        {
            weapon.itemName = baseName;
        }
        
        // Generate stats
        weapon.stats = new ItemStats();
        float rarityMultiplier = weapon.rarity switch
        {
            ItemRarity.Common => 1f,
            ItemRarity.Uncommon => 1.3f,
            ItemRarity.Rare => 1.6f,
            ItemRarity.Epic => 2f,
            ItemRarity.Legendary => 2.5f,
            _ => 1f
        };
        
        weapon.stats.damage = (10 + weapon.level * 5) * rarityMultiplier;
        weapon.stats.critChance = 5f + (float)weapon.rarity * 2f;
        weapon.stats.attackSpeed = weapon.weaponType switch
        {
            WeaponType.Dagger => 1.5f,
            WeaponType.Sword => 1.2f,
            WeaponType.Axe => 1.0f,
            WeaponType.Mace => 0.9f,
            WeaponType.Staff => 0.8f,
            _ => 1f
        };
        
        weapon.range = weapon.weaponType == WeaponType.Bow ? 15f : 2.5f;
        
        // Generate the 3D model for this weapon
        ProceduralWeaponModels modelGenerator = FindObjectOfType<ProceduralWeaponModels>();
        if (modelGenerator != null)
        {
            modelGenerator.GenerateWeaponModel(weapon);
        }
        
        return weapon;
    }    
    
    public Armor GenerateArmor(int playerLevel)
    {
        Armor armor = new Armor();
        armor.itemID = System.Guid.NewGuid().ToString();
        armor.level = playerLevel + UnityEngine.Random.Range(-2, 3);
        armor.level = Mathf.Max(1, armor.level);
        
        // Determine rarity
        float rarityRoll = UnityEngine.Random.value;
        if (rarityRoll < 0.5f) armor.rarity = ItemRarity.Common;
        else if (rarityRoll < 0.75f) armor.rarity = ItemRarity.Uncommon;
        else if (rarityRoll < 0.90f) armor.rarity = ItemRarity.Rare;
        else if (rarityRoll < 0.97f) armor.rarity = ItemRarity.Epic;
        else armor.rarity = ItemRarity.Legendary;
        
        // Generate armor type
        armor.armorType = (ArmorType)UnityEngine.Random.Range(0, 5);
        
        // Generate name
        string baseName = armor.armorType switch
        {
            ArmorType.Helmet => "Helmet",
            ArmorType.Chestplate => "Chestplate",
            ArmorType.Leggings => "Leggings",
            ArmorType.Gloves => "Gauntlets",
            ArmorType.Boots => "Boots",
            ArmorType.Shield => "Shield",
            _ => "Armor"
        };
        
        if (armor.rarity >= ItemRarity.Rare)
        {
            armor.itemName = $"{prefixes[UnityEngine.Random.Range(0, prefixes.Length)]} {baseName} {suffixes[UnityEngine.Random.Range(0, suffixes.Length)]}";
        }
        else
        {
            armor.itemName = baseName;
        }
        
        // Generate stats
        armor.stats = new ItemStats();
        float rarityMultiplier = armor.rarity switch
        {
            ItemRarity.Common => 1f,
            ItemRarity.Uncommon => 1.3f,
            ItemRarity.Rare => 1.6f,
            ItemRarity.Epic => 2f,
            ItemRarity.Legendary => 2.5f,
            _ => 1f
        };
        
        armor.stats.armor = (5 + armor.level * 3) * rarityMultiplier;
        armor.stats.health = (20 + armor.level * 10) * rarityMultiplier;
        
        // Generate icon
        ProceduralIconGenerator iconGen = GetComponent<ProceduralIconGenerator>();
        if (iconGen != null)
        {
            armor.icon = iconGen.GenerateArmorIcon(armor);
        }
        
        // Generate 3D model
        ProceduralArmorModels armorModelGenerator = FindObjectOfType<ProceduralArmorModels>();
        if (armorModelGenerator != null)
        {
            armorModelGenerator.GenerateArmorModel(armor);
        }
        
        return armor;
    }
    
    public Collectible GenerateCollectible()
    {
        Collectible c = new Collectible();
        c.itemID = System.Guid.NewGuid().ToString();
        c.collectibleID = UnityEngine.Random.Range(1, 1000);
        
        string[] types = { "Ancient Coin", "Mysterious Rune", "Crystal Shard", 
                          "Ancient Scroll", "Rare Gem", "Lost Artifact" };
        c.itemName = types[UnityEngine.Random.Range(0, types.Length)];
        c.description = "A rare collectible item";
        c.rarity = ItemRarity.Rare;
        
        return c;
    }
    
    // OPTIONAL: Enhanced character spawning with procedural rigs
    // Only use these if you want procedural characters
    // Otherwise, use your existing player/enemy setup
    
    public void AddProceduralRigToExistingCharacter(GameObject character, bool isEnemy = false)
    {
        if (!useProceduralRigs) return;
        
        // Check if rig already exists
        ProceduralCharacterRig existingRig = character.GetComponent<ProceduralCharacterRig>();
        if (existingRig != null)
        {
            Debug.Log($"Character {character.name} already has a procedural rig");
            return;
        }
        
        // Add rig component
        ProceduralCharacterRig rig = character.AddComponent<ProceduralCharacterRig>();
        
        // Generate the rig
        rig.GenerateCompleteRig();
        
        // Set appropriate color
        Color skinColor = isEnemy ? enemySkinColor : playerSkinColor;
        if (isEnemy)
        {
            // Add slight variation for enemies
            skinColor += new Color(
                UnityEngine.Random.Range(-0.1f, 0.1f),
                UnityEngine.Random.Range(-0.1f, 0.1f),
                UnityEngine.Random.Range(-0.1f, 0.1f),
                0
            );
        }
        rig.SetSkinColor(skinColor);
        
        Debug.Log($"✅ Added procedural rig to {character.name}");
    }
    
    public GameObject CreateProceduralPlayer(Vector3 position)
    {
        if (!useProceduralRigs)
        {
            Debug.LogWarning("Procedural rigs disabled. Enable 'Use Procedural Rigs' in makeItems component.");
            return null;
        }
        
        GameObject player = new GameObject("Player");
        player.transform.position = position;
        player.tag = "Player";
        
        // Add CharacterController
        CharacterController controller = player.AddComponent<CharacterController>();
        controller.radius = 0.3f;
        controller.height = 1.8f;
        controller.center = new Vector3(0, 0.9f, 0);
        
        // Add procedural rig
        ProceduralCharacterRig rig = player.AddComponent<ProceduralCharacterRig>();
        rig.GenerateCompleteRig();
        rig.SetSkinColor(playerSkinColor);
        
        // Add standard player components
        player.AddComponent<PlayerStats>();
        player.AddComponent<InventoryManager>();
        player.AddComponent<ExperienceManager>();
        player.AddComponent<ThirdPersonController>();
        player.AddComponent<CombatAnimationController>();
        player.AddComponent<PlayerWeaponHandler>();
        player.AddComponent<RangedWeaponHandler>();
        
        // Add optional procedural animator
        if (player.GetComponent<ProceduralAnimationController>() == null)
        {
            player.AddComponent<ProceduralAnimationController>();
        }
        
        Debug.Log($"✅ Created procedural player at {position}");
        return player;
    }
    
    public GameObject CreateProceduralEnemy(Vector3 position, int level = 1)
    {
        if (!useProceduralRigs)
        {
            Debug.LogWarning("Procedural rigs disabled. Enable 'Use Procedural Rigs' in makeItems component.");
            return null;
        }
        
        GameObject enemy = new GameObject($"Enemy_Lvl{level}");
        enemy.transform.position = position;
        enemy.tag = "Enemy";
        
        // Add CharacterController
        CharacterController controller = enemy.AddComponent<CharacterController>();
        controller.radius = 0.3f;
        controller.height = 1.8f;
        controller.center = new Vector3(0, 0.9f, 0);
        
        // Add procedural rig
        ProceduralCharacterRig rig = enemy.AddComponent<ProceduralCharacterRig>();
        rig.GenerateCompleteRig();
        
        Color skinColor = enemySkinColor + new Color(
            UnityEngine.Random.Range(-0.1f, 0.1f),
            UnityEngine.Random.Range(-0.1f, 0.1f),
            UnityEngine.Random.Range(-0.1f, 0.1f),
            0
        );
        rig.SetSkinColor(skinColor);
        
        // Add standard enemy components
        enemy.AddComponent<EnemyStats>();
        enemy.AddComponent<EnemyWeaponHandler>();
        
        // Add optional procedural animator
        if (enemy.GetComponent<ProceduralAnimationController>() == null)
        {
            enemy.AddComponent<ProceduralAnimationController>();
        }
        
        // Generate and equip weapon
        Weapon enemyWeapon = GenerateWeapon(level);
        if (enemyWeapon.weaponModel != null)
        {
            Transform weaponBone = rig.GetWeaponBone();
            if (weaponBone != null)
            {
                enemyWeapon.weaponModel.transform.SetParent(weaponBone);
                enemyWeapon.weaponModel.transform.localPosition = Vector3.zero;
                enemyWeapon.weaponModel.transform.localRotation = Quaternion.identity;
                enemyWeapon.weaponModel.SetActive(true);
            }
        }
        
        Debug.Log($"✅ Created procedural enemy at {position}");
        return enemy;
    }
}