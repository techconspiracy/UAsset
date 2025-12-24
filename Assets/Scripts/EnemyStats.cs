// EnemyStats.cs - FIXED VERSION
// Now properly awards experience on death
// This fixes the core progression loop

using UnityEngine;
using UnityEngine.UI;

public class EnemyStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private int level = 1;
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private float damage = 5f;
    [SerializeField] private float armor = 2f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 2f;
    
    [Header("Rewards")]
    [SerializeField] private int expReward = 25;
    [SerializeField] private float lootDropChance = 0.3f;
    
    [Header("UI References")]
    [SerializeField] private Canvas healthBarCanvas;
    [SerializeField] private Image healthBarFill;
    
    private float currentHealth;
    private float lastAttackTime;
    private Transform player;
    private PlayerStats playerStats;
    private bool isDead = false;
    
    void Start()
    {
        currentHealth = maxHealth;
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerStats = playerObj.GetComponent<PlayerStats>();
        }
        
        UpdateHealthBar();
        
        if (healthBarCanvas != null)
        {
            healthBarCanvas.worldCamera = Camera.main;
        }
    }
    
    void Update()
    {
        if (isDead || player == null) return;
        
        // Face player
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0;
        if (directionToPlayer != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(directionToPlayer);
        }
        
        // Attack if in range
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
        }
        
        // Make health bar face camera
        if (healthBarCanvas != null && Camera.main != null)
        {
            healthBarCanvas.transform.LookAt(Camera.main.transform);
            healthBarCanvas.transform.Rotate(0, 180, 0);
        }
    }
    
    void Attack()
    {
        lastAttackTime = Time.time;
        
        if (playerStats != null)
        {
            playerStats.TakeDamage(damage, false);
            Debug.Log($"Enemy attacked player for {damage} damage!");
        }
    }
    
    public void TakeDamage(float damageAmount, bool isCrit)
    {
        if (isDead) return;
        
        float armorReduction = Mathf.Min(armor / 100f, 0.75f);
        float actualDamage = damageAmount * (1 - armorReduction);
        
        currentHealth -= actualDamage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        ShowDamageText(actualDamage, isCrit);
        UpdateHealthBar();
        
        Debug.Log($"Enemy took {actualDamage:F0} damage! Health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void ShowDamageText(float damage, bool isCrit)
    {
        GameObject damageTextObj = new GameObject("DamageText");
        damageTextObj.transform.position = transform.position + Vector3.up * 2f;
        
        DamageText damageText = damageTextObj.AddComponent<DamageText>();
        if (damageText != null)
        {
            damageText.Initialize(damage, isCrit);
        }
    }
    
    void Die()
    {
        isDead = true;
        Debug.Log($"Enemy died! Rewarding {expReward} XP to player.");
        
        // ✅ FIXED: Award experience to player
        if (playerStats != null)
        {
            ExperienceManager expManager = playerStats.GetComponent<ExperienceManager>();
            if (expManager != null)
            {
                expManager.GainExperience(expReward);
                Debug.Log($"✓ Awarded {expReward} XP to player");
            }
            else
            {
                Debug.LogWarning("ExperienceManager not found on player! Cannot award XP.");
            }
        }
        else
        {
            Debug.LogWarning("PlayerStats reference is null! Cannot award XP.");
        }
        
        // Drop loot
        if (Random.value < lootDropChance)
        {
            DropLoot();
        }
        
        Destroy(gameObject, 0.5f);
    }
    
    void DropLoot()
    {
        makeItems generator = FindFirstObjectByType<makeItems>();
        if (generator != null)
        {
            Item item;
            
            // 50/50 weapon or armor
            if (Random.value < 0.5f)
            {
                item = generator.GenerateWeapon(level);
            }
            else
            {
                item = generator.GenerateArmor(level);
            }
            
            CreateLootDrop(item);
        }
        else
        {
            Debug.LogWarning("makeItems (ItemGenerator) not found! Cannot drop loot.");
        }
    }
    
    void CreateLootDrop(Item item)
    {
        GameObject lootObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lootObj.transform.position = transform.position + Vector3.up;
        lootObj.transform.localScale = Vector3.one * 0.5f;
        
        Renderer renderer = lootObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = item.GetRarityColor();
        }
        
        LootDrop lootDrop = lootObj.AddComponent<LootDrop>();
        if (lootDrop != null)
        {
            lootDrop.SetItem(item);
        }
        
        Debug.Log($"Dropped {item.rarity} {item.itemName}");
    }
    
    void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            float healthPercent = currentHealth / maxHealth;
            healthBarFill.fillAmount = healthPercent;
        
            // Color gradient: Green → Yellow → Red
            if (healthPercent > 0.5f)
            {
                // Green to Yellow
                healthBarFill.color = Color.Lerp(Color.yellow, Color.green, (healthPercent - 0.5f) * 2f);
            }
            else
            {
                // Yellow to Red
                healthBarFill.color = Color.Lerp(Color.red, Color.yellow, healthPercent * 2f);
            }
        }
    }
    
    // Public getters for debugging/UI
    public int GetLevel() => level;
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public bool IsDead() => isDead;
}