// WeaponGeneration//Debug.cs
// Attach to GameManager - monitors weapon generation
// Shows exactly when and how weapons are created

using UnityEngine;

public class WeaponGenerationDebug : MonoBehaviour
{
    private int weaponsGenerated = 0;
    private int modelsCreated = 0;
    private int modelsLost = 0;
    
    void Start()
    {
        //Debug.Log("=== WEAPON GENERATION DEBUG ACTIVE ===");
        
        // Check components exist
        makeItems itemGen = GetComponent<makeItems>();
        ProceduralWeaponModels modelGen = GetComponent<ProceduralWeaponModels>();
        
        //Debug.Log($"ItemGenerator present: {itemGen != null}");
        //Debug.Log($"ModelGenerator present: {modelGen != null}");
        
        // Monitor generation
        InvokeRepeating(nameof(CheckWeaponModels), 1f, 2f);
    }
    
    void CheckWeaponModels()
    {
        // Find all weapon models in scene
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        int foundModels = 0;
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("_Model"))
            {
                foundModels++;
            }
        }
        
        if (foundModels != modelsCreated)
        {
            //Debug.LogWarning($"MODEL MISMATCH! Created: {modelsCreated}, Found in scene: {foundModels}");
            modelsLost = modelsCreated - foundModels;
        }
    }
    
    // Call this when weapon is generated
    public void OnWeaponGenerated(Weapon weapon)
    {
        weaponsGenerated++;
        
        //Debug.Log($"[{weaponsGenerated}] Weapon Generated: {weapon.itemName}");
        //Debug.Log($"  Type: {weapon.weaponType}");
        //Debug.Log($"  Has Model: {weapon.weaponModel != null}");
        
        if (weapon.weaponModel != null)
        {
            modelsCreated++;
            //Debug.Log($"  Model Name: {weapon.weaponModel.name}");
            //Debug.Log($"  Model Active: {weapon.weaponModel.activeSelf}");
            //Debug.Log($"  Model Children: {weapon.weaponModel.transform.childCount}");
            //Debug.Log($"  Model Parent: {weapon.weaponModel.transform.parent?.name ?? "None"}");
        }
        else
        {
            //Debug.LogError($"  ERROR: Weapon generated WITHOUT MODEL!");
            
            // Try to figure out why
            ProceduralWeaponModels modelGen = GetComponent<ProceduralWeaponModels>();
            if (modelGen == null)
            {
                //Debug.LogError("  Reason: ProceduralWeaponModels is MISSING!");
            }
            else
            {
                //Debug.LogError("  Reason: GenerateWeaponModel was not called or failed!");
            }
        }
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(Screen.width - 310, 10, 300, 200));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("=== WEAPON GEN DEBUG ===");
        GUILayout.Label($"Weapons Generated: {weaponsGenerated}");
        GUILayout.Label($"Models Created: {modelsCreated}");
        GUILayout.Label($"Models Lost: {modelsLost}");
        
        if (modelsLost > 0)
        {
            GUILayout.Label($"WARNING: {modelsLost} models disappeared!", GUI.skin.box);
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}