// GameManager.cs - COMPLETE GAME MANAGEMENT SYSTEM
// Orchestrates player/enemy spawning, game loop, and all systems
// Now fully integrated with procedural character rig system

using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private bool autoSpawnPlayer = true;
    [SerializeField] private Vector3 playerSpawnPosition = new Vector3(0, 1, 0);
    [SerializeField] private int initialEnemyCount = 5;
    [SerializeField] private float enemySpawnRadius = 20f;
    
    [Header("Enemy Spawning")]
    [SerializeField] private bool continuousEnemySpawning = true;
    [SerializeField] private float enemySpawnInterval = 10f;
    [SerializeField] private int maxEnemies = 10;
    [SerializeField] private float minSpawnDistance = 10f;
    [SerializeField] private float maxSpawnDistance = 30f;
    
    [Header("References")]
    [SerializeField] private makeItems itemGenerator;
    [SerializeField] private Camera mainCamera;
    
    // Game state
    private GameObject player;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private float nextEnemySpawnTime = 0f;
    private bool gameStarted = false;
    
    // Singleton
    private static GameManager instance;
    public static GameManager Instance => instance;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Find or create item generator
        if (itemGenerator == null)
        {
            itemGenerator = FindObjectOfType<makeItems>();
            if (itemGenerator == null)
            {
                GameObject generatorObj = new GameObject("ItemGenerator");
                itemGenerator = generatorObj.AddComponent<makeItems>();
                Debug.Log("✅ Created ItemGenerator");
            }
        }
        
        // Setup camera
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject camObj = new GameObject("MainCamera");
                mainCamera = camObj.AddComponent<Camera>();
                camObj.AddComponent<AudioListener>();
                camObj.tag = "MainCamera";
                Debug.Log("✅ Created main camera");
            }
        }
    }
    
    void Start()
    {
        StartGame();
    }
    
    void Update()
    {
        if (!gameStarted) return;
        
        // Clean up dead enemies
        activeEnemies.RemoveAll(e => e == null);
        
        // Continuous enemy spawning
        if (continuousEnemySpawning && Time.time >= nextEnemySpawnTime)
        {
            if (activeEnemies.Count < maxEnemies)
            {
                SpawnRandomEnemy();
                nextEnemySpawnTime = Time.time + enemySpawnInterval;
            }
        }
        
        // Update camera to follow player
        if (player != null && mainCamera != null)
        {
            UpdateCameraFollow();
        }
    }
    
    public void StartGame()
    {
        Debug.Log("🎮 Starting Game...");
        
        // Spawn player
        if (autoSpawnPlayer)
        {
            player = itemGenerator.SpawnPlayer(playerSpawnPosition);
            
            // Setup camera to follow player
            if (mainCamera != null)
            {
                ThirdPersonCamera camController = mainCamera.GetComponent<ThirdPersonCamera>();
                if (camController == null)
                {
                    camController = mainCamera.gameObject.AddComponent<ThirdPersonCamera>();
                }
            }
        }
        
        // Spawn initial enemies
        for (int i = 0; i < initialEnemyCount; i++)
        {
            SpawnRandomEnemy();
        }
        
        gameStarted = true;
        nextEnemySpawnTime = Time.time + enemySpawnInterval;
        
        Debug.Log($"✅ Game started! Player spawned with {initialEnemyCount} enemies");
    }
    
    void SpawnRandomEnemy()
    {
        if (player == null) return;
        
        // Find random spawn position around player
        Vector3 spawnPos = GetRandomSpawnPosition();
        
        // Random level based on player level
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        int enemyLevel = 1;
        if (playerStats != null)
        {
            enemyLevel = playerStats.GetLevel() + Random.Range(-1, 2);
            enemyLevel = Mathf.Max(1, enemyLevel);
        }
        
        // Spawn enemy
        GameObject enemy = itemGenerator.SpawnEnemy(spawnPos, enemyLevel);
        activeEnemies.Add(enemy);
        
        Debug.Log($"Spawned level {enemyLevel} enemy at {spawnPos}");
    }
    
    Vector3 GetRandomSpawnPosition()
    {
        if (player == null) return Vector3.zero;
        
        // Get random angle
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        
        // Get random distance
        float distance = Random.Range(minSpawnDistance, maxSpawnDistance);
        
        // Calculate position
        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * distance,
            0,
            Mathf.Sin(angle) * distance
        );
        
        Vector3 spawnPos = player.transform.position + offset;
        
        // Raycast down to find ground
        if (Physics.Raycast(spawnPos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
        {
            spawnPos.y = hit.point.y + 0.1f;
        }
        else
        {
            spawnPos.y = 0;
        }
        
        return spawnPos;
    }
    
    void UpdateCameraFollow()
    {
        // Simple third-person camera follow
        Vector3 cameraOffset = new Vector3(0, 5, -8);
        Vector3 targetPos = player.transform.position + cameraOffset;
        
        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            targetPos,
            Time.deltaTime * 5f
        );
        
        mainCamera.transform.LookAt(player.transform.position + Vector3.up * 1.5f);
    }
    
    // Public accessors
    public GameObject GetPlayer() => player;
    
    public List<GameObject> GetActiveEnemies() => activeEnemies;
    
    public void RegisterEnemyDeath(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
        Debug.Log($"Enemy died. {activeEnemies.Count} enemies remaining.");
    }
    
    public void SetPlayerSpawnPosition(Vector3 position)
    {
        playerSpawnPosition = position;
    }
    
    public void SpawnEnemyAt(Vector3 position, int level = 1)
    {
        GameObject enemy = itemGenerator.SpawnEnemy(position, level);
        activeEnemies.Add(enemy);
    }
    
    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();
        Debug.Log("✅ All enemies cleared");
    }
    
    public void RestartGame()
    {
        Debug.Log("🔄 Restarting game...");
        
        // Destroy old player and enemies
        if (player != null) Destroy(player);
        ClearAllEnemies();
        
        // Restart
        gameStarted = false;
        StartGame();
    }
    
    // Debug commands
    [ContextMenu("Spawn Test Enemy")]
    void SpawnTestEnemy()
    {
        if (player != null)
        {
            Vector3 spawnPos = player.transform.position + player.transform.forward * 5f;
            SpawnEnemyAt(spawnPos, 1);
        }
    }
    
    [ContextMenu("Spawn 5 Enemies")]
    void Spawn5Enemies()
    {
        for (int i = 0; i < 5; i++)
        {
            SpawnRandomEnemy();
        }
    }
    
    [ContextMenu("Clear All Enemies")]
    void ClearEnemiesCommand()
    {
        ClearAllEnemies();
    }
    
    [ContextMenu("Give Player Items")]
    void GivePlayerItems()
    {
        if (player == null) return;
        
        InventoryManager inventory = player.GetComponent<InventoryManager>();
        if (inventory == null) return;
        
        PlayerStats stats = player.GetComponent<PlayerStats>();
        int level = stats != null ? stats.GetLevel() : 1;
        
        // Generate random items
        for (int i = 0; i < 5; i++)
        {
            if (Random.value < 0.5f)
            {
                Weapon weapon = itemGenerator.GenerateWeapon(level);
                inventory.AddItem(weapon);
            }
            else
            {
                Armor armor = itemGenerator.GenerateArmor(level);
                inventory.AddItem(armor);
            }
        }
        
        Debug.Log("✅ Gave player 5 random items");
    }
    
    void OnDrawGizmos()
    {
        if (player == null) return;
        
        // Draw spawn radius
        Gizmos.color = Color.yellow;
        DrawCircle(player.transform.position, minSpawnDistance);
        
        Gizmos.color = Color.red;
        DrawCircle(player.transform.position, maxSpawnDistance);
    }
    
    void DrawCircle(Vector3 center, float radius)
    {
        int segments = 32;
        float angleStep = 360f / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;
            
            Vector3 point1 = center + new Vector3(Mathf.Cos(angle1) * radius, 0, Mathf.Sin(angle1) * radius);
            Vector3 point2 = center + new Vector3(Mathf.Cos(angle2) * radius, 0, Mathf.Sin(angle2) * radius);
            
            Gizmos.DrawLine(point1, point2);
        }
    }
}