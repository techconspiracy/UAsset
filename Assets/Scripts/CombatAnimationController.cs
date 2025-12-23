// CombatAnimationController.cs
// Handles all combat animations, weapon swinging, and procedural animation
// Attach to Player GameObject alongside ThirdPersonController

using UnityEngine;

public class CombatAnimationController : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float attackAnimationSpeed = 1f;
    [SerializeField] private float meleeSwingDuration = 0.4f;
    [SerializeField] private float rangedDrawDuration = 0.6f;
    
    [Header("Weapon Swing Settings")]
    [SerializeField] private AnimationCurve swingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private Vector3 meleeSwingStartRotation = new Vector3(-90, 0, 0);
    [SerializeField] private Vector3 meleeSwingEndRotation = new Vector3(90, 0, 0);
    
    [Header("References")]
    [SerializeField] private Transform rightHandBone;
    [SerializeField] private Transform leftHandBone;
    [SerializeField] private Transform weaponAttachPoint;
    
    private Animator animator;
    private PlayerStats playerStats;
    private GameObject currentWeaponModel;
    private WeaponType currentWeaponType;
    
    private bool isAttacking = false;
    private float attackTimer = 0f;
    
    // Animation parameter IDs
    private int animIDAttackType;
    private int animIDIsAttacking;
    private int animIDAttackSpeed;
    
    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        playerStats = GetComponent<PlayerStats>();
        
        if (animator != null)
        {
            animIDAttackType = Animator.StringToHash("AttackType");
            animIDIsAttacking = Animator.StringToHash("IsAttacking");
            animIDAttackSpeed = Animator.StringToHash("AttackSpeed");
        }
        
        // Find hand bones if not assigned
        if (rightHandBone == null)
        {
            rightHandBone = FindBone("RightHand");
        }
        if (leftHandBone == null)
        {
            leftHandBone = FindBone("LeftHand");
        }
        
        // Create weapon attach point if not exists
        if (weaponAttachPoint == null && rightHandBone != null)
        {
            GameObject attachObj = new GameObject("WeaponAttachPoint");
            attachObj.transform.SetParent(rightHandBone);
            attachObj.transform.localPosition = new Vector3(0, 0.1f, 0);
            attachObj.transform.localRotation = Quaternion.Euler(0, 0, 90);
            weaponAttachPoint = attachObj.transform;
        }
    }
    
    Transform FindBone(string boneName)
    {
        // Search through all children for the bone
        Transform[] bones = GetComponentsInChildren<Transform>();
        foreach (Transform bone in bones)
        {
            if (bone.name.Contains(boneName))
            {
                return bone;
            }
        }
        return null;
    }
    
    void Update()
    {
        if (isAttacking)
        {
            attackTimer += Time.deltaTime;
            
            // Perform weapon swing animation
            if (currentWeaponModel != null)
            {
                AnimateWeaponSwing();
            }
            
            // Check if attack is complete
            float attackDuration = GetAttackDuration(currentWeaponType);
            if (attackTimer >= attackDuration)
            {
                EndAttack();
            }
        }
    }
    
    public void EquipWeapon(Weapon weapon)
    {
        // Remove old weapon
        if (currentWeaponModel != null)
        {
            currentWeaponModel.SetActive(false);
        }
        
        // Equip new weapon
        if (weapon != null && weapon.weaponModel != null)
        {
            currentWeaponModel = weapon.weaponModel;
            currentWeaponType = weapon.weaponType;
            
            // Attach to hand
            currentWeaponModel.transform.SetParent(weaponAttachPoint);
            currentWeaponModel.transform.localPosition = Vector3.zero;
            currentWeaponModel.transform.localRotation = Quaternion.identity;
            currentWeaponModel.SetActive(true);
            
            Debug.Log($"Equipped {weapon.itemName} to hand");
        }
    }
    
    public void PerformAttack(WeaponType weaponType)
    {
        if (isAttacking) return;
        
        isAttacking = true;
        attackTimer = 0f;
        currentWeaponType = weaponType;
        
        // Trigger animator
        if (animator != null)
        {
            animator.SetInteger(animIDAttackType, (int)weaponType);
            animator.SetBool(animIDIsAttacking, true);
            animator.SetFloat(animIDAttackSpeed, attackAnimationSpeed);
        }
        
        // Play attack sound/effects here
        Debug.Log($"Performing {weaponType} attack");
    }
    
    void AnimateWeaponSwing()
    {
        if (currentWeaponModel == null || weaponAttachPoint == null) return;
        
        float progress = attackTimer / GetAttackDuration(currentWeaponType);
        float curveValue = swingCurve.Evaluate(progress);
        
        switch (currentWeaponType)
        {
            case WeaponType.Sword:
            case WeaponType.Axe:
            case WeaponType.Mace:
                AnimateMeleeSwing(curveValue);
                break;
            case WeaponType.Dagger:
                AnimateDaggerStab(curveValue);
                break;
            case WeaponType.Staff:
                AnimateStaffCast(curveValue);
                break;
            case WeaponType.Bow:
                AnimateBowDraw(curveValue);
                break;
        }
    }
    
    void AnimateMeleeSwing(float progress)
    {
        // Rotate weapon from start to end position
        Quaternion startRot = Quaternion.Euler(meleeSwingStartRotation);
        Quaternion endRot = Quaternion.Euler(meleeSwingEndRotation);
        
        weaponAttachPoint.localRotation = Quaternion.Lerp(startRot, endRot, progress);
    }
    
    void AnimateDaggerStab(float progress)
    {
        // Quick forward thrust
        Vector3 stabOffset = Vector3.forward * Mathf.Sin(progress * Mathf.PI) * 0.5f;
        weaponAttachPoint.localPosition = stabOffset;
    }
    
    void AnimateStaffCast(float progress)
    {
        // Raise staff overhead
        float angle = Mathf.Lerp(0, 45, progress);
        weaponAttachPoint.localRotation = Quaternion.Euler(-angle, 0, 0);
        
        // Add glow effect at peak
        if (progress > 0.5f && progress < 0.7f)
        {
            // Trigger particle effects here
        }
    }
    
    void AnimateBowDraw(float progress)
    {
        // Pull bowstring back
        if (progress < 0.7f)
        {
            // Drawing phase
            float drawAmount = progress / 0.7f;
            weaponAttachPoint.localPosition = Vector3.back * drawAmount * 0.3f;
        }
        else
        {
            // Release phase
            weaponAttachPoint.localPosition = Vector3.zero;
        }
    }
    
    float GetAttackDuration(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Dagger:
                return meleeSwingDuration * 0.7f; // Faster
            case WeaponType.Sword:
            case WeaponType.Mace:
                return meleeSwingDuration;
            case WeaponType.Axe:
                return meleeSwingDuration * 1.2f; // Slower
            case WeaponType.Staff:
            case WeaponType.Bow:
                return rangedDrawDuration;
            default:
                return meleeSwingDuration;
        }
    }
    
    void EndAttack()
    {
        isAttacking = false;
        
        if (animator != null)
        {
            animator.SetBool(animIDIsAttacking, false);
        }
        
        // Reset weapon position
        if (weaponAttachPoint != null)
        {
            weaponAttachPoint.localPosition = Vector3.zero;
            weaponAttachPoint.localRotation = Quaternion.identity;
        }
    }
    
    // Called from animation events
    public void OnAttackHitFrame()
    {
        // This is when damage should be applied
        // Called from animation event at the peak of the swing
        Debug.Log("Attack hit frame!");
    }
    
    public void OnBowReleaseFrame()
    {
        // This is when the arrow should be fired
        Debug.Log("Bow release frame!");
    }
    
    // Public methods for external control
    public bool IsAttacking() => isAttacking;
    public WeaponType GetCurrentWeaponType() => currentWeaponType;
    
    void OnDrawGizmos()
    {
        if (weaponAttachPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(weaponAttachPoint.position, 0.05f);
        }
    }
}