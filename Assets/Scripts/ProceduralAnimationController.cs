// ProceduralAnimationController.cs
// Procedurally animates the character rig by rotating bones
// Replaces Unity's Animator system with pure code-based animation

using UnityEngine;

public class ProceduralAnimationController : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float walkSpeed = 1.5f;
    [SerializeField] private float walkCycleSpeed = 4f;
    [SerializeField] private float armSwingAmount = 30f;
    [SerializeField] private float legSwingAmount = 40f;
    [SerializeField] private float bobAmount = 0.1f;
    
    [Header("Attack Settings")]
    [SerializeField] private float attackSwingSpeed = 8f;
    [SerializeField] private float attackSwingAngle = 120f;
    [SerializeField] private float attackDuration = 0.5f;
    
    [Header("Idle Settings")]
    [SerializeField] private float idleBreathingSpeed = 1f;
    [SerializeField] private float idleBreathingAmount = 0.02f;
    [SerializeField] private float idleSwayAmount = 2f;
    
    private ProceduralCharacterRig rig;
    private CharacterController controller;
    
    // Animation state
    private float walkCycle = 0f;
    private float idleCycle = 0f;
    private bool isAttacking = false;
    private float attackTimer = 0f;
    private WeaponType currentWeaponType = WeaponType.Sword;
    
    // Bone references
    private Transform leftUpperArm;
    private Transform leftLowerArm;
    private Transform rightUpperArm;
    private Transform rightLowerArm;
    private Transform rightHand;
    private Transform leftUpperLeg;
    private Transform leftLowerLeg;
    private Transform rightUpperLeg;
    private Transform rightLowerLeg;
    private Transform chest;
    private Transform pelvis;
    private Transform head;
    
    void Start()
    {
        rig = GetComponent<ProceduralCharacterRig>();
        controller = GetComponent<CharacterController>();
        
        if (rig != null)
        {
            CacheBoneReferences();
        }
        else
        {
            Debug.LogError("ProceduralAnimationController requires ProceduralCharacterRig!");
        }
    }
    
    void CacheBoneReferences()
    {
        leftUpperArm = rig.GetBone("LeftUpperArm");
        leftLowerArm = rig.GetBone("LeftLowerArm");
        rightUpperArm = rig.GetBone("RightUpperArm");
        rightLowerArm = rig.GetBone("RightLowerArm");
        rightHand = rig.GetRightHand();
        
        leftUpperLeg = rig.GetBone("LeftUpperLeg");
        leftLowerLeg = rig.GetBone("LeftLowerLeg");
        rightUpperLeg = rig.GetBone("RightUpperLeg");
        rightLowerLeg = rig.GetBone("RightLowerLeg");
        
        chest = rig.GetChest();
        pelvis = rig.GetPelvis();
        head = rig.GetHead();
    }
    
    void Update()
    {
        if (rig == null) return;
        
        // Determine animation state
        bool isMoving = false;
        if (controller != null)
        {
            isMoving = controller.velocity.magnitude > 0.1f;
        }
        
        // Update animations
        if (isAttacking)
        {
            UpdateAttackAnimation();
        }
        else if (isMoving)
        {
            UpdateWalkAnimation();
        }
        else
        {
            UpdateIdleAnimation();
        }
    }
    
    void UpdateIdleAnimation()
    {
        idleCycle += Time.deltaTime * idleBreathingSpeed;
        
        // Breathing motion in chest
        if (chest != null)
        {
            float breathe = Mathf.Sin(idleCycle * 2f) * idleBreathingAmount;
            Vector3 chestScale = chest.localScale;
            chestScale.y = 1f + breathe;
            chest.localScale = chestScale;
            
            // Slight sway
            float sway = Mathf.Sin(idleCycle) * idleSwayAmount;
            chest.localRotation = Quaternion.Euler(0, 0, sway);
        }
        
        // Head look around
        if (head != null)
        {
            float headTurn = Mathf.Sin(idleCycle * 0.5f) * 15f;
            head.localRotation = Quaternion.Euler(0, headTurn, 0);
        }
        
        // Relaxed arm positions
        if (rightUpperArm != null)
        {
            rightUpperArm.localRotation = Quaternion.Euler(0, 0, 90 + Mathf.Sin(idleCycle) * 5f);
        }
        if (leftUpperArm != null)
        {
            leftUpperArm.localRotation = Quaternion.Euler(0, 0, -90 + Mathf.Sin(idleCycle + 1f) * 5f);
        }
    }
    
    void UpdateWalkAnimation()
    {
        walkCycle += Time.deltaTime * walkCycleSpeed;
        
        // Bob up and down
        if (pelvis != null)
        {
            float bob = Mathf.Abs(Mathf.Sin(walkCycle * 2f)) * bobAmount;
            pelvis.localPosition = new Vector3(0, 1f + bob, 0);
        }
        
        // Lean forward slightly when moving
        if (chest != null)
        {
            chest.localRotation = Quaternion.Euler(10, 0, 0);
        }
        
        // Swing arms opposite to legs
        float armSwing = Mathf.Sin(walkCycle) * armSwingAmount;
        
        if (rightUpperArm != null)
        {
            rightUpperArm.localRotation = Quaternion.Euler(-armSwing, 0, 90);
        }
        if (leftUpperArm != null)
        {
            leftUpperArm.localRotation = Quaternion.Euler(armSwing, 0, -90);
        }
        
        // Bend elbows slightly
        if (rightLowerArm != null)
        {
            rightLowerArm.localRotation = Quaternion.Euler(0, 0, -20);
        }
        if (leftLowerArm != null)
        {
            leftLowerArm.localRotation = Quaternion.Euler(0, 0, 20);
        }
        
        // Swing legs
        float legSwing = Mathf.Sin(walkCycle) * legSwingAmount;
        
        if (rightUpperLeg != null)
        {
            rightUpperLeg.localRotation = Quaternion.Euler(legSwing, 0, 0);
        }
        if (leftUpperLeg != null)
        {
            leftUpperLeg.localRotation = Quaternion.Euler(-legSwing, 0, 0);
        }
        
        // Bend knees
        float kneeBend = Mathf.Max(0, Mathf.Sin(walkCycle)) * 30f;
        float kneeBend2 = Mathf.Max(0, -Mathf.Sin(walkCycle)) * 30f;
        
        if (rightLowerLeg != null)
        {
            rightLowerLeg.localRotation = Quaternion.Euler(kneeBend, 0, 0);
        }
        if (leftLowerLeg != null)
        {
            leftLowerLeg.localRotation = Quaternion.Euler(kneeBend2, 0, 0);
        }
    }
    
    void UpdateAttackAnimation()
    {
        attackTimer += Time.deltaTime;
        float progress = attackTimer / attackDuration;
        
        if (progress >= 1f)
        {
            EndAttack();
            return;
        }
        
        // Different attack animations based on weapon type
        switch (currentWeaponType)
        {
            case WeaponType.Sword:
            case WeaponType.Axe:
            case WeaponType.Mace:
                AnimateMeleeSwing(progress);
                break;
            case WeaponType.Dagger:
                AnimateDaggerStab(progress);
                break;
            case WeaponType.Staff:
                AnimateStaffCast(progress);
                break;
            case WeaponType.Bow:
                AnimateBowShoot(progress);
                break;
        }
    }
    
    void AnimateMeleeSwing(float progress)
    {
        // Wind up (0-0.3), swing (0.3-0.7), follow through (0.7-1.0)
        float swingAngle;
        
        if (progress < 0.3f)
        {
            // Wind up
            swingAngle = Mathf.Lerp(0, -45, progress / 0.3f);
        }
        else if (progress < 0.7f)
        {
            // Main swing
            float swingProgress = (progress - 0.3f) / 0.4f;
            swingAngle = Mathf.Lerp(-45, attackSwingAngle, swingProgress);
        }
        else
        {
            // Follow through
            float followProgress = (progress - 0.7f) / 0.3f;
            swingAngle = Mathf.Lerp(attackSwingAngle, 45, followProgress);
        }
        
        if (rightUpperArm != null)
        {
            rightUpperArm.localRotation = Quaternion.Euler(swingAngle, 0, 90);
        }
        
        // Torso rotation for power
        if (chest != null)
        {
            float torsoRotation = swingAngle * 0.3f;
            chest.localRotation = Quaternion.Euler(0, torsoRotation, 0);
        }
    }
    
    void AnimateDaggerStab(float progress)
    {
        // Quick thrust forward
        float stabProgress = Mathf.Sin(progress * Mathf.PI);
        
        if (rightUpperArm != null)
        {
            Vector3 stabPos = Vector3.forward * stabProgress * 0.5f;
            rightUpperArm.localPosition = stabPos;
            rightUpperArm.localRotation = Quaternion.Euler(-90 * stabProgress, 0, 90);
        }
    }
    
    void AnimateStaffCast(float progress)
    {
        // Raise staff overhead
        float raiseAngle = Mathf.Sin(progress * Mathf.PI) * 90f;
        
        if (rightUpperArm != null)
        {
            rightUpperArm.localRotation = Quaternion.Euler(-raiseAngle, 0, 90);
        }
        
        if (leftUpperArm != null)
        {
            leftUpperArm.localRotation = Quaternion.Euler(-raiseAngle * 0.5f, 0, -90);
        }
    }
    
    void AnimateBowShoot(float progress)
    {
        if (progress < 0.6f)
        {
            // Draw bow
            float drawProgress = progress / 0.6f;
            
            if (rightUpperArm != null)
            {
                rightUpperArm.localRotation = Quaternion.Euler(0, 0, 90 - 45 * drawProgress);
            }
            
            if (leftUpperArm != null)
            {
                Vector3 pullBack = Vector3.back * drawProgress * 0.3f;
                leftUpperArm.localPosition = pullBack;
            }
        }
        else
        {
            // Release
            if (rightUpperArm != null)
            {
                rightUpperArm.localRotation = Quaternion.Euler(0, 0, 90);
            }
            
            if (leftUpperArm != null)
            {
                leftUpperArm.localPosition = Vector3.zero;
            }
        }
    }
    
    // Public methods
    public void PlayAttack(WeaponType weaponType)
    {
        if (isAttacking) return;
        
        isAttacking = true;
        attackTimer = 0f;
        currentWeaponType = weaponType;
        
        Debug.Log($"Playing {weaponType} attack animation");
    }
    
    void EndAttack()
    {
        isAttacking = false;
        attackTimer = 0f;
        
        // Reset bone rotations
        if (rightUpperArm != null)
        {
            rightUpperArm.localRotation = Quaternion.Euler(0, 0, 90);
            rightUpperArm.localPosition = Vector3.zero;
        }
        
        if (leftUpperArm != null)
        {
            leftUpperArm.localRotation = Quaternion.Euler(0, 0, -90);
            leftUpperArm.localPosition = Vector3.zero;
        }
        
        if (chest != null)
        {
            chest.localRotation = Quaternion.identity;
        }
    }
    
    public bool IsAttacking() => isAttacking;
    
    public void SetWalkSpeed(float speed)
    {
        walkCycleSpeed = speed;
    }
}