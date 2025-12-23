// ProceduralArmorModels.cs
// Generates 3D armor models that attach to player/NPC body parts
// Add this alongside ProceduralWeaponModels on GameManager

using UnityEngine;

public class ProceduralArmorModels : MonoBehaviour
{
    [Header("Armor Materials")]
    [SerializeField] private Material leatherMaterial;
    [SerializeField] private Material chainmailMaterial;
    [SerializeField] private Material plateMaterial;
    [SerializeField] private Material clothMaterial;
    
    [Header("Armor Settings")]
    [SerializeField] private bool useRarityColors = true;
    [SerializeField] private float glowIntensity = 1.5f;
    
    void Awake()
    {
        // Create default materials if not assigned
        if (leatherMaterial == null)
        {
            leatherMaterial = CreateDefaultMaterial(new Color(0.4f, 0.3f, 0.2f));
        }
        if (chainmailMaterial == null)
        {
            chainmailMaterial = CreateDefaultMaterial(new Color(0.6f, 0.6f, 0.6f));
        }
        if (plateMaterial == null)
        {
            plateMaterial = CreateDefaultMaterial(new Color(0.7f, 0.7f, 0.8f));
        }
        if (clothMaterial == null)
        {
            clothMaterial = CreateDefaultMaterial(new Color(0.3f, 0.3f, 0.4f));
        }
    }
    
    Material CreateDefaultMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        mat.SetFloat("_Metallic", 0.5f);
        mat.SetFloat("_Glossiness", 0.6f);
        return mat;
    }
    
    // Main method to generate armor model
    public GameObject GenerateArmorModel(Armor armor)
    {
        GameObject armorModel = new GameObject($"{armor.itemName}_Model");
        DontDestroyOnLoad(armorModel);
        
        Color rarityColor = armor.GetRarityColor();
        
        switch (armor.armorType)
        {
            case ArmorType.Helmet:
                CreateHelmetModel(armorModel, rarityColor, armor.rarity);
                break;
            case ArmorType.Chestplate:
                CreateChestplateModel(armorModel, rarityColor, armor.rarity);
                break;
            case ArmorType.Leggings:
                CreateLeggingsModel(armorModel, rarityColor, armor.rarity);
                break;
            case ArmorType.Boots:
                CreateBootsModel(armorModel, rarityColor, armor.rarity);
                break;
            case ArmorType.Gloves:
                CreateGlovesModel(armorModel, rarityColor, armor.rarity);
                break;
            case ArmorType.Shield:
                CreateShieldModel(armorModel, rarityColor, armor.rarity);
                break;
        }
        
        // Add glow for rare+ items
        if (armor.rarity >= ItemRarity.Rare && useRarityColors)
        {
            AddArmorGlow(armorModel, rarityColor);
        }
        
        armor.armorModel = armorModel;
        armorModel.SetActive(false);
        
        return armorModel;
    }
    
    void CreateHelmetModel(GameObject parent, Color rarityColor, ItemRarity rarity)
    {
        // Main helmet dome
        GameObject dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dome.name = "HelmetDome";
        dome.transform.SetParent(parent.transform);
        dome.transform.localPosition = Vector3.zero;
        dome.transform.localScale = new Vector3(0.3f, 0.35f, 0.35f);
        
        Material helmetMat = GetArmorMaterial(rarity);
        helmetMat.color = Color.Lerp(helmetMat.color, rarityColor, 0.2f);
        dome.GetComponent<Renderer>().material = helmetMat;
        
        // Face guard
        GameObject faceGuard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        faceGuard.name = "FaceGuard";
        faceGuard.transform.SetParent(parent.transform);
        faceGuard.transform.localPosition = new Vector3(0, -0.05f, 0.15f);
        faceGuard.transform.localScale = new Vector3(0.25f, 0.2f, 0.05f);
        faceGuard.GetComponent<Renderer>().material = helmetMat;
        
        // Crest/plume for epic+ helmets
        if (rarity >= ItemRarity.Epic)
        {
            GameObject crest = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crest.name = "Crest";
            crest.transform.SetParent(parent.transform);
            crest.transform.localPosition = new Vector3(0, 0.25f, -0.05f);
            crest.transform.localScale = new Vector3(0.08f, 0.2f, 0.15f);
            
            Material crestMat = new Material(Shader.Find("Standard"));
            crestMat.color = rarityColor;
            crest.GetComponent<Renderer>().material = crestMat;
        }
    }
    
    void CreateChestplateModel(GameObject parent, Color rarityColor, ItemRarity rarity)
    {
        // Main chest piece
        GameObject chest = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chest.name = "ChestPiece";
        chest.transform.SetParent(parent.transform);
        chest.transform.localPosition = Vector3.zero;
        chest.transform.localScale = new Vector3(0.5f, 0.6f, 0.25f);
        
        Material chestMat = GetArmorMaterial(rarity);
        chestMat.color = Color.Lerp(chestMat.color, rarityColor, 0.2f);
        chest.GetComponent<Renderer>().material = chestMat;
        
        // Shoulder pads
        CreateShoulderPad(parent, new Vector3(0.3f, 0.25f, 0), chestMat, "LeftShoulder");
        CreateShoulderPad(parent, new Vector3(-0.3f, 0.25f, 0), chestMat, "RightShoulder");
        
        // Belt/lower chest
        GameObject belt = GameObject.CreatePrimitive(PrimitiveType.Cube);
        belt.name = "Belt";
        belt.transform.SetParent(parent.transform);
        belt.transform.localPosition = new Vector3(0, -0.35f, 0);
        belt.transform.localScale = new Vector3(0.52f, 0.1f, 0.26f);
        
        Material beltMat = new Material(chestMat);
        beltMat.color = Color.Lerp(beltMat.color, Color.black, 0.3f);
        belt.GetComponent<Renderer>().material = beltMat;
        
        // Decorative emblem for rare+
        if (rarity >= ItemRarity.Rare)
        {
            GameObject emblem = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            emblem.name = "Emblem";
            emblem.transform.SetParent(parent.transform);
            emblem.transform.localPosition = new Vector3(0, 0.1f, 0.14f);
            emblem.transform.localScale = new Vector3(0.15f, 0.15f, 0.05f);
            
            Material emblemMat = new Material(Shader.Find("Standard"));
            emblemMat.color = rarityColor;
            emblemMat.EnableKeyword("_EMISSION");
            emblemMat.SetColor("_EmissionColor", rarityColor * 0.5f);
            emblem.GetComponent<Renderer>().material = emblemMat;
        }
    }
    
    void CreateShoulderPad(GameObject parent, Vector3 position, Material mat, string name)
    {
        GameObject shoulder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        shoulder.name = name;
        shoulder.transform.SetParent(parent.transform);
        shoulder.transform.localPosition = position;
        shoulder.transform.localScale = new Vector3(0.2f, 0.25f, 0.2f);
        shoulder.GetComponent<Renderer>().material = mat;
        
        // Pauldron spike
        GameObject spike = GameObject.CreatePrimitive(PrimitiveType.Cube);
        spike.name = name + "Spike";
        spike.transform.SetParent(parent.transform);
        spike.transform.localPosition = position + new Vector3(0, 0.15f, 0);
        spike.transform.localScale = new Vector3(0.08f, 0.15f, 0.08f);
        spike.GetComponent<Renderer>().material = mat;
    }
    
    void CreateLeggingsModel(GameObject parent, Color rarityColor, ItemRarity rarity)
    {
        Material legMat = GetArmorMaterial(rarity);
        legMat.color = Color.Lerp(legMat.color, rarityColor, 0.2f);
        
        // Left leg armor
        CreateLegPiece(parent, new Vector3(0.12f, -0.2f, 0), legMat, "LeftThigh");
        CreateLegPiece(parent, new Vector3(0.12f, -0.5f, 0), legMat, "LeftShin");
        
        // Right leg armor
        CreateLegPiece(parent, new Vector3(-0.12f, -0.2f, 0), legMat, "RightThigh");
        CreateLegPiece(parent, new Vector3(-0.12f, -0.5f, 0), legMat, "RightShin");
        
        // Knee guards
        CreateKneeGuard(parent, new Vector3(0.12f, -0.35f, 0.08f), legMat, "LeftKnee");
        CreateKneeGuard(parent, new Vector3(-0.12f, -0.35f, 0.08f), legMat, "RightKnee");
    }
    
    void CreateLegPiece(GameObject parent, Vector3 position, Material mat, string name)
    {
        GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        leg.name = name;
        leg.transform.SetParent(parent.transform);
        leg.transform.localPosition = position;
        leg.transform.localScale = new Vector3(0.12f, 0.15f, 0.12f);
        leg.GetComponent<Renderer>().material = mat;
    }
    
    void CreateKneeGuard(GameObject parent, Vector3 position, Material mat, string name)
    {
        GameObject knee = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        knee.name = name;
        knee.transform.SetParent(parent.transform);
        knee.transform.localPosition = position;
        knee.transform.localScale = new Vector3(0.15f, 0.12f, 0.15f);
        knee.GetComponent<Renderer>().material = mat;
    }
    
    void CreateBootsModel(GameObject parent, Color rarityColor, ItemRarity rarity)
    {
        Material bootMat = GetArmorMaterial(rarity);
        bootMat.color = Color.Lerp(bootMat.color, rarityColor, 0.2f);
        
        // Left boot
        CreateBoot(parent, new Vector3(0.12f, 0, 0), bootMat, "LeftBoot");
        
        // Right boot
        CreateBoot(parent, new Vector3(-0.12f, 0, 0), bootMat, "RightBoot");
    }
    
    void CreateBoot(GameObject parent, Vector3 position, Material mat, string name)
    {
        // Boot base
        GameObject boot = GameObject.CreatePrimitive(PrimitiveType.Cube);
        boot.name = name;
        boot.transform.SetParent(parent.transform);
        boot.transform.localPosition = position;
        boot.transform.localScale = new Vector3(0.15f, 0.2f, 0.25f);
        boot.GetComponent<Renderer>().material = mat;
        
        // Toe cap
        GameObject toe = GameObject.CreatePrimitive(PrimitiveType.Cube);
        toe.name = name + "Toe";
        toe.transform.SetParent(parent.transform);
        toe.transform.localPosition = position + new Vector3(0, -0.05f, 0.15f);
        toe.transform.localScale = new Vector3(0.14f, 0.1f, 0.08f);
        toe.GetComponent<Renderer>().material = mat;
    }
    
    void CreateGlovesModel(GameObject parent, Color rarityColor, ItemRarity rarity)
    {
        Material gloveMat = GetArmorMaterial(rarity);
        gloveMat.color = Color.Lerp(gloveMat.color, rarityColor, 0.2f);
        
        // Left glove
        CreateGlove(parent, new Vector3(0.12f, 0, 0), gloveMat, "LeftGlove");
        
        // Right glove
        CreateGlove(parent, new Vector3(-0.12f, 0, 0), gloveMat, "RightGlove");
    }
    
    void CreateGlove(GameObject parent, Vector3 position, Material mat, string name)
    {
        // Palm/hand
        GameObject hand = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hand.name = name;
        hand.transform.SetParent(parent.transform);
        hand.transform.localPosition = position;
        hand.transform.localScale = new Vector3(0.1f, 0.15f, 0.08f);
        hand.GetComponent<Renderer>().material = mat;
        
        // Wrist guard
        GameObject wrist = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        wrist.name = name + "Wrist";
        wrist.transform.SetParent(parent.transform);
        wrist.transform.localPosition = position + new Vector3(0, 0.1f, 0);
        wrist.transform.localScale = new Vector3(0.09f, 0.05f, 0.09f);
        wrist.GetComponent<Renderer>().material = mat;
    }
    
    void CreateShieldModel(GameObject parent, Color rarityColor, ItemRarity rarity)
    {
        Material shieldMat = GetArmorMaterial(rarity);
        shieldMat.color = Color.Lerp(shieldMat.color, rarityColor, 0.2f);
        
        // Shield face
        GameObject face = GameObject.CreatePrimitive(PrimitiveType.Cube);
        face.name = "ShieldFace";
        face.transform.SetParent(parent.transform);
        face.transform.localPosition = Vector3.zero;
        face.transform.localScale = new Vector3(0.5f, 0.6f, 0.08f);
        face.GetComponent<Renderer>().material = shieldMat;
        
        // Shield boss (center)
        GameObject boss = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        boss.name = "ShieldBoss";
        boss.transform.SetParent(parent.transform);
        boss.transform.localPosition = new Vector3(0, 0, 0.06f);
        boss.transform.localScale = new Vector3(0.15f, 0.15f, 0.1f);
        boss.GetComponent<Renderer>().material = shieldMat;
        
        // Border trim
        CreateShieldBorder(parent, shieldMat);
        
        // Emblem for rare+
        if (rarity >= ItemRarity.Rare)
        {
            GameObject emblem = GameObject.CreatePrimitive(PrimitiveType.Cube);
            emblem.name = "ShieldEmblem";
            emblem.transform.SetParent(parent.transform);
            emblem.transform.localPosition = new Vector3(0, 0.15f, 0.05f);
            emblem.transform.localScale = new Vector3(0.2f, 0.15f, 0.02f);
            
            Material emblemMat = new Material(Shader.Find("Standard"));
            emblemMat.color = rarityColor;
            emblemMat.EnableKeyword("_EMISSION");
            emblemMat.SetColor("_EmissionColor", rarityColor);
            emblem.GetComponent<Renderer>().material = emblemMat;
        }
    }
    
    void CreateShieldBorder(GameObject parent, Material mat)
    {
        // Top border
        CreateBorderPiece(parent, new Vector3(0, 0.32f, 0), new Vector3(0.52f, 0.04f, 0.09f), mat, "TopBorder");
        // Bottom border
        CreateBorderPiece(parent, new Vector3(0, -0.32f, 0), new Vector3(0.52f, 0.04f, 0.09f), mat, "BottomBorder");
        // Left border
        CreateBorderPiece(parent, new Vector3(-0.27f, 0, 0), new Vector3(0.04f, 0.6f, 0.09f), mat, "LeftBorder");
        // Right border
        CreateBorderPiece(parent, new Vector3(0.27f, 0, 0), new Vector3(0.04f, 0.6f, 0.09f), mat, "RightBorder");
    }
    
    void CreateBorderPiece(GameObject parent, Vector3 position, Vector3 scale, Material mat, string name)
    {
        GameObject border = GameObject.CreatePrimitive(PrimitiveType.Cube);
        border.name = name;
        border.transform.SetParent(parent.transform);
        border.transform.localPosition = position;
        border.transform.localScale = scale;
        
        Material borderMat = new Material(mat);
        borderMat.color = Color.Lerp(borderMat.color, Color.yellow, 0.3f);
        border.GetComponent<Renderer>().material = borderMat;
    }
    
    Material GetArmorMaterial(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common:
            case ItemRarity.Uncommon:
                return new Material(leatherMaterial);
            case ItemRarity.Rare:
                return new Material(chainmailMaterial);
            case ItemRarity.Epic:
            case ItemRarity.Legendary:
                return new Material(plateMaterial);
            default:
                return new Material(leatherMaterial);
        }
    }
    
    void AddArmorGlow(GameObject armor, Color glowColor)
    {
        GameObject lightObj = new GameObject("ArmorGlow");
        lightObj.transform.SetParent(armor.transform);
        lightObj.transform.localPosition = Vector3.zero;
        
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = glowColor;
        light.intensity = glowIntensity * 0.5f;
        light.range = 2f;
        light.shadows = LightShadows.None;
    }
    
    // Helper to attach armor to body part
    public void AttachArmorToBone(GameObject armorModel, Transform boneTransform, ArmorType type)
    {
        if (armorModel == null || boneTransform == null) return;
        
        armorModel.transform.SetParent(boneTransform);
        armorModel.transform.localPosition = GetArmorOffset(type);
        armorModel.transform.localRotation = Quaternion.identity;
        armorModel.SetActive(true);
    }
    
    Vector3 GetArmorOffset(ArmorType type)
    {
        switch (type)
        {
            case ArmorType.Helmet:
                return new Vector3(0, 0.1f, 0);
            case ArmorType.Chestplate:
                return Vector3.zero;
            case ArmorType.Leggings:
                return new Vector3(0, -0.2f, 0);
            case ArmorType.Boots:
                return new Vector3(0, -0.1f, 0);
            case ArmorType.Gloves:
                return Vector3.zero;
            case ArmorType.Shield:
                return new Vector3(-0.2f, 0, 0.1f);
            default:
                return Vector3.zero;
        }
    }
    
    public void DetachArmor(GameObject armorModel)
    {
        if (armorModel == null) return;
        
        armorModel.transform.SetParent(null);
        armorModel.SetActive(false);
    }
}