// ProjectileSystem.cs
// Handles all ranged weapon projectiles with different physics
// Attach to GameManager or create a ProjectileManager GameObject

using UnityEngine;
using System.Collections;

public enum ProjectileType
{
    Arrow,          // Bow - Arc trajectory with gravity
    ThrownAxe,      // Thrown weapon - Rotation + arc
    ThrownKnife,    // Fast throw - Less arc, rotates
    Bullet,         // Guns - Fast linear with slight drop
    EnergyBeam,     // Energy weapons - Instant raycast
    PsiBlast,       // Psi weapons - Homing projectile
    Magic          // Magic - Particle with special effects
}

public class ProjectileSystem : MonoBehaviour
{
    [Header("Projectile Prefabs")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private GameObject axePrefab;
    [SerializeField] private GameObject knifePrefab;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject energyBeamPrefab;
    [SerializeField] private GameObject psiBlastPrefab;
    
    [Header("Projectile Settings")]
    [SerializeField] private float arrowSpeed = 20f;
    [SerializeField] private float thrownWeaponSpeed = 15f;
    [SerializeField] private float bulletSpeed = 100f;
    [SerializeField] private float psiSpeed = 12f;
    [SerializeField] private float psiHomingStrength = 5f;
    
    [Header("Gravity Settings")]
    [SerializeField] private float arrowGravity = 9.81f;
    [SerializeField] private float thrownGravity = 12f;
    [SerializeField] private float bulletGravity = 2f;
    
    [Header("Effects")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private GameObject muzzleFlash;
    
    private static ProjectileSystem instance;
    
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    
    public static ProjectileSystem Instance => instance;
    
    // Main method to fire any projectile type
    public void FireProjectile(ProjectileType type, Vector3 origin, Vector3 direction, float damage, bool isCrit, LayerMask targetLayers, Transform shooter = null)
    {
        switch (type)
        {
            case ProjectileType.Arrow:
                FireArrow(origin, direction, damage, isCrit, targetLayers);
                break;
            case ProjectileType.ThrownAxe:
                FireThrownWeapon(origin, direction, damage, isCrit, targetLayers, true);
                break;
            case ProjectileType.ThrownKnife:
                FireThrownWeapon(origin, direction, damage, isCrit, targetLayers, false);
                break;
            case ProjectileType.Bullet:
                FireBullet(origin, direction, damage, isCrit, targetLayers);
                break;
            case ProjectileType.EnergyBeam:
                FireEnergyBeam(origin, direction, damage, isCrit, targetLayers);
                break;
            case ProjectileType.PsiBlast:
                FirePsiBlast(origin, direction, damage, isCrit, targetLayers, shooter);
                break;
        }
    }
    
    void FireArrow(Vector3 origin, Vector3 direction, float damage, bool isCrit, LayerMask targetLayers)
    {
        GameObject arrow = CreateProjectile(arrowPrefab, origin, direction);
        if (arrow == null) return;
        
        ArrowProjectile proj = arrow.AddComponent<ArrowProjectile>();
        proj.Initialize(direction, arrowSpeed, arrowGravity, damage, isCrit, targetLayers);
    }
    
    void FireThrownWeapon(Vector3 origin, Vector3 direction, float damage, bool isCrit, LayerMask targetLayers, bool isAxe)
    {
        GameObject prefab = isAxe ? axePrefab : knifePrefab;
        GameObject thrown = CreateProjectile(prefab, origin, direction);
        if (thrown == null) return;
        
        ThrownWeaponProjectile proj = thrown.AddComponent<ThrownWeaponProjectile>();
        proj.Initialize(direction, thrownWeaponSpeed, thrownGravity, damage, isCrit, targetLayers, isAxe);
    }
    
    void FireBullet(Vector3 origin, Vector3 direction, float damage, bool isCrit, LayerMask targetLayers)
    {
        // Muzzle flash
        if (muzzleFlash != null)
        {
            Instantiate(muzzleFlash, origin, Quaternion.LookRotation(direction));
        }
        
        GameObject bullet = CreateProjectile(bulletPrefab, origin, direction);
        if (bullet == null) return;
        
        BulletProjectile proj = bullet.AddComponent<BulletProjectile>();
        proj.Initialize(direction, bulletSpeed, bulletGravity, damage, isCrit, targetLayers);
    }
    
    void FireEnergyBeam(Vector3 origin, Vector3 direction, float damage, bool isCrit, LayerMask targetLayers)
    {
        // Instant raycast
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, 100f, targetLayers))
        {
            // Hit something
            EnemyStats enemy = hit.collider.GetComponent<EnemyStats>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, isCrit);
            }
            
            // Visual beam
            StartCoroutine(DrawEnergyBeam(origin, hit.point));
            
            if (hitEffect != null)
            {
                Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
        else
        {
            // No hit, draw to max distance
            StartCoroutine(DrawEnergyBeam(origin, origin + direction * 100f));
        }
    }
    
    void FirePsiBlast(Vector3 origin, Vector3 direction, float damage, bool isCrit, LayerMask targetLayers, Transform shooter)
    {
        GameObject blast = CreateProjectile(psiBlastPrefab, origin, direction);
        if (blast == null) return;
        
        PsiProjectile proj = blast.AddComponent<PsiProjectile>();
        proj.Initialize(direction, psiSpeed, damage, isCrit, targetLayers, psiHomingStrength, shooter);
    }
    
    GameObject CreateProjectile(GameObject prefab, Vector3 position, Vector3 direction)
    {
        GameObject proj;
        
        if (prefab != null)
        {
            proj = Instantiate(prefab, position, Quaternion.LookRotation(direction));
        }
        else
        {
            // Create default projectile
            proj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            proj.transform.position = position;
            proj.transform.localScale = Vector3.one * 0.1f;
            proj.GetComponent<Collider>().isTrigger = true;
        }
        
        return proj;
    }
    
    IEnumerator DrawEnergyBeam(Vector3 start, Vector3 end)
    {
        GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        
        Vector3 midpoint = (start + end) / 2f;
        float distance = Vector3.Distance(start, end);
        
        beam.transform.position = midpoint;
        beam.transform.LookAt(end);
        beam.transform.Rotate(90, 0, 0);
        beam.transform.localScale = new Vector3(0.1f, distance / 2f, 0.1f);
        
        Renderer renderer = beam.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", Color.cyan * 2f);
        mat.color = Color.cyan;
        renderer.material = mat;
        
        Destroy(beam.GetComponent<Collider>());
        
        yield return new WaitForSeconds(0.1f);
        
        Destroy(beam);
    }
    
    public void CreateHitEffect(Vector3 position)
    {
        if (hitEffect != null)
        {
            Instantiate(hitEffect, position, Quaternion.identity);
        }
    }
}

// Individual projectile behaviors
public class ArrowProjectile : MonoBehaviour
{
    private Vector3 velocity;
    private float gravity;
    private float damage;
    private bool isCrit;
    private LayerMask targetLayers;
    private bool hasHit;
    
    public void Initialize(Vector3 direction, float speed, float grav, float dmg, bool crit, LayerMask layers)
    {
        velocity = direction.normalized * speed;
        gravity = grav;
        damage = dmg;
        isCrit = crit;
        targetLayers = layers;
        
        Destroy(gameObject, 5f);
    }
    
    void Update()
    {
        if (hasHit) return;
        
        // Apply gravity
        velocity.y -= gravity * Time.deltaTime;
        
        // Point arrow in direction of travel
        transform.rotation = Quaternion.LookRotation(velocity);
        
        // Move and check for collision
        RaycastHit hit;
        if (Physics.Raycast(transform.position, velocity.normalized, out hit, velocity.magnitude * Time.deltaTime, targetLayers))
        {
            Hit(hit);
        }
        else
        {
            transform.position += velocity * Time.deltaTime;
        }
    }
    
    void Hit(RaycastHit hit)
    {
        hasHit = true;
        
        EnemyStats enemy = hit.collider.GetComponent<EnemyStats>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, isCrit);
        }
        
        // Stick arrow in surface
        transform.position = hit.point;
        transform.rotation = Quaternion.LookRotation(hit.normal);
        
        Destroy(gameObject, 2f);
    }
}

public class ThrownWeaponProjectile : MonoBehaviour
{
    private Vector3 velocity;
    private float gravity;
    private float damage;
    private bool isCrit;
    private LayerMask targetLayers;
    private bool hasHit;
    private float rotationSpeed;
    
    public void Initialize(Vector3 direction, float speed, float grav, float dmg, bool crit, LayerMask layers, bool isAxe)
    {
        velocity = direction.normalized * speed;
        gravity = grav;
        damage = dmg;
        isCrit = crit;
        targetLayers = layers;
        rotationSpeed = isAxe ? 720f : 1080f; // Axes rotate slower
        
        Destroy(gameObject, 5f);
    }
    
    void Update()
    {
        if (hasHit) return;
        
        velocity.y -= gravity * Time.deltaTime;
        
        // Rotate weapon
        transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, velocity.normalized, out hit, velocity.magnitude * Time.deltaTime, targetLayers))
        {
            Hit(hit);
        }
        else
        {
            transform.position += velocity * Time.deltaTime;
        }
    }
    
    void Hit(RaycastHit hit)
    {
        hasHit = true;
        
        EnemyStats enemy = hit.collider.GetComponent<EnemyStats>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, isCrit);
        }
        
        Destroy(gameObject, 0.1f);
    }
}

public class BulletProjectile : MonoBehaviour
{
    private Vector3 velocity;
    private float gravity;
    private float damage;
    private bool isCrit;
    private LayerMask targetLayers;
    
    public void Initialize(Vector3 direction, float speed, float grav, float dmg, bool crit, LayerMask layers)
    {
        velocity = direction.normalized * speed;
        gravity = grav;
        damage = dmg;
        isCrit = crit;
        targetLayers = layers;
        
        Destroy(gameObject, 2f);
    }
    
    void Update()
    {
        velocity.y -= gravity * Time.deltaTime;
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, velocity.normalized, out hit, velocity.magnitude * Time.deltaTime, targetLayers))
        {
            Hit(hit);
        }
        else
        {
            transform.position += velocity * Time.deltaTime;
        }
    }
    
    void Hit(RaycastHit hit)
    {
        EnemyStats enemy = hit.collider.GetComponent<EnemyStats>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, isCrit);
        }
        
        Destroy(gameObject);
    }
}

public class PsiProjectile : MonoBehaviour
{
    private Vector3 velocity;
    private float damage;
    private bool isCrit;
    private LayerMask targetLayers;
    private float homingStrength;
    private Transform target;
    private Transform shooter;
    
    public void Initialize(Vector3 direction, float speed, float dmg, bool crit, LayerMask layers, float homing, Transform shoot)
    {
        velocity = direction.normalized * speed;
        damage = dmg;
        isCrit = crit;
        targetLayers = layers;
        homingStrength = homing;
        shooter = shoot;
        
        FindNearestTarget();
        Destroy(gameObject, 10f);
    }
    
    void FindNearestTarget()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, 50f, targetLayers);
        float closestDist = float.MaxValue;
        
        foreach (Collider col in enemies)
        {
            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                target = col.transform;
            }
        }
    }
    
    void Update()
    {
        if (target != null)
        {
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            velocity = Vector3.Lerp(velocity, directionToTarget * velocity.magnitude, homingStrength * Time.deltaTime);
        }
        
        transform.rotation = Quaternion.LookRotation(velocity);
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, velocity.normalized, out hit, velocity.magnitude * Time.deltaTime, targetLayers))
        {
            Hit(hit);
        }
        else
        {
            transform.position += velocity * Time.deltaTime;
        }
    }
    
    void Hit(RaycastHit hit)
    {
        EnemyStats enemy = hit.collider.GetComponent<EnemyStats>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, isCrit);
        }
        
        Destroy(gameObject);
    }
}