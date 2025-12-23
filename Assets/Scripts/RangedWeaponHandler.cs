// RangedWeaponHandler.cs
// Handles ranged weapon attacks with projectile system
// Attach to Player GameObject alongside ThirdPersonController

using UnityEngine;
using UnityEngine.InputSystem;

public class RangedWeaponHandler : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private float aimAssistRadius = 2f;
    
    [Header("Ranged Weapon Cooldowns")]
    [SerializeField] private float bowCooldown = 0.8f;
    [SerializeField] private float crossbowCooldown = 1.2f;
    [SerializeField] private float gunCooldown = 0.3f;
    [SerializeField] private float throwCooldown = 1f;
    
    [Header("Projectile Spawn Offsets")]
    [SerializeField] private Vector3 bowOffset = new Vector3(0, 1.5f, 0.5f);
    [SerializeField] private Vector3 gunOffset = new Vector3(0.3f, 1.4f, 0.5f);
    [SerializeField] private Vector3 throwOffset = new Vector3(0, 1.6f, 0.3f);
    
    private PlayerStats playerStats;
    private CombatAnimationController animController;
    private Transform cameraTransform;
    private ProjectileSystem projectileSystem;
    
    private float lastRangedAttackTime;
    private Weapon equippedWeapon;
    
    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        animController = GetComponent<CombatAnimationController>();
        cameraTransform = Camera.main.transform;
        projectileSystem = FindFirstObjectByType<ProjectileSystem>();
        
        if (projectileSystem == null)
        {
            Debug.LogWarning("ProjectileSystem not found in scene! Ranged weapons won't work.");
        }
        
        // Create projectile spawn point if not assigned
        if (projectileSpawnPoint == null)
        {
            GameObject spawnObj = new GameObject("ProjectileSpawnPoint");
            spawnObj.transform.SetParent(transform);
            spawnObj.transform.localPosition = bowOffset;
            projectileSpawnPoint = spawnObj.transform;
        }
    }
    
    // Call this when player presses attack with ranged weapon equipped
    public bool TryRangedAttack()
    {
        if (equippedWeapon == null) return false;
        if (!IsRangedWeapon(equippedWeapon.weaponType)) return false;
        if (Time.time < lastRangedAttackTime + GetWeaponCooldown(equippedWeapon.weaponType)) return false;
        
        PerformRangedAttack();
        return true;
    }
    
    void PerformRangedAttack()
    {
        lastRangedAttackTime = Time.time;
        
        // Get aim direction
        Vector3 aimDirection = GetAimDirection();
        Vector3 spawnPosition = GetProjectileSpawnPosition();
        
        // Calculate damage
        float damage = playerStats.GetTotalDamage();
        bool isCrit = Random.value < (playerStats.GetCritChance() / 100f);
        if (isCrit) damage *= 2f;
        
        // Determine projectile type
        ProjectileType projectileType = GetProjectileType(equippedWeapon.weaponType);
        
        // Fire projectile
        if (projectileSystem != null)
        {
            projectileSystem.FireProjectile(
                projectileType,
                spawnPosition,
                aimDirection,
                damage,
                isCrit,
                enemyLayers,
                transform
            );
        }
        
        // Trigger animation
        if (animController != null)
        {
            animController.PerformAttack(equippedWeapon.weaponType);
        }
        
        Debug.Log($"Fired {projectileType} projectile dealing {damage:F0} damage");
    }
    
    Vector3 GetAimDirection()
    {
        // Raycast from camera center
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;
        
        Vector3 targetPoint;
        
        if (Physics.Raycast(ray, out hit, 100f))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100f);
        }
        
        // Apply aim assist for enemies
        targetPoint = ApplyAimAssist(targetPoint);
        
        Vector3 direction = (targetPoint - projectileSpawnPoint.position).normalized;
        return direction;
    }
    
    Vector3 ApplyAimAssist(Vector3 targetPoint)
    {
        // Find nearby enemies
        Collider[] nearbyEnemies = Physics.OverlapSphere(targetPoint, aimAssistRadius, enemyLayers);
        
        if (nearbyEnemies.Length > 0)
        {
            // Find closest enemy to crosshair
            float closestDist = float.MaxValue;
            Vector3 closestEnemyPos = targetPoint;
            
            foreach (Collider enemy in nearbyEnemies)
            {
                float dist = Vector3.Distance(targetPoint, enemy.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestEnemyPos = enemy.transform.position + Vector3.up * 1f; // Aim at torso
                }
            }
            
            // Soft aim assist - blend toward enemy
            return Vector3.Lerp(targetPoint, closestEnemyPos, 0.3f);
        }
        
        return targetPoint;
    }
    
    Vector3 GetProjectileSpawnPosition()
    {
        Vector3 offset;
        
        switch (equippedWeapon.weaponType)
        {
            case WeaponType.Bow:
                offset = bowOffset;
                break;
            default:
                offset = gunOffset;
                break;
        }
        
        // Convert local offset to world space
        return transform.position + transform.TransformDirection(offset);
    }
    
    ProjectileType GetProjectileType(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.Bow:
                return ProjectileType.Arrow;
            case WeaponType.Axe: // Thrown axe
                return ProjectileType.ThrownAxe;
            case WeaponType.Dagger: // Thrown dagger
                return ProjectileType.ThrownKnife;
            default:
                return ProjectileType.Arrow;
        }
    }
    
    float GetWeaponCooldown(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Bow:
                return bowCooldown;
            case WeaponType.Dagger:
            case WeaponType.Axe:
                return throwCooldown;
            default:
                return bowCooldown;
        }
    }
    
    bool IsRangedWeapon(WeaponType type)
    {
        return type == WeaponType.Bow || 
               type == WeaponType.Staff;
        // Note: Axe and Dagger can be both melee and thrown
    }
    
    public void SetEquippedWeapon(Weapon weapon)
    {
        equippedWeapon = weapon;
    }
    
    // Input callback (connect to Input System)
    public void OnRangedAttack(InputValue value)
    {
        if (value.isPressed)
        {
            TryRangedAttack();
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (projectileSpawnPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(projectileSpawnPoint.position, 0.1f);
            Gizmos.DrawRay(projectileSpawnPoint.position, transform.forward * 2f);
        }
    }
}

// Add this to modify your existing ThirdPersonController
// This allows the controller to handle both melee and ranged attacks
public static class ThirdPersonControllerExtensions
{
    public static void HandleCombatInput(this ThirdPersonController controller)
    {
        // Get components
        RangedWeaponHandler rangedHandler = controller.GetComponent<RangedWeaponHandler>();
        PlayerStats playerStats = controller.GetComponent<PlayerStats>();
        
        if (playerStats == null) return;
        
        Weapon equippedWeapon = playerStats.GetEquippedWeapon();
        if (equippedWeapon == null) return;
        
        // Determine if weapon is ranged
        bool isRanged = equippedWeapon.weaponType == WeaponType.Bow || 
                       equippedWeapon.weaponType == WeaponType.Staff;
        
        if (isRanged && rangedHandler != null)
        {
            // Try ranged attack
            rangedHandler.TryRangedAttack();
        }
        else
        {
            // Use existing melee attack
            // controller.Attack(); // Your existing attack method
        }
    }
}