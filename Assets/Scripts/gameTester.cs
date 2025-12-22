// GameTester.cs - Enhanced Debug Tool
// Attach to Player - helps test and debug all systems
// Shows on-screen debug info and test buttons

using UnityEngine;

public class GameTester : MonoBehaviour
{
    [Header("Debug Display")]
    [SerializeField] private bool showDebugUI = true;
    [SerializeField] private KeyCode toggleDebugKey = KeyCode.F1;
    
    private bool showWeaponDebug = true;
    
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
        
        GUILayout.BeginArea(new Rect(10, 10, 400, Screen.height - 20));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("=== GAME TESTER ===", GUI.skin.box);
        GUILayout.Label($"Press {toggleDebugKey} to toggle this window");
        GUILayout.Space(10);
        
        // Inventory Section
        DrawInventoryDebug();
        GUILayout.Space(10);
        
        // Collectibles Section
        DrawCollectiblesDebug();
        GUILayout.Space(10);
        
        // Stats Section
        DrawStatsDebug();
        GUILayout.Space(10);
        
        // Weapon Model Section
        DrawWeaponModelDebug();
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    void DrawInventoryDebug()
    {
        GUILayout.Label("--- INVENTORY ---", GUI.skin.box);
        
        InventoryManager inv = GetComponent<InventoryManager>();
        if (inv != null)
        {
            GUILayout.Label($"Items: {inv.GetItemCount()} / {inv.GetMaxSlots()}");
            
            if (GUILayout.Button("Add Test Weapon"))
            {
                AddTestWeapon();
            }
            
            if (GUILayout.Button("Add Test Armor"))
            {
                AddTestArmor();
            }
            
            if (GUILayout.Button("Clear Inventory"))
            {
                ClearInventory();
            }
        }
        else
        {
            GUILayout.Label("ERROR: No InventoryManager!", GUI.skin.box);
        }
    }
    
    void DrawCollectiblesDebug()
    {
        GUILayout.Label("--- COLLECTIBLES ---", GUI.skin.box);
        
        CollectibleManager col = GetComponent<CollectibleManager>();
        if (col != null)
        {
            GUILayout.Label($"Collected: {col.GetTotalCollected()}");
        }
        else
        {
            GUILayout.Label("No CollectibleManager");
        }
    }
    
    void DrawStatsDebug()
    {
        GUILayout.Label("--- PLAYER STATS ---", GUI.skin.box);
        
        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats != null)
        {
            GUILayout.Label($"Health: {stats.GetCurrentHealth():F0} / {stats.GetMaxHealth():F0}");
            GUILayout.Label($"Damage: {stats.GetTotalDamage():F0}");
            GUILayout.Label($"Armor: {stats.GetTotalArmor():F0}");
            GUILayout.Label($"Crit: {stats.GetCritChance():F1}%");
            
            Weapon equipped = stats.GetEquippedWeapon();
            if (equipped != null)
            {
                GUILayout.Label($"Weapon: {equipped.itemName}");
            }
            else
            {
                GUILayout.Label("No weapon equipped");
            }
        }
    }
    
    void DrawWeaponModelDebug()
    {
        GUILayout.Label("--- WEAPON MODEL DEBUG ---", GUI.skin.box);
        
        showWeaponDebug = GUILayout.Toggle(showWeaponDebug, "Show Details");
        
        if (!showWeaponDebug) return;
        
        // Check for components
        makeItems itemGen = FindFirstObjectByType<makeItems>();
        ProceduralWeaponModels modelGen = FindFirstObjectByType<ProceduralWeaponModels>();
        PlayerWeaponHandler weaponHandler = GetComponent<PlayerWeaponHandler>();
        PlayerStats stats = GetComponent<PlayerStats>();
        
        GUILayout.Label("Component Status:");
        GUILayout.Label($"  ItemGenerator: {(itemGen != null ? "✓" : "✗ MISSING")}");
        GUILayout.Label($"  ModelGenerator: {(modelGen != null ? "✓" : "✗ MISSING")}");
        GUILayout.Label($"  WeaponHandler: {(weaponHandler != null ? "✓" : "✗ MISSING")}");
        GUILayout.Label($"  PlayerStats: {(stats != null ? "✓" : "✗ MISSING")}");
        
        GUILayout.Space(5);
        
        // Check equipped weapon
        if (stats != null)
        {
            Weapon equipped = stats.GetEquippedWeapon();
            
            if (equipped != null)
            {
                GUILayout.Label($"Equipped Weapon: {equipped.itemName}");
                GUILayout.Label($"  Type: {equipped.weaponType}");
                GUILayout.Label($"  Rarity: {equipped.rarity}");
                GUILayout.Label($"  Has Model: {(equipped.weaponModel != null ? "✓" : "✗")}");
                
                if (equipped.weaponModel != null)
                {
                    GUILayout.Label($"  Model Active: {equipped.weaponModel.activeSelf}");
                    GUILayout.Label($"  Model Parent: {equipped.weaponModel.transform.parent?.name ?? "None"}");
                    GUILayout.Label($"  Model Pos: {equipped.weaponModel.transform.position}");
                    GUILayout.Label($"  Child Count: {equipped.weaponModel.transform.childCount}");
                }
                else
                {
                    GUILayout.Label("  ERROR: Weapon has no model!", GUI.skin.box);
                    
                    if (GUILayout.Button("Generate Model Now"))
                    {
                        if (modelGen != null)
                        {
                            equipped.weaponModel = modelGen.GenerateWeaponModel(equipped);
                            if (weaponHandler != null)
                            {
                                weaponHandler.UpdateEquippedWeapon();
                            }
                            //Debug.Log($"Generated model for {equipped.itemName}");
                        }
                    }
                }
            }
            else
            {
                GUILayout.Label("No weapon equipped");
            }
        }
        
        GUILayout.Space(5);
        
        // Check hand transforms
        if (weaponHandler != null)
        {
            Transform rightHand = weaponHandler.GetType().GetField("rightHandTransform", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(weaponHandler) as Transform;
            
            if (rightHand != null)
            {
                GUILayout.Label($"Right Hand: {rightHand.name}");
                GUILayout.Label($"  Position: {rightHand.position}");
                GUILayout.Label($"  Children: {rightHand.childCount}");
            }
            else
            {
                GUILayout.Label("ERROR: No right hand transform!", GUI.skin.box);
            }
        }
        
        GUILayout.Space(5);
        
        // Test buttons
        if (GUILayout.Button("Force Weapon Update"))
        {
            if (weaponHandler != null)
            {
                weaponHandler.UpdateEquippedWeapon();
                //Debug.Log("Forced weapon update");
            }
        }
        
        if (GUILayout.Button("Generate & Equip Test Sword"))
        {
            GenerateAndEquipTestWeapon(WeaponType.Sword);
        }
        
        if (GUILayout.Button("Generate & Equip Test Staff"))
        {
            GenerateAndEquipTestWeapon(WeaponType.Staff);
        }
        
        if (GUILayout.Button("List All Weapon Models in Scene"))
        {
            ListWeaponModelsInScene();
        }
    }
    
    void AddTestWeapon()
    {
        var gen = FindFirstObjectByType<makeItems>();
        var inv = GetComponent<InventoryManager>();
        
        if (gen != null && inv != null)
        {
            Weapon weapon = gen.GenerateWeapon(GetComponent<PlayerStats>()?.GetLevel() ?? 1);
            
            // Check if model was generated
            if (weapon.weaponModel == null)
            {
                //Debug.LogWarning($"Generated weapon {weapon.itemName} has NO MODEL!");
                
                // Try to generate it
                var modelGen = FindFirstObjectByType<ProceduralWeaponModels>();
                if (modelGen != null)
                {
                    weapon.weaponModel = modelGen.GenerateWeaponModel(weapon);
                    //Debug.Log($"Manually generated model for {weapon.itemName}");
                }
                else
                {
                    //Debug.LogError("ProceduralWeaponModels not found in scene!");
                }
            }
            
            inv.AddItem(weapon);
            //Debug.Log($"Added weapon: {weapon.itemName} (Type: {weapon.weaponType}, Has Model: {weapon.weaponModel != null})");
        }
        else
        {
            //Debug.LogError($"Missing: Generator={gen != null}, Inventory={inv != null}");
        }
    }
    
    void AddTestArmor()
    {
        var gen = FindFirstObjectByType<makeItems>();
        var inv = GetComponent<InventoryManager>();
        
        if (gen != null && inv != null)
        {
            Armor armor = gen.GenerateArmor(GetComponent<PlayerStats>()?.GetLevel() ?? 1);
            inv.AddItem(armor);
            //Debug.Log($"Added armor: {armor.itemName}");
        }
    }
    
    void ClearInventory()
    {
        var inv = GetComponent<InventoryManager>();
        if (inv != null)
        {
            var items = inv.GetAllItems();
            foreach (var item in items)
            {
                inv.RemoveItem(item);
            }
            //Debug.Log("Inventory cleared");
        }
    }
    
    void GenerateAndEquipTestWeapon(WeaponType type)
    {
        var gen = FindFirstObjectByType<makeItems>();
        var modelGen = FindFirstObjectByType<ProceduralWeaponModels>();
        var inv = GetComponent<InventoryManager>();
        var stats = GetComponent<PlayerStats>();
        
        if (gen == null || modelGen == null || inv == null || stats == null)
        {
            //Debug.LogError($"Missing components! Gen:{gen != null}, ModelGen:{modelGen != null}, Inv:{inv != null}, Stats:{stats != null}");
            return;
        }
        
        // Generate weapon
        Weapon weapon = gen.GenerateWeapon(stats.GetLevel());
        
        // Force the weapon type
        weapon.weaponType = type;
        weapon.itemName = $"Test {type}";
        
        // Generate model
        weapon.weaponModel = modelGen.GenerateWeaponModel(weapon);
        
        //Debug.Log($"Generated {weapon.itemName}");
        //Debug.Log($"  Model exists: {weapon.weaponModel != null}");
        
        if (weapon.weaponModel != null)
        {
            //Debug.Log($"  Model children: {weapon.weaponModel.transform.childCount}");
            //Debug.Log($"  Model active: {weapon.weaponModel.activeSelf}");
        }
        
        // Add to inventory
        inv.AddItem(weapon);
        
        // Equip immediately
        inv.EquipItem(weapon);
        
        //Debug.Log($"Equipped {weapon.itemName} - check your hand!");
    }
    
    void ListWeaponModelsInScene()
    {
        //Debug.Log("=== WEAPON MODELS IN SCENE ===");
        
        // Find all GameObjects with "_Model" in name
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int modelCount = 0;
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("_Model"))
            {
                modelCount++;
                //Debug.Log($"Found model: {obj.name}");
                //Debug.Log($"  Active: {obj.activeSelf}");
                //Debug.Log($"  Parent: {obj.transform.parent?.name ?? "None"}");
                //Debug.Log($"  Position: {obj.transform.position}");
                //Debug.Log($"  Children: {obj.transform.childCount}");
            }
        }
        
        //Debug.Log($"Total weapon models found: {modelCount}");
        
        if (modelCount == 0)
        {
            //Debug.LogWarning("NO WEAPON MODELS FOUND IN SCENE!");
            //Debug.LogWarning("This means models are not being generated.");
            //Debug.LogWarning("Check if ProceduralWeaponModels is on GameManager.");
        }
    }
}