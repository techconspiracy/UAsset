// GameTester.cs - Enhanced Debug Tool with Tabbed Interface
// Attach to Player - comprehensive testing for ALL game systems
// Press F1 to toggle | Use tabs to navigate different test sections

using UnityEngine;
using System.Collections.Generic;

public class GameTester : MonoBehaviour
{
    [Header("Debug Display")]
    [SerializeField] private bool showDebugUI = true;
    [SerializeField] private KeyCode toggleDebugKey = KeyCode.F1;
    
    [Header("Enemy Spawning")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnDistance = 5f;
    
    private enum DebugTab
    {
        Overview,
        Inventory,
        Combat,
        Experience,
        Weapons,
        Armor,
        Enemies,
        Projectiles
    }
    
    private DebugTab currentTab = DebugTab.Overview;
    private Vector2 scrollPosition = Vector2.zero;
    private GUIStyle headerStyle;
    private GUIStyle tabButtonStyle;
    private GUIStyle activeTabStyle;
    
    void Update()
    {
        if (Input.GetKeyDown(toggleDebugKey))
        {
            showDebugUI = !showDebugUI;
        }
    }
    
    void OnGUI()
    {
        if (!showDebugUI) return;
        
        InitStyles();
        
        // Main debug window
        GUILayout.BeginArea(new Rect(10, 10, 450, Screen.height - 20));
        GUILayout.BeginVertical("box");
        
        // Header
        GUILayout.Label("=== GAME TESTER ===", headerStyle);
        GUILayout.Label($"Press {toggleDebugKey} to toggle | FPS: {(int)(1f / Time.deltaTime)}");
        
        // Tab buttons
        DrawTabButtons();
        
        GUILayout.Space(5);
        
        // Scrollable content area
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(Screen.height - 120));
        
        // Draw content based on selected tab
        switch (currentTab)
        {
            case DebugTab.Overview:
                DrawOverviewTab();
                break;
            case DebugTab.Inventory:
                DrawInventoryTab();
                break;
            case DebugTab.Combat:
                DrawCombatTab();
                break;
            case DebugTab.Experience:
                DrawExperienceTab();
                break;
            case DebugTab.Weapons:
                DrawWeaponsTab();
                break;
            case DebugTab.Armor:
                DrawArmorTab();
                break;
            case DebugTab.Enemies:
                DrawEnemiesTab();
                break;
            case DebugTab.Projectiles:
                DrawProjectilesTab();
                break;
        }
        
        GUILayout.EndScrollView();
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    void InitStyles()
    {
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(GUI.skin.box);
            headerStyle.fontSize = 16;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.alignment = TextAnchor.MiddleCenter;
        }
        
        if (tabButtonStyle == null)
        {
            tabButtonStyle = new GUIStyle(GUI.skin.button);
            tabButtonStyle.fontSize = 11;
            tabButtonStyle.padding = new RectOffset(8, 8, 4, 4);
        }
        
        if (activeTabStyle == null)
        {
            activeTabStyle = new GUIStyle(tabButtonStyle);
            activeTabStyle.normal.background = tabButtonStyle.active.background;
        }
    }
    
    void DrawTabButtons()
    {
        GUILayout.BeginHorizontal();
        
        if (TabButton("Overview", DebugTab.Overview)) currentTab = DebugTab.Overview;
        if (TabButton("Inventory", DebugTab.Inventory)) currentTab = DebugTab.Inventory;
        if (TabButton("Combat", DebugTab.Combat)) currentTab = DebugTab.Combat;
        if (TabButton("XP", DebugTab.Experience)) currentTab = DebugTab.Experience;
        
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        
        if (TabButton("Weapons", DebugTab.Weapons)) currentTab = DebugTab.Weapons;
        if (TabButton("Armor", DebugTab.Armor)) currentTab = DebugTab.Armor;
        if (TabButton("Enemies", DebugTab.Enemies)) currentTab = DebugTab.Enemies;
        if (TabButton("Projectiles", DebugTab.Projectiles)) currentTab = DebugTab.Projectiles;
        
        GUILayout.EndHorizontal();
    }
    
    bool TabButton(string label, DebugTab tab)
    {
        GUIStyle style = currentTab == tab ? activeTabStyle : tabButtonStyle;
        return GUILayout.Button(label, style);
    }
    
    // ==================== TAB CONTENTS ====================
    
    void DrawOverviewTab()
    {
        GUILayout.Label("--- SYSTEM STATUS ---", GUI.skin.box);
        
        // Check all critical components
        CheckComponent<PlayerStats>("PlayerStats");
        CheckComponent<InventoryManager>("InventoryManager");
        CheckComponent<ExperienceManager>("ExperienceManager");
        CheckComponent<ThirdPersonController>("ThirdPersonController");
        CheckComponent<CombatAnimationController>("CombatAnimationController");
        CheckComponent<RangedWeaponHandler>("RangedWeaponHandler");
        
        GUILayout.Space(10);
        
        GUILayout.Label("--- SCENE MANAGERS ---", GUI.skin.box);
        CheckSceneComponent<makeItems>("makeItems (Item Generator)");
        CheckSceneComponent<ProceduralWeaponModels>("ProceduralWeaponModels");
        CheckSceneComponent<ProceduralArmorModels>("ProceduralArmorModels");
        CheckSceneComponent<ProjectileSystem>("ProjectileSystem");
        
        GUILayout.Space(10);
        
        // Quick stats
        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats != null)
        {
            GUILayout.Label("--- QUICK STATS ---", GUI.skin.box);
            GUILayout.Label($"Health: {stats.GetCurrentHealth():F0} / {stats.GetMaxHealth():F0}");
            GUILayout.Label($"Level: {stats.GetLevel()}");
            GUILayout.Label($"Damage: {stats.GetTotalDamage():F0}");
            GUILayout.Label($"Armor: {stats.GetTotalArmor():F0}");
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Run Full System Check", GUILayout.Height(30)))
        {
            RunFullSystemCheck();
        }
    }
    
    void DrawInventoryTab()
    {
        GUILayout.Label("--- INVENTORY MANAGEMENT ---", GUI.skin.box);
        
        InventoryManager inv = GetComponent<InventoryManager>();
        if (inv != null)
        {
            GUILayout.Label($"Items: {inv.GetItemCount()} / {inv.GetMaxSlots()}");
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("Add Random Weapon"))
            {
                AddRandomItem(ItemType.Weapon);
            }
            
            if (GUILayout.Button("Add Random Armor"))
            {
                AddRandomItem(ItemType.Armor);
            }
            
            if (GUILayout.Button("Add 5 Random Items"))
            {
                for (int i = 0; i < 5; i++)
                {
                    AddRandomItem(Random.value > 0.5f ? ItemType.Weapon : ItemType.Armor);
                }
            }
            
            GUILayout.Space(10);
            
            GUILayout.Label("--- RARITY TESTING ---", GUI.skin.box);
            
            if (GUILayout.Button("Add Common Weapon"))
            {
                AddWeaponOfRarity(ItemRarity.Common);
            }
            
            if (GUILayout.Button("Add Rare Weapon"))
            {
                AddWeaponOfRarity(ItemRarity.Rare);
            }
            
            if (GUILayout.Button("Add Legendary Weapon"))
            {
                AddWeaponOfRarity(ItemRarity.Legendary);
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Clear Inventory", GUILayout.Height(25)))
            {
                ClearInventory();
            }
            
            GUILayout.Space(10);
            
            // List current items
            GUILayout.Label("--- CURRENT ITEMS ---", GUI.skin.box);
            var items = inv.GetAllItems();
            if (items.Count == 0)
            {
                GUILayout.Label("(Empty)");
            }
            else
            {
                foreach (var item in items)
                {
                    string itemInfo = $"{item.rarity} {item.itemName}";
                    if (item is Weapon weapon)
                    {
                        itemInfo += $" ({weapon.weaponType}, {weapon.stats.damage:F0} dmg)";
                    }
                    else if (item is Armor armor)
                    {
                        itemInfo += $" ({armor.armorType}, {armor.stats.armor:F0} arm)";
                    }
                    GUILayout.Label(itemInfo);
                }
            }
        }
        else
        {
            GUILayout.Label("ERROR: No InventoryManager!", GUI.skin.box);
        }
    }
    
    void DrawCombatTab()
    {
        GUILayout.Label("--- COMBAT SYSTEMS ---", GUI.skin.box);
        
        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats != null)
        {
            GUILayout.Label($"Total Damage: {stats.GetTotalDamage():F0}");
            GUILayout.Label($"Crit Chance: {stats.GetCritChance():F1}%");
            GUILayout.Label($"Armor: {stats.GetTotalArmor():F0}");
            GUILayout.Label($"Damage Reduction: {Mathf.Min(stats.GetTotalArmor() / 100f, 0.75f) * 100f:F0}%");
            
            Weapon equipped = stats.GetEquippedWeapon();
            if (equipped != null)
            {
                GUILayout.Label($"Weapon: {equipped.itemName}");
                GUILayout.Label($"Type: {equipped.weaponType}");
                GUILayout.Label($"Base Damage: {equipped.stats.damage:F0}");
            }
            else
            {
                GUILayout.Label("No weapon equipped");
            }
        }
        
        GUILayout.Space(10);
        
        GUILayout.Label("--- COMBAT TESTING ---", GUI.skin.box);
        
        if (GUILayout.Button("Take 20 Damage"))
        {
            stats?.TakeDamage(20f, false);
        }
        
        if (GUILayout.Button("Take 50 Damage (Crit)"))
        {
            stats?.TakeDamage(50f, true);
        }
        
        if (GUILayout.Button("Heal to Full"))
        {
            stats?.Heal(9999f);
        }
        
        GUILayout.Space(10);
        
        // Animation controller status
        CombatAnimationController combat = GetComponent<CombatAnimationController>();
        if (combat != null)
        {
            GUILayout.Label("Combat Animation: Active");
            GUILayout.Label($"Is Attacking: {combat.IsAttacking()}");
            GUILayout.Label($"Weapon Type: {combat.GetCurrentWeaponType()}");
        }
        else
        {
            GUILayout.Label("ERROR: No CombatAnimationController!");
        }
    }
    
    void DrawExperienceTab()
    {
        GUILayout.Label("--- EXPERIENCE & LEVELING ---", GUI.skin.box);
        
        ExperienceManager exp = GetComponent<ExperienceManager>();
        PlayerStats stats = GetComponent<PlayerStats>();
        
        if (exp != null && stats != null)
        {
            GUILayout.Label($"Current Level: {exp.GetCurrentLevel()}");
            GUILayout.Label($"Current XP: {exp.GetCurrentExp()} / {exp.GetExpToNextLevel()}");
            GUILayout.Label($"Progress: {exp.GetLevelProgress() * 100f:F1}%");
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Gain 50 XP"))
            {
                exp.GainExperience(50);
            }
            
            if (GUILayout.Button("Gain 200 XP"))
            {
                exp.GainExperience(200);
            }
            
            if (GUILayout.Button("Gain 1000 XP (Level Up!)"))
            {
                exp.GainExperience(1000);
            }
            
            GUILayout.Space(10);
            
            GUILayout.Label("--- LEVEL CURVE INFO ---", GUI.skin.box);
            
            int currentLevel = exp.GetCurrentLevel();
            for (int i = 0; i < 5 && (currentLevel + i) <= 10; i++)
            {
                int level = currentLevel + i;
                int xpRequired = Mathf.RoundToInt(100 * Mathf.Pow(level, 1.5f));
                GUILayout.Label($"Level {level} → {level + 1}: {xpRequired} XP");
            }
        }
        else
        {
            if (exp == null) GUILayout.Label("ERROR: No ExperienceManager!");
            if (stats == null) GUILayout.Label("ERROR: No PlayerStats!");
        }
    }
    
    void DrawWeaponsTab()
    {
        GUILayout.Label("--- WEAPON SYSTEMS ---", GUI.skin.box);
        
        PlayerStats stats = GetComponent<PlayerStats>();
        Weapon equipped = stats?.GetEquippedWeapon();
        
        if (equipped != null)
        {
            GUILayout.Label($"Equipped: {equipped.itemName}");
            GUILayout.Label($"Type: {equipped.weaponType}");
            GUILayout.Label($"Rarity: {equipped.rarity}");
            GUILayout.Label($"Damage: {equipped.stats.damage:F0}");
            GUILayout.Label($"Crit Chance: {equipped.stats.critChance:F1}%");
            GUILayout.Label($"Has Model: {(equipped.weaponModel != null ? "✓" : "✗")}");
            
            if (equipped.weaponModel != null)
            {
                GUILayout.Label($"Model Active: {equipped.weaponModel.activeSelf}");
                GUILayout.Label($"Children: {equipped.weaponModel.transform.childCount}");
            }
        }
        else
        {
            GUILayout.Label("No weapon equipped");
        }
        
        GUILayout.Space(10);
        
        GUILayout.Label("--- GENERATE & EQUIP ---", GUI.skin.box);
        
        if (GUILayout.Button("Generate Sword"))
        {
            GenerateAndEquipWeapon(WeaponType.Sword);
        }
        
        if (GUILayout.Button("Generate Axe"))
        {
            GenerateAndEquipWeapon(WeaponType.Axe);
        }
        
        if (GUILayout.Button("Generate Bow"))
        {
            GenerateAndEquipWeapon(WeaponType.Bow);
        }
        
        if (GUILayout.Button("Generate Staff"))
        {
            GenerateAndEquipWeapon(WeaponType.Staff);
        }
        
        if (GUILayout.Button("Generate Dagger"))
        {
            GenerateAndEquipWeapon(WeaponType.Dagger);
        }
        
        if (GUILayout.Button("Generate Mace"))
        {
            GenerateAndEquipWeapon(WeaponType.Mace);
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("List All Weapon Models in Scene"))
        {
            ListWeaponModels();
        }
    }
    
    void DrawArmorTab()
    {
        GUILayout.Label("--- ARMOR SYSTEMS ---", GUI.skin.box);
        
        PlayerStats stats = GetComponent<PlayerStats>();
        
        if (stats != null)
        {
            var equippedArmor = stats.GetAllEquippedArmor();
            
            GUILayout.Label($"Total Armor: {stats.GetTotalArmor():F0}");
            GUILayout.Label($"Health Bonus: {stats.GetMaxHealth() - 100 - stats.GetLevel() * 10:F0}");
            
            GUILayout.Space(5);
            
            GUILayout.Label("Equipped Armor:");
            
            foreach (ArmorType type in System.Enum.GetValues(typeof(ArmorType)))
            {
                if (equippedArmor.TryGetValue(type, out Armor armor) && armor != null)
                {
                    GUILayout.Label($"  {type}: {armor.itemName} (+{armor.stats.armor:F0})");
                }
                else
                {
                    GUILayout.Label($"  {type}: (empty)");
                }
            }
        }
        
        GUILayout.Space(10);
        
        GUILayout.Label("--- GENERATE ARMOR ---", GUI.skin.box);
        
        if (GUILayout.Button("Generate Helmet"))
        {
            GenerateArmorPiece(ArmorType.Helmet);
        }
        
        if (GUILayout.Button("Generate Chestplate"))
        {
            GenerateArmorPiece(ArmorType.Chestplate);
        }
        
        if (GUILayout.Button("Generate Leggings"))
        {
            GenerateArmorPiece(ArmorType.Leggings);
        }
        
        if (GUILayout.Button("Generate Gloves"))
        {
            GenerateArmorPiece(ArmorType.Gloves);
        }
        
        if (GUILayout.Button("Generate Boots"))
        {
            GenerateArmorPiece(ArmorType.Boots);
        }
        
        if (GUILayout.Button("Generate Shield"))
        {
            GenerateArmorPiece(ArmorType.Shield);
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Generate Full Armor Set"))
        {
            foreach (ArmorType type in System.Enum.GetValues(typeof(ArmorType)))
            {
                GenerateArmorPiece(type);
            }
        }
    }
    
    void DrawEnemiesTab()
    {
        GUILayout.Label("--- ENEMY MANAGEMENT ---", GUI.skin.box);
        
        // Count enemies in scene
        EnemyStats[] enemies = FindObjectsOfType<EnemyStats>();
        GUILayout.Label($"Enemies in Scene: {enemies.Length}");
        
        GUILayout.Space(10);
        
        GUILayout.Label("--- SPAWN ENEMIES ---", GUI.skin.box);
        
        if (GUILayout.Button("Spawn Level 1 Enemy"))
        {
            SpawnEnemy(1);
        }
        
        if (GUILayout.Button("Spawn Level 5 Enemy"))
        {
            SpawnEnemy(5);
        }
        
        if (GUILayout.Button("Spawn Level 10 Enemy"))
        {
            SpawnEnemy(10);
        }
        
        if (GUILayout.Button("Spawn 5 Random Enemies"))
        {
            for (int i = 0; i < 5; i++)
            {
                SpawnEnemy(Random.Range(1, 6));
            }
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Kill All Enemies"))
        {
            KillAllEnemies();
        }
        
        GUILayout.Space(10);
        
        // List enemies
        if (enemies.Length > 0)
        {
            GUILayout.Label("--- ACTIVE ENEMIES ---", GUI.skin.box);
            foreach (var enemy in enemies)
            {
                if (enemy != null)// && !enemy.IsDead())
                {
                    GUILayout.Label($"Level UNKNOWN Enemy - {Vector3.Distance(transform.position, enemy.transform.position):F1}m away");//{enemy.GetLevel()}
                }
            }
        }
    }
    
    void DrawProjectilesTab()
    {
        GUILayout.Label("--- PROJECTILE SYSTEMS ---", GUI.skin.box);
        
        ProjectileSystem projSys = FindObjectOfType<ProjectileSystem>();
        RangedWeaponHandler ranged = GetComponent<RangedWeaponHandler>();
        
        if (projSys != null)
        {
            GUILayout.Label("ProjectileSystem: Active ✓");
        }
        else
        {
            GUILayout.Label("ERROR: No ProjectileSystem in scene!");
        }
        
        if (ranged != null)
        {
            GUILayout.Label("RangedWeaponHandler: Active ✓");
        }
        else
        {
            GUILayout.Label("ERROR: No RangedWeaponHandler on player!");
        }
        
        GUILayout.Space(10);
        
        GUILayout.Label("--- TEST PROJECTILES ---", GUI.skin.box);
        
        if (projSys != null)
        {
            if (GUILayout.Button("Fire Arrow"))
            {
                TestFireProjectile(ProjectileType.Arrow);
            }
            
            if (GUILayout.Button("Fire Thrown Axe"))
            {
                TestFireProjectile(ProjectileType.ThrownAxe);
            }
            
            if (GUILayout.Button("Fire Bullet"))
            {
                TestFireProjectile(ProjectileType.Bullet);
            }
            
            if (GUILayout.Button("Fire Energy Beam"))
            {
                TestFireProjectile(ProjectileType.EnergyBeam);
            }
            
            if (GUILayout.Button("Fire Psi Blast (Homing)"))
            {
                TestFireProjectile(ProjectileType.PsiBlast);
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Fire 10 Arrows Rapidly"))
            {
                StartCoroutine(RapidFireTest(ProjectileType.Arrow, 10));
            }
        }
        
        GUILayout.Space(10);
        
        GUILayout.Label("--- PROJECTILE INFO ---", GUI.skin.box);
        GUILayout.Label("Arrow: Arc trajectory + gravity");
        GUILayout.Label("Thrown Axe: Rotation + arc");
        GUILayout.Label("Bullet: Fast + minimal drop");
        GUILayout.Label("Energy: Instant raycast");
        GUILayout.Label("Psi: Homing projectile");
    }
    
    // ==================== HELPER METHODS ====================
    
    void CheckComponent<T>(string name) where T : Component
    {
        T component = GetComponent<T>();
        string status = component != null ? "✓" : "✗ MISSING";
        GUILayout.Label($"{name}: {status}");
    }
    
    void CheckSceneComponent<T>(string name) where T : Object
    {
        T component = FindObjectOfType<T>();
        string status = component != null ? "✓" : "✗ MISSING";
        GUILayout.Label($"{name}: {status}");
    }
    
    void RunFullSystemCheck()
    {
        Debug.Log("=== FULL SYSTEM CHECK ===");
        
        // Check player components
        Debug.Log($"PlayerStats: {GetComponent<PlayerStats>() != null}");
        Debug.Log($"InventoryManager: {GetComponent<InventoryManager>() != null}");
        Debug.Log($"ExperienceManager: {GetComponent<ExperienceManager>() != null}");
        Debug.Log($"ThirdPersonController: {GetComponent<ThirdPersonController>() != null}");
        Debug.Log($"CombatAnimationController: {GetComponent<CombatAnimationController>() != null}");
        Debug.Log($"RangedWeaponHandler: {GetComponent<RangedWeaponHandler>() != null}");
        
        // Check scene managers
        Debug.Log($"makeItems: {FindObjectOfType<makeItems>() != null}");
        Debug.Log($"ProceduralWeaponModels: {FindObjectOfType<ProceduralWeaponModels>() != null}");
        Debug.Log($"ProceduralArmorModels: {FindObjectOfType<ProceduralArmorModels>() != null}");
        Debug.Log($"ProjectileSystem: {FindObjectOfType<ProjectileSystem>() != null}");
        
        Debug.Log("Check complete! See Console for results.");
    }
    
    void AddRandomItem(ItemType type)
    {
        makeItems gen = FindObjectOfType<makeItems>();
        InventoryManager inv = GetComponent<InventoryManager>();
        PlayerStats stats = GetComponent<PlayerStats>();
        
        if (gen != null && inv != null && stats != null)
        {
            int level = stats.GetLevel();
            
            if (type == ItemType.Weapon)
            {
                Weapon weapon = gen.GenerateWeapon(level);
                inv.AddItem(weapon);
                Debug.Log($"Added {weapon.rarity} {weapon.itemName}");
            }
            else if (type == ItemType.Armor)
            {
                Armor armor = gen.GenerateArmor(level);
                inv.AddItem(armor);
                Debug.Log($"Added {armor.rarity} {armor.itemName}");
            }
        }
    }
    
    void AddWeaponOfRarity(ItemRarity targetRarity)
    {
        makeItems gen = FindObjectOfType<makeItems>();
        InventoryManager inv = GetComponent<InventoryManager>();
        PlayerStats stats = GetComponent<PlayerStats>();
        
        if (gen != null && inv != null && stats != null)
        {
            // Keep generating until we get the desired rarity
            Weapon weapon = null;
            int attempts = 0;
            while (attempts < 100)
            {
                weapon = gen.GenerateWeapon(stats.GetLevel());
                if (weapon.rarity == targetRarity) break;
                attempts++;
            }
            
            if (weapon != null)
            {
                inv.AddItem(weapon);
                Debug.Log($"Added {weapon.rarity} {weapon.itemName}");
            }
        }
    }
    
    void ClearInventory()
    {
        InventoryManager inv = GetComponent<InventoryManager>();
        if (inv != null)
        {
            var items = inv.GetAllItems();
            foreach (var item in items)
            {
                inv.RemoveItem(item);
            }
            Debug.Log("Inventory cleared");
        }
    }
    
    void GenerateAndEquipWeapon(WeaponType type)
    {
        makeItems gen = FindObjectOfType<makeItems>();
        InventoryManager inv = GetComponent<InventoryManager>();
        PlayerStats stats = GetComponent<PlayerStats>();
        
        if (gen != null && inv != null && stats != null)
        {
            Weapon weapon = gen.GenerateWeapon(stats.GetLevel());
            weapon.weaponType = type;
            weapon.itemName = $"{type} of Testing";
            
            inv.AddItem(weapon);
            inv.EquipItem(weapon);
            
            Debug.Log($"Generated and equipped {weapon.itemName}");
        }
    }
    
    void GenerateArmorPiece(ArmorType type)
    {
        makeItems gen = FindObjectOfType<makeItems>();
        InventoryManager inv = GetComponent<InventoryManager>();
        PlayerStats stats = GetComponent<PlayerStats>();
        
        if (gen != null && inv != null && stats != null)
        {
            Armor armor = gen.GenerateArmor(stats.GetLevel());
            armor.armorType = type;
            armor.itemName = $"{type} of Testing";
            
            inv.AddItem(armor);
            
            Debug.Log($"Generated {armor.itemName}");
        }
    }
    
    void ListWeaponModels()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int count = 0;
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("_Model"))
            {
                count++;
                Debug.Log($"Model: {obj.name} - Active: {obj.activeSelf} - Parent: {obj.transform.parent?.name}");
            }
        }
        
        Debug.Log($"Found {count} weapon models in scene");
    }
    
    void SpawnEnemy(int level)
    {
        Vector3 spawnPos = transform.position + transform.forward * spawnDistance + Vector3.up;
        
        // Try to find enemy prefab if not assigned
        if (enemyPrefab == null)
        {
            // Look for existing enemy in scene to use as template
            EnemyStats existingEnemy = FindObjectOfType<EnemyStats>();
            if (existingEnemy != null)
            {
                enemyPrefab = existingEnemy.gameObject;
            }
        }
        
        if (enemyPrefab != null)
        {
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            EnemyStats stats = enemy.GetComponent<EnemyStats>();
            
            // Set level using reflection since it's private
            if (stats != null)
            {
                var field = typeof(EnemyStats).GetField("level", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(stats, level);
                }
            }
            
            Debug.Log($"Spawned level {level} enemy at {spawnPos}");
        }
        else
        {
            Debug.LogError("No enemy prefab assigned! Add one in the inspector or have an enemy in the scene.");
        }
    }
    
    void KillAllEnemies()
    {
        EnemyStats[] enemies = FindObjectsOfType<EnemyStats>();
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        Debug.Log($"Killed {enemies.Length} enemies");
    }
    
    void TestFireProjectile(ProjectileType type)
    {
        ProjectileSystem projSys = FindObjectOfType<ProjectileSystem>();
        PlayerStats stats = GetComponent<PlayerStats>();
        
        if (projSys != null && stats != null)
        {
            Vector3 origin = transform.position + Vector3.up * 1.5f + transform.forward * 0.5f;
            Vector3 direction = transform.forward;
            float damage = stats.GetTotalDamage();
            
            projSys.FireProjectile(type, origin, direction, damage, false, LayerMask.GetMask("Enemy"), transform);
            
            Debug.Log($"Fired {type} projectile");
        }
    }
    
    System.Collections.IEnumerator RapidFireTest(ProjectileType type, int count)
    {
        for (int i = 0; i < count; i++)
        {
            TestFireProjectile(type);
            yield return new WaitForSeconds(0.1f);
        }
    }
}