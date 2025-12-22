// ThirdPersonController.cs - GAMEPAD SUPPORT RESTORED
// Replace your current controller with this version
// Full gamepad support + keyboard/mouse

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
    
    private PlayerStats playerStats;
    
    // Input device detection
    private string currentControlScheme = "";
    
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponentInChildren<Animator>();
        playerStats = GetComponent<PlayerStats>();
        
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        if (animator != null)
        {
            AssignAnimationIDs();
        }
        
        // Subscribe to control scheme changes
        if (playerInput != null)
        {
            playerInput.onControlsChanged += OnControlsChanged;
            currentControlScheme = playerInput.currentControlScheme;
            //Debug.Log($"Starting control scheme: {currentControlScheme}");
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
        //Debug.Log($"Control scheme changed to: {currentControlScheme}");
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
        ////Debug.LogWarning("Last attack time was " + lastAttackTime + "!");

        if (animator != null)
        {
            animator.SetTrigger(animIDAttack);
        }

        if (attackPoint == null)
        {
            //Debug.LogWarning("Attack Point not assigned!");
            attackPressed = false;
            return;
        }

        // Find all colliders within the attack sphere
        ////Debug.LogWarning("Creating a collider!");
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);

        // Check if any enemies were hit
        if (hitEnemies.Length > 0)
        {
            foreach (Collider enemy in hitEnemies)
            {
                ////Debug.LogWarning("Hit!");
                EnemyStats enemyStats = enemy.GetComponent<EnemyStats>();
                if (enemyStats != null && playerStats != null)
                {
                    ////Debug.LogWarning("Found enemystats & playerstats!");
                    float damage = playerStats.GetTotalDamage();
                    bool isCrit = Random.value < (playerStats.GetCritChance() / 100f);
                
                    if (isCrit)
                    {
                        //Debug.LogWarning("Critical Hit!");
                        damage *= 2f;
                    }
                
                    ////Debug.LogWarning("Sending TakeDamage message!");
                    enemyStats.TakeDamage(damage, isCrit);
                }
            }
        }
        else // This else block is correctly paired with the if (hitEnemies.Length > 0) check
        {
            ////Debug.LogWarning("No Hit!");
        }
    
        attackPressed = false;
    }

    
    // ===== INPUT SYSTEM CALLBACKS =====
    // These receive input from BOTH keyboard/mouse AND gamepad
    
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
        ////Debug.Log($"OnAttack called! attackPressed = {attackPressed}");
        //Attack();
    }
    
    public void OnSprint(InputValue value)
    {
        sprintPressed = value.isPressed;
    }
    
    public void OnInventory(InputValue value)
    {
        if (value.isPressed)
        {
            // Use FindFirstObjectByType and specify we only want active objects
            InventoryUI inventoryUI = FindFirstObjectByType<InventoryUI>(FindObjectsInactive.Include);
            
            if (inventoryUI != null)
            {
                ////Debug.Log("Found InventoryUI - toggling!");
                inventoryUI.ToggleInventory();
            }
            else
            {
                //Debug.LogWarning("InventoryUI not found! Make sure InventoryUI script is attached to InventoryPanel.");
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