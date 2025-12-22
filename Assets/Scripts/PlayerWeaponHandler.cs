// PlayerWeaponHandler.cs
// Attach to Player - manages equipped weapon display
// Handles showing/hiding weapons on the player model

using UnityEngine;

public class PlayerWeaponHandler : MonoBehaviour
{
    [Header("Hand Attachments")]
    [SerializeField] private Transform rightHandTransform;
    [SerializeField] private Transform leftHandTransform;
    
    [Header("Weapon Positioning")]
    [SerializeField] private Vector3 weaponPositionOffset = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 weaponRotationOffset = new Vector3(0, 0, 0);
    
    [Header("Sheathed Position (Optional)")]
    [SerializeField] private Transform sheathTransform;
    [SerializeField] private bool autoSheath = false;
    
    private GameObject currentWeaponModel;
    private PlayerStats playerStats;
    private ProceduralWeaponModels weaponModels;
    private bool weaponDrawn = true;
    
    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        weaponModels = FindFirstObjectByType<ProceduralWeaponModels>();
        
        // Create hand transforms if they don't exist
        if (rightHandTransform == null)
        {
            CreateHandTransforms();
        }
        
        // Listen for equipment changes
        if (playerStats != null)
        {
            // Check for equipped weapon on start
            UpdateEquippedWeapon();
        }
    }
    
    void CreateHandTransforms()
    {
        // Create right hand transform
        GameObject rightHand = new GameObject("RightHand");
        rightHand.transform.SetParent(transform);
        rightHand.transform.localPosition = new Vector3(0.3f, 1.0f, 0.4f);
        rightHand.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        rightHandTransform = rightHand.transform;
        
        // Create left hand transform  
        GameObject leftHand = new GameObject("LeftHand");
        leftHand.transform.SetParent(transform);
        leftHand.transform.localPosition = new Vector3(-0.3f, 1.0f, 0.4f);
        leftHand.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        leftHandTransform = leftHand.transform;
        
        //Debug.Log("Created hand attachment points");
    }
    
    void Update()
    {
        // Optional: Press X to sheath/unsheath weapon
        if (Input.GetKeyDown(KeyCode.X))
        {
            ToggleWeaponDrawn();
        }
        
        // Check if equipped weapon changed
        if (playerStats != null)
        {
            Weapon equipped = playerStats.GetEquippedWeapon();
            bool weaponChanged = (equipped != null && currentWeaponModel != equipped.weaponModel) ||
                               (equipped == null && currentWeaponModel != null);
            
            if (weaponChanged)
            {
                UpdateEquippedWeapon();
            }
        }
    }
    
    public void UpdateEquippedWeapon()
    {
        if (playerStats == null) return;
        
        // Remove current weapon
        if (currentWeaponModel != null)
        {
            if (weaponModels != null)
            {
                weaponModels.DetachWeapon(currentWeaponModel);
            }
            currentWeaponModel = null;
        }
        
        // Get newly equipped weapon
        Weapon equippedWeapon = playerStats.GetEquippedWeapon();
        
        if (equippedWeapon != null && equippedWeapon.weaponModel != null)
        {
            currentWeaponModel = equippedWeapon.weaponModel;
            
            // Attach to hand
            if (weaponModels != null && rightHandTransform != null)
            {
                weaponModels.AttachWeaponToHand(currentWeaponModel, rightHandTransform);
                
                // Apply offsets
                currentWeaponModel.transform.localPosition = weaponPositionOffset;
                currentWeaponModel.transform.localRotation = Quaternion.Euler(weaponRotationOffset);
                
                //Debug.Log($"Equipped weapon model: {equippedWeapon.itemName}");
            }
        }
    }
    
    public void ToggleWeaponDrawn()
    {
        weaponDrawn = !weaponDrawn;
        
        if (currentWeaponModel != null)
        {
            if (weaponDrawn)
            {
                // Draw weapon - attach to hand
                if (weaponModels != null && rightHandTransform != null)
                {
                    weaponModels.AttachWeaponToHand(currentWeaponModel, rightHandTransform);
                    currentWeaponModel.transform.localPosition = weaponPositionOffset;
                    currentWeaponModel.transform.localRotation = Quaternion.Euler(weaponRotationOffset);
                }
                //Debug.Log("Weapon drawn");
            }
            else if (sheathTransform != null)
            {
                // Sheath weapon - attach to back/hip
                currentWeaponModel.transform.SetParent(sheathTransform);
                currentWeaponModel.transform.localPosition = Vector3.zero;
                currentWeaponModel.transform.localRotation = Quaternion.identity;
                //Debug.Log("Weapon sheathed");
            }
        }
    }
    
    // Called when entering combat (optional)
    public void DrawWeapon()
    {
        if (!weaponDrawn)
        {
            ToggleWeaponDrawn();
        }
    }
    
    // Called when leaving combat (optional)
    public void SheathWeapon()
    {
        if (weaponDrawn && autoSheath)
        {
            ToggleWeaponDrawn();
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Visualize hand positions
        if (rightHandTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(rightHandTransform.position, 0.1f);
            Gizmos.DrawLine(rightHandTransform.position, rightHandTransform.position + rightHandTransform.forward * 0.3f);
        }
        
        if (leftHandTransform != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(leftHandTransform.position, 0.1f);
            Gizmos.DrawLine(leftHandTransform.position, leftHandTransform.position + leftHandTransform.forward * 0.3f);
        }
        
        if (sheathTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(sheathTransform.position, Vector3.one * 0.2f);
        }
    }
}