// ProceduralWeaponModels.cs
// Generates 3D weapon models that can be held by player/NPCs
// Add this to GameManager alongside other generators

using UnityEngine;

public class ProceduralWeaponModels : MonoBehaviour
{
    [Header("Weapon Materials")]
    [SerializeField] private Material bladeMaterial;
    [SerializeField] private Material handleMaterial;
    [SerializeField] private Material metalMaterial;
    [SerializeField] private Material woodMaterial;
    
    [Header("Weapon Settings")]
    [SerializeField] private bool useRarityColors = true;
    [SerializeField] private float glowIntensity = 1.5f;
    
    void Awake()
    {
        //Debug.Log("=== ProceduralWeaponModels.Awake() CALLED ===");
        
        // Create default materials if not assigned
        if (bladeMaterial == null)
        {
            bladeMaterial = CreateDefaultMaterial(new Color(0.8f, 0.8f, 0.9f));
            //Debug.Log("Created default blade material");
        }
        if (handleMaterial == null)
        {
            handleMaterial = CreateDefaultMaterial(new Color(0.4f, 0.3f, 0.2f));
            //Debug.Log("Created default handle material");
        }
        if (metalMaterial == null)
        {
            metalMaterial = CreateDefaultMaterial(new Color(0.6f, 0.6f, 0.7f));
            //Debug.Log("Created default metal material");
        }
        if (woodMaterial == null)
        {
            woodMaterial = CreateDefaultMaterial(new Color(0.5f, 0.35f, 0.2f));
            //Debug.Log("Created default wood material");
        }
        
        //Debug.Log("ProceduralWeaponModels initialization complete!");
    }
    
    Material CreateDefaultMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        mat.SetFloat("_Metallic", 0.5f);
        mat.SetFloat("_Glossiness", 0.6f);
        return mat;
    }
    
    // Generate weapon model and attach to weapon data
    public GameObject GenerateWeaponModel(Weapon weapon)
    {
        //Debug.Log($">>> GenerateWeaponModel START for {weapon.itemName} (Type: {weapon.weaponType})");
        
        GameObject weaponModel = null;
        
        try
        {
            weaponModel = new GameObject($"{weapon.itemName}_Model");
            //Debug.Log($"Created GameObject: {weaponModel.name}");
            
            // CRITICAL: Make model persistent across scenes/reloads
            DontDestroyOnLoad(weaponModel);
            //Debug.Log("Applied DontDestroyOnLoad");
            
            // Get rarity color
            Color rarityColor = weapon.GetRarityColor();
            //Debug.Log($"Rarity color: {rarityColor}");
        
        switch (weapon.weaponType)
        {
            case WeaponType.Sword:
                //Debug.Log("Creating sword model...");
                CreateSwordModel(weaponModel, rarityColor, weapon.rarity);
                break;
            case WeaponType.Axe:
                //Debug.Log("Creating axe model...");
                CreateAxeModel(weaponModel, rarityColor, weapon.rarity);
                break;
            case WeaponType.Mace:
                //Debug.Log("Creating mace model...");
                CreateMaceModel(weaponModel, rarityColor, weapon.rarity);
                break;
            case WeaponType.Dagger:
                //Debug.Log("Creating dagger model...");
                CreateDaggerModel(weaponModel, rarityColor, weapon.rarity);
                break;
            case WeaponType.Staff:
                //Debug.Log("Creating staff model...");
                CreateStaffModel(weaponModel, rarityColor, weapon.rarity);
                break;
            case WeaponType.Bow:
                //Debug.Log("Creating bow model...");
                CreateBowModel(weaponModel, rarityColor, weapon.rarity);
                break;
        }
        
        //Debug.Log($"Model creation complete. Children: {weaponModel.transform.childCount}");
        
        // Add glow for rare+ items
        if (weapon.rarity >= ItemRarity.Rare && useRarityColors)
        {
            //Debug.Log("Adding weapon glow...");
            AddWeaponGlow(weaponModel, rarityColor);
        }
        
        // Store reference in weapon
        weapon.weaponModel = weaponModel;
        //Debug.Log("Stored model reference in weapon");
        
        // Disable initially (will be enabled when equipped)
        weaponModel.SetActive(false);
        //Debug.Log("Set model to inactive");
        
        //Debug.Log($"<<< GenerateWeaponModel COMPLETE: {weaponModel.name} with {weaponModel.transform.childCount} children");
        
        return weaponModel;
        }
        catch (System.Exception e)
        {
            //Debug.LogError($"EXCEPTION in GenerateWeaponModel: {e.Message}");
            //Debug.LogError($"Stack trace: {e.StackTrace}");
            return null;
        }
    }
    
    void CreateSwordModel(GameObject parent, Color rarityColor, ItemRarity rarity)
    {
        // Blade
        GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
        blade.name = "Blade";
        blade.transform.SetParent(parent.transform);
        blade.transform.localPosition = new Vector3(0, 0.5f, 0);
        blade.transform.localScale = new Vector3(0.1f, 1.0f, 0.05f);
        
        Material bladeMat = new Material(bladeMaterial);
        bladeMat.color = Color.Lerp(bladeMat.color, rarityColor, 0.3f);
        if (rarity >= ItemRarity.Epic)
            bladeMat.EnableKeyword("_EMISSION");
        blade.GetComponent<Renderer>().material = bladeMat;
        
        // Blade tip (sharp point)
        GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tip.name = "BladeTip";
        tip.transform.SetParent(parent.transform);
        tip.transform.localPosition = new Vector3(0, 1.05f, 0);
        tip.transform.localScale = new Vector3(0.08f, 0.1f, 0.04f);
        tip.GetComponent<Renderer>().material = bladeMat;
        
        // Crossguard
        GameObject guard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        guard.name = "Crossguard";
        guard.transform.SetParent(parent.transform);
        guard.transform.localPosition = new Vector3(0, 0, 0);
        guard.transform.localScale = new Vector3(0.4f, 0.08f, 0.08f);
        guard.GetComponent<Renderer>().material = metalMaterial;
        
        // Handle
        GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        handle.name = "Handle";
        handle.transform.SetParent(parent.transform);
        handle.transform.localPosition = new Vector3(0, -0.2f, 0);
        handle.transform.localScale = new Vector3(0.06f, 0.2f, 0.06f);
        handle.GetComponent<Renderer>().material = handleMaterial;
        
        // Pommel
        GameObject pommel = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pommel.name = "Pommel";
        pommel.transform.SetParent(parent.transform);
        pommel.transform.localPosition = new Vector3(0, -0.42f, 0);
        pommel.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        pommel.GetComponent<Renderer>().material = metalMaterial;
    }
    
    void CreateAxeModel(GameObject parent, Color rarityColor, ItemRarity rarity)
    {
        // Handle
        GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        handle.name = "Handle";
        handle.transform.SetParent(parent.transform);
        handle.transform.localPosition = new Vector3(0, 0, 0);
        handle.transform.localScale = new Vector3(0.05f, 0.6f, 0.05f);
        handle.GetComponent<Renderer>().material = woodMaterial;
        
        // Axe head base
        GameObject axeBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        axeBase.name = "AxeBase";
        axeBase.transform.SetParent(parent.transform);
        axeBase.transform.localPosition = new Vector3(0, 0.6f, 0);
        axeBase.transform.localScale = new Vector3(0.15f, 0.1f, 0.1f);
        
        Material axeMat = new Material(metalMaterial);
        axeMat.color = Color.Lerp(axeMat.color, rarityColor, 0.3f);
        axeBase.GetComponent<Renderer>().material = axeMat;
        
        // Blade
        GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
        blade.name = "Blade";
        blade.transform.SetParent(parent.transform);
        blade.transform.localPosition = new Vector3(0.2f, 0.65f, 0);
        blade.transform.localScale = new Vector3(0.3f, 0.2f, 0.05f);
        blade.transform.localRotation = Quaternion.Euler(0, 0, 15);
        blade.GetComponent<Renderer>().material = axeMat;
    }
    
    void CreateMaceModel(GameObject parent, Color rarityColor, ItemRarity rarity)
    {
        // Handle
        GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        handle.name = "Handle";
        handle.transform.SetParent(parent.transform);
        handle.transform.localPosition = new Vector3(0, 0, 0);
        handle.transform.localScale = new Vector3(0.05f, 0.5f, 0.05f);
        handle.GetComponent<Renderer>().material = woodMaterial;
        
        // Mace head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "MaceHead";
        head.transform.SetParent(parent.transform);
        head.transform.localPosition = new Vector3(0, 0.65f, 0);
        head.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        
        Material maceMat = new Material(metalMaterial);
        maceMat.color = Color.Lerp(maceMat.color, rarityColor, 0.3f);
        head.GetComponent<Renderer>().material = maceMat;
        
        // Spikes
        for (int i = 0; i < 6; i++)
        {
            GameObject spike = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spike.name = $"Spike{i}";
            spike.transform.SetParent(parent.transform);
            
            float angle = i * 60f;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * 0.15f;
            
            spike.transform.localPosition = new Vector3(0, 0.65f, 0) + offset;
            spike.transform.localScale = new Vector3(0.05f, 0.15f, 0.05f);
            spike.transform.LookAt(parent.transform.position + new Vector3(0, 0.65f, 0) + offset * 2);
            spike.GetComponent<Renderer>().material = maceMat;
        }
    }
    
    void CreateDaggerModel(GameObject parent, Color rarityColor, ItemRarity rarity)
    {
        // Blade (shorter than sword)
        GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
        blade.name = "Blade";
        blade.transform.SetParent(parent.transform);
        blade.transform.localPosition = new Vector3(0, 0.25f, 0);
        blade.transform.localScale = new Vector3(0.08f, 0.5f, 0.04f);
        
        Material bladeMat = new Material(bladeMaterial);
        bladeMat.color = Color.Lerp(bladeMat.color, rarityColor, 0.3f);
        blade.GetComponent<Renderer>().material = bladeMat;
        
        // Sharp tip
        GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tip.name = "Tip";
        tip.transform.SetParent(parent.transform);
        tip.transform.localPosition = new Vector3(0, 0.52f, 0);
        tip.transform.localScale = new Vector3(0.06f, 0.08f, 0.03f);
        tip.GetComponent<Renderer>().material = bladeMat;
        
        // Small guard
        GameObject guard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        guard.name = "Guard";
        guard.transform.SetParent(parent.transform);
        guard.transform.localPosition = new Vector3(0, 0, 0);
        guard.transform.localScale = new Vector3(0.2f, 0.05f, 0.05f);
        guard.GetComponent<Renderer>().material = metalMaterial;
        
        // Wrapped handle
        GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        handle.name = "Handle";
        handle.transform.SetParent(parent.transform);
        handle.transform.localPosition = new Vector3(0, -0.12f, 0);
        handle.transform.localScale = new Vector3(0.045f, 0.12f, 0.045f);
        handle.GetComponent<Renderer>().material = handleMaterial;
    }
    
    void CreateStaffModel(GameObject parent, Color rarityColor, ItemRarity rarity)
    {
        // Long staff shaft
        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shaft.name = "Shaft";
        shaft.transform.SetParent(parent.transform);
        shaft.transform.localPosition = new Vector3(0, 0.4f, 0);
        shaft.transform.localScale = new Vector3(0.05f, 0.8f, 0.05f);
        shaft.GetComponent<Renderer>().material = woodMaterial;
        
        // Magical orb at top
        GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        orb.name = "MagicOrb";
        orb.transform.SetParent(parent.transform);
        orb.transform.localPosition = new Vector3(0, 1.25f, 0);
        orb.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        
        Material orbMat = new Material(Shader.Find("Standard"));
        orbMat.color = rarityColor;
        orbMat.EnableKeyword("_EMISSION");
        orbMat.SetColor("_EmissionColor", rarityColor * glowIntensity);
        orb.GetComponent<Renderer>().material = orbMat;
        
        // Orb holder rings
        for (int i = 0; i < 3; i++)
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            if (ring == null) ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = $"Ring{i}";
            ring.transform.SetParent(parent.transform);
            ring.transform.localPosition = new Vector3(0, 1.15f + i * 0.05f, 0);
            ring.transform.localScale = new Vector3(0.15f, 0.02f, 0.15f);
            ring.GetComponent<Renderer>().material = metalMaterial;
        }
    }
    
    void CreateBowModel(GameObject parent, Color rarityColor, ItemRarity rarity)
    {
        // Bow body (curved)
        GameObject upperLimb = GameObject.CreatePrimitive(PrimitiveType.Cube);
        upperLimb.name = "UpperLimb";
        upperLimb.transform.SetParent(parent.transform);
        upperLimb.transform.localPosition = new Vector3(-0.1f, 0.4f, 0);
        upperLimb.transform.localScale = new Vector3(0.05f, 0.6f, 0.05f);
        upperLimb.transform.localRotation = Quaternion.Euler(0, 0, 15);
        upperLimb.GetComponent<Renderer>().material = woodMaterial;
        
        GameObject lowerLimb = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lowerLimb.name = "LowerLimb";
        lowerLimb.transform.SetParent(parent.transform);
        lowerLimb.transform.localPosition = new Vector3(-0.1f, -0.4f, 0);
        lowerLimb.transform.localScale = new Vector3(0.05f, 0.6f, 0.05f);
        lowerLimb.transform.localRotation = Quaternion.Euler(0, 0, -15);
        lowerLimb.GetComponent<Renderer>().material = woodMaterial;
        
        // Grip
        GameObject grip = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        grip.name = "Grip";
        grip.transform.SetParent(parent.transform);
        grip.transform.localPosition = new Vector3(0, 0, 0);
        grip.transform.localScale = new Vector3(0.06f, 0.15f, 0.06f);
        grip.GetComponent<Renderer>().material = handleMaterial;
        
        // String (thin cylinder)
        GameObject bowstring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bowstring.name = "String";
        bowstring.transform.SetParent(parent.transform);
        bowstring.transform.localPosition = new Vector3(-0.18f, 0, 0);
        bowstring.transform.localScale = new Vector3(0.01f, 0.9f, 0.01f);
        
        Material stringMat = new Material(Shader.Find("Standard"));
        stringMat.color = new Color(0.9f, 0.9f, 0.85f);
        bowstring.GetComponent<Renderer>().material = stringMat;
    }
    
    void AddWeaponGlow(GameObject weapon, Color glowColor)
    {
        // Add point light for glow effect
        GameObject lightObj = new GameObject("WeaponGlow");
        lightObj.transform.SetParent(weapon.transform);
        lightObj.transform.localPosition = Vector3.zero;
        
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = glowColor;
        light.intensity = glowIntensity;
        light.range = 3f;
        light.shadows = LightShadows.None;
    }
    
    // Helper to attach weapon to hand
    public void AttachWeaponToHand(GameObject weaponModel, Transform handTransform)
    {
        if (weaponModel == null || handTransform == null) return;
        
        weaponModel.transform.SetParent(handTransform);
        weaponModel.transform.localPosition = Vector3.zero;
        weaponModel.transform.localRotation = Quaternion.identity;
        weaponModel.SetActive(true);
    }
    
    // Helper to detach weapon
    public void DetachWeapon(GameObject weaponModel)
    {
        if (weaponModel == null) return;
        
        weaponModel.transform.SetParent(null);
        weaponModel.SetActive(false);
    }
}