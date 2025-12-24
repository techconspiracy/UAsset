// ThirdPersonController.cs - FIXED VERSION
// Now properly integrates with CombatAnimationController
// Supports both melee and ranged weapons

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class ThirdPersonController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -15f;
    
    [Header("Combat")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private LayerMask enemyLayers;
    
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    
    private CharacterController controller;
    private PlayerInput playerInput;
    private Animator animator;
    private PlayerStats playerStats;
    private CombatAnimationController combatController;
    private RangedWeaponHandler rangedHandler;
    
    // Input values
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool attackPressed;
    private bool sprintPressed;
    
    // Movement
    private Vector3 velocity;
    private bool isGrounded;
    private float currentSpeed;
    
    // Combat
    private float lastAttackTime;
    private float attackCooldown = 0.5f;
    
    // Animation IDs
    private int animIDSpeed;
    private int animIDGrounded;
    private int animIDJump;
    private int animIDFreeFall;
    private int animIDAttack;
    
    private string currentControlScheme = "";
    
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponentInChildren<Animator>();
        playerStats = GetComponent<PlayerStats>();
        combatController = GetComponent<CombatAnimationController>();
        rangedHandler = GetComponent<RangedWeaponHandler>();
        
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        if (animator != null)
        {
            AssignAnimationIDs();
        }
        
        if (playerInput != null)
        {
            playerInput.onControlsChanged += OnControlsChanged;
            currentControlScheme = playerInput.currentControlScheme;
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void OnDestroy()
    {
        if (playerInput != null)
        {
            playerInput.onControlsChanged -= OnControlsChanged;
        }
    }
    
    void OnControlsChanged(PlayerInput input)
    {
        currentControlScheme = input.currentControlScheme;
    }
    
    void AssignAnimationIDs()
    {
        animIDSpeed = Animator.StringToHash("Speed");
        animIDGrounded = Animator.StringToHash("Grounded");
        animIDJump = Animator.StringToHash("Jump");
        animIDFreeFall = Animator.StringToHash("FreeFall");
        animIDAttack = Animator.StringToHash("Attack");
    }
    
    void Update()
    {
        GroundCheck();
        Move();
        Jump();
        
        if (attackPressed && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
        }
    }
    
    void GroundCheck()
    {
        isGrounded = controller.isGrounded;
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        if (animator != null)
        {
            animator.SetBool(animIDGrounded, isGrounded);
        }
    }
    
    void Move()
    {
        currentSpeed = sprintPressed ? runSpeed : walkSpeed;
        
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        Vector3 moveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;
        
        if (moveDirection.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            
            controller.Move(moveDirection * currentSpeed * Time.deltaTime);
        }
        
        if (animator != null)
        {
            float animationSpeed = moveDirection.magnitude * currentSpeed / runSpeed;
            animator.SetFloat(animIDSpeed, animationSpeed);
        }
    }
    
    void Jump()
    {
        if (jumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            
            if (animator != null)
            {
                animator.SetTrigger(animIDJump);
            }
        }
        
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        
        if (animator != null && !isGrounded)
        {
            animator.SetBool(animIDFreeFall, velocity.y < 0);
        }
        
        jumpPressed = false;
    }
    
    void Attack()
    {
        lastAttackTime = Time.time;
        attackPressed = false;

        if (playerStats == null)
        {
            Debug.LogWarning("PlayerStats not found!");
            return;
        }

        Weapon equippedWeapon = playerStats.GetEquippedWeapon();
        
        // Determine if weapon is ranged
        bool isRangedWeapon = false;
        WeaponType weaponType = WeaponType.Sword; // Default
        
        if (equippedWeapon != null)
        {
            weaponType = equippedWeapon.weaponType;
            isRangedWeapon = (weaponType == WeaponType.Bow || weaponType == WeaponType.Staff);
        }

        // ✅ FIXED: Integrate with CombatAnimationController
        if (combatController != null)
        {
            combatController.PerformAttack(weaponType);
            Debug.Log($"Triggered {weaponType} attack animation");
        }

        // Handle ranged vs melee attacks
        if (isRangedWeapon)
        {
            // Use ranged weapon handler
            if (rangedHandler != null)
            {
                bool fired = rangedHandler.TryRangedAttack();
                if (fired)
                {
                    Debug.Log($"Fired ranged weapon: {weaponType}");
                }
            }
            else
            {
                Debug.LogWarning("RangedWeaponHandler not found! Cannot fire ranged weapon.");
            }
        }
        else
        {
            // Perform melee attack
            PerformMeleeAttack();
        }
    }

    void PerformMeleeAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger(animIDAttack);
        }

        if (attackPoint == null)
        {
            Debug.LogWarning("Attack Point not assigned!");
            return;
        }

        // Find all enemies in attack range
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);

        if (hitEnemies.Length > 0)
        {
            foreach (Collider enemy in hitEnemies)
            {
                EnemyStats enemyStats = enemy.GetComponent<EnemyStats>();
                if (enemyStats != null && playerStats != null)
                {
                    float damage = playerStats.GetTotalDamage();
                    bool isCrit = Random.value < (playerStats.GetCritChance() / 100f);
                
                    if (isCrit)
                    {
                        damage *= 2f;
                        Debug.Log("Critical Hit!");
                    }
                
                    enemyStats.TakeDamage(damage, isCrit);
                    Debug.Log($"Hit enemy for {damage:F0} damage" + (isCrit ? " (CRIT)" : ""));
                }
            }
        }
    }
    
    // ===== INPUT SYSTEM CALLBACKS =====
    
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
    
    public void OnJump(InputValue value)
    {
        jumpPressed = value.isPressed;
    }
    
    public void OnAttack(InputValue value)
    {
        attackPressed = value.isPressed;
    }
    
    public void OnSprint(InputValue value)
    {
        sprintPressed = value.isPressed;
    }
    
    public void OnInventory(InputValue value)
    {
        if (value.isPressed)
        {
            InventoryUI inventoryUI = FindFirstObjectByType<InventoryUI>(FindObjectsInactive.Include);
            
            if (inventoryUI != null)
            {
                inventoryUI.ToggleInventory();
            }
            else
            {
                Debug.LogWarning("InventoryUI not found!");
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}