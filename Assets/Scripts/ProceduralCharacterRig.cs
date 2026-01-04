// ProceduralCharacterRig.cs - FIXED VERSION
// Now properly integrates with existing systems WITHOUT breaking changes
// Auto-wires all references, maintains compatibility with existing scripts

using UnityEngine;
using System.Collections.Generic;

public class ProceduralCharacterRig : MonoBehaviour
{
    [Header("Rig Configuration")]
    [SerializeField] private bool isEnemy = false;
    [SerializeField] private Color skinColor = Color.white;
    [SerializeField] private float bodyScale = 1f;
    
    [Header("Bone Dimensions")]
    [SerializeField] private float headRadius = 0.15f;
    [SerializeField] private float headHeight = 0.3f;
    [SerializeField] private float torsoRadius = 0.18f;
    [SerializeField] private float torsoHeight = 0.5f;
    [SerializeField] private float pelvisRadius = 0.15f;
    [SerializeField] private float pelvisHeight = 0.25f;
    [SerializeField] private float upperArmRadius = 0.08f;
    [SerializeField] private float upperArmLength = 0.3f;
    [SerializeField] private float lowerArmRadius = 0.06f;
    [SerializeField] private float lowerArmLength = 0.25f;
    [SerializeField] private float upperLegRadius = 0.1f;
    [SerializeField] private float upperLegLength = 0.4f;
    [SerializeField] private float lowerLegRadius = 0.08f;
    [SerializeField] private float lowerLegLength = 0.4f;
    
    [Header("Auto-Generated References")]
    public Transform rightHandBone;
    public Transform leftHandBone;
    public Transform weaponAttachPoint;
    public Transform headBone;
    public Transform chestBone;
    public Transform pelvisBone;
    
    // Bone references
    private Transform root;
    private Transform pelvis;
    private Transform spine;
    private Transform chest;
    private Transform neck;
    private Transform head;
    
    private Transform leftShoulder;
    private Transform leftUpperArm;
    private Transform leftLowerArm;
    private Transform leftHand;
    
    private Transform rightShoulder;
    private Transform rightUpperArm;
    private Transform rightLowerArm;
    private Transform rightHand;
    
    private Transform leftHip;
    private Transform leftUpperLeg;
    private Transform leftLowerLeg;
    private Transform leftFoot;
    
    private Transform rightHip;
    private Transform rightUpperLeg;
    private Transform rightLowerLeg;
    private Transform rightFoot;
    
    // Reference dictionaries for easy access
    private Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
    private Dictionary<ArmorType, Transform> armorBones = new Dictionary<ArmorType, Transform>();
    
    void Awake()
    {
        // Only generate if no children exist (prevents re-generation)
        if (transform.childCount == 0)
        {
            GenerateCompleteRig();
            AutoWireReferences();
        }
        else
        {
            // If rig already exists, just map the references
            MapExistingRig();
        }
    }
    
    public void GenerateCompleteRig()
    {
        // Clear existing children
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        
        // Create root bone
        root = CreateBone("Root", Vector3.zero, Quaternion.identity, 0.1f, 0.1f, transform);
        root.localPosition = Vector3.zero;
        
        // Create pelvis
        pelvis = CreateBone("Pelvis", Vector3.up * 1f, Quaternion.identity, pelvisRadius, pelvisHeight, root);
        
        // Create spine chain
        spine = CreateBone("Spine", Vector3.up * pelvisHeight, Quaternion.identity, torsoRadius * 0.9f, torsoHeight * 0.4f, pelvis);
        chest = CreateBone("Chest", Vector3.up * (torsoHeight * 0.4f), Quaternion.identity, torsoRadius, torsoHeight * 0.6f, spine);
        neck = CreateBone("Neck", Vector3.up * (torsoHeight * 0.6f), Quaternion.identity, headRadius * 0.7f, 0.15f, chest);
        head = CreateBone("Head", Vector3.up * 0.15f, Quaternion.identity, headRadius, headHeight, neck);
        
        // Create left arm
        leftShoulder = CreateBone("LeftShoulder", new Vector3(-torsoRadius * 1.2f, torsoHeight * 0.5f, 0), 
            Quaternion.Euler(0, 0, -10), 0.06f, 0.1f, chest);
        leftUpperArm = CreateBone("LeftUpperArm", Vector3.down * 0.1f, 
            Quaternion.Euler(0, 0, -90), upperArmRadius, upperArmLength, leftShoulder);
        leftLowerArm = CreateBone("LeftLowerArm", Vector3.right * upperArmLength, 
            Quaternion.identity, lowerArmRadius, lowerArmLength, leftUpperArm);
        leftHand = CreateBone("LeftHand", Vector3.right * lowerArmLength, 
            Quaternion.identity, 0.05f, 0.1f, leftLowerArm);
        
        // Create right arm
        rightShoulder = CreateBone("RightShoulder", new Vector3(torsoRadius * 1.2f, torsoHeight * 0.5f, 0), 
            Quaternion.Euler(0, 0, 10), 0.06f, 0.1f, chest);
        rightUpperArm = CreateBone("RightUpperArm", Vector3.down * 0.1f, 
            Quaternion.Euler(0, 0, 90), upperArmRadius, upperArmLength, rightShoulder);
        rightLowerArm = CreateBone("RightLowerArm", Vector3.left * upperArmLength, 
            Quaternion.identity, lowerArmRadius, lowerArmLength, rightUpperArm);
        rightHand = CreateBone("RightHand", Vector3.left * lowerArmLength, 
            Quaternion.identity, 0.05f, 0.1f, rightLowerArm);
        
        // Create left leg
        leftHip = CreateBone("LeftHip", new Vector3(-pelvisRadius * 0.6f, -pelvisHeight * 0.5f, 0), 
            Quaternion.identity, 0.08f, 0.1f, pelvis);
        leftUpperLeg = CreateBone("LeftUpperLeg", Vector3.down * 0.1f, 
            Quaternion.identity, upperLegRadius, upperLegLength, leftHip);
        leftLowerLeg = CreateBone("LeftLowerLeg", Vector3.down * upperLegLength, 
            Quaternion.identity, lowerLegRadius, lowerLegLength, leftUpperLeg);
        leftFoot = CreateBone("LeftFoot", Vector3.down * lowerLegLength, 
            Quaternion.Euler(90, 0, 0), 0.08f, 0.15f, leftLowerLeg);
        
        // Create right leg
        rightHip = CreateBone("RightHip", new Vector3(pelvisRadius * 0.6f, -pelvisHeight * 0.5f, 0), 
            Quaternion.identity, 0.08f, 0.1f, pelvis);
        rightUpperLeg = CreateBone("RightUpperLeg", Vector3.down * 0.1f, 
            Quaternion.identity, upperLegRadius, upperLegLength, rightHip);
        rightLowerLeg = CreateBone("RightLowerLeg", Vector3.down * upperLegLength, 
            Quaternion.identity, lowerLegRadius, lowerLegLength, rightUpperLeg);
        rightFoot = CreateBone("RightFoot", Vector3.down * lowerLegLength, 
            Quaternion.Euler(90, 0, 0), 0.08f, 0.15f, rightLowerLeg);
        
        // Build bone map
        BuildBoneMap();
        
        // Setup armor attachment points
        SetupArmorBones();
        
        // Scale entire rig
        root.localScale = Vector3.one * bodyScale;
    }
    
    Transform CreateBone(string boneName, Vector3 localPos, Quaternion localRot, float radius, float height, Transform parent)
    {
        GameObject boneObj = new GameObject(boneName);
        boneObj.transform.SetParent(parent);
        boneObj.transform.localPosition = localPos;
        boneObj.transform.localRotation = localRot;
        boneObj.layer = parent.gameObject.layer; // Inherit layer from parent
        
        // Create capsule mesh
        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.name = boneName + "_Mesh";
        capsule.transform.SetParent(boneObj.transform);
        capsule.transform.localPosition = Vector3.up * (height * 0.5f);
        capsule.transform.localRotation = Quaternion.identity;
        capsule.transform.localScale = new Vector3(radius * 2, height * 0.5f, radius * 2);
        capsule.layer = parent.gameObject.layer;
        
        // Remove the default collider from the primitive (we'll add our own)
        Destroy(capsule.GetComponent<Collider>());
        
        // Setup mesh renderer
        Renderer renderer = capsule.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = skinColor;
        }
        
        // Setup capsule collider on parent bone
        CapsuleCollider collider = boneObj.AddComponent<CapsuleCollider>();
        collider.center = Vector3.up * (height * 0.5f);
        collider.radius = radius;
        collider.height = height;
        collider.direction = 1; // Y-axis
        
        // Add rigidbody for physics (kinematic by default)
        Rigidbody rb = boneObj.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        
        return boneObj.transform;
    }
    
    void BuildBoneMap()
    {
        boneMap.Clear();
        
        // Add all bones to map
        boneMap["Root"] = root;
        boneMap["Pelvis"] = pelvis;
        boneMap["Spine"] = spine;
        boneMap["Chest"] = chest;
        boneMap["Neck"] = neck;
        boneMap["Head"] = head;
        
        boneMap["LeftShoulder"] = leftShoulder;
        boneMap["LeftUpperArm"] = leftUpperArm;
        boneMap["LeftLowerArm"] = leftLowerArm;
        boneMap["LeftHand"] = leftHand;
        
        boneMap["RightShoulder"] = rightShoulder;
        boneMap["RightUpperArm"] = rightUpperArm;
        boneMap["RightLowerArm"] = rightLowerArm;
        boneMap["RightHand"] = rightHand;
        
        boneMap["LeftHip"] = leftHip;
        boneMap["LeftUpperLeg"] = leftUpperLeg;
        boneMap["LeftLowerLeg"] = leftLowerLeg;
        boneMap["LeftFoot"] = leftFoot;
        
        boneMap["RightHip"] = rightHip;
        boneMap["RightUpperLeg"] = rightUpperLeg;
        boneMap["RightLowerLeg"] = rightLowerLeg;
        boneMap["RightFoot"] = rightFoot;
    }
    
    void SetupArmorBones()
    {
        armorBones.Clear();
        armorBones[ArmorType.Helmet] = head;
        armorBones[ArmorType.Chestplate] = chest;
        armorBones[ArmorType.Leggings] = pelvis;
        armorBones[ArmorType.Gloves] = rightHand;
        armorBones[ArmorType.Boots] = rightFoot;
        armorBones[ArmorType.Shield] = leftLowerArm;
    }
    
    // AUTO-WIRE REFERENCES FOR EXISTING SYSTEMS
    void AutoWireReferences()
    {
        // Set public references that other scripts can access
        rightHandBone = rightHand;
        leftHandBone = leftHand;
        headBone = head;
        chestBone = chest;
        pelvisBone = pelvis;
        
        // Create weapon attach point
        GameObject attachObj = new GameObject("WeaponAttachPoint");
        attachObj.transform.SetParent(rightHand);
        attachObj.transform.localPosition = new Vector3(0, 0.1f, 0);
        attachObj.transform.localRotation = Quaternion.Euler(0, 0, 90);
        weaponAttachPoint = attachObj.transform;
        
        // Wire up CombatAnimationController if it exists
        CombatAnimationController combat = GetComponent<CombatAnimationController>();
        if (combat != null)
        {
            // Use reflection to set private fields
            var rightHandField = typeof(CombatAnimationController).GetField("rightHandBone", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var leftHandField = typeof(CombatAnimationController).GetField("leftHandBone", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var weaponPointField = typeof(CombatAnimationController).GetField("weaponAttachPoint", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (rightHandField != null) rightHandField.SetValue(combat, rightHandBone);
            if (leftHandField != null) leftHandField.SetValue(combat, leftHandBone);
            if (weaponPointField != null) weaponPointField.SetValue(combat, weaponAttachPoint);
            
            Debug.Log("✅ Auto-wired CombatAnimationController references");
        }
        
        Debug.Log("✅ Auto-wired all bone references");
    }
    
    void MapExistingRig()
    {
        // If rig already exists, find and map bones
        Transform[] children = GetComponentsInChildren<Transform>();
        
        foreach (Transform child in children)
        {
            string name = child.name;
            
            if (name.Contains("RightHand")) rightHandBone = child;
            else if (name.Contains("LeftHand")) leftHandBone = child;
            else if (name.Contains("Head")) headBone = child;
            else if (name.Contains("Chest")) chestBone = child;
            else if (name.Contains("Pelvis")) pelvisBone = child;
            else if (name.Contains("WeaponAttachPoint")) weaponAttachPoint = child;
        }
        
        // If weapon attach point doesn't exist, create it
        if (weaponAttachPoint == null && rightHandBone != null)
        {
            GameObject attachObj = new GameObject("WeaponAttachPoint");
            attachObj.transform.SetParent(rightHandBone);
            attachObj.transform.localPosition = new Vector3(0, 0.1f, 0);
            attachObj.transform.localRotation = Quaternion.Euler(0, 0, 90);
            weaponAttachPoint = attachObj.transform;
        }
    }
    
    // Public accessors
    public Transform GetBone(string boneName)
    {
        boneMap.TryGetValue(boneName, out Transform bone);
        return bone;
    }
    
    public Transform GetWeaponBone()
    {
        return rightHand != null ? rightHand : rightHandBone;
    }
    
    public Transform GetArmorBone(ArmorType armorType)
    {
        armorBones.TryGetValue(armorType, out Transform bone);
        return bone;
    }
    
    public Transform GetHead() => head != null ? head : headBone;
    public Transform GetChest() => chest != null ? chest : chestBone;
    public Transform GetPelvis() => pelvis != null ? pelvis : pelvisBone;
    public Transform GetLeftHand() => leftHand != null ? leftHand : leftHandBone;
    public Transform GetRightHand() => rightHand != null ? rightHand : rightHandBone;
    public Transform GetLeftFoot() => leftFoot;
    public Transform GetRightFoot() => rightFoot;
    
    // Change skin color dynamically
    public void SetSkinColor(Color color)
    {
        skinColor = color;
        
        // Update all bone renderers
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer.material != null)
            {
                renderer.material.color = skinColor;
            }
        }
    }
    
    // For debugging
    void OnDrawGizmos()
    {
        if (boneMap == null || boneMap.Count == 0) return;
        
        Gizmos.color = Color.yellow;
        foreach (var bone in boneMap.Values)
        {
            if (bone != null)
            {
                Gizmos.DrawWireSphere(bone.position, 0.05f);
            }
        }
    }
}