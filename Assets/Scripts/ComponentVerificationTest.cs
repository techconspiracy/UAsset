// ComponentVerificationTest.cs
// Attach to GameManager - verifies all components are present and functional
// This will tell us EXACTLY what's missing

using UnityEngine;

public class ComponentVerificationTest : MonoBehaviour
{
    void Start()
    {
        //Debug.Log("====================================");
        //Debug.Log("COMPONENT VERIFICATION TEST");
        //Debug.Log("====================================");
        
        // Test 1: Check this GameObject
        //Debug.Log($"This GameObject: {gameObject.name}");
        
        // Test 2: List ALL components on this GameObject
        Component[] allComponents = GetComponents<Component>();
        //Debug.Log($"Total components on {gameObject.name}: {allComponents.Length}");
        
        foreach (Component comp in allComponents)
        {
            //Debug.Log($"  - {comp.GetType().Name}");
        }
        
        //Debug.Log("---");
        
        // Test 3: Specifically check for our components
        makeItems itemGen = GetComponent<makeItems>();
        //Debug.Log($"makeItems: {(itemGen != null ? "✓ FOUND" : "✗ MISSING")}");
        
        ProceduralIconGenerator iconGen = GetComponent<ProceduralIconGenerator>();
        //Debug.Log($"ProceduralIconGenerator: {(iconGen != null ? "✓ FOUND" : "✗ MISSING")}");
        
        ProceduralWeaponModels modelGen = GetComponent<ProceduralWeaponModels>();
        //Debug.Log($"ProceduralWeaponModels: {(modelGen != null ? "✓ FOUND" : "✗ MISSING")}");
        
        WeaponGenerationDebug debugMon = GetComponent<WeaponGenerationDebug>();
        //Debug.Log($"WeaponGenerationDebug: {(debugMon != null ? "✓ FOUND" : "✗ MISSING")}");
        
        //Debug.Log("---");
        
        // Test 4: Try GetComponent in different ways
        if (modelGen == null)
        {
            //Debug.LogError("ProceduralWeaponModels is NULL via GetComponent!");
            
            // Try FindFirstObjectByType
            ProceduralWeaponModels foundModelGen = FindFirstObjectByType<ProceduralWeaponModels>();
            //Debug.Log($"FindFirstObjectByType result: {(foundModelGen != null ? "✓ FOUND" : "✗ MISSING")}");
            
            if (foundModelGen != null)
            {
                //Debug.Log($"Found on GameObject: {foundModelGen.gameObject.name}");
                //Debug.Log($"Is same GameObject? {foundModelGen.gameObject == gameObject}");
            }
        }
        else
        {
            //Debug.Log("ProceduralWeaponModels found successfully via GetComponent!");
        }
        
        //Debug.Log("---");
        
        // Test 5: Try to generate a weapon
        if (itemGen != null)
        {
            //Debug.Log("Testing weapon generation...");
            
            Weapon testWeapon = itemGen.GenerateWeapon(1);
            
            if (testWeapon != null)
            {
                //Debug.Log($"✓ Weapon generated: {testWeapon.itemName}");
                //Debug.Log($"  Type: {testWeapon.weaponType}");
                //Debug.Log($"  Has Model: {(testWeapon.weaponModel != null ? "✓ YES" : "✗ NO")}");
                
                if (testWeapon.weaponModel != null)
                {
                    //Debug.Log($"  Model Name: {testWeapon.weaponModel.name}");
                    //Debug.Log($"  Model Children: {testWeapon.weaponModel.transform.childCount}");
                }
            }
            else
            {
                //Debug.LogError("✗ GenerateWeapon returned NULL!");
            }
        }
        
        //Debug.Log("====================================");
        //Debug.Log("VERIFICATION TEST COMPLETE");
        //Debug.Log("====================================");
    }
}