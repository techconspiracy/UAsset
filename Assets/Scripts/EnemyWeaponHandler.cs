// EnemyWeaponHandler.cs
// Attach to Enemy - gives enemies random weapons
// Makes combat more visual and varied

using UnityEngine;

public class EnemyWeaponHandler : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private bool giveRandomWeapon = true;
    [SerializeField] private float weaponChance = 0.7f; // 70% chance to have weapon
    
    [Header("Hand Attachment")]
    [SerializeField] private Transform rightHandTransform;
    [SerializeField] private Vector3 weaponPositionOffset = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 weaponRotationOffset = new Vector3(0, 0, 0);
    
    private GameObject equippedWeaponModel;
    private ProceduralWeaponModels weaponModels;
    private makeItems itemGenerator;
    
    void Start()
    {
        weaponModels = FindFirstObjectByType<ProceduralWeaponModels>();
        itemGenerator = FindFirstObjectByType<makeItems>();
        
        // Create hand transform if missing
        if (rightHandTransform == null)
        {
            CreateHandTransform();
        }
        
        // Give enemy a weapon
        if (giveRandomWeapon && Random.value < weaponChance)
        {
            EquipRandomWeapon();
        }
    }
    
    void CreateHandTransform()
    {
        GameObject rightHand = new GameObject("RightHand");
        rightHand.transform.SetParent(transform);
        rightHand.transform.localPosition = new Vector3(0.3f, 0.5f, 0.4f);
        rightHand.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        rightHandTransform = rightHand.transform;
    }
    
    void EquipRandomWeapon()
    {
        if (itemGenerator == null || weaponModels == null) return;
        
        // Get enemy level
        EnemyStats stats = GetComponent<EnemyStats>();
        int enemyLevel = stats != null ? 1 : 1; // Use enemy level if available
        
        // Generate random weapon
        Weapon weapon = itemGenerator.GenerateWeapon(enemyLevel);
        
        if (weapon != null && weapon.weaponModel != null)
        {
            equippedWeaponModel = weapon.weaponModel;
            
            // Attach to hand
            weaponModels.AttachWeaponToHand(equippedWeaponModel, rightHandTransform);
            
            // Apply offsets
            equippedWeaponModel.transform.localPosition = weaponPositionOffset;
            equippedWeaponModel.transform.localRotation = Quaternion.Euler(weaponRotationOffset);
            
            //Debug.Log($"Enemy equipped: {weapon.itemName}");
        }
    }
    
    public void DropWeaponOnDeath()
    {
        // When enemy dies, the weapon will be dropped as loot
        // This is handled by EnemyStats DropLoot()
        if (equippedWeaponModel != null)
        {
            equippedWeaponModel.SetActive(false);
        }
    }
    
    void OnDestroy()
    {
        // Clean up weapon model
        if (equippedWeaponModel != null)
        {
            Destroy(equippedWeaponModel);
        }
    }
}