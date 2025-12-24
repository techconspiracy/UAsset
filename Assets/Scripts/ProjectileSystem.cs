// ProjectileSystem.cs - NEW FILE
// Complete projectile system for ranged weapons
// Add this to a GameObject in your scene (can be on GameManager)

using UnityEngine;
using System.Collections.Generic;

public enum ProjectileType
{
    Arrow,
    ThrownAxe,
    ThrownKnife,
    Bullet,
    EnergyBeam,
    PsiBlast,
    Fireball,
    IceShard
}

public class ProjectileSystem : MonoBehaviour
{
    [Header("Projectile Prefabs (Optional - procedural if not assigned)")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private GameObject axePrefab;
    [SerializeField] private GameObject knifePrefab;
    
    [Header("Projectile Settings")]
    [SerializeField] private float arrowSpeed = 30f;
    [SerializeField] private float bulletSpeed = 100f;
    [SerializeField] private float thrownSpeed = 20f;
    [SerializeField] private float energySpeed = 50f;
    [SerializeField] private float homingSpeed = 25f;
    
    [Header("Physics")]
    [SerializeField] private float arrowGravity = 9.8f;
    [SerializeField] private float thrownGravity = 12f;
    [SerializeField] private float bulletGravity = 2f;
    
    [Header("Homing Settings")]
    [SerializeField] private float homingStrength = 5f;
    [SerializeField] private float homingRange = 20f;
    
    private List<Projectile> activeProjectiles = new List<Projectile>();
    
    void Update()
    {
        // Update all active projectiles
        for (int i = activeProjectiles.Count - 1; i >= 0; i--)
        {
            if (activeProjectiles[i] == null || activeProjectiles[i].gameObject == null)
            {
                activeProjectiles.RemoveAt(i);
            }
        }
    }
    
    public GameObject FireProjectile(
        ProjectileType type,
        Vector3 origin,
        Vector3 direction,
        float damage,
        bool isCrit,
        LayerMask targetLayers,
        Transform shooter)
    {
        GameObject projectileObj = CreateProjectileObject(type);
        projectileObj.transform.position = origin;
        projectileObj.transform.rotation = Quaternion.LookRotation(direction);
        
        Projectile projectile = projectileObj.AddComponent<Projectile>();
        projectile.Initialize(type, direction, damage, isCrit, targetLayers, shooter, this);
        
        activeProjectiles.Add(projectile);
        
        return projectileObj;
    }
    
    GameObject CreateProjectileObject(ProjectileType type)
    {
        GameObject obj = null;
        
        // Try to use prefab if assigned
        switch (type)
        {
            case ProjectileType.Arrow:
                if (arrowPrefab != null) obj = Instantiate(arrowPrefab);
                break;
            case ProjectileType.ThrownAxe:
                if (axePrefab != null) obj = Instantiate(axePrefab);
                break;
            case ProjectileType.ThrownKnife:
                if (knifePrefab != null) obj = Instantiate(knifePrefab);
                break;
        }
        
        // Create procedural projectile if no prefab
        if (obj == null)
        {
            obj = CreateProceduralProjectile(type);
        }
        
        return obj;
    }
    
    GameObject CreateProceduralProjectile(ProjectileType type)
    {
        GameObject obj = new GameObject($"Projectile_{type}");
        
        switch (type)
        {
            case ProjectileType.Arrow:
                CreateArrowModel(obj);
                break;
            case ProjectileType.ThrownAxe:
                CreateThrownAxeModel(obj);
                break;
            case ProjectileType.ThrownKnife:
                CreateThrownKnifeModel(obj);
                break;
            case ProjectileType.Bullet:
                CreateBulletModel(obj);
                break;
            case ProjectileType.EnergyBeam:
                CreateEnergyBeamModel(obj);
                break;
            case ProjectileType.PsiBlast:
                CreatePsiBlastModel(obj);
                break;
            case ProjectileType.Fireball:
                CreateFireballModel(obj);
                break;
            case ProjectileType.IceShard:
                CreateIceShardModel(obj);
                break;
        }
        
        return obj;
    }
    
    void CreateArrowModel(GameObject parent)
    {
        // Arrow shaft
        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shaft.transform.SetParent(parent.transform);
        shaft.transform.localPosition = Vector3.zero;
        shaft.transform.localRotation = Quaternion.Euler(90, 0, 0);
        shaft.transform.localScale = new Vector3(0.02f, 0.3f, 0.02f);
        
        Renderer renderer = shaft.GetComponent<Renderer>();
        renderer.material.color = new Color(0.6f, 0.4f, 0.2f);
        Destroy(shaft.GetComponent<Collider>());
        
        // Arrow tip
        GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tip.transform.SetParent(parent.transform);
        tip.transform.localPosition = new Vector3(0, 0, 0.35f);
        tip.transform.localRotation = Quaternion.Euler(0, 0, 0);
        tip.transform.localScale = new Vector3(0.05f, 0.08f, 0.05f);
        
        renderer = tip.GetComponent<Renderer>();
        renderer.material.color = new Color(0.7f, 0.7f, 0.8f);
        Destroy(tip.GetComponent<Collider>());
        
        // Add sphere collider for hit detection
        SphereCollider collider = parent.AddComponent<SphereCollider>();
        collider.radius = 0.1f;
        collider.isTrigger = true;
        
        // Add rigidbody
        Rigidbody rb = parent.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }
    
    void CreateThrownAxeModel(GameObject parent)
    {
        GameObject axe = GameObject.CreatePrimitive(PrimitiveType.Cube);
        axe.transform.SetParent(parent.transform);
        axe.transform.localScale = new Vector3(0.3f, 0.05f, 0.3f);
        
        Renderer renderer = axe.GetComponent<Renderer>();
        renderer.material.color = new Color(0.6f, 0.6f, 0.7f);
        Destroy(axe.GetComponent<Collider>());
        
        SphereCollider collider = parent.AddComponent<SphereCollider>();
        collider.radius = 0.2f;
        collider.isTrigger = true;
        
        Rigidbody rb = parent.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }
    
    void CreateThrownKnifeModel(GameObject parent)
    {
        GameObject knife = GameObject.CreatePrimitive(PrimitiveType.Cube);
        knife.transform.SetParent(parent.transform);
        knife.transform.localScale = new Vector3(0.05f, 0.02f, 0.2f);
        
        Renderer renderer = knife.GetComponent<Renderer>();
        renderer.material.color = new Color(0.8f, 0.8f, 0.9f);
        Destroy(knife.GetComponent<Collider>());
        
        SphereCollider collider = parent.AddComponent<SphereCollider>();
        collider.radius = 0.1f;
        collider.isTrigger = true;
        
        Rigidbody rb = parent.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }
    
    void CreateBulletModel(GameObject parent)
    {
        GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bullet.transform.SetParent(parent.transform);
        bullet.transform.localScale = Vector3.one * 0.05f;
        
        Renderer renderer = bullet.GetComponent<Renderer>();
        renderer.material.color = Color.yellow;
        Destroy(bullet.GetComponent<Collider>());
        
        SphereCollider collider = parent.AddComponent<SphereCollider>();
        collider.radius = 0.05f;
        collider.isTrigger = true;
        
        Rigidbody rb = parent.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }
    
    void CreateEnergyBeamModel(GameObject parent)
    {
        GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beam.transform.SetParent(parent.transform);
        beam.transform.localRotation = Quaternion.Euler(90, 0, 0);
        beam.transform.localScale = new Vector3(0.1f, 2f, 0.1f);
        
        Renderer renderer = beam.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.cyan;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", Color.cyan * 2f);
        renderer.material = mat;
        Destroy(beam.GetComponent<Collider>());
        
        CapsuleCollider collider = parent.AddComponent<CapsuleCollider>();
        collider.radius = 0.1f;
        collider.height = 4f;
        collider.direction = 2; // Z-axis
        collider.isTrigger = true;
    }
    
    void CreatePsiBlastModel(GameObject parent)
    {
        GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        orb.transform.SetParent(parent.transform);
        orb.transform.localScale = Vector3.one * 0.3f;
        
        Renderer renderer = orb.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.5f, 0f, 1f, 0.8f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(0.5f, 0f, 1f) * 3f);
        renderer.material = mat;
        Destroy(orb.GetComponent<Collider>());
        
        SphereCollider collider = parent.AddComponent<SphereCollider>();
        collider.radius = 0.15f;
        collider.isTrigger = true;
        
        Rigidbody rb = parent.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }
    
    void CreateFireballModel(GameObject parent)
    {
        GameObject fireball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fireball.transform.SetParent(parent.transform);
        fireball.transform.localScale = Vector3.one * 0.4f;
        
        Renderer renderer = fireball.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(1f, 0.3f, 0f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(1f, 0.5f, 0f) * 4f);
        renderer.material = mat;
        Destroy(fireball.GetComponent<Collider>());
        
        SphereCollider collider = parent.AddComponent<SphereCollider>();
        collider.radius = 0.2f;
        collider.isTrigger = true;
    }
    
    void CreateIceShardModel(GameObject parent)
    {
        GameObject shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shard.transform.SetParent(parent.transform);
        shard.transform.localScale = new Vector3(0.1f, 0.1f, 0.5f);
        
        Renderer renderer = shard.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.5f, 0.8f, 1f, 0.7f);
        mat.SetFloat("_Metallic", 0.8f);
        mat.SetFloat("_Glossiness", 0.9f);
        renderer.material = mat;
        Destroy(shard.GetComponent<Collider>());
        
        CapsuleCollider collider = parent.AddComponent<CapsuleCollider>();
        collider.radius = 0.1f;
        collider.height = 0.5f;
        collider.direction = 2; // Z-axis
        collider.isTrigger = true;
    }
    
    public float GetProjectileSpeed(ProjectileType type)
    {
        switch (type)
        {
            case ProjectileType.Arrow: return arrowSpeed;
            case ProjectileType.ThrownAxe:
            case ProjectileType.ThrownKnife: return thrownSpeed;
            case ProjectileType.Bullet: return bulletSpeed;
            case ProjectileType.EnergyBeam: return energySpeed;
            case ProjectileType.PsiBlast: return homingSpeed;
            case ProjectileType.Fireball: return energySpeed * 0.7f;
            case ProjectileType.IceShard: return energySpeed * 0.8f;
            default: return 20f;
        }
    }
    
    public float GetProjectileGravity(ProjectileType type)
    {
        switch (type)
        {
            case ProjectileType.Arrow: return arrowGravity;
            case ProjectileType.ThrownAxe:
            case ProjectileType.ThrownKnife: return thrownGravity;
            case ProjectileType.Bullet: return bulletGravity;
            default: return 0f; // Energy/magic projectiles ignore gravity
        }
    }
    
    public bool IsHoming(ProjectileType type)
    {
        return type == ProjectileType.PsiBlast;
    }
    
    public float GetHomingStrength() => homingStrength;
    public float GetHomingRange() => homingRange;
}

// Individual projectile behavior
public class Projectile : MonoBehaviour
{
    private ProjectileType type;
    private Vector3 velocity;
    private float damage;
    private bool isCrit;
    private LayerMask targetLayers;
    private Transform shooter;
    private ProjectileSystem system;
    
    private float lifetime = 5f;
    private float lifetimeTimer = 0f;
    private Transform homingTarget;
    
    public void Initialize(
        ProjectileType projType,
        Vector3 direction,
        float dmg,
        bool crit,
        LayerMask targets,
        Transform shooterTransform,
        ProjectileSystem projSystem)
    {
        type = projType;
        damage = dmg;
        isCrit = crit;
        targetLayers = targets;
        shooter = shooterTransform;
        system = projSystem;
        
        float speed = system.GetProjectileSpeed(type);
        velocity = direction.normalized * speed;
        
        // Find homing target if applicable
        if (system.IsHoming(type))
        {
            FindHomingTarget();
        }
    }
    
    void Update()
    {
        lifetimeTimer += Time.deltaTime;
        
        if (lifetimeTimer >= lifetime)
        {
            Destroy(gameObject);
            return;
        }
        
        // Apply gravity
        float gravity = system.GetProjectileGravity(type);
        velocity.y -= gravity * Time.deltaTime;
        
        // Apply homing
        if (system.IsHoming(type) && homingTarget != null)
        {
            Vector3 directionToTarget = (homingTarget.position - transform.position).normalized;
            float homingStrength = system.GetHomingStrength();
            velocity = Vector3.Lerp(velocity, directionToTarget * velocity.magnitude, homingStrength * Time.deltaTime);
        }
        
        // Move projectile
        transform.position += velocity * Time.deltaTime;
        
        // Rotate thrown weapons
        if (type == ProjectileType.ThrownAxe || type == ProjectileType.ThrownKnife)
        {
            transform.Rotate(Vector3.right, 720f * Time.deltaTime);
        }
        else
        {
            // Point in direction of movement
            if (velocity != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(velocity);
            }
        }
    }
    
    void FindHomingTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, system.GetHomingRange(), targetLayers);
        
        float closestDist = float.MaxValue;
        foreach (Collider hit in hits)
        {
            if (hit.transform == shooter) continue;
            
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                homingTarget = hit.transform;
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Ignore shooter
        if (other.transform == shooter || other.transform.IsChildOf(shooter))
        {
            return;
        }
        
        // Check if hit valid target
        if (((1 << other.gameObject.layer) & targetLayers) != 0)
        {
            // Deal damage
            EnemyStats enemy = other.GetComponent<EnemyStats>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, isCrit);
                Debug.Log($"Projectile hit enemy for {damage:F0} damage" + (isCrit ? " (CRIT)" : ""));
            }
            
            // Destroy projectile
            Destroy(gameObject);
        }
        // Hit environment
        else if (!other.isTrigger)
        {
            Debug.Log($"Projectile hit {other.name}");
            Destroy(gameObject);
        }
    }
}